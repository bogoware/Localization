namespace Bogoware.Localization;

/// <summary>
/// Marker interface — opt-in for localization.
/// Any type implementing this interface can be formatted by <see cref="ILocalizationFormatter"/>.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on types that should participate in the localization pipeline.
/// By itself, <see cref="ILocalizable"/> carries no members — it simply marks a type as eligible
/// for the <see cref="ILocalizationFormatter"/> resolution chain.
/// </para>
/// <para>
/// For self-localizing types, also implement <see cref="ILocalizationProvider"/>.
/// For external localization via dependency injection, register an <see cref="ILocalizationProvider{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // A simple localizable validation error with properties used as template placeholders.
/// public record InvalidEmailError(string Email) : ILocalizable;
///
/// // Register a template: "The email '{Email}' is not valid."
/// // The formatter replaces {Email} with the property value at runtime.
/// </code>
/// </example>
public interface ILocalizable;
