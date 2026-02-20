using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Configures <see cref="RequestLocalizationOptions"/> from
/// <see cref="BogowareLocalizationMiddlewareOptions"/>.
/// This is registered via DI so that the framework's <c>UseRequestLocalization()</c>
/// picks up Bogoware's culture settings, enabling coexistence with
/// ASP.NET Core's built-in <c>AddLocalization()</c>.
/// </summary>
internal sealed class BogowareLocalizationRequestLocalizationConfigure(
    IOptions<BogowareLocalizationMiddlewareOptions> bogowareOptions)
    : IConfigureOptions<RequestLocalizationOptions>
{
    public void Configure(RequestLocalizationOptions options)
    {
        var mwOptions = bogowareOptions.Value;

        if (mwOptions.SupportedCultures.Count > 0)
        {
            options.SetDefaultCulture(mwOptions.DefaultCulture.Name);
            options.AddSupportedCultures(mwOptions.SupportedCultures.Select(c => c.Name).ToArray());
            options.AddSupportedUICultures(mwOptions.SupportedCultures.Select(c => c.Name).ToArray());
        }
        else if (mwOptions.DefaultCulture.Name is { Length: > 0 } name)
        {
            options.SetDefaultCulture(name);
        }
    }
}
