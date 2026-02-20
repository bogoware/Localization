using System.Diagnostics;

namespace Bogoware.Localization.AspNetCore.Diagnostics;

/// <summary>
/// Provides an <see cref="ActivitySource"/> for Bogoware localization tracing.
/// Wire this source name with your OpenTelemetry configuration to capture localization spans.
/// </summary>
public static class BogowareLocalizationActivitySource
{
    /// <summary>
    /// The activity source name: <c>"Bogoware.Localization.AspNetCore"</c>.
    /// </summary>
    public const string SourceName = "Bogoware.Localization.AspNetCore";

    internal static readonly ActivitySource Instance = new(SourceName);

    internal static Activity? StartCultureResolution(string? culture, string? source)
    {
        var activity = Instance.StartActivity("bogoware.localization.culture_resolution");
        activity?.SetTag("bogoware.localization.culture", culture ?? "invariant");
        activity?.SetTag("bogoware.localization.culture_source", source ?? "default");
        return activity;
    }

    internal static Activity? StartResponse(string path)
    {
        var activity = Instance.StartActivity("bogoware.localization.response");
        activity?.SetTag("bogoware.localization.path", path);
        return activity;
    }
}
