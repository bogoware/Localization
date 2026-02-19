using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bogoware.Localization.Serialization;

/// <summary>
/// Write-only JSON converter that serializes a value as its localized string representation.
/// </summary>
/// <remarks>
/// This converter delegates to <see cref="ILocalizationFormatter.Format{T}"/> at write-time.
/// Reading is not supported — deserialization throws <see cref="NotSupportedException"/>
/// because localization is a lossy one-way transform.
/// </remarks>
/// <typeparam name="T">The type to convert.</typeparam>
internal sealed class LocalizableJsonConverter<T>(
    ILocalizationFormatter formatter,
    CultureInfo? culture) : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException(
            $"Deserialization of localized properties is not supported. " +
            $"Type: {typeof(T).FullName}");

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var localized = formatter.Format(value, culture);
        writer.WriteStringValue(localized);
    }
}
