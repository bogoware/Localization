using System.Globalization;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bogoware.Localization.Tests;

public class JsonLocalizedMessageRegistryTests
{
    private JsonLocalizedMessageRegistry LoadTestRegistry()
    {
        var builder = new JsonLocalizedMessageRegistryBuilder();
        builder.AddFromAssemblyResources(typeof(JsonLocalizedMessageRegistryTests).Assembly);
        return builder.Build();
    }

    // --- Registry builder tests ---

    [Fact]
    public void AddFromAssemblyResources_LoadsTestTemplates()
    {
        var registry = LoadTestRegistry();

        Assert.True(registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out var template));
        Assert.Equal("'{FieldName}' is required", template);
    }

    [Fact]
    public void AddFromAssemblyResources_SupportsMultipleCultures()
    {
        var registry = LoadTestRegistry();

        Assert.True(registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("it-IT"),
            out var itTemplate));
        Assert.Contains("obbligatorio", itTemplate);
    }

    [Fact]
    public void LaterLoad_OverridesEarlier()
    {
        var registry = new JsonLocalizedMessageRegistry();

        var baseJson = """{ "Key": "base" }""";
        registry.LoadFromJson(baseJson, new CultureInfo("en-US"));

        var overrideJson = """{ "Key": "override" }""";
        registry.LoadFromJson(overrideJson, new CultureInfo("en-US"));

        Assert.True(registry.TryGetTemplate("Key", new CultureInfo("en-US"), out var template));
        Assert.Equal("override", template);
    }

    [Fact]
    public void Build_ReturnsRegistryWithAllLoadedTemplates()
    {
        var registry = LoadTestRegistry();

        Assert.True(registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestMaxLengthError",
            new CultureInfo("en-US"),
            out _));
        Assert.True(registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestInvalidEmailError",
            new CultureInfo("en-US"),
            out _));
    }

    [Fact]
    public void CultureFallback_ParentCulture()
    {
        var registry = new JsonLocalizedMessageRegistry();
        registry.LoadFromJson("""{ "Key": "from en" }""", new CultureInfo("en"));

        Assert.True(registry.TryGetTemplate("Key", new CultureInfo("en-US"), out var template));
        Assert.Equal("from en", template);
    }

    // --- Formatter with JSON registry tests (formerly ValueErrorLocalizationTests) ---

    [Fact]
    public void RequiredFieldError_FormatsCorrectly_EnUs()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestRequiredFieldError("Code");
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Code' is required", result);
    }

    [Fact]
    public void RequiredFieldError_FormatsCorrectly_ItIt()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestRequiredFieldError("Code");
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("it-IT"));

        Assert.Equal("'Code' è obbligatorio", result);
    }

    [Fact]
    public void InvalidEmailError_FormatsCorrectly_EnUs()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestInvalidEmailError("Email");
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Email' is not a valid email address", result);
    }

    [Fact]
    public void MaxLengthError_SubstitutesPlaceholders_EnUs()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestMaxLengthError("Name", 5);
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Name' must not exceed 5 characters", result);
    }

    [Fact]
    public void MaxLengthError_SubstitutesPlaceholders_ItIt()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestMaxLengthError("Name", 5);
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("it-IT"));

        Assert.Equal("'Name' non deve superare 5 caratteri", result);
    }

    [Fact]
    public void MustBeGreaterThanError_SubstitutesPlaceholders()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestMustBeGreaterThanError("Length", 0);
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Length' must be greater than 0", result);
    }

    [Fact]
    public void OutOfRangeError_SubstitutesMinAndMax()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestOutOfRangeError("Port", 99999, 1, 65535);
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Port' must be between 1 and 65535", result);
    }

    [Fact]
    public void InvalidFormatError_SubstitutesExpectedFormat()
    {
        var registry = LoadTestRegistry();
        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var error = new TestInvalidFormatError("Code", "3 digits");
        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("'Code' does not match expected format: 3 digits", result);
    }

    // --- Override tests (formerly ErrorMessageOverrideTests) ---

    [Fact]
    public void Override_ReplacesBaseTemplate()
    {
        var registry = LoadTestRegistry();

        var overrideJson = """
        {
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError": "Field '{FieldName}' cannot be blank"
        }
        """;
        registry.LoadFromJson(overrideJson, new CultureInfo("en-US"));

        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());
        var error = new TestRequiredFieldError("Code");

        var result = formatter.Format((ILocalizableString)error, new CultureInfo("en-US"));

        Assert.Equal("Field 'Code' cannot be blank", result);
    }

    [Fact]
    public void Override_DoesNotAffectNonOverriddenTemplates()
    {
        var registry = LoadTestRegistry();

        var overrideJson = """
        {
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError": "OVERRIDDEN"
        }
        """;
        registry.LoadFromJson(overrideJson, new CultureInfo("en-US"));

        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        var required = new TestRequiredFieldError("X");
        Assert.Equal("OVERRIDDEN", formatter.Format((ILocalizableString)required, new CultureInfo("en-US")));

        var maxLength = new TestMaxLengthError("Name", 5);
        Assert.Equal("'Name' must not exceed 5 characters", formatter.Format((ILocalizableString)maxLength, new CultureInfo("en-US")));
    }

    [Fact]
    public void Override_DoesNotAffectOtherCultures()
    {
        var registry = LoadTestRegistry();

        var overrideJson = """
        {
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError": "OVERRIDDEN"
        }
        """;
        registry.LoadFromJson(overrideJson, new CultureInfo("en-US"));

        var formatter = new LocalizedMessageFormatter(registry, new ServiceCollection().BuildServiceProvider());

        Assert.Equal("OVERRIDDEN", formatter.Format((ILocalizableString)new TestRequiredFieldError("X"), new CultureInfo("en-US")));
        Assert.Equal("'X' è obbligatorio", formatter.Format((ILocalizableString)new TestRequiredFieldError("X"), new CultureInfo("it-IT")));
    }
}
