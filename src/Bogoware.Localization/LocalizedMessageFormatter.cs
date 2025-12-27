using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Unified formatter with resolution chain for <see cref="ILocalizableString"/> and arbitrary values.
/// </summary>
public class LocalizedMessageFormatter(
    ILocalizedMessageRegistry registry,
    IServiceProvider serviceProvider,
    ILogger<LocalizedMessageFormatter>? logger = null)
    : ILocalizedMessageFormatter
{
    /// <inheritdoc />
    public string Format(ILocalizableString value, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;

        // 1. Self-provider
        if (value is ILocalizableStringProvider selfProvider)
        {
            return selfProvider.Localize(culture);
        }

        // 2. DI provider for runtime type
        var runtimeType = value.GetType();
        var diResult = TryFormatViaDiProvider(runtimeType, value, culture);
        if (diResult is not null)
        {
            return diResult;
        }

        // 3. Registry template
        var fqdn = runtimeType.FullName!;
        if (registry.TryGetTemplate(fqdn, culture, out var template))
        {
            return FormatTemplate(template, value);
        }

        // 4. Fallback
        return BuildFallback(value);
    }

    /// <inheritdoc />
    public string Format<T>(T value, CultureInfo? culture = null)
    {
        // 1. If it's an ILocalizableString, delegate
        if (value is ILocalizableString ls)
        {
            return Format(ls, culture);
        }

        culture ??= CultureInfo.CurrentUICulture;

        // 2. DI provider for runtime type
        if (value is not null)
        {
            var runtimeType = value.GetType();
            var diResult = TryFormatViaDiProvider(runtimeType, value, culture);
            if (diResult is not null)
            {
                return diResult;
            }
        }

        // 3. ToString fallback + warning
        var result = value?.ToString() ?? "";
        logger?.LogWarning(
            "No localization found for type {TypeName}, falling back to ToString()",
            value?.GetType().FullName ?? "null");
        return result;
    }

    private string? TryFormatViaDiProvider(Type runtimeType, object value, CultureInfo culture)
    {
        var providerType = typeof(ILocalizableStringProvider<>).MakeGenericType(runtimeType);
        var provider = serviceProvider.GetService(providerType);
        if (provider is null) return null;

        var localizeMethod = providerType.GetMethod("Localize")!;
        return (string)localizeMethod.Invoke(provider, [value, culture])!;
    }

    internal static string FormatTemplate(string template, object source)
    {
        var props = source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Message" && p.CanRead);

        var result = template;
        foreach (var prop in props)
        {
            var value = prop.GetValue(source);
            var placeholder = $"{{{prop.Name}}}";
            if (result.Contains(placeholder))
            {
                result = result.Replace(placeholder, value?.ToString() ?? "");
            }
        }
        return result;
    }

    internal static string BuildFallback(object source)
    {
        var typeName = source.GetType().Name;
        var props = source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.Name != "Message" && p.CanRead);

        var attrs = string.Join(", ", props.Select(p => $"{p.Name}={p.GetValue(source)}"));
        return attrs.Length > 0 ? $"{typeName}({attrs})" : typeName;
    }
}
