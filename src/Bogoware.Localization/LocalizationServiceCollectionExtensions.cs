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
    /// The builder receives an <see cref="ILogger"/> from the service provider at resolution time.
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
    ///     .AddFromAssemblyResources(typeof(Program).Assembly)
    ///     .AddFromLoadedAssemblies("MyApp"));
    /// </code>
    /// </example>
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
    /// <param name="services">The service collection to add to.</param>
    /// <param name="assemblies">The assemblies to scan for embedded JSON resources.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddLocalization(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddLocalization(builder =>
        {
            foreach (var assembly in assemblies)
                builder.AddFromAssemblyResources(assembly);
        });
    }
}
