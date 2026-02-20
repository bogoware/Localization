using System.Globalization;
using System.Net;
using AwesomeAssertions;
using Bogoware.Localization.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bogoware.Localization.AspNetCore.Tests.AdditiveSetup;

/// <summary>
/// Fixture that calls <c>AddBogowareLocalization</c> twice: first with embedded resources,
/// then with an inline override. Verifies additive DI registration in ASP.NET Core context.
/// </summary>
public sealed class AdditiveSetupFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // First call: standard assembly scan
            services.AddBogowareLocalization(
                registry: b => b.AddFromAssembly(typeof(Program).Assembly),
                middleware: options =>
                {
                    options.DefaultCulture = new CultureInfo("en-US");
                    options.SupportedCultures.AddRange([
                        new CultureInfo("en-US"),
                        new CultureInfo("it-IT"),
                    ]);
                });

            // Second call: adds an override for OrderStatus en-US (additive)
            services.AddBogowareLocalization(
                registry: b => b.Build().LoadFromJson(
                    """{ "Bogoware.Localization.Sample.Api.Models.OrderStatus": "OVERRIDDEN: Order #{OrderNumber}" }""",
                    new CultureInfo("en-US")));
        });
    }
}

public class AdditiveRegistrationTests(AdditiveSetupFixture fixture)
    : IClassFixture<AdditiveSetupFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public void AdditiveRegistration_RegistryAndFormatterResolvable()
    {
        using var scope = fixture.Services.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<ILocalizationRegistry>();
        var formatter = scope.ServiceProvider.GetRequiredService<ILocalizationFormatter>();

        registry.Should().NotBeNull();
        formatter.Should().NotBeNull();
    }

    [Fact]
    public async Task AdditiveRegistration_OverrideTakesEffectForEnUs()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders/42");
        request.Headers.Add("Accept-Language", "en-US");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        // The second AddBogowareLocalization call overrides OrderStatus for en-US
        body.Should().Contain("OVERRIDDEN");
    }

    [Fact]
    public async Task AdditiveRegistration_OriginalTemplatesStillAvailableForOtherCultures()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders/42");
        request.Headers.Add("Accept-Language", "it-IT");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        // Italian template from the first (assembly) call should still work
        body.Should().Contain("ordine");
    }
}
