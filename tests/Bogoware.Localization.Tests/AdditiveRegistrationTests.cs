using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
namespace Bogoware.Localization.Tests;

public class AdditiveRegistrationTests
{
    private const string BaseJson = """
        {
          "Acme.Orders.OrderNotFound": "Order '{OrderId}' was not found in the system",
          "Acme.Orders.PaymentFailed": "Payment for order '{OrderId}' failed: {Reason}",
          "Acme.Orders.ItemOutOfStock": "Item '{ItemName}' is currently out of stock"
        }
        """;

    private const string OverrideJson = """
        {
          "Acme.Orders.OrderNotFound": "Sorry, we couldn't find order '{OrderId}'",
          "Acme.Orders.ShippingDelayed": "Shipping for order '{OrderId}' has been delayed"
        }
        """;

    [Fact]
    public void MultipleAddLocalization_MergesTemplates()
    {
        var services = new ServiceCollection();

        services.AddLocalization(b =>
            b.Build().LoadFromJson(BaseJson, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(OverrideJson, CultureInfo.InvariantCulture));

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ILocalizationRegistry>();

        // Base-only key
        registry.TryGetTemplate("Acme.Orders.ItemOutOfStock", CultureInfo.InvariantCulture, out _)
            .Should().BeTrue();

        // Override-only key
        registry.TryGetTemplate("Acme.Orders.ShippingDelayed", CultureInfo.InvariantCulture, out _)
            .Should().BeTrue();
    }

    [Fact]
    public void MultipleAddLocalization_LaterOverridesEarlier()
    {
        var services = new ServiceCollection();

        services.AddLocalization(b =>
            b.Build().LoadFromJson(BaseJson, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(OverrideJson, CultureInfo.InvariantCulture));

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ILocalizationRegistry>();

        registry.TryGetTemplate("Acme.Orders.OrderNotFound", CultureInfo.InvariantCulture, out var template)
            .Should().BeTrue();
        template.Should().Be("Sorry, we couldn't find order '{OrderId}'");
    }

    [Fact]
    public void MultipleAddLocalization_FormatterResolvesFromMergedRegistry()
    {
        var services = new ServiceCollection();

        services.AddLocalization(b =>
            b.Build().LoadFromJson(BaseJson, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(OverrideJson, CultureInfo.InvariantCulture));

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ILocalizationRegistry>();
        var formatter = sp.GetRequiredService<ILocalizationFormatter>();

        // Both services should resolve to the same merged registry
        formatter.Should().NotBeNull();
        registry.TryGetTemplate("Acme.Orders.ItemOutOfStock", CultureInfo.InvariantCulture, out _)
            .Should().BeTrue("base-only key should be in merged registry");
        registry.TryGetTemplate("Acme.Orders.OrderNotFound", CultureInfo.InvariantCulture, out var template)
            .Should().BeTrue("overridden key should be in merged registry");
        template.Should().Be("Sorry, we couldn't find order '{OrderId}'");
    }

    [Fact]
    public void AdditiveRegistration_WithEmbeddedResources_MergesLayered()
    {
        var services = new ServiceCollection();

        // Layer 1: base templates from embedded resource
        services.AddLocalization(b =>
            b.AddFromAssembly(typeof(AdditiveRegistrationTests).Assembly, "additive-base"));

        // Layer 2: override templates from embedded resource
        services.AddLocalization(b =>
            b.AddFromAssembly(typeof(AdditiveRegistrationTests).Assembly, "additive-override"));

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ILocalizationRegistry>();

        // Base-only key preserved
        registry.TryGetTemplate("Acme.Orders.PaymentFailed", CultureInfo.InvariantCulture, out _)
            .Should().BeTrue();

        // Override-only key added
        registry.TryGetTemplate("Acme.Orders.ShippingDelayed", CultureInfo.InvariantCulture, out _)
            .Should().BeTrue();

        // Overlapping key uses override value
        registry.TryGetTemplate("Acme.Orders.OrderNotFound", CultureInfo.InvariantCulture, out var template)
            .Should().BeTrue();
        template.Should().Be("Sorry, we couldn't find order '{OrderId}'");
    }

    [Fact]
    public void SingletonRegistrations_AreNotDuplicated()
    {
        var services = new ServiceCollection();

        services.AddLocalization(b =>
            b.Build().LoadFromJson(BaseJson, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(OverrideJson, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson("{}", CultureInfo.InvariantCulture));

        services.Count(d => d.ServiceType == typeof(ILocalizationRegistry)).Should().Be(1);
        services.Count(d => d.ServiceType == typeof(ILocalizationFormatter)).Should().Be(1);
    }

    [Fact]
    public void ThreeCallsWithLayering_AllMerged()
    {
        var layer1 = """{ "Key.A": "from-1", "Key.B": "from-1" }""";
        var layer2 = """{ "Key.B": "from-2", "Key.C": "from-2" }""";
        var layer3 = """{ "Key.C": "from-3", "Key.D": "from-3" }""";

        var services = new ServiceCollection();
        services.AddLocalization(b =>
            b.Build().LoadFromJson(layer1, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(layer2, CultureInfo.InvariantCulture));
        services.AddLocalization(b =>
            b.Build().LoadFromJson(layer3, CultureInfo.InvariantCulture));

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ILocalizationRegistry>();

        registry.TryGetTemplate("Key.A", CultureInfo.InvariantCulture, out var a).Should().BeTrue();
        a.Should().Be("from-1");

        registry.TryGetTemplate("Key.B", CultureInfo.InvariantCulture, out var b).Should().BeTrue();
        b.Should().Be("from-2");

        registry.TryGetTemplate("Key.C", CultureInfo.InvariantCulture, out var c).Should().BeTrue();
        c.Should().Be("from-3");

        registry.TryGetTemplate("Key.D", CultureInfo.InvariantCulture, out var d).Should().BeTrue();
        d.Should().Be("from-3");
    }

    [Fact]
    public void OverrideWithSource_ReplacesTemplate()
    {
        var registry = new JsonLocalizationRegistry();
        registry.LoadFromJson("""{ "Key.A": "first" }""", CultureInfo.InvariantCulture, "source-1");
        registry.LoadFromJson("""{ "Key.A": "second" }""", CultureInfo.InvariantCulture, "source-2");

        registry.TryGetTemplate("Key.A", CultureInfo.InvariantCulture, out var template).Should().BeTrue();
        template.Should().Be("second");
    }

    [Fact]
    public void LoadFromJson_MalformedJson_ThrowsLocalizationConfigurationException()
    {
        var registry = new JsonLocalizationRegistry();
        var act = () => registry.LoadFromJson("{ invalid json }", CultureInfo.InvariantCulture);
        act.Should().ThrowExactly<LocalizationConfigurationException>();
    }
}
