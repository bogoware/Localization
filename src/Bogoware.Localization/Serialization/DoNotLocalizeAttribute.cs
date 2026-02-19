namespace Bogoware.Localization.Serialization;

/// <summary>
/// Prevents a property from being localized during JSON serialization,
/// overriding the behavior of <see cref="LocalizationSerializationMode.Auto"/>
/// and <see cref="LocalizationSerializationMode.Exhaustive"/> modes.
/// </summary>
/// <remarks>
/// The property will be serialized using its default JSON representation (object structure)
/// regardless of the serialization mode in effect.
/// </remarks>
/// <example>
/// <code>
/// public class DetailedResponse
/// {
///     public RequiredFieldError Error { get; set; }         // localized in Auto mode
///
///     [DoNotLocalize]
///     public RequiredFieldError RawError { get; set; }      // always serialized as object
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DoNotLocalizeAttribute : Attribute;
