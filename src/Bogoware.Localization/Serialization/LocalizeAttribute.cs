namespace Bogoware.Localization.Serialization;

/// <summary>
/// Marks a property for localized serialization.
/// When applied, the property value is formatted via <see cref="ILocalizationFormatter.Format{T}"/>
/// and written as a JSON string instead of its default object structure.
/// </summary>
/// <remarks>
/// <para>
/// In <see cref="LocalizationSerializationMode.Auto"/> mode, <see cref="ILocalizable"/> properties
/// are localized automatically; use this attribute on non-<see cref="ILocalizable"/> types that have
/// an <see cref="ILocalizationProvider{T}"/> registered in DI.
/// </para>
/// <para>
/// In <see cref="LocalizationSerializationMode.Explicit"/> mode, only properties with this attribute
/// are localized — all others use their default serialization.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ApiResponse
/// {
///     [Localize]
///     public OrderConfirmation Confirmation { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class LocalizeAttribute : Attribute;
