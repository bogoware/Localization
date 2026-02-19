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

public class ExhaustiveToStringFallbackDto
{
    public int Code { get; set; }
    public NonLocalizableAddress? Address { get; set; }
}

public class StructAutoModeDto
{
    public int StatusCode { get; set; }
    public StructRequiredFieldError? Error { get; set; }       // Nullable<struct>
    public StructRequiredFieldError DirectError { get; set; }  // Non-nullable struct
}

public class StructSelfProviderDto
{
    public StructSelfProvider DirectProvider { get; set; }
    public StructSelfProvider? NullableProvider { get; set; }
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
    public void Deserialization_ThrowsLocalizationSerializationException()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        // Serialize first to get valid JSON
        var dto = new AutoModeDto { StatusCode = 400, Error = new TestRequiredFieldError("Email") };
        var json = JsonSerializer.Serialize(dto, options);

        // Deserialization should throw because the converter is write-only
        Assert.Throws<LocalizationSerializationException>(() =>
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

    // ──── Collection with null elements ────

    [Fact]
    public void AutoMode_CollectionWithNullElements_SerializesNullsAsJsonNull()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new AutoModeDto
        {
            StatusCode = 400,
            Warnings = [new TestRequiredFieldError("Name"), null!, new TestRequiredFieldError("Email")]
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        var warnings = doc.RootElement.GetProperty("Warnings");
        Assert.Equal(JsonValueKind.Array, warnings.ValueKind);
        Assert.Equal(3, warnings.GetArrayLength());
        Assert.Equal("'Name' is required", warnings[0].GetString());
        Assert.Equal(JsonValueKind.Null, warnings[1].ValueKind);
        Assert.Equal("'Email' is required", warnings[2].GetString());
    }

    // ──── Format<T> with null ────

    [Fact]
    public void FormatGeneric_NullValue_ReturnsEmptyString()
    {
        var formatter = CreateFormatter();

        var result = formatter.Format<TestLocalizable>(null!);

        Assert.Equal("", result);
    }

    // ──── Exhaustive mode ToString fallback ────

    [Fact]
    public void ExhaustiveMode_NoProvider_FallsBackToToString()
    {
        var formatter = CreateFormatter();
        var options = CreateOptions(formatter, LocalizationSerializationMode.Exhaustive);

        var dto = new ExhaustiveToStringFallbackDto
        {
            Code = 200,
            Address = new NonLocalizableAddress { Street = "123 Main St", City = "Springfield" }
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(200, doc.RootElement.GetProperty("Code").GetInt32());
        Assert.Equal("123 Main St, Springfield", doc.RootElement.GetProperty("Address").GetString());
    }

    // ──── [DoNotLocalize] in Exhaustive mode ────

    [Fact]
    public void ExhaustiveMode_DoNotLocalize_SerializesAsObject()
    {
        var formatter = CreateFormatter(
            r => r.Add<TestRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter, LocalizationSerializationMode.Exhaustive);

        var dto = new DoNotLocalizeDto
        {
            Error = new TestRequiredFieldError("Email"),
            RawError = new TestRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        // Error is localized even in Exhaustive mode (ILocalizable)
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
        // RawError has [DoNotLocalize] → serialized as object even in Exhaustive mode
        Assert.Equal(JsonValueKind.Object, doc.RootElement.GetProperty("RawError").ValueKind);
        Assert.Equal("Email", doc.RootElement.GetProperty("RawError").GetProperty("FieldName").GetString());
    }

    // ──── Invariant culture fallback in registry ────

    [Fact]
    public void InvariantCultureFallback_ResolvesWhenSpecificCultureMissing()
    {
        // Build a JsonLocalizationRegistry with only an invariant-culture template
        var registry = new JsonLocalizationRegistry();
        var fqdn = typeof(TestRequiredFieldError).FullName!;
        var json = $$"""{ "{{fqdn}}": "'{FieldName}' is required" }""";
        registry.LoadFromJson(json, CultureInfo.InvariantCulture);

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var formatter = new LocalizationFormatter(registry, sp);

        // Look up with a specific culture that has no entry → should fall back to invariant
        var error = new TestRequiredFieldError("Email");
        var result = formatter.Format(error, new CultureInfo("fr-FR"));

        Assert.Equal("'Email' is required", result);
    }

    // ──── Struct ILocalizable support ────

    [Fact]
    public void Format_StructILocalizable_UsesRegistryTemplate()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));

        var error = new StructRequiredFieldError("Email");
        var result = formatter.Format(error);

        Assert.Equal("'Email' is required", result);
    }

    [Fact]
    public void Format_StructSelfProvider_UsesLocalize()
    {
        var formatter = CreateFormatter();

        var provider = new StructSelfProvider("localized value");
        var result = formatter.Format(provider);

        Assert.Equal("localized value", result);
    }

    [Fact]
    public void Format_NullableStructILocalizable_Null_ReturnsEmptyString()
    {
        var formatter = CreateFormatter();

        StructRequiredFieldError? value = null;
        var result = formatter.Format(value);

        Assert.Equal("", result);
    }

    // ──── Struct serialization ────

    [Fact]
    public void AutoMode_StructILocalizable_SerializesAsString()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new StructAutoModeDto
        {
            StatusCode = 400,
            DirectError = new StructRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(400, doc.RootElement.GetProperty("StatusCode").GetInt32());
        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("DirectError").GetString());
    }

    [Fact]
    public void AutoMode_NullableStructILocalizable_NonNull_SerializesAsString()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new StructAutoModeDto
        {
            StatusCode = 400,
            Error = new StructRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("'Email' is required", doc.RootElement.GetProperty("Error").GetString());
    }

    [Fact]
    public void AutoMode_NullableStructILocalizable_Null_SerializesAsJsonNull()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new StructAutoModeDto
        {
            StatusCode = 200,
            Error = null
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("Error").ValueKind);
    }

    // ──── Exception tests ────

    [Fact]
    public void MalformedJson_ThrowsLocalizationConfigurationException()
    {
        var registry = new JsonLocalizationRegistry();

        Assert.Throws<LocalizationConfigurationException>(() =>
            registry.LoadFromJson("{ not valid json }", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void MissingFile_ThrowsLocalizationConfigurationException()
    {
        var builder = new JsonLocalizationRegistryBuilder();

        Assert.Throws<LocalizationConfigurationException>(() =>
            builder.AddFromFile("/nonexistent/path/file.json", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void AutoMode_StructSelfProvider_SerializesAsLocalizedString()
    {
        var formatter = CreateFormatter();
        var options = CreateOptions(formatter);

        var dto = new StructSelfProviderDto
        {
            DirectProvider = new StructSelfProvider("direct value"),
            NullableProvider = new StructSelfProvider("nullable value")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("direct value", doc.RootElement.GetProperty("DirectProvider").GetString());
        Assert.Equal("nullable value", doc.RootElement.GetProperty("NullableProvider").GetString());
    }

    [Fact]
    public void Deserialization_NullableStruct_ThrowsLocalizationSerializationException()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new StructAutoModeDto
        {
            StatusCode = 400,
            Error = new StructRequiredFieldError("Email")
        };
        var json = JsonSerializer.Serialize(dto, options);

        Assert.Throws<LocalizationSerializationException>(() =>
            JsonSerializer.Deserialize<StructAutoModeDto>(json, options));
    }
}
