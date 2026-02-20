using System.Globalization;
using Bogoware.Localization.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bogoware.Localization.AspNetCore.Tests.Fixtures;

/// <summary>
/// Setup with Layer 2 response-buffering middleware enabled.
/// </summary>
public sealed class ResponseBufferingFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddBogowareLocalization(
                registry: b => b.AddFromAssemblyResources(typeof(Program).Assembly),
                middleware: options =>
                {
                    options.DefaultCulture = new CultureInfo("en-US");
                    options.SupportedCultures.AddRange([
                        new CultureInfo("en-US"),
                        new CultureInfo("it-IT"),
                    ]);
                    options.EnableResponseBuffering = true;
                    options.IncludePaths.Add("/api/");
                    options.ExcludePaths.Add("/api/health");
                });
        });
    }
}
