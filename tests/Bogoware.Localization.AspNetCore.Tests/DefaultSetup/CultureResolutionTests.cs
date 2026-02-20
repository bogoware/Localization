using System.Text.Json;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Bogoware.Localization.AspNetCore.Tests.DefaultSetup;

public sealed class CultureResolutionTests(
    DefaultSetupFixture factory,
    ITestOutputHelper output) : IClassFixture<DefaultSetupFixture>
{
    [Fact]
    public async Task Accept_Language_Header_Determines_Culture()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.GetAsync("/api/orders/1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Accept-Language: it-IT → {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.StartsWith("L'ordine", status);
    }

    [Fact]
    public async Task Query_String_Culture_Determines_Culture()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/1?culture=it-IT");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"?culture=it-IT → {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.StartsWith("L'ordine", status);
    }

    [Fact]
    public async Task No_Culture_Specified_Uses_Default()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"No culture → {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.StartsWith("Order #", status);
    }

    [Fact]
    public async Task Unsupported_Culture_Falls_Back_To_Default()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("fr-FR");

        var response = await client.GetAsync("/api/orders/1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Accept-Language: fr-FR (unsupported) → {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        // Should fall back to en-US default
        Assert.StartsWith("Order #", status);
    }
}
