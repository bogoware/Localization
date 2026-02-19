namespace Bogoware.Localization;

/// <summary>
/// Thrown when localization setup or resource loading fails (e.g., malformed JSON, file I/O errors).
/// </summary>
public class LocalizationConfigurationException : LocalizationException
{
    /// <inheritdoc />
    public LocalizationConfigurationException(string message) : base(message) { }

    /// <inheritdoc />
    public LocalizationConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
