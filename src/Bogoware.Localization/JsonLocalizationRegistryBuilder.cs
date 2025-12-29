using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Builder for <see cref="JsonLocalizationRegistry"/> with assembly scanning,
/// file loading, and duplicate-override logging.
/// </summary>
/// <remarks>
/// All <c>Add*</c> methods are fluent and return the builder for chaining.
/// When the same FQDN key appears in multiple sources, the last-loaded value wins,
/// enabling downstream assemblies or files to override upstream templates.
/// </remarks>
/// <example>
/// <code>
/// var registry = new JsonLocalizationRegistryBuilder(logger)
///     .AddFromAssemblyResources(typeof(Program).Assembly)
///     .AddFromFile("overrides.en.json", new CultureInfo("en"))
///     .Build();
/// </code>
/// </example>
/// <param name="logger">Optional logger for diagnostic messages during resource loading.</param>
public class JsonLocalizationRegistryBuilder(ILogger? logger = null)
{
    private static readonly string[] DefaultPatterns = ["localized-messages", "error-messages"];
    private readonly JsonLocalizationRegistry _registry = new();

    /// <summary>
    /// Scans embedded resources in the given assembly for JSON files matching the provided patterns
    /// (defaults to <c>["localized-messages", "error-messages"]</c>).
    /// </summary>
    /// <param name="assembly">The assembly whose embedded resources to scan.</param>
    /// <param name="patterns">
    /// Substring patterns to match against resource names. When empty, defaults to
    /// <c>["localized-messages", "error-messages"]</c>.
    /// </param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Embedded resources must follow the naming convention
    /// <c>{Namespace}.{pattern}.{culture}.json</c> (e.g. <c>MyApp.localized-messages.it.json</c>).
    /// Resources without a culture segment are loaded as <see cref="CultureInfo.InvariantCulture"/>.
    /// </remarks>
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
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="culture">The culture these templates belong to.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// <param name="prefixes">Assembly name prefixes to match (e.g. <c>"MyApp"</c>).</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Assemblies are ordered by reference count (ascending), so base libraries are loaded first
    /// and application-level assemblies can override their templates.
    /// </remarks>
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
    /// <param name="assemblies">The assemblies to scan, in order.</param>
    /// <param name="patterns">
    /// Substring patterns to match against resource names. When empty, uses defaults.
    /// </param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public JsonLocalizationRegistryBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies, params string[] patterns)
    {
        foreach (var assembly in assemblies)
            AddFromAssemblyResources(assembly, patterns);

        return this;
    }

    /// <summary>
    /// Builds the <see cref="JsonLocalizationRegistry"/> with all loaded templates.
    /// </summary>
    /// <returns>A fully populated <see cref="JsonLocalizationRegistry"/>.</returns>
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
