using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the localization system with full builder customization.
    /// The builder receives an <see cref="ILogger"/> from the service provider at resolution time.
    /// </summary>
    public static IServiceCollection AddLocalization(
        this IServiceCollection services, Action<JsonLocalizationRegistryBuilder> configure)
    {
        services.AddSingleton<ILocalizationRegistry>(sp =>
        {
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<JsonLocalizationRegistryBuilder>();
            var builder = new JsonLocalizationRegistryBuilder(logger);
            configure(builder);
            return builder.Build();
        });

        services.AddSingleton<ILocalizationFormatter>(sp =>
        {
            var registry = sp.GetRequiredService<ILocalizationRegistry>();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<LocalizationFormatter>();
            return new LocalizationFormatter(registry, sp, logger);
        });

        return services;
    }

    /// <summary>
    /// Convenience overload: scans the provided assemblies for embedded resources in order
    /// (latter overrides earlier). Default patterns: <c>["localized-messages", "error-messages"]</c>.
    /// </summary>
    public static IServiceCollection AddLocalization(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddLocalization(builder =>
        {
            foreach (var assembly in assemblies)
                builder.AddFromAssemblyResources(assembly);
        });
    }
}
