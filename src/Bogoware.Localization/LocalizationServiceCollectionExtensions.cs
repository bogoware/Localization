using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        // Each call registers its configure delegate as an individual singleton.
        // All delegates are collected at resolution time via GetServices<>.
        services.AddSingleton(new LocalizationRegistryAction(configure));

        // Factory registrations are added at most once via TryAddSingleton.
        services.TryAddSingleton<ILocalizationRegistry>(sp =>
        {
            var actions = sp.GetServices<LocalizationRegistryAction>();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<JsonLocalizationRegistryBuilder>();
            var builder = new JsonLocalizationRegistryBuilder(logger);
            foreach (var action in actions)
                action.Apply(builder);
            return builder.Build();
        });

        services.TryAddSingleton<ILocalizationFormatter>(sp =>
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
