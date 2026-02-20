using System.Globalization;
using Bogoware.Localization.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bogoware.Localization.AspNetCore.Tests.Fixtures;

/// <summary>
/// Standard Bogoware-only setup: assembly scan + default middleware options.
/// </summary>
public sealed class DefaultSetupFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Re-register with explicit cultures for test determinism
            services.AddBogowareLocalization(
                registry: b => b.AddFromAssembly(typeof(Program).Assembly),
                middleware: options =>
                {
                    options.DefaultCulture = new CultureInfo("en-US");
                    options.SupportedCultures.AddRange([
                        new CultureInfo("en-US"),
                        new CultureInfo("it-IT"),
                    ]);
                    options.LocalizeProblemDetails = true;
                });
        });
    }
}
