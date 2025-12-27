using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Builder for <see cref="JsonLocalizationRegistry"/> with assembly scanning,
/// file loading, and duplicate-override logging.
/// </summary>
public class JsonLocalizationRegistryBuilder(ILogger? logger = null)
{
    private static readonly string[] DefaultPatterns = ["localized-messages", "error-messages"];
    private readonly JsonLocalizationRegistry _registry = new();

    /// <summary>
    /// Scans embedded resources in the given assembly for JSON files matching the provided patterns
    /// (defaults to <c>["localized-messages", "error-messages"]</c>).
    /// </summary>
    public JsonLocalizationRegistryBuilder AddFromAssemblyResources(Assembly assembly, params string[] patterns)
    {
        var effectivePatterns = patterns.Length > 0 ? patterns : DefaultPatterns;

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => effectivePatterns.Any(p => n.Contains(p)) && n.EndsWith(".json"));

        foreach (var resourceName in resourceNames)
        {
            var cultureName = ExtractCulture(resourceName);
            var culture = string.IsNullOrEmpty(cultureName)
                ? CultureInfo.InvariantCulture
                : new CultureInfo(cultureName);

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null) continue;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            logger?.LogDebug(
                "Loading localization resources from {Assembly}:{Resource} for culture '{Culture}'",
                assembly.GetName().Name, resourceName, culture.Name);

            _registry.LoadFromJson(json, culture);
        }

        return this;
    }

    /// <summary>
    /// Loads a JSON file from the filesystem for the specified culture.
    /// </summary>
    public JsonLocalizationRegistryBuilder AddFromFile(string path, CultureInfo culture)
    {
        var json = File.ReadAllText(path);
        logger?.LogDebug("Loading localization from file {Path} for culture '{Culture}'", path, culture.Name);
        _registry.LoadFromJson(json, culture);
        return this;
    }

    /// <summary>
    /// Scans all loaded assemblies in the current AppDomain matching the given name prefixes.
    /// Assemblies are topologically sorted (dependencies first, dependents override).
    /// </summary>
    public JsonLocalizationRegistryBuilder AddFromLoadedAssemblies(params string[] prefixes)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => prefixes.Any(p => a.GetName().Name?.StartsWith(p, StringComparison.Ordinal) == true))
            .OrderBy(a => a.GetReferencedAssemblies().Length)
            .ToList();

        foreach (var assembly in assemblies)
            AddFromAssemblyResources(assembly);

        return this;
    }

    /// <summary>
    /// Loads resources from the given assemblies in order (latter overrides earlier).
    /// </summary>
    public JsonLocalizationRegistryBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies, params string[] patterns)
    {
        foreach (var assembly in assemblies)
            AddFromAssemblyResources(assembly, patterns);

        return this;
    }

    /// <summary>
    /// Builds the <see cref="JsonLocalizationRegistry"/> with all loaded templates.
    /// </summary>
    public JsonLocalizationRegistry Build() => _registry;

    private static string ExtractCulture(string resourceName)
    {
        // Pattern: *.messages.{culture}.json or *.error-messages.{culture}.json
        var parts = resourceName.Split('.');
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].EndsWith("messages", StringComparison.OrdinalIgnoreCase)
                && i + 2 < parts.Length
                && parts[i + 2] == "json")
            {
                return parts[i + 1];
            }
        }
        return "";
    }
}
