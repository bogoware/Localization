using System.Globalization;

namespace Bogoware.Localization;

/// <summary>
/// Represents a contract for managing culture-specific message templates
/// associated with fully qualified domain names (FQDNs).
/// </summary>
/// <remarks>
/// Template keys use <c>typeof(T).FullName</c> as the FQDN convention. Implementations
/// typically provide a culture fallback chain: exact culture, parent culture, then invariant culture.
/// </remarks>
public interface ILocalizationRegistry
{
    /// <summary>
    /// Attempts to retrieve a localized template for the given FQDN and culture.
    /// </summary>
    /// <param name="fqdn">
    /// The fully qualified type name used as the template key (e.g. <c>"MyApp.Errors.InvalidEmailError"</c>).
    /// </param>
    /// <param name="culture">The desired culture for the template lookup.</param>
    /// <param name="template">
    /// When this method returns <see langword="true"/>, contains the resolved template string
    /// with <c>{PropertyName}</c> placeholders. When <see langword="false"/>, set to <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if a matching template was found; otherwise <see langword="false"/>.</returns>
    bool TryGetTemplate(string fqdn, CultureInfo culture, out string template);
}
