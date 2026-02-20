using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Extension methods for registering the Bogoware.Localization services in an
/// <see cref="IServiceCollection"/> container.
/// </summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the localization system with full builder customization.
    /// Multiple calls are <b>additive</b>: each call appends its configuration delegate
    /// to the builder pipeline. All delegates execute in registration order at resolution time,
    /// so later templates override earlier ones on a per-key/per-culture basis.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">
    /// A delegate that receives a <see cref="JsonLocalizationRegistryBuilder"/> for loading templates.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <remarks>
    /// Both <see cref="ILocalizationRegistry"/> and <see cref="ILocalizationFormatter"/> are
    /// registered as singletons. The builder is created lazily at first resolution.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs:
    /// builder.Services.AddLocalization(b => b
    ///     .AddFromAssembly(typeof(Program).Assembly)
    ///     .AddFromLoadedAssemblies(["MyApp"]));
    /// </code>
    /// </example>
    public static IServiceCollection AddLocalization(
        this IServiceCollection services, Action<JsonLocalizationRegistryBuilder> configure)
    {
        // Retrieve or create the configurator that accumulates builder delegates.
        var configurator = (LocalizationRegistryConfigurator?)services
            .FirstOrDefault(d => d.ServiceType == typeof(LocalizationRegistryConfigurator))
            ?.ImplementationInstance;

        if (configurator is null)
        {
            configurator = new LocalizationRegistryConfigurator();
            services.AddSingleton(configurator);

            services.AddSingleton<ILocalizationRegistry>(sp =>
            {
                var cfg = sp.GetRequiredService<LocalizationRegistryConfigurator>();
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<JsonLocalizationRegistryBuilder>();
                return cfg.Build(logger);
            });

            services.AddSingleton<ILocalizationFormatter>(sp =>
            {
                var registry = sp.GetRequiredService<ILocalizationRegistry>();
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<LocalizationFormatter>();
                return new LocalizationFormatter(registry, sp, logger);
            });
        }

        configurator.Add(configure);
        return services;
    }

    /// <summary>
    /// Convenience overload: scans the provided assemblies for embedded resources in order
    /// (latter overrides earlier). Default patterns: <c>["localized-messages", "error-messages"]</c>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="assemblies">The assemblies to scan for embedded JSON resources.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddLocalization(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddLocalization(builder =>
        {
            foreach (var assembly in assemblies)
                builder.AddFromAssembly(assembly);
        });
    }
}
