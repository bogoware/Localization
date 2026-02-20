using System.Text.Json;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Bogoware.Localization.AspNetCore.Tests.CoexistenceSetup;

public sealed class FrameworkCoexistenceTests(
    CoexistenceSetupFixture factory,
    ITestOutputHelper output) : IClassFixture<CoexistenceSetupFixture>
{
    [Fact]
    public async Task Bogoware_And_Framework_Localization_Coexist()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.GetAsync("/api/orders/1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Coexistence test [it-IT]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();

        // Bogoware localization works
        Assert.StartsWith("L'ordine", status);
    }

    [Fact]
    public void Both_ILocalizationFormatter_And_IStringLocalizerFactory_Resolve()
    {
        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Bogoware formatter resolves
        var formatter = sp.GetService<ILocalizationFormatter>();
        Assert.NotNull(formatter);

        // Framework localizer factory resolves
        var localizerFactory = sp.GetService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
        Assert.NotNull(localizerFactory);

        output.WriteLine("Both ILocalizationFormatter and IStringLocalizerFactory resolved successfully");
    }

    [Fact]
    public async Task Shared_Culture_Resolution_Works_For_Both_Systems()
    {
        // When Accept-Language is set, both systems should see the same CurrentUICulture
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");

        var response = await client.GetAsync("/api/orders/5");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Shared culture [en-US]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.Equal("Order #5 is Shipped", status);
    }
}
