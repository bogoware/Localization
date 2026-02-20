using Bogoware.Localization.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Post-configures <see cref="JsonOptions"/> (used by MVC/Web API controllers)
/// to add the Bogoware localization type info modifier.
/// </summary>
internal sealed class BogowareLocalizationMvcJsonPostConfigure(
    ILocalizationFormatter formatter,
    IOptions<BogowareLocalizationMiddlewareOptions> bogowareOptions)
    : IPostConfigureOptions<JsonOptions>
{
    public void PostConfigure(string? name, JsonOptions options)
    {
        var mode = bogowareOptions.Value.SerializationMode;
        options.JsonSerializerOptions.AddLocalization(formatter, mode, culture: null);
    }
}
