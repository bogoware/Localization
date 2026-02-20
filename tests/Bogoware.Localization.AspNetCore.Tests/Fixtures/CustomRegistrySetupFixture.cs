using System.Globalization;
using Bogoware.Localization.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bogoware.Localization.AspNetCore.Tests.Fixtures;

/// <summary>
/// Setup with manual JSON file loading via the registry builder.
/// </summary>
public sealed class CustomRegistrySetupFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddBogowareLocalization(
                registry: b =>
                {
                    // Load custom messages from file instead of embedded resources
                    var basePath = Path.Combine(AppContext.BaseDirectory, "Resources");
                    b.AddFromFile(
                        Path.Combine(basePath, "custom-messages.json"),
                        CultureInfo.InvariantCulture);
                },
                middleware: options =>
                {
                    options.DefaultCulture = new CultureInfo("en-US");
                    options.SupportedCultures.AddRange([
                        new CultureInfo("en-US"),
                    ]);
                });
        });
    }
}
