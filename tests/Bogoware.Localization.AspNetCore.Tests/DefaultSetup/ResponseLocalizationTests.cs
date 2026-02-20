using System.Text.Json;
using AwesomeAssertions;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;

namespace Bogoware.Localization.AspNetCore.Tests.DefaultSetup;

public sealed class ResponseLocalizationTests(
    DefaultSetupFixture factory,
    ITestOutputHelper output) : IClassFixture<DefaultSetupFixture>
{
    [Fact]
    public async Task Get_Order_Returns_Localized_English()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");

        var response = await client.GetAsync("/api/orders/42");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"GET /api/orders/42 [en-US]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();

        status.Should().Be("Order #42 is Shipped");
    }

    [Fact]
    public async Task Get_Order_Returns_Localized_Italian()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.GetAsync("/api/orders/7");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"GET /api/orders/7 [it-IT]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();

        status.Should().Be("L'ordine #7 è Shipped");
    }

    [Fact]
    public async Task Get_Order_Falls_Back_To_Default_Culture()
    {
        var client = factory.CreateClient();
        // No Accept-Language header — should fall back to en-US (default)

        var response = await client.GetAsync("/api/orders/1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"GET /api/orders/1 [no header]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();

        status.Should().Be("Order #1 is Shipped");
    }
}
