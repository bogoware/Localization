using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Formats <see cref="ILocalizable"/> instances and arbitrary values
/// into culture-aware human-readable strings using FQDN-keyed template registries.
/// </summary>
/// <remarks>
/// The formatter applies the following resolution chain in order:
/// <list type="number">
///   <item><description><strong>Self-provider</strong> — if the value implements <see cref="ILocalizationProvider"/>, its <c>Localize</c> method is called directly.</description></item>
///   <item><description><strong>DI provider</strong> — an <see cref="ILocalizationProvider{T}"/> resolved from the service provider for the runtime type.</description></item>
///   <item><description><strong>Registry template</strong> — an <see cref="ILocalizationRegistry"/> template keyed by <c>Type.FullName</c>, with <c>{PropertyName}</c> placeholders.</description></item>
///   <item><description><strong>Fallback</strong> — a generated string in the form <c>TypeName(Prop=val)</c>.</description></item>
/// </list>
/// For the generic <see cref="Format{T}"/> overload, non-<see cref="ILocalizable"/> types skip steps 1 and 3,
/// falling back to <c>ToString()</c> if no DI provider is registered.
/// </remarks>
public interface ILocalizationFormatter
{
    /// <summary>
    /// Formats an <see cref="ILocalizable"/> using the full resolution chain:
    /// self-provider, DI provider, registry template, fallback.
    /// </summary>
    /// <param name="value">The localizable instance to format.</param>
    /// <param name="culture">
    /// The target culture. When <see langword="null"/>, defaults to <see cref="CultureInfo.CurrentUICulture"/>.
    /// </param>
    /// <returns>A culture-aware human-readable string.</returns>
    string Format(ILocalizable value, CultureInfo? culture = null);

    /// <summary>
    /// Formats an arbitrary value. If it implements <see cref="ILocalizable"/>,
    /// delegates to <see cref="Format(ILocalizable, CultureInfo?)"/>.
    /// Otherwise tries the DI provider, then <c>ToString()</c> fallback.
    /// </summary>
    /// <typeparam name="T">The type of value to format.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="culture">
    /// The target culture. When <see langword="null"/>, defaults to <see cref="CultureInfo.CurrentUICulture"/>.
    /// </param>
    /// <returns>A culture-aware human-readable string.</returns>
    string Format<T>(T value, CultureInfo? culture = null);
}
