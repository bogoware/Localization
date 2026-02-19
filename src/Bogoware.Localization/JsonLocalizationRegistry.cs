using System.Globalization;
using System.Text.Json;

namespace Bogoware.Localization;

/// <summary>
/// JSON-backed registry for managing localized message templates with culture fallback chain.
/// </summary>
public class JsonLocalizationRegistry : ILocalizationRegistry
{
    private readonly Dictionary<string, Dictionary<string, string>> _templates = new();

    /// <summary>
    /// Loads message templates from a JSON string (FQDN -> template pairs) for the specified culture.
    /// </summary>
    public void LoadFromJson(string json, CultureInfo culture)
    {
        var key = culture.Name;
        if (!_templates.ContainsKey(key))
            _templates[key] = new Dictionary<string, string>();

        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (entries is null) return;

        foreach (var (fqdn, template) in entries)
        {
            _templates[key][fqdn] = template;
        }
    }

    /// <summary>
    /// Attempts to retrieve a template for the given FQDN, trying exact culture, parent culture,
    /// then invariant culture in order.
    /// </summary>
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
