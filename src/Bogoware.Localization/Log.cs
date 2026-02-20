using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// High-performance, source-generated log methods for the localization system.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Loading localization from assembly {Assembly}, resource {Resource}, culture '{Culture}'")]
    internal static partial void LoadingFromAssembly(ILogger? logger, string assembly, string resource, string culture);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Loading localization from file {Path}, culture '{Culture}'")]
    internal static partial void LoadingFromFile(ILogger? logger, string path, string culture);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Scanning assembly tree rooted at {RootAssembly}, found {Count} assemblies")]
    internal static partial void ScanningAssemblyTree(ILogger? logger, string rootAssembly, int count);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Scanning loaded assemblies with prefixes [{Prefixes}], found {Count} matching")]
    internal static partial void ScanningLoadedAssemblies(ILogger? logger, string prefixes, int count);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Template override: key '{Fqdn}' for culture '{Culture}' replaced (source: {Source})")]
    internal static partial void TemplateOverride(ILogger? logger, string fqdn, string culture, string source);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Registered {Count} templates for culture '{Culture}' from {Source}")]
    internal static partial void TemplateRegistered(ILogger? logger, int count, string culture, string source);

    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Resolving localization for {TypeName}, culture '{Culture}'")]
    internal static partial void ResolvingLocalization(ILogger? logger, string typeName, string culture);

    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Resolved {TypeName} via {Step}")]
    internal static partial void ResolvedVia(ILogger? logger, string typeName, string step);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No localization template found for {TypeName}; using fallback")]
    internal static partial void FallbackUsed(ILogger? logger, string typeName);
}
