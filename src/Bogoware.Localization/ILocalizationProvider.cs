using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Self-localizing types that can produce their own localized string representation.
/// </summary>
/// <remarks>
/// This is the highest-priority step in the <see cref="ILocalizationFormatter"/> resolution chain.
/// When a type implements both <see cref="ILocalizable"/> and <see cref="ILocalizationProvider"/>,
/// the formatter calls <see cref="Localize"/> directly, bypassing DI providers, registry templates,
/// and fallback formatting entirely.
/// </remarks>
/// <example>
/// <code>
/// public record Greeting(string Name) : ILocalizationProvider
/// {
///     public string Localize(CultureInfo? culture = null)
///     {
///         var c = culture ?? CultureInfo.CurrentUICulture;
///         return c.TwoLetterISOLanguageName == "it"
///             ? $"Ciao, {Name}!"
///             : $"Hello, {Name}!";
///     }
/// }
/// </code>
/// </example>
public interface ILocalizationProvider : ILocalizable
{
    /// <summary>
    /// Produces a localized string representation of the current instance.
    /// </summary>
    /// <param name="culture">
    /// The target culture. When <see langword="null"/>, defaults to
    /// <see cref="CultureInfo.CurrentUICulture"/> at the discretion of the implementor.
    /// </param>
    /// <returns>A culture-aware human-readable string.</returns>
    string Localize(CultureInfo? culture = null);
}
