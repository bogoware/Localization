using System.Text.Json;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Bogoware.Localization.AspNetCore.Tests.ResponseBufferingSetup;

public sealed class ResponseBufferingTests(
    ResponseBufferingFixture factory,
    ITestOutputHelper output) : IClassFixture<ResponseBufferingFixture>
{
    [Fact]
    public async Task Buffered_Response_Is_Localized()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");

        var response = await client.GetAsync("/api/orders/10");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Buffered [en-US]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.Equal("Order #10 is Shipped", status);
    }

    [Fact]
    public async Task Buffered_Response_Italian()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.GetAsync("/api/orders/3");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Buffered [it-IT]: {body}");

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.StartsWith("L'ordine", status);
    }

    [Fact]
    public async Task Health_Endpoint_Is_Excluded_From_Buffering()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Health (excluded): {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.Equal("healthy", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Non_Json_Response_Is_Passed_Through()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/text");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Text (pass-through): {body}");

        Assert.Equal("plain text response", body);
    }
}
