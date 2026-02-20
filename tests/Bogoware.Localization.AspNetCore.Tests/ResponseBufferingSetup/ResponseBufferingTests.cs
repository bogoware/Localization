using System.Text.Json;
using AwesomeAssertions;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;

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
        status.Should().Be("Order #10 is Shipped");
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
        status.Should().StartWith("L'ordine");
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
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
    }

    [Fact]
    public async Task Non_Json_Response_Is_Passed_Through()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/text");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Text (pass-through): {body}");

        body.Should().Be("plain text response");
    }
}
