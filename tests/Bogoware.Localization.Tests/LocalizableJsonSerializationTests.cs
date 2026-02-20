using System.Globalization;
using System.Text.Json;
using AwesomeAssertions;
using Bogoware.Localization.Serialization;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
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

public class StructExplicitModeDto
{
    public int Code { get; set; }

    [Localize]
    public StructRequiredFieldError? MarkedError { get; set; }

    public StructRequiredFieldError? UnmarkedError { get; set; }
}

public class StructDoNotLocalizeDto
{
    public StructRequiredFieldError? Error { get; set; }

    [DoNotLocalize]
    public StructRequiredFieldError? RawError { get; set; }
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

        doc.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(400);
        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
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
        warnings.ValueKind.Should().Be(JsonValueKind.Array);
        warnings.GetArrayLength().Should().Be(2);
        warnings[0].GetString().Should().Be("'Name' is required");
        warnings[1].GetString().Should().Be("'Bio' must not exceed 200 characters");
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

        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
        // OrderConfirmation is localized because of [Localize] + DI provider
        doc.RootElement.GetProperty("Confirmation").GetString().Should().StartWith("Order #123");
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
        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
        // RawError is serialized as object
        doc.RootElement.GetProperty("RawError").ValueKind.Should().Be(JsonValueKind.Object);
        doc.RootElement.GetProperty("RawError").GetProperty("FieldName").GetString().Should().Be("Email");
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

        doc.RootElement.GetProperty("Code").GetInt32().Should().Be(400);
        // UserMessage has [Localize] → localized string
        doc.RootElement.GetProperty("UserMessage").GetString().Should().Be("'Email' is required");
        // InternalError has no attribute → serialized as object
        doc.RootElement.GetProperty("InternalError").ValueKind.Should().Be(JsonValueKind.Object);
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
        doc.RootElement.GetProperty("Code").GetInt32().Should().Be(200);
        // Error is ILocalizable → localized
        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
        // Confirmation has a DI provider → localized
        doc.RootElement.GetProperty("Confirmation").GetString().Should().StartWith("Order #456");
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

