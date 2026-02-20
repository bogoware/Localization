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

    // --- Generic type FQDN key resolution ---

    [Fact]
    public void Format_GenericType_InMemoryRegistry_FormatsWithPlaceholders()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<GenericResult<string>>("Result: {Value} (code {Code})")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var result = formatter.Format(new GenericResult<string>("OK", 200));

        result.Should().Be("Result: OK (code 200)");
    }

    [Fact]
    public void Format_GenericType_NoTemplate_BuildsFallback()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var result = formatter.Format(new GenericResult<string>("OK", 200));

        result.Should().Contain("GenericResult`1");
        result.Should().Contain("Value=OK");
        result.Should().Contain("Code=200");
    }

    [Fact]
    public void GenericType_FullName_ContainsBacktickAndAssemblyQualifiedArgs()
    {
        var fullName = typeof(GenericResult<string>).FullName!;

        // Generic FQDN contains backtick notation for arity
        fullName.Should().Contain("`1");
        // Generic FQDN contains assembly-qualified type arguments in double brackets
        fullName.Should().Contain("[[System.String,");
    }

    [Fact]
    public void Format_GenericType_DifferentTypeArgs_ResolveDifferentTemplates()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<GenericResult<string>>("String result: {Value} ({Code})")
            .Add<GenericResult<int>>("Int result: {Value} ({Code})")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        formatter.Format(new GenericResult<string>("OK", 200))
            .Should().Be("String result: OK (200)");
        formatter.Format(new GenericResult<int>(42, 404))
            .Should().Be("Int result: 42 (404)");
    }

    // --- Nested/Recursive localization ---

    [Fact]
    public void Format_NestedLocalizable_FormatsRecursively()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestInvalidCurrencyError>("'{CurrencyCode}' is not a valid currency")
            .Add<TestPaymentError>("Payment field '{FieldName}' is invalid: {Detail}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestPaymentError("Amount", new TestInvalidCurrencyError("XYZ"));
        var result = formatter.Format(error);

        result.Should().Be("Payment field 'Amount' is invalid: 'XYZ' is not a valid currency");
    }

    [Fact]
    public void Format_NestedLocalizable_NoTemplateForInner_FallsBackRecursively()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestPaymentError>("Payment field '{FieldName}' is invalid: {Detail}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestPaymentError("Amount", new TestInvalidCurrencyError("XYZ"));
        var result = formatter.Format(error);

        result.Should().Be("Payment field 'Amount' is invalid: TestInvalidCurrencyError(CurrencyCode=XYZ)");
    }

    [Fact]
    public void Format_NestedLocalizable_NoTemplateForOuter_BuildFallbackRecursesInner()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestInvalidCurrencyError>("'{CurrencyCode}' is not a valid currency")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestPaymentError("Amount", new TestInvalidCurrencyError("XYZ"));
        var result = formatter.Format(error);

        result.Should().Be("TestPaymentError(FieldName=Amount, Detail='XYZ' is not a valid currency)");
    }

    [Fact]
    public void Format_NestedLocalizable_NullInnerProperty_ReplacesWithEmptyString()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestNullablePaymentError>("Payment field '{FieldName}' is invalid: {Detail}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestNullablePaymentError("Amount", null);
        var result = formatter.Format(error);

        result.Should().Be("Payment field 'Amount' is invalid: ");
    }

    [Fact]
    public void Format_NestedLocalizable_ThreeLevelsDeep_FormatsRecursively()
    {
        var registry = new InMemoryLocalizationRegistryBuilder()
            .Add<TestInvalidCurrencyError>("'{CurrencyCode}' is not a valid currency")
            .Add<TestPaymentError>("Payment field '{FieldName}' is invalid: {Detail}")
            .Add<TestTransactionError>("Transaction {TransactionId} failed: {Payment}")
            .Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestTransactionError("TX-001",
            new TestPaymentError("Amount", new TestInvalidCurrencyError("XYZ")));
        var result = formatter.Format(error);

        result.Should().Be("Transaction TX-001 failed: Payment field 'Amount' is invalid: 'XYZ' is not a valid currency");
    }

    [Fact]
    public void Format_NestedLocalizable_NoTemplatesForAny_DoubleFallback()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var error = new TestPaymentError("Amount", new TestInvalidCurrencyError("XYZ"));
        var result = formatter.Format(error);

        result.Should().Be("TestPaymentError(FieldName=Amount, Detail=TestInvalidCurrencyError(CurrencyCode=XYZ))");
    }
}
