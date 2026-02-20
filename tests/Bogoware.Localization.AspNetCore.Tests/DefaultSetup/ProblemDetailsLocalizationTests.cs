using System.Text.Json;
using AwesomeAssertions;
using Bogoware.Localization.AspNetCore.Tests.Fixtures;

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

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"POST /api/orders [en-US]: {body}");

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("title").GetString().Should().Be("Validation failed");

        var errors = doc.RootElement.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);
        errors.GetArrayLength().Should().Be(3);

        errors[0].GetString().Should().Be("'CustomerName' is required");
        errors[1].GetString().Should().Be("'Email' is not a valid email address");
        errors[2].GetString().Should().Be("'Notes' must not exceed 500 characters");
    }

    [Fact]
    public async Task Post_Order_Returns_Localized_ProblemDetails_Italian()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("it-IT");

        var response = await client.PostAsync("/api/orders", null);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"POST /api/orders [it-IT]: {body}");

        using var doc = JsonDocument.Parse(body);

        var errors = doc.RootElement.GetProperty("errors");
        errors[0].GetString().Should().Be("'CustomerName' è obbligatorio");
        errors[1].GetString().Should().Be("'Email' non è un indirizzo email valido");
        errors[2].GetString().Should().Be("'Notes' non deve superare 500 caratteri");
    }
}
