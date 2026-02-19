using System.Globalization;
using System.Text.Json;
using Bogoware.Localization.Serialization;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bogoware.Localization.Tests;

// ──────────────────────── Test DTOs ────────────────────────

public class AutoModeDto
{
    public int StatusCode { get; set; }
    public TestRequiredFieldError? Error { get; set; }
    public List<ILocalizable>? Warnings { get; set; }
}

public class ExplicitModeDto
{
    public int Code { get; set; }

    [Localize]
    public TestRequiredFieldError? UserMessage { get; set; }

    public TestRequiredFieldError? InternalError { get; set; }
}

public class DoNotLocalizeDto
{
    public TestRequiredFieldError? Error { get; set; }

    [DoNotLocalize]
    public TestRequiredFieldError? RawError { get; set; }
}

public class NullableDto
{
    public TestRequiredFieldError? Error { get; set; }
    public List<ILocalizable>? Warnings { get; set; }
}

/// <summary>Non-ILocalizable type that can be localized via DI provider.</summary>
public class OrderConfirmation
{
    public string OrderId { get; set; } = "";
    public decimal Total { get; set; }
}

public class OrderConfirmationProvider : ILocalizationProvider<OrderConfirmation>
{
    public string Localize(OrderConfirmation value, CultureInfo? culture = null)
    {
        var c = culture ?? CultureInfo.CurrentUICulture;
        return c.TwoLetterISOLanguageName == "it"
            ? $"Ordine #{value.OrderId}"
            : $"Order #{value.OrderId}";
    }
}

/// <summary>Culture-aware self-localizing error for testing culture propagation.</summary>
public class CultureAwareError(string fieldName) : ILocalizationProvider
{
    public string FieldName { get; } = fieldName;

    public string Localize(CultureInfo? culture = null)
    {
        var c = culture ?? CultureInfo.CurrentUICulture;
        return c.TwoLetterISOLanguageName == "it"
            ? $"'{FieldName}' è obbligatorio"
            : $"'{FieldName}' is required";
    }
}

public class CultureTestDto
{
    public int StatusCode { get; set; }
    public CultureAwareError? Error { get; set; }
}

public class LocalizeNonLocalizableDto
{
    public TestRequiredFieldError? Error { get; set; }

    [Localize]
    public OrderConfirmation? Confirmation { get; set; }
}

public class ExhaustiveModeDto
{
    public int Code { get; set; }
    public TestRequiredFieldError? Error { get; set; }
    public OrderConfirmation? Confirmation { get; set; }
}

// ──────────────────────── Tests ────────────────────────

public class LocalizableJsonSerializationTests
{
    private static ILocalizationFormatter CreateFormatter(
        Action<InMemoryLocalizationRegistryBuilder>? configureRegistry = null,
        Action<ServiceCollection>? configureServices = null)
    {
        var registryBuilder = new InMemoryLocalizationRegistryBuilder();
        configureRegistry?.Invoke(registryBuilder);
        var registry = registryBuilder.Build();

        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var sp = services.BuildServiceProvider();

        return new LocalizationFormatter(registry, sp);
    }

    private static JsonSerializerOptions CreateOptions(
        ILocalizationFormatter formatter,
        LocalizationSerializationMode mode = LocalizationSerializationMode.Auto,
        CultureInfo? culture = null)
    {
        var options = new JsonSerializerOptions();
        options.AddLocalization(formatter, mode, culture);
        return options;
    }

    // ──── Auto Mode ────

    [Fact]
    public void AutoMode_ILocalizableProperty_SerializesAsLocalizedString()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new AutoModeDto
        {
            StatusCode = 400,
            Error = new TestRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(400, doc.RootElement.GetProperty("StatusCode").GetInt32());
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
    }

    [Fact]
    public void AutoMode_ILocalizableCollection_SerializesAsArrayOfStrings()
    {
        var formatter = CreateFormatter(r =>
        {
            r.Add<TestRequiredFieldError>("'{FieldName}' is required");
            r.Add<TestMaxLengthError>("'{FieldName}' must not exceed {MaxLength} characters");
        });
        var options = CreateOptions(formatter);

        var dto = new AutoModeDto
        {
            StatusCode = 400,
            Warnings = [new TestRequiredFieldError("Name"), new TestMaxLengthError("Bio", 200)]
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        var warnings = doc.RootElement.GetProperty("Warnings");
        Assert.Equal(JsonValueKind.Array, warnings.ValueKind);
        Assert.Equal(2, warnings.GetArrayLength());
        Assert.Equal("'Name' is required", warnings[0].GetString());
        Assert.Equal("'Bio' must not exceed 200 characters", warnings[1].GetString());
    }

    [Fact]
    public void AutoMode_LocalizeAttribute_OnNonILocalizable_LocalizesViaDiProvider()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"),
            s => s.AddSingleton<ILocalizationProvider<OrderConfirmation>>(new OrderConfirmationProvider()));
        var options = CreateOptions(formatter);

