namespace Bogoware.Localization.Serialization;

/// <summary>
/// Thrown when a localization serialization or deserialization operation fails.
/// </summary>
public class LocalizationSerializationException : LocalizationException
{
    /// <inheritdoc />
    public LocalizationSerializationException(string message) : base(message) { }

    /// <inheritdoc />
    public LocalizationSerializationException(string message, Exception innerException) : base(message, innerException) { }
}
