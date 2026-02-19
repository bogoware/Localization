namespace Bogoware.Localization;

/// <summary>
/// Abstract base exception for all Bogoware.Localization failures.
/// </summary>
public abstract class LocalizationException : Exception
{
    /// <inheritdoc />
    protected LocalizationException(string message) : base(message) { }

    /// <inheritdoc />
    protected LocalizationException(string message, Exception innerException) : base(message, innerException) { }
}
