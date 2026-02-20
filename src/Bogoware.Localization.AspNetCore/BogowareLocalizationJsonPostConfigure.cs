using Bogoware.Localization.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Post-configures <see cref="JsonOptions"/> (used by minimal APIs)
/// to add the Bogoware localization type info modifier.
/// Culture is <see langword="null"/> so that <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>
/// is evaluated at serialization time — enabling per-request localization without response buffering.
/// </summary>
internal sealed class BogowareLocalizationJsonPostConfigure(
    ILocalizationFormatter formatter,
    IOptions<BogowareLocalizationMiddlewareOptions> bogowareOptions)
    : IPostConfigureOptions<JsonOptions>
{
    public void PostConfigure(string? name, JsonOptions options)
    {
        var mode = bogowareOptions.Value.SerializationMode;
        options.SerializerOptions.AddLocalization(formatter, mode, culture: null);
    }
}
