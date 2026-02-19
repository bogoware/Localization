namespace Bogoware.Localization;

/// <summary>
/// Thrown when formatting a localizable value fails at runtime, including
/// when a DI-resolved localization provider cannot be invoked via reflection.
/// </summary>
public class LocalizationFormattingException : LocalizationException
{
    /// <summary>
    /// The type that was being localized when the failure occurred, if known.
    /// </summary>
    public Type? TargetType { get; }

    /// <param name="message">A description of the formatting failure.</param>
    /// <param name="targetType">The type being localized when the failure occurred, or <see langword="null"/> if unknown.</param>
    public LocalizationFormattingException(string message, Type? targetType = null)
        : base(message)
    {
        TargetType = targetType;
    }

    /// <param name="message">A description of the formatting failure.</param>
    /// <param name="innerException">The exception that caused the formatting failure.</param>
    /// <param name="targetType">The type being localized when the failure occurred, or <see langword="null"/> if unknown.</param>
    public LocalizationFormattingException(string message, Exception innerException, Type? targetType = null)
        : base(message, innerException)
    {
        TargetType = targetType;
    }
}
