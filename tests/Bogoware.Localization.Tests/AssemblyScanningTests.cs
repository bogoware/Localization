using System.Globalization;
using AwesomeAssertions;

namespace Bogoware.Localization.Tests;

public class AssemblyScanningTests
{
    [Fact]
    public void AddFromAssembly_LoadsSingleAssemblyResources()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromAssembly(typeof(AssemblyScanningTests).Assembly);
        var registry = builder.Build();

        // Should load from the test assembly's embedded resources
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out var template).Should().BeTrue();
        template.Should().Be("'{FieldName}' is required");
    }

    [Fact]
    public void AddFromAssembly_AcceptsCustomPatterns()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        // Use a pattern that matches the additive-base resource
        builder.AddFromAssembly(typeof(AssemblyScanningTests).Assembly, "additive-base");
        var registry = builder.Build();

        registry.TryGetTemplate(
            "Acme.Orders.OrderNotFound",
            CultureInfo.InvariantCulture,
            out var template).Should().BeTrue();
        template.Should().Contain("not found");

        // Should NOT have loaded the standard localized-messages
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out _).Should().BeFalse();
    }

    [Fact]
    public void AddFromAssemblyTree_LoadsRootAssemblyResources()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromAssemblyTree(typeof(AssemblyScanningTests).Assembly);
        var registry = builder.Build();

        // Root assembly resources should be loaded
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out _).Should().BeTrue();
    }

    [Fact]
    public void AddFromLoadedAssemblies_FiltersMatchingPrefixes()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromLoadedAssemblies(["Bogoware.Localization.Tests"]);
        var registry = builder.Build();

        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out _).Should().BeTrue();
    }

    [Fact]
    public void AddFromLoadedAssemblies_AcceptsCustomPatterns()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromLoadedAssemblies(["Bogoware.Localization.Tests"], "additive-base");
        var registry = builder.Build();

        registry.TryGetTemplate(
            "Acme.Orders.OrderNotFound",
            CultureInfo.InvariantCulture,
            out _).Should().BeTrue();

        // Default patterns NOT used, so standard messages should NOT be loaded
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out _).Should().BeFalse();
    }

    [Fact]
    public void AddFromLoadedAssemblies_DefaultPatternsUsedWhenEmpty()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromLoadedAssemblies(["Bogoware.Localization.Tests"]);
        var registry = builder.Build();

        // Default patterns include "localized-messages" and "error-messages"
        registry.TryGetTemplate(
            "Bogoware.Localization.Tests.Helpers.TestRequiredFieldError",
            new CultureInfo("en-US"),
            out _).Should().BeTrue();
    }

    [Fact]
    public void AddFromLoadedAssemblies_NoMatchingPrefix_ProducesEmptyRegistry()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromLoadedAssemblies(["ZZZ.NoSuchPrefix"]);
        var registry = builder.Build();

        registry.TryGetTemplate("AnyKey", CultureInfo.InvariantCulture, out _)
            .Should().BeFalse();
    }

    [Fact]
    public void AddFromAssembly_NoMatchingPatterns_ProducesEmptyRegistry()
    {
        var builder = new JsonLocalizationRegistryBuilder();
        builder.AddFromAssembly(typeof(AssemblyScanningTests).Assembly, "zzz-nonexistent-pattern");
        var registry = builder.Build();

        registry.TryGetTemplate("AnyKey", CultureInfo.InvariantCulture, out _)
            .Should().BeFalse();
    }
}
