using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Formats <see cref="ILocalizableString"/> instances and arbitrary values
/// into culture-aware human-readable strings using FQDN-keyed template registries.
/// </summary>
public interface ILocalizedMessageFormatter
{
    /// <summary>
    /// Formats an <see cref="ILocalizableString"/> using the resolution chain:
    /// self-provider, DI provider, registry template, fallback.
    /// </summary>
    string Format(ILocalizableString value, CultureInfo? culture = null);

    /// <summary>
    /// Formats an arbitrary value. If it implements <see cref="ILocalizableString"/>,
    /// delegates to <see cref="Format(ILocalizableString, CultureInfo?)"/>.
    /// Otherwise tries DI provider, then <c>ToString()</c> fallback.
    /// </summary>
    string Format<T>(T value, CultureInfo? culture = null);
}
