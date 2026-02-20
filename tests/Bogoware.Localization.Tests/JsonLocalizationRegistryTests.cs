using System.Globalization;
using AwesomeAssertions;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
namespace Bogoware.Localization.Tests;

public class JsonLocalizationRegistryTests
{
    private JsonLocalizationRegistry LoadTestRegistry()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromAssembly(typeof(JsonLocalizationRegistryTests).Assembly);
        return builder.Build();
    }

    private LocalizationFormatter CreateTestFormatter(JsonLocalizationRegistry? registry = null)
    {
        return new LocalizationFormatter(
            registry ?? LoadTestRegistry(),
            new ServiceCollection().BuildServiceProvider());
    }

    // --- Registry builder tests ---

    [Fact]
    public void AddFromAssembly_LoadsTestTemplates()
    {
        var registry = LoadTestRegistry();

        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out var template).Should().BeTrue();
        template.Should().Be("'{FieldName}' is required");
    }

    [Fact]
    public void AddFromAssembly_SupportsMultipleCultures()
    {
        var registry = LoadTestRegistry();

        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("it-IT"),
            out var itTemplate).Should().BeTrue();
        itTemplate.Should().Contain("obbligatorio");
    }

    [Fact]
    public void LaterLoad_OverridesEarlier()
    {
        var registry = new JsonLocalizationRegistry();

        var baseJson = """{ "Key": "base" }""";
        registry.LoadFromJson(baseJson, new CultureInfo("en-US"));

        var overrideJson = """{ "Key": "override" }""";
        registry.LoadFromJson(overrideJson, new CultureInfo("en-US"));

        registry.TryGetTemplate("Key", new CultureInfo("en-US"), out var template).Should().BeTrue();
        template.Should().Be("override");
    }

    [Fact]
    public void Build_ReturnsRegistryWithAllLoadedTemplates()
    {
        var registry = LoadTestRegistry();

        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestMaxLengthError",
            new CultureInfo("en-US"),
            out _).Should().BeTrue();
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestInvalidEmailError",
            new CultureInfo("en-US"),
            out _).Should().BeTrue();
    }

    [Fact]
    public void CultureFallback_ParentCulture()
    {
        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson("""{ "Key": "from en" }""", new CultureInfo("en"));

        registry.TryGetTemplate("Key", new CultureInfo("en-US"), out var template).Should().BeTrue();
        template.Should().Be("from en");
    }

    // --- Formatter with JSON registry tests (formerly ValueErrorLocalizationTests) ---

    [Fact]
    public void RequiredFieldError_FormatsCorrectly_EnUs()
    {
        var formatter = CreateTestFormatter();

        var error = new TestRequiredFieldError("Code");
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Code' is required");
    }

    [Fact]
    public void RequiredFieldError_FormatsCorrectly_ItIt()
    {
        var formatter = CreateTestFormatter();

        var error = new TestRequiredFieldError("Code");
        var result = formatter.Format((ILocalizable)error, new CultureInfo("it-IT"));

        result.Should().Be("'Code' è obbligatorio");
    }

    [Fact]
    public void InvalidEmailError_FormatsCorrectly_EnUs()
    {
        var formatter = CreateTestFormatter();

        var error = new TestInvalidEmailError("Email");
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Email' is not a valid email address");
    }

    [Fact]
    public void MaxLengthError_SubstitutesPlaceholders_EnUs()
    {
        var formatter = CreateTestFormatter();

        var error = new TestMaxLengthError("Name", 5);
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Name' must not exceed 5 characters");
    }

    [Fact]
    public void MaxLengthError_SubstitutesPlaceholders_ItIt()
    {
        var formatter = CreateTestFormatter();

        var error = new TestMaxLengthError("Name", 5);
        var result = formatter.Format((ILocalizable)error, new CultureInfo("it-IT"));

        result.Should().Be("'Name' non deve superare 5 caratteri");
    }

    [Fact]
    public void MustBeGreaterThanError_SubstitutesPlaceholders()
    {
        var formatter = CreateTestFormatter();

        var error = new TestMustBeGreaterThanError("Length", 0);
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Length' must be greater than 0");
    }

    [Fact]
    public void OutOfRangeError_SubstitutesMinAndMax()
    {
        var formatter = CreateTestFormatter();

        var error = new TestOutOfRangeError("Port", 99999, 1, 65535);
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Port' must be between 1 and 65535");
    }

    [Fact]
    public void InvalidFormatError_SubstitutesExpectedFormat()
    {
        var formatter = CreateTestFormatter();

        var error = new TestInvalidFormatError("Code", "3 digits");
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Code' does not match expected format: 3 digits");
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

        var formatter = CreateTestFormatter(registry);
        var error = new TestRequiredFieldError("Code");

        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("Field 'Code' cannot be blank");
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

        var formatter = CreateTestFormatter(registry);

        var required = new TestRequiredFieldError("X");
        formatter.Format((ILocalizable)required, new CultureInfo("en-US")).Should().Be("OVERRIDDEN");

        var maxLength = new TestMaxLengthError("Name", 5);
        formatter.Format((ILocalizable)maxLength, new CultureInfo("en-US")).Should().Be("'Name' must not exceed 5 characters");
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

        var formatter = CreateTestFormatter(registry);

        formatter.Format((ILocalizable)new TestRequiredFieldError("X"), new CultureInfo("en-US")).Should().Be("OVERRIDDEN");
        formatter.Format((ILocalizable)new TestRequiredFieldError("X"), new CultureInfo("it-IT")).Should().Be("'X' è obbligatorio");
    }

    // --- Record class tests ---

    [Fact]
    public void RecordClass_WithTemplate_FormatsCorrectly()
    {
        var formatter = CreateTestFormatter();

        var error = new RecordRequiredFieldError("Email");
        var result = formatter.Format((ILocalizable)error, new CultureInfo("en-US"));

        result.Should().Be("'Email' is required");
    }

    [Fact]
    public void RecordClass_SelfProvider_UsesLocalizeMethod()
    {
        var formatter = CreateTestFormatter();

        var provider = new RecordSelfProvider("custom message");
        var result = formatter.Format(provider, new CultureInfo("en-US"));

        result.Should().Be("custom message");
    }
}
