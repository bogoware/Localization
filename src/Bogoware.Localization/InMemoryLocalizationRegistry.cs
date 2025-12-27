using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// A simple in-memory registry backed by a flat dictionary (FQDN -> format pattern).
/// Culture-insensitive — always returns the registered template regardless of the requested culture.
/// Intended for unit testing scenarios where culture fallback is not under test.
/// </summary>
public class InMemoryLocalizationRegistry : ILocalizationRegistry
{
    private readonly Dictionary<string, string> _templates;

    internal InMemoryLocalizationRegistry(Dictionary<string, string> templates)
    {
        _templates = templates;
    }

    /// <inheritdoc />
    public bool TryGetTemplate(string fqdn, CultureInfo culture, out string template)
    {
        return _templates.TryGetValue(fqdn, out template!);
    }
}
