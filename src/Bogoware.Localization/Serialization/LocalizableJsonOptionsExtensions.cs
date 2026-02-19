using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Bogoware.Localization.Serialization;

/// <summary>
/// Extension methods for configuring localized JSON serialization
/// on <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class LocalizableJsonOptionsExtensions
{
    /// <summary>
    /// Adds localization support to JSON serialization. Properties are formatted via
    /// <see cref="ILocalizationFormatter"/> based on the selected <paramref name="mode"/>.
    /// </summary>
    /// <param name="options">The serializer options to configure.</param>
    /// <param name="formatter">The localization formatter to use for converting values to localized strings.</param>
    /// <param name="mode">
    /// The serialization mode. Defaults to <see cref="LocalizationSerializationMode.Auto"/>
    /// which localizes all <see cref="ILocalizable"/> properties plus <c>[Localize]</c>-marked ones.
    /// </param>
    /// <param name="culture">
    /// An optional fixed culture for localization. When <see langword="null"/> (default),
    /// <see cref="CultureInfo.CurrentUICulture"/> is evaluated at serialization time,
    /// which is the correct behavior for per-request culture in ASP.NET.
    /// </param>
    /// <returns>The same <paramref name="options"/> instance for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// var options = new JsonSerializerOptions { WriteIndented = true };
    /// options.AddLocalization(formatter);
    ///
    /// // With explicit mode and fixed culture:
    /// options.AddLocalization(formatter, LocalizationSerializationMode.Explicit, new CultureInfo("it-IT"));
    /// </code>
    /// </example>
    public static JsonSerializerOptions AddLocalization(
        this JsonSerializerOptions options,
        ILocalizationFormatter formatter,
        LocalizationSerializationMode mode = LocalizationSerializationMode.Auto,
        CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(formatter);

        var modifier = LocalizableJsonModifier.CreateModifier(formatter, mode, culture);

        options.TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            .WithAddedModifier(modifier);

        return options;
    }
}
