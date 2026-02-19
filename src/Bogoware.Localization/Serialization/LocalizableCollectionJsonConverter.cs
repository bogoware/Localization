using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bogoware.Localization.Serialization;

/// <summary>
/// Write-only JSON converter that serializes an <see cref="IEnumerable{T}"/>
/// as a JSON array of localized strings.
/// </summary>
/// <remarks>
/// Each element is formatted via <see cref="ILocalizationFormatter.Format{T}"/>.
/// Null elements are written as JSON <c>null</c>.
/// Reading is not supported.
/// </remarks>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class LocalizableCollectionJsonConverter<T>(
    ILocalizationFormatter formatter,
    CultureInfo? culture) : JsonConverter<IEnumerable<T>>
{
    public override IEnumerable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException(
            $"Deserialization of localized collections is not supported. " +
            $"Type: {typeToConvert.FullName}");

    public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            if (item is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var localized = formatter.Format(item, culture);
                writer.WriteStringValue(localized);
            }
        }
        writer.WriteEndArray();
    }
}