        doc.RootElement.GetProperty("Error").ValueKind.Should().Be(JsonValueKind.Null);
        doc.RootElement.GetProperty("Warnings").ValueKind.Should().Be(JsonValueKind.Null);
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

        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' è obbligatorio");
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
            doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' è obbligatorio");
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
        FluentActions.Invoking(() =>
            JsonSerializer.Deserialize<AutoModeDto>(json, options)).Should().Throw<LocalizationSerializationException>();
    }

    // ──── Fluent chaining ────

    [Fact]
    public void AddLocalization_ReturnsSameOptionsInstance()
    {
        var formatter = CreateFormatter();
        var options = new JsonSerializerOptions();

        var result = options.AddLocalization(formatter);

        result.Should().BeSameAs(options);
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
        warnings.ValueKind.Should().Be(JsonValueKind.Array);
        warnings.GetArrayLength().Should().Be(3);
        warnings[0].GetString().Should().Be("'Name' is required");
        warnings[1].ValueKind.Should().Be(JsonValueKind.Null);
        warnings[2].GetString().Should().Be("'Email' is required");
    }

    // ──── Format<T> with null ────

    [Fact]
    public void FormatGeneric_NullValue_ReturnsEmptyString()
    {
        var formatter = CreateFormatter();

        var result = formatter.Format<TestLocalizable>(null!);

        result.Should().Be("");
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

        doc.RootElement.GetProperty("Code").GetInt32().Should().Be(200);
        doc.RootElement.GetProperty("Address").GetString().Should().Be("123 Main St, Springfield");
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
        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
        // RawError has [DoNotLocalize] → serialized as object even in Exhaustive mode
        doc.RootElement.GetProperty("RawError").ValueKind.Should().Be(JsonValueKind.Object);
        doc.RootElement.GetProperty("RawError").GetProperty("FieldName").GetString().Should().Be("Email");
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

        result.Should().Be("'Email' is required");
    }

    // ──── Struct ILocalizable support ────

    [Fact]
    public void Format_StructILocalizable_UsesRegistryTemplate()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));

        var error = new StructRequiredFieldError("Email");
        var result = formatter.Format(error);

        result.Should().Be("'Email' is required");
    }

    [Fact]
    public void Format_StructSelfProvider_UsesLocalize()
    {
        var formatter = CreateFormatter();

        var provider = new StructSelfProvider("localized value");
        var result = formatter.Format(provider);

        result.Should().Be("localized value");
    }

    [Fact]
    public void Format_NullableStructILocalizable_Null_ReturnsEmptyString()
    {
        var formatter = CreateFormatter();

        StructRequiredFieldError? value = null;
        var result = formatter.Format(value);

        result.Should().Be("");
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

        doc.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(400);
        doc.RootElement.GetProperty("DirectError").GetString().Should().Be("'Email' is required");
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

        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
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

        doc.RootElement.GetProperty("Error").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ──── Exception tests ────

    [Fact]
    public void MalformedJson_ThrowsLocalizationConfigurationException()
    {
        var registry = new JsonLocalizationRegistry();

        FluentActions.Invoking(() =>
            registry.LoadFromJson("{ not valid json }", CultureInfo.InvariantCulture)).Should().Throw<LocalizationConfigurationException>();
    }

    [Fact]
    public void MissingFile_ThrowsLocalizationConfigurationException()
    {
        var builder = new JsonLocalizationRegistryBuilder();

        FluentActions.Invoking(() =>
            builder.AddFromFile("/nonexistent/path/file.json", CultureInfo.InvariantCulture)).Should().Throw<LocalizationConfigurationException>();
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

        doc.RootElement.GetProperty("DirectProvider").GetString().Should().Be("direct value");
        doc.RootElement.GetProperty("NullableProvider").GetString().Should().Be("nullable value");
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

        FluentActions.Invoking(() =>
            JsonSerializer.Deserialize<StructAutoModeDto>(json, options)).Should().Throw<LocalizationSerializationException>();
    }

    [Fact]
    public void ExplicitMode_StructWithLocalizeAttribute_SerializesAsString()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter, LocalizationSerializationMode.Explicit);

        var dto = new StructExplicitModeDto
        {
            Code = 400,
            MarkedError = new StructRequiredFieldError("Email"),
            UnmarkedError = new StructRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("Code").GetInt32().Should().Be(400);
        // MarkedError has [Localize] → localized string
        doc.RootElement.GetProperty("MarkedError").GetString().Should().Be("'Email' is required");
        // UnmarkedError has no attribute → falls through to default STJ serialization (object)
        doc.RootElement.GetProperty("UnmarkedError").ValueKind.Should().Be(JsonValueKind.Object);
        doc.RootElement.GetProperty("UnmarkedError").GetProperty("FieldName").GetString().Should().Be("Email");
    }

    [Fact]
    public void ExhaustiveMode_StructWithDoNotLocalize_SerializesAsObject()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter, LocalizationSerializationMode.Exhaustive);

        var dto = new StructDoNotLocalizeDto
        {
            Error = new StructRequiredFieldError("Email"),
            RawError = new StructRequiredFieldError("Email")
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        // Error → localized string (ILocalizable, no [DoNotLocalize])
        doc.RootElement.GetProperty("Error").GetString().Should().Be("'Email' is required");
        // RawError has [DoNotLocalize] → serialized as object even in Exhaustive mode
        doc.RootElement.GetProperty("RawError").ValueKind.Should().Be(JsonValueKind.Object);
        doc.RootElement.GetProperty("RawError").GetProperty("FieldName").GetString().Should().Be("Email");
    }

    [Fact]
    public void AutoMode_EmptyCollection_SerializesAsEmptyArray()
    {
        var formatter = CreateFormatter();
        var options = CreateOptions(formatter);

        var dto = new AutoModeDto
        {
            StatusCode = 200,
            Warnings = new List<ILocalizable>()
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        var warnings = doc.RootElement.GetProperty("Warnings");
        warnings.ValueKind.Should().Be(JsonValueKind.Array);
        warnings.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void Format_BoxedStructILocalizable_UsesRegistryTemplate()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));

        ILocalizable boxed = new StructRequiredFieldError("Email");
        var result = formatter.Format(boxed, CultureInfo.InvariantCulture);

        result.Should().Be("'Email' is required");
    }

    [Fact]
    public void AutoMode_CollectionWithBoxedStructElements_SerializesAsArrayOfStrings()
    {
        var formatter = CreateFormatter(
            r => r.Add<StructRequiredFieldError>("'{FieldName}' is required"));
        var options = CreateOptions(formatter);

        var dto = new AutoModeDto
        {
            StatusCode = 400,
            Warnings = [new StructRequiredFieldError("Email")]  // boxed to ILocalizable
        };

        var json = JsonSerializer.Serialize(dto, options);
        using var doc = JsonDocument.Parse(json);

        var warnings = doc.RootElement.GetProperty("Warnings");
        warnings.ValueKind.Should().Be(JsonValueKind.Array);
        warnings.GetArrayLength().Should().Be(1);
        warnings[0].GetString().Should().Be("'Email' is required");
    }
}
