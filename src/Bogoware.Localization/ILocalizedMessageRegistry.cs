using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Represents a contract for managing culture-specific message templates
/// associated with fully qualified domain names (FQDNs).
/// </summary>
public interface ILocalizedMessageRegistry
{
    bool TryGetTemplate(string fqdn, CultureInfo culture, out string template);
}
