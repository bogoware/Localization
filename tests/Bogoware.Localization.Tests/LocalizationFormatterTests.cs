using System.Globalization;
using Bogoware.Localization.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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

        Assert.Equal("hello from self", result);
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

        Assert.Equal("from DI provider", result);
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

        Assert.Equal("'Email' is required", result);
    }


    [Fact]
    public void Format_BuildsFallbackWhenNoTemplate()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());
        var error = new TestMaxLengthError("Name", 5);

        var result = formatter.Format(error);

        Assert.Contains("TestMaxLengthError", result);
        Assert.Contains("MaxLength=5", result);
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

        Assert.Equal("'Name' is required", result);
    }

    [Fact]
    public void FormatT_NonLocalizable_FallsBackToToString()
    {
        var registry = new InMemoryLocalizationRegistryBuilder().Build();
        var formatter = new LocalizationFormatter(registry, EmptyServiceProvider());

        var result = formatter.Format(42);

        Assert.Equal("42", result);
    }

    [Fact]
    public void TestType_IsLocalizableString()
    {
        var error = new TestRequiredFieldError("X");
        Assert.IsAssignableFrom<ILocalizable>(error);
    }
}
