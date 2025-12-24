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
        this IServiceCollection services, Action<JsonLocalizedMessageRegistryBuilder> configure)
    {
        services.AddSingleton<ILocalizedMessageRegistry>(sp =>
        {
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<JsonLocalizedMessageRegistryBuilder>();
            var builder = new JsonLocalizedMessageRegistryBuilder(logger);
            configure(builder);
            return builder.Build();
        });

        services.AddSingleton<ILocalizedMessageFormatter>(sp =>
        {
            var registry = sp.GetRequiredService<ILocalizedMessageRegistry>();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<LocalizedMessageFormatter>();
            return new LocalizedMessageFormatter(registry, sp, logger);
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
