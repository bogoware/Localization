using System.Globalization;
using Bogoware.Localization.Serialization;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Configuration options for the Bogoware localization ASP.NET Core integration.
/// </summary>
public sealed class BogowareLocalizationMiddlewareOptions
{
    /// <summary>
    /// The serialization mode applied to JSON responses.
    /// Defaults to <see cref="LocalizationSerializationMode.Auto"/>.
    /// </summary>
    public LocalizationSerializationMode SerializationMode { get; set; } = LocalizationSerializationMode.Auto;

    /// <summary>
    /// The default culture used when no culture can be determined from the request.
    /// Defaults to <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// The cultures supported by this application. Used to configure
    /// <see cref="Microsoft.AspNetCore.Builder.RequestLocalizationOptions"/>.
    /// </summary>
    public List<CultureInfo> SupportedCultures { get; } = [];

    /// <summary>
    /// When <see langword="true"/>, <see cref="ILocalizable"/> values found in
    /// <c>ProblemDetails.Extensions</c> are formatted using the localization formatter.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool LocalizeProblemDetails { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, enables OpenTelemetry-compatible diagnostics
    /// via <see cref="System.Diagnostics.ActivitySource"/>.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableDiagnostics { get; set; }

    /// <summary>
    /// When <see langword="true"/>, enables the response-buffering middleware (Layer 2)
    /// that intercepts raw JSON writes for localization.
    /// Defaults to <see langword="false"/>. Most applications do not need this.
    /// </summary>
    public bool EnableResponseBuffering { get; set; }

    /// <summary>
    /// Request path prefixes that the response-buffering middleware should process.
    /// Only evaluated when <see cref="EnableResponseBuffering"/> is <see langword="true"/>.
    /// An empty list means all paths are included.
    /// </summary>
    public List<string> IncludePaths { get; } = [];

    /// <summary>
    /// Request path prefixes that the response-buffering middleware should skip.
    /// Takes precedence over <see cref="IncludePaths"/>.
    /// Only evaluated when <see cref="EnableResponseBuffering"/> is <see langword="true"/>.
    /// </summary>
    public List<string> ExcludePaths { get; } = [];
}
