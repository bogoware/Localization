using System.Text.Json;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Bogoware.Localization.AspNetCore.Tests.DefaultSetup;

public sealed class ProblemDetailsLocalizationTests(
    DefaultSetupFixture factory,
    ITestOutputHelper output) : IClassFixture<DefaultSetupFixture>
{
    [Fact]
    public async Task Post_Order_Returns_Localized_ProblemDetails_English()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");

        var response = await client.PostAsync("/api/orders", null);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"POST /api/orders [en-US]: {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Validation failed", doc.RootElement.GetProperty("title").GetString());

        var errors = doc.RootElement.GetProperty("errors");
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        Assert.Equal(3, errors.GetArrayLength());

        Assert.Equal("'CustomerName' is required", errors[0].GetString());
        Assert.Equal("'Email' is not a valid email address", errors[1].GetString());
        Assert.Equal("'Notes' must not exceed 500 characters", errors[2].GetString());
    }

    [Fact]
    public async Task Post_Order_Returns_Localized_ProblemDetails_Italian()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.PostAsync("/api/orders", null);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"POST /api/orders [it-IT]: {body}");

        using var doc = JsonDocument.Parse(body);

        var errors = doc.RootElement.GetProperty("errors");
        Assert.Equal("'CustomerName' è obbligatorio", errors[0].GetString());
        Assert.Equal("'Email' non è un indirizzo email valido", errors[1].GetString());
        Assert.Equal("'Notes' non deve superare 500 caratteri", errors[2].GetString());
    }
}
