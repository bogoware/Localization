using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Formats <see cref="ILocalizable"/> instances and arbitrary values
/// into culture-aware human-readable strings using FQDN-keyed template registries.
/// </summary>
public interface ILocalizationFormatter
{
    /// <summary>
    /// Formats an <see cref="ILocalizable"/> using the resolution chain:
    /// self-provider, DI provider, registry template, fallback.
    /// </summary>
    string Format(ILocalizable value, CultureInfo? culture = null);

    /// <summary>
    /// Formats an arbitrary value. If it implements <see cref="ILocalizable"/>,
    /// delegates to <see cref="Format(ILocalizable, CultureInfo?)"/>.
    /// Otherwise tries DI provider, then <c>ToString()</c> fallback.
    /// </summary>
    string Format<T>(T value, CultureInfo? culture = null);
}
