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
///     .AddFromAssembly(typeof(Program).Assembly)
///     .AddFromFile("overrides.en.json", new CultureInfo("en"))
///     .Build();
/// </code>
/// </example>
/// <param name="logger">Optional logger for diagnostic messages during resource loading.</param>
public class JsonLocalizationRegistryBuilder(ILogger? logger = null)
{
    private static readonly string[] DefaultPatterns = ["localized-messages", "error-messages"];
    private readonly JsonLocalizationRegistry _registry = new(logger);

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
    public JsonLocalizationRegistryBuilder AddFromAssembly(Assembly assembly, params string[] patterns)
    {
        var effectivePatterns = patterns.Length > 0 ? patterns : DefaultPatterns;

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => effectivePatterns.Any(p => n.Contains(p)) && n.EndsWith(".json"));

        var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? "unknown";

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

            if (logger is not null)
                Log.LoadingFromAssembly(logger, assemblyName, resourceName, culture.Name);

            _registry.LoadFromJson(json, culture, $"assembly:{assemblyName}:{resourceName}");
        }

        return this;
    }

    /// <summary>
    /// Scans the given assembly and all its <b>transitive</b> referenced assemblies for embedded
    /// localization resources. Assemblies are loaded in topological order (deepest dependencies
    /// first, root last), so the root assembly's templates override its dependencies'.
    /// </summary>
    /// <param name="root">The root assembly whose dependency tree to scan.</param>
    /// <param name="patterns">
    /// Substring patterns to match against resource names. When empty, defaults to
    /// <c>["localized-messages", "error-messages"]</c>.
    /// </param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <remarks>
    /// System assemblies (<c>System.*</c>, <c>Microsoft.*</c>, <c>netstandard</c>) are excluded
    /// from scanning. Assemblies that cannot be loaded are silently skipped.
    /// </remarks>
    public JsonLocalizationRegistryBuilder AddFromAssemblyTree(Assembly root, params string[] patterns)
    {
        var visited = new HashSet<string>();
        var ordered = new List<Assembly>();
        CollectReferences(root, visited, ordered);
        ordered.Add(root); // Root last (overrides)

        if (logger is not null)
            Log.ScanningAssemblyTree(logger, root.GetName().Name ?? "unknown", ordered.Count);

        foreach (var assembly in ordered)
            AddFromAssembly(assembly, patterns);

        return this;
    }

    /// <summary>
    /// Scans all loaded assemblies in the current AppDomain matching the given name prefixes.
    /// Assemblies are topologically sorted (dependencies first, dependents override).
    /// </summary>
    /// <param name="prefixes">Assembly name prefixes to match (e.g. <c>"MyApp"</c>).</param>
    /// <param name="patterns">
    /// Substring patterns to match against resource names. When empty, defaults to
    /// <c>["localized-messages", "error-messages"]</c>.
    /// </param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Assemblies are ordered by reference count (ascending), so base libraries are loaded first
    /// and application-level assemblies can override their templates.
    /// </remarks>
    public JsonLocalizationRegistryBuilder AddFromLoadedAssemblies(string[] prefixes, params string[] patterns)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => prefixes.Any(p => a.GetName().Name?.StartsWith(p, StringComparison.Ordinal) == true))
            // Approximate topological order: assemblies with fewer references tend to be
            // deeper dependencies. Assemblies at the same depth may load in any order.
            .OrderBy(a => a.GetReferencedAssemblies().Length)
            .ToList();

        if (logger is not null)
            Log.ScanningLoadedAssemblies(logger, string.Join(", ", prefixes), assemblies.Count);

        foreach (var assembly in assemblies)
            AddFromAssembly(assembly, patterns);

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
        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            throw new LocalizationConfigurationException(
                $"Failed to read localization file '{path}': {ex.Message}", ex);
        }
        if (logger is not null)
            Log.LoadingFromFile(logger, path, culture.Name);
        _registry.LoadFromJson(json, culture, $"file:{path}");
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
            AddFromAssembly(assembly, patterns);

        return this;
    }

    /// <summary>
    /// Builds the <see cref="JsonLocalizationRegistry"/> with all loaded templates.
    /// </summary>
    /// <returns>A fully populated <see cref="JsonLocalizationRegistry"/>.</returns>
    public JsonLocalizationRegistry Build() => _registry;

    private static void CollectReferences(Assembly assembly, HashSet<string> visited, List<Assembly> ordered)
    {
        foreach (var refName in assembly.GetReferencedAssemblies())
        {
            if (visited.Contains(refName.FullName)) continue;
            if (IsSystemAssembly(refName)) continue;
            visited.Add(refName.FullName);
            try
            {
                var referenced = Assembly.Load(refName);
                CollectReferences(referenced, visited, ordered);
                ordered.Add(referenced);
            }
            catch (FileNotFoundException) { /* Assembly not loadable, skip */ }
        }
    }

    private static bool IsSystemAssembly(AssemblyName name) =>
        name.Name?.StartsWith("System", StringComparison.Ordinal) == true ||
        name.Name?.StartsWith("Microsoft", StringComparison.Ordinal) == true ||
        name.Name == "netstandard";

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
                var candidate = parts[i + 1];
                try
                {
                    CultureInfo.GetCultureInfo(candidate);
                    return candidate;
                }
                catch (CultureNotFoundException)
                {
                    // Not a valid culture tag; try next segment.
                }
            }
        }
        return "";
    }
}
