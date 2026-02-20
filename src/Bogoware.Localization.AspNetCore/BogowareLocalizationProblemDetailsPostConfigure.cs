using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Post-configures <see cref="ProblemDetailsOptions"/> to localize
/// <see cref="ILocalizable"/> values found in <c>ProblemDetails.Extensions</c>.
/// Title and Detail are left to the developer — only Extensions values are processed.
/// </summary>
internal sealed class BogowareLocalizationProblemDetailsPostConfigure(
    ILocalizationFormatter formatter,
    IOptions<BogowareLocalizationMiddlewareOptions> bogowareOptions)
    : IPostConfigureOptions<ProblemDetailsOptions>
{
    public void PostConfigure(string? name, ProblemDetailsOptions options)
    {
        if (!bogowareOptions.Value.LocalizeProblemDetails) return;

        var previousCustomizer = options.CustomizeProblemDetails;

        options.CustomizeProblemDetails = context =>
        {
            previousCustomizer?.Invoke(context);

            var extensions = context.ProblemDetails.Extensions;
            if (extensions.Count == 0) return;

            // Snapshot keys to avoid modifying collection during iteration
            var keys = extensions.Keys.ToList();
            foreach (var key in keys)
            {
                if (extensions[key] is ILocalizable localizable)
                {
                    extensions[key] = formatter.Format(localizable);
                }
                else if (extensions[key] is IEnumerable<ILocalizable> localizables)
                {
                    extensions[key] = localizables.Select(l => formatter.Format(l)).ToList();
                }
            }
        };
    }
}
