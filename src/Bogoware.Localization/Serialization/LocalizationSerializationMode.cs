namespace Bogoware.Localization.Serialization;

/// <summary>
/// Controls which properties are localized during JSON serialization.
/// </summary>
public enum LocalizationSerializationMode
{
    /// <summary>
    /// Localizes only properties explicitly marked with <see cref="LocalizeAttribute"/>.
    /// All other properties are serialized using their default JSON representation.
    /// This is the most performant mode and gives full control over which properties are localized.
    /// </summary>
    Explicit,

    /// <summary>
    /// Localizes all <see cref="ILocalizable"/> properties automatically,
    /// plus any properties explicitly marked with <see cref="LocalizeAttribute"/>.
    /// Properties marked with <see cref="DoNotLocalizeAttribute"/> are excluded.
    /// This is the default mode, balancing convenience and performance.
    /// </summary>
    Auto,

    /// <summary>
    /// Attempts to localize every property via <see cref="ILocalizationFormatter.Format{T}"/>.
    /// Falls back to <c>ToString()</c> for types without a registered provider.
    /// Properties marked with <see cref="DoNotLocalizeAttribute"/> are excluded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Performance warning:</strong> This mode calls <see cref="ILocalizationFormatter.Format{T}"/>
    /// on every property, which involves DI service resolution and reflection.
    /// Use only for debugging or testing — not recommended for production workloads.
    /// </para>
    /// </remarks>
    Exhaustive
}
