namespace Bogoware.Localization;

/// <summary>
/// Abstract base for types that carry a fallback message for localization.
/// When no localized template is found, <see cref="FallbackMessage"/> is used.
/// </summary>
public abstract class LocalizedMessage(string fallbackMessage) : ILocalizableString
{
    /// <summary>
    /// The default message text, used as fallback when no localized template matches.
    /// </summary>
    public string FallbackMessage { get; } = fallbackMessage;
}
