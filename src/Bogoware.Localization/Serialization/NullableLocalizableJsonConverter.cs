using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bogoware.Localization.Serialization;

/// <summary>
/// Write-only JSON converter for <see cref="Nullable{T}"/> properties where <typeparamref name="T"/>
/// is a value type that should be localized. Delegates to <see cref="ILocalizationFormatter.Format{T}"/>.
/// </summary>
/// <remarks>
/// Reading is not supported — deserialization throws <see cref="LocalizationSerializationException"/>
/// because localization is a lossy one-way transform.
/// </remarks>
/// <typeparam name="T">The underlying struct type.</typeparam>
internal sealed class NullableLocalizableJsonConverter<T>(
    ILocalizationFormatter formatter,
    CultureInfo? culture) : JsonConverter<T?> where T : struct
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new LocalizationSerializationException(
            $"Deserialization of localized properties is not supported. " +
            $"Type: {typeof(T?).FullName}");

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var localized = formatter.Format(value.Value, culture);
        writer.WriteStringValue(localized);
    }
}