        var dto = new LocalizeNonLocalizableDto
        {
            Error = new TestRequiredFieldError("Email"),
            Confirmation = new OrderConfirmation { OrderId = "123", Total = 99.99m }
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
        // OrderConfirmation is localized because of [Localize] + DI provider
        Assert.StartsWith("Order #123", doc.RootElement.GetProperty("Confirmation").GetString());
    }

    [Fact]
    public void AutoMode_DoNotLocalize_SerializesAsObject()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new DoNotLocalizeDto
        {
            Error = new TestRequiredFieldError("Email"),
            RawError = new TestRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        // Error is localized
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
        // RawError is serialized as object
        Assert.Equal(JsonValueKind.Object, doc.RootElement.GetProperty("RawError").ValueKind);
        Assert.Equal("Email", doc.RootElement.GetProperty("RawError").GetProperty("FieldName").GetString());
    }

    // ──── Explicit Mode ────

    [Fact]
    public void ExplicitMode_OnlyLocalizeMarkedProperties()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter, LocalizationSerializationMode.Explicit);

        var dto = new ExplicitModeDto
        {
            Code = 400,
            UserMessage = new TestRequiredFieldError("Email"),
            InternalError = new TestRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(400, doc.RootElement.GetProperty("Code").GetInt32());
        // UserMessage has [Localize] → localized string
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("UserMessage").GetString());
        // InternalError has no attribute → serialized as object
        Assert.Equal(JsonValueKind.Object, doc.RootElement.GetProperty("InternalError").ValueKind);
    }

    // ──── Exhaustive Mode ────

    [Fact]
    public void ExhaustiveMode_AttemptsLocalizationOnAllProperties()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"),
            s => s.AddSingleton<ILocalizationProvider<OrderConfirmation>>(new OrderConfirmationProvider()));
        var options = CreateOptions(formatter, LocalizationSerializationMode.Exhaustive);

        var dto = new ExhaustiveModeDto
        {
            Code = 200,
            Error = new TestRequiredFieldError("Email"),
            Confirmation = new OrderConfirmation { OrderId = "456", Total = 50m }
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        // Code is a primitive → not localized
        Assert.Equal(200, doc.RootElement.GetProperty("Code").GetInt32());
        // Error is ILocalizable → localized
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
        // Confirmation has a DI provider → localized
        Assert.StartsWith("Order #456", doc.RootElement.GetProperty("Confirmation").GetString());
    }

    // ──── Null handling ────

    [Fact]
    public void NullILocalizableProperty_SerializesAsJsonNull()
    {
        var formatter = CreateFormatter();
        var options = CreateOptions(formatter);

        var dto = new NullableDto { Error = null, Warnings = null };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("Error").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("Warnings").ValueKind);
    }

    // ──── Fixed culture ────

    [Fact]
    public void FixedCulture_UsesSpecifiedCulture()
    {
        var formatter = CreateFormatter();
        var options = CreateOptions(formatter, culture: new CultureInfo("it-IT"));

        var dto = new CultureTestDto
        {
            StatusCode = 400,
            Error = new CultureAwareError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("'Email' è obbligatorio", doc.RootElement.GetProperty("Error").GetString());
    }

    // ──── Dynamic culture (CurrentUICulture) ────

    [Fact]
    public void DynamicCulture_UsesCurrentUICulture()
    {
        var formatter = CreateFormatter();
        // No fixed culture → uses CurrentUICulture
        var options = CreateOptions(formatter);

        var dto = new CultureTestDto
        {
            StatusCode = 400,
            Error = new CultureAwareError("Email")
        };

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("it-IT");
            var json = JsonSerializer.Serialize(dto, options);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("'Email' è obbligatorio", doc.RootElement.GetProperty("Error").GetString());
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    // ──── Deserialization throws ────

    [Fact]
    public void Deserialization_ThrowsNotSupportedException()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        // Serialize first to get valid JSON
        var dto = new AutoModeDto { StatusCode = 400, Error = new TestRequiredFieldError("Email") };
        var json = JsonSerializer.Serialize(dto, options);

        // Deserialization should throw because the converter is write-only
        Assert.Throws<NotSupportedException>(() =>
            JsonSerializer.Deserialize<AutoModeDto>(json, options));
    }

    // ──── Fluent chaining ────

    [Fact]
    public void AddLocalization_ReturnsSameOptionsInstance()
    {
        var formatter = CreateFormatter();
        var options = new JsonSerializerOptions();

        var result = options.AddLocalization(formatter);

        Assert.Same(options, result);
    }
}
