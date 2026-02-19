using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// External DI provider that can localize instances of <typeparamref name="T"/>.
/// No constraint on T — any type can have an external localization provider.
/// </summary>
/// <remarks>
/// <para>
/// Register implementations in the DI container to provide localization for types that
/// do not (or cannot) implement <see cref="ILocalizationProvider"/> themselves.
/// This is the second step in the resolution chain, after self-providers and before
/// registry template lookup.
/// </para>
/// <para>
/// Multiple providers can coexist for different types. The formatter resolves the
/// provider for the runtime type of the value being formatted.
/// </para>
/// </remarks>
/// <typeparam name="T">The type this provider can localize.</typeparam>
/// <example>
/// <code>
/// // Register a provider for OrderConfirmation in DI:
/// public class OrderConfirmationProvider : ILocalizationProvider&lt;OrderConfirmation&gt;
/// {
///     public string Localize(OrderConfirmation value, CultureInfo? culture = null)
///         => $"Order #{value.OrderId} confirmed on {value.Date.ToString("d", culture)}";
/// }
///
/// // In DI setup:
/// services.AddSingleton&lt;ILocalizationProvider&lt;OrderConfirmation&gt;, OrderConfirmationProvider&gt;();
/// </code>
/// </example>
public interface ILocalizationProvider<in T>
{
    /// <summary>
    /// Produces a localized string representation of the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The instance to localize.</param>
    /// <param name="culture">
    /// The target culture. When <see langword="null"/>, defaults to
    /// <see cref="CultureInfo.CurrentUICulture"/> at the discretion of the implementor.
    /// </param>
    /// <returns>A culture-aware human-readable string for <paramref name="value"/>.</returns>
    string Localize(T value, CultureInfo? culture = null);
}
