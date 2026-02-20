using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// JSON-backed registry for managing localized message templates with culture fallback chain.
/// </summary>
public class JsonLocalizationRegistry(ILogger? logger = null) : ILocalizationRegistry
{
    private static readonly JsonSerializerOptions JsoncOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly Dictionary<string, Dictionary<string, string>> _templates = new();

    /// <summary>
    /// Loads message templates from a JSON or JSONC string (FQDN → template pairs) for the specified culture.
    /// </summary>
    /// <param name="json">
    /// A JSON (or JSONC) object mapping fully qualified type names to format template strings
    /// (e.g. <c>{ "MyApp.Errors.NotFound": "Resource '{Id}' was not found." }</c>).
    /// Both single-line (<c>//</c>) and block (<c>/* */</c>) comments are accepted, as well as trailing commas.
    /// </param>
    /// <param name="culture">The culture these templates belong to. Use <see cref="CultureInfo.InvariantCulture"/> for the default fallback.</param>
    /// <param name="source">Descriptive source of the templates (e.g. <c>"assembly:MyLib:messages.json"</c>) for diagnostic logging.</param>
    /// <remarks>
    /// When the same FQDN key already exists for the given culture, the new value silently
    /// overrides the previous one. This merge-on-conflict behavior lets downstream assemblies
    /// override templates defined by upstream assemblies. A warning is logged when overrides occur.
    /// </remarks>
    public void LoadFromJson(string json, CultureInfo culture, string source = "inline")
    {
        var key = culture.Name;
        if (!_templates.TryGetValue(key, out var cultureDict))
        {
            cultureDict = new Dictionary<string, string>();
            _templates[key] = cultureDict;
        }

        Dictionary<string, string>? entries;
        try
        {
            entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsoncOptions);
        }
        catch (JsonException ex)
        {
            throw new LocalizationConfigurationException(
                $"Failed to parse localization JSON for culture '{culture.Name}': {ex.Message}", ex);
        }
        if (entries is null) return;

        foreach (var (fqdn, template) in entries)
        {
            if (logger is not null && cultureDict.ContainsKey(fqdn))
                Log.TemplateOverride(logger, fqdn, key, source);

            cultureDict[fqdn] = template;
        }

        if (logger is not null)
            Log.TemplateRegistered(logger, entries.Count, key, source);
    }

    /// <summary>
    /// Attempts to retrieve a template for the given FQDN and culture.
    /// </summary>
    /// <param name="fqdn">The fully qualified type name used as the template key.</param>
    /// <param name="culture">The desired culture for the lookup.</param>
    /// <param name="template">
    /// When this method returns <see langword="true"/>, contains the resolved template.
    /// When <see langword="false"/>, set to <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if a matching template was found; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The lookup follows a 3-tier fallback: exact culture → parent culture → invariant culture
    /// (empty culture name). The first match wins.
    /// </remarks>
    public bool TryGetTemplate(string fqdn, CultureInfo culture, out string template)
    {
        if (_templates.TryGetValue(culture.Name, out var cultureDict) && cultureDict.TryGetValue(fqdn, out template!))
            return true;

        if (culture.Parent is { Name.Length: > 0 } parent)
        {
            if (_templates.TryGetValue(parent.Name, out var parentDict) && parentDict.TryGetValue(fqdn, out template!))
                return true;
        }

        if (_templates.TryGetValue("", out var invariantDict) && invariantDict.TryGetValue(fqdn, out template!))
            return true;

        template = null!;
        return false;
    }
}
