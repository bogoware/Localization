using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Unified formatter with resolution chain for <see cref="ILocalizable"/> and arbitrary values.
/// </summary>
public class LocalizationFormatter(
    ILocalizationRegistry registry,
    IServiceProvider serviceProvider,
    ILogger<LocalizationFormatter>? logger = null)
    : ILocalizationFormatter
{
    /// <inheritdoc />
    public string Format(ILocalizable value, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;
        var runtimeType = value.GetType();
        var typeName = runtimeType.FullName!;

        if (logger is not null)
            Log.ResolvingLocalization(logger, typeName, culture.Name);

        // 1. Self-provider
        if (value is ILocalizationProvider selfProvider)
        {
            if (logger is not null)
                Log.ResolvedVia(logger, typeName, "self-provider");
            return selfProvider.Localize(culture);
        }

        // 2. DI provider for runtime type
        var diResult = TryFormatViaDiProvider(runtimeType, value, culture);
        if (diResult is not null)
        {
            if (logger is not null)
                Log.ResolvedVia(logger, typeName, "DI provider");
            return diResult;
        }

        // 3. Registry template
        var fqdn = typeName;
        if (registry.TryGetTemplate(fqdn, culture, out var template))
        {
            if (logger is not null)
                Log.ResolvedVia(logger, typeName, "registry template");
            return FormatTemplate(template, value);
        }

        // 4. Fallback
        if (logger is not null)
            Log.FallbackUsed(logger, typeName);
        return BuildFallback(value);
    }

    /// <inheritdoc />
    public string Format<T>(T value, CultureInfo? culture = null)
    {
        // 1. If it's an ILocalizable, delegate
        if (value is ILocalizable ls)
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
                if (logger is not null)
                    Log.ResolvedVia(logger, runtimeType.FullName ?? "unknown", "DI provider (generic)");
                return diResult;
            }
        }

        // 3. ToString fallback + warning
        var result = value?.ToString() ?? "";
        if (logger is not null)
            Log.FallbackUsed(logger, value?.GetType().FullName ?? "null");
        return result;
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> _localizeMethodCache = new();

    private string? TryFormatViaDiProvider(Type runtimeType, object value, CultureInfo culture)
    {
        var providerType = typeof(ILocalizationProvider<>).MakeGenericType(runtimeType);
        var provider = serviceProvider.GetService(providerType);
        if (provider is null) return null;

        var localizeMethod = _localizeMethodCache.GetOrAdd(providerType, t =>
            t.GetMethod(nameof(ILocalizationProvider<object>.Localize))
            ?? throw new LocalizationFormattingException(
                $"The localization provider type '{t.FullName}' does not expose a 'Localize' method.", t));

        try
        {
            return (string)localizeMethod.Invoke(provider, [value, culture])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new LocalizationFormattingException(
                $"The localization provider for type '{runtimeType.FullName}' threw an exception during formatting.",
                ex.InnerException, runtimeType);
        }
    }

    /// <summary>
    /// Replaces <c>{PropertyName}</c> placeholders in a template with values from the source object.
    /// </summary>
    /// <param name="template">The format template containing <c>{PropertyName}</c> placeholders.</param>
    /// <param name="source">The object whose public instance properties supply placeholder values.</param>
    /// <returns>The template with all matching placeholders replaced by property values.</returns>
    /// <remarks>
    /// Properties named <c>Message</c> are excluded from substitution. Only public readable
    /// instance properties are considered. Unmatched placeholders are left as-is.
    /// </remarks>
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

    /// <summary>
    /// Produces a fallback string in the form <c>TypeName(Prop=val, ...)</c>.
    /// </summary>
    /// <param name="source">The object to describe.</param>
    /// <returns>A diagnostic-style string representation.</returns>
    /// <remarks>
    /// Only public instance properties declared directly on the source type are included
    /// (<see cref="BindingFlags.DeclaredOnly"/>), excluding any property named <c>Message</c>.
    /// If no properties match, the type name alone is returned.
    /// </remarks>
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
