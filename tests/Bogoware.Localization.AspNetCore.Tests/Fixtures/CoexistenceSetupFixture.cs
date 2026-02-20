using System.Globalization;
using Bogoware.Localization.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bogoware.Localization.AspNetCore.Tests.Fixtures;

/// <summary>
/// Setup with both Bogoware and framework AddLocalization for coexistence testing.
/// </summary>
public sealed class CoexistenceSetupFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Framework localization (IStringLocalizer)
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Bogoware localization
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
        });
    }
}
