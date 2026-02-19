using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Bogoware.Localization.Serialization;

/// <summary>
/// STJ contract modifier that assigns localization converters to properties
/// based on the configured <see cref="LocalizationSerializationMode"/>.
/// </summary>
internal static class LocalizableJsonModifier
{
    // Primitive types and strings should never be localized — they already serialize natively.
    private static readonly HashSet<Type> SkipTypes =
    [
        typeof(string),
        typeof(bool),
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan),
        typeof(Guid),
        typeof(char)
    ];

    /// <summary>
    /// Creates a modifier action that can be passed to
    /// <see cref="DefaultJsonTypeInfoResolver"/> via <c>WithAddedModifier</c>.
    /// </summary>
    internal static Action<JsonTypeInfo> CreateModifier(
        ILocalizationFormatter formatter,
        LocalizationSerializationMode mode,
        CultureInfo? culture)
    {
        return typeInfo =>
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            foreach (var property in typeInfo.Properties)
            {
                if (ShouldLocalize(property, mode))
                {
                    AssignConverter(property, formatter, culture);
                }
            }
        };
    }

    private static bool ShouldLocalize(JsonPropertyInfo property, LocalizationSerializationMode mode)
    {
        var propertyType = property.PropertyType;

        // Nullable<T> → unwrap
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Never localize primitives or strings
        if (SkipTypes.Contains(underlyingType) || underlyingType.IsEnum)
            return false;

        var hasLocalize = HasAttribute<LocalizeAttribute>(property);
        var hasDoNotLocalize = HasAttribute<DoNotLocalizeAttribute>(property);

        // [DoNotLocalize] always wins
        if (hasDoNotLocalize)
            return false;

        return mode switch
        {
            LocalizationSerializationMode.Explicit => hasLocalize,
            LocalizationSerializationMode.Auto => hasLocalize || IsLocalizableType(underlyingType),
            LocalizationSerializationMode.Exhaustive => true,
            _ => false
        };
    }

    private static bool IsLocalizableType(Type type)
    {
        var unwrapped = Nullable.GetUnderlyingType(type) ?? type;
        return typeof(ILocalizable).IsAssignableFrom(unwrapped) || IsEnumerableOfLocalizable(unwrapped);
    }

    private static bool IsEnumerableOfLocalizable(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                typeof(ILocalizable).IsAssignableFrom(iface.GetGenericArguments()[0]))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasAttribute<TAttribute>(JsonPropertyInfo property) where TAttribute : Attribute
    {
        var memberInfo = property.AttributeProvider;
        return memberInfo is not null &&
               memberInfo.GetCustomAttributes(typeof(TAttribute), inherit: true).Length > 0;
    }

    private static void AssignConverter(
        JsonPropertyInfo property,
        ILocalizationFormatter formatter,
        CultureInfo? culture)
    {
        var propertyType = property.PropertyType;
        var unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Check if the property is an IEnumerable<T> where T : ILocalizable
        var elementType = GetEnumerableElementType(unwrappedType);
        if (elementType is not null)
        {
            var unwrappedElement = Nullable.GetUnderlyingType(elementType) ?? elementType;
            if (typeof(ILocalizable).IsAssignableFrom(unwrappedElement))
            {
                var collectionConverterType = typeof(LocalizableCollectionJsonConverter<>).MakeGenericType(unwrappedElement);
                var collectionConverter = Activator.CreateInstance(collectionConverterType, formatter, culture)!;
                property.CustomConverter = (System.Text.Json.Serialization.JsonConverter)collectionConverter;
                return;
            }
        }

        // Single value → localized string
        if (propertyType != unwrappedType && unwrappedType.IsValueType)
        {
            // Nullable<T> struct → use the nullable-aware converter
            var nullableConverterType = typeof(NullableLocalizableJsonConverter<>).MakeGenericType(unwrappedType);
            var nullableConverter = Activator.CreateInstance(nullableConverterType, formatter, culture)!;
            property.CustomConverter = (System.Text.Json.Serialization.JsonConverter)nullableConverter;
        }
        else
        {
            var converterType = typeof(LocalizableJsonConverter<>).MakeGenericType(unwrappedType);
            var converter = Activator.CreateInstance(converterType, formatter, culture)!;
            property.CustomConverter = (System.Text.Json.Serialization.JsonConverter)converter;
        }
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        // Don't treat string as IEnumerable<char>
        if (type == typeof(string))
            return null;

        // Check the type itself first if it's a generic IEnumerable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        // Fall back to scanning interfaces; collect all IEnumerable<T> matches
        Type? match = null;
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                if (match is not null)
                    return null; // Ambiguous — multiple IEnumerable<T> implementations
                match = iface.GetGenericArguments()[0];
            }
        }

        return match;
    }
}
