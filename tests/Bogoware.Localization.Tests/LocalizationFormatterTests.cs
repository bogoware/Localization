using System.Globalization;
using AwesomeAssertions;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
namespace Bogoware.Localization.Tests;

public class LocalizationFormatterTests
{
    private static IServiceProvider EmptyServiceProvider()
        => new ServiceCollection().BuildServiceProvider();

    [Fact]
    public void Format_SelfProvider_UsesLocalize()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());
        var self = new TestSelfProvider("hello from self");

        var result = formatter.Format(self);

        result.Should().Be("hello from self");
    }

    [Fact]
    public void Format_DiProvider_ResolvesFromContainer()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationProvider<TestLocalizable>>(new TestDiProvider());
        var sp = services.BuildServiceProvider();

        var formatter = new LocalizationFormatter(registry, sp);
        var value = new TestLocalizable();

        var result = formatter.Format(value);

        result.Should().Be("from DI provider");
    }

    [Fact]
    public void Format_RegistryTemplate_FormatsWithPlaceholders()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestRequiredFieldError>("'{FieldName}' is required")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestRequiredFieldError("Email");

        var result = formatter.Format(error);

        result.Should().Be("'Email' is required");
    }


    [Fact]
    public void Format_BuildsFallbackWhenNoTemplate()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());
        var error = new TestMaxLengthError("Name", 5);

        var result = formatter.Format(error);

        result.Should().Contain("TestMaxLengthError");
        result.Should().Contain("MaxLength=5");
    }

    [Fact]
    public void FormatT_ILocalizable_DelegatesToFormatOverload()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestRequiredFieldError>("'{FieldName}' is required")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestRequiredFieldError("Name");

        var result = formatter.Format<TestRequiredFieldError>(error);

        result.Should().Be("'Name' is required");
    }

    [Fact]
    public void FormatT_NonLocalizable_FallsBackToToString()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var result = formatter.Format(42);

        result.Should().Be("42");
    }

    [Fact]
    public void TestType_IsLocalizableString()
    {
        var error = new TestRequiredFieldError("X");
        error.Should().BeAssignableTo<ILocalizable>();
    }

    // --- Criticality-8: DI provider exception wrapping ---

    [Fact]
    public void Format_DiProvider_ThrowsDuringLocalize_WrapsInLocalizationFormattingException()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationProvider<TestLocalizable>>(new ThrowingDiProvider());
        var sp = services.BuildServiceProvider();

        var formatter = new LocalizationFormatter(registry, sp);
        var value = new TestLocalizable();

        FluentActions.Invoking(() => formatter.Format(value))
            .Should().Throw<LocalizationFormattingException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Provider blew up");
    }

    // --- Criticality-8: FormatTemplate with null property values ---

    [Fact]
    public void Format_TemplateWithNullProperty_ReplacesWithEmptyString()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestNullablePropertyError>("'{FieldName}' failed: {Details}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestNullablePropertyError("Email", null);
        var result = formatter.Format(error);

        result.Should().Be("'Email' failed: ");
    }

    [Fact]
    public void Format_TemplateWithNonNullProperty_ReplacesNormally()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestNullablePropertyError>("'{FieldName}' failed: {Details}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestNullablePropertyError("Email", "bad format");
        var result = formatter.Format(error);

        result.Should().Be("'Email' failed: bad format");
    }

    // --- Criticality-6: BuildFallback with DeclaredOnly (inheritance) ---

    [Fact]
    public void Format_BuildsFallback_OnlyIncludesDeclaredProperties()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestDerivedError("Email", 42);
        var result = formatter.Format(error);

        // DeclaredOnly: Code is declared on TestDerivedError, FieldName is inherited
        result.Should().Be("TestDerivedError(Code=42)");
    }

    // --- Criticality-6: Format<T> with non-ILocalizable + DI provider ---

    [Fact]
    public void FormatT_NonLocalizable_WithDiProvider_UsesProvider()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationProvider<NonLocalizableOrder>>(new NonLocalizableOrderProvider());
        var sp = services.BuildServiceProvider();

        var formatter = new LocalizationFormatter(registry, sp);
        var order = new NonLocalizableOrder { OrderId = "ABC" };

        var result = formatter.Format(order);

        result.Should().Be("Order #ABC");
    }

    // --- Criticality-5: Format<T>(null) for non-ILocalizable ---

    [Fact]
    public void FormatT_NonLocalizable_Null_ReturnsEmptyString()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var result = formatter.Format<NonLocalizableOrder>(null!);

        result.Should().Be("");
    }

    // --- Criticality-5: TryGetTemplate miss on populated registry ---

    [Fact]
    public void Format_PopulatedRegistry_UnknownKey_FallsBackToTypeName()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestRequiredFieldError>("'{FieldName}' is required")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        // TestMaxLengthError has no template registered
        var error = new TestMaxLengthError("Name", 10);
        var result = formatter.Format(error);

        result.Should().StartWith("TestMaxLengthError(");
        result.Should().Contain("MaxLength=10");
    }

    // --- Criticality-5: InMemoryLocalizationRegistry is culture-insensitive ---

    [Fact]
    public void InMemoryRegistry_ReturnsTemplateRegardlessOfCulture()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestRequiredFieldError>("'{FieldName}' is required")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestRequiredFieldError("Email");

        // Same template returned for any culture — InMemoryRegistry is culture-insensitive by design
        formatter.Format(error, new CultureInfo("en-US")).Should().Be("'Email' is required");
        formatter.Format(error, new CultureInfo("it-IT")).Should().Be("'Email' is required");
        formatter.Format(error, CultureInfo.InvariantCulture).Should().Be("'Email' is required");
    }
}
