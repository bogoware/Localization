using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Self-localizing types that can produce their own localized string representation.
/// </summary>
public interface ILocalizableStringProvider : ILocalizableString
{
    string Localize(CultureInfo? culture = null);
}
