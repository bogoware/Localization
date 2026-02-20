using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;
using MinimalApiJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Extension methods for integrating Bogoware.Localization into the ASP.NET Core pipeline.
/// </summary>
public static class BogowareLocalizationExtensions
{
    /// <summary>
    /// Registers Bogoware localization services with full builder and middleware customization.
    /// This method is idempotent: the JSON post-configure hooks are registered at most once,
    /// while registry and middleware options are always updated (last call wins).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registry">
    /// A delegate that receives a <see cref="JsonLocalizationRegistryBuilder"/> for loading
    /// localization templates from embedded resources, JSON files, or assembly scanning.
    /// </param>
    /// <param name="middleware">
    /// An optional delegate to configure <see cref="BogowareLocalizationMiddlewareOptions"/>
    /// for culture resolution, serialization mode, ProblemDetails integration, and diagnostics.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddBogowareLocalization(
    ///     registry: b => b.AddFromAssembly(typeof(Program).Assembly),
    ///     middleware: options =>
    ///     {
    ///         options.DefaultCulture = new CultureInfo("en-US");
    ///         options.SupportedCultures.AddRange(new[]
    ///         {
    ///             new CultureInfo("en-US"),
    ///             new CultureInfo("it-IT"),
    ///         });
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddBogowareLocalization(
        this IServiceCollection services,
        Action<JsonLocalizationRegistryBuilder> registry,
        Action<BogowareLocalizationMiddlewareOptions>? middleware = null)
    {
        // Layer 0: core library DI (ILocalizationRegistry + ILocalizationFormatter)
        // Additive: each call appends to the registry builder; later templates override earlier ones.
        services.AddLocalization(registry);

        // Middleware options — Configure merges; last call's delegate wins for each property.
        if (middleware is not null)
            services.Configure(middleware);
        else
            services.AddOptions<BogowareLocalizationMiddlewareOptions>();

        // Post-configure hooks and culture resolution are registered at most once.
        // TryAddSingleton prevents duplicates per registration type individually.
        services.TryAddSingleton<IConfigureOptions<RequestLocalizationOptions>,
            BogowareLocalizationRequestLocalizationConfigure>();

        services.TryAddSingleton<IPostConfigureOptions<MinimalApiJsonOptions>,
            BogowareLocalizationJsonPostConfigure>();

        services.TryAddSingleton<IPostConfigureOptions<MvcJsonOptions>,
            BogowareLocalizationMvcJsonPostConfigure>();

        services.TryAddSingleton<IPostConfigureOptions<ProblemDetailsOptions>,
            BogowareLocalizationProblemDetailsPostConfigure>();

        return services;
    }

    /// <summary>
    /// Convenience overload: scans the provided assemblies for embedded localization resources
    /// using default patterns (<c>localized-messages</c>, <c>error-messages</c>).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for embedded JSON resources.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddBogowareLocalization(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return services.AddBogowareLocalization(
            registry: builder =>
            {
                foreach (var assembly in assemblies)
                    builder.AddFromAssembly(assembly);
            });
    }

    /// <summary>
    /// Convenience overload: scans assemblies with default middleware options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="middleware">Middleware options configuration.</param>
    /// <param name="assemblies">The assemblies to scan for embedded JSON resources.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddBogowareLocalization(
        this IServiceCollection services,
        Action<BogowareLocalizationMiddlewareOptions> middleware,
        params Assembly[] assemblies)
    {
        return services.AddBogowareLocalization(
            registry: builder =>
            {
                foreach (var assembly in assemblies)
                    builder.AddFromAssembly(assembly);
            },
            middleware: middleware);
    }

    /// <summary>
    /// Adds the Bogoware localization middleware to the ASP.NET Core pipeline.
    /// This calls <see cref="ApplicationBuilderExtensions.UseRequestLocalization(IApplicationBuilder)"/>
    /// to enable per-request culture resolution (shared with framework localization),
    /// and optionally enables the response-buffering middleware if configured.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/> instance for fluent chaining.</returns>
    public static IApplicationBuilder UseBogowareLocalization(this IApplicationBuilder app)
    {
        // Shared culture resolution middleware (framework's own)
        app.UseRequestLocalization();

        // Layer 2: opt-in response buffering
        var options = app.ApplicationServices
            .GetRequiredService<IOptions<BogowareLocalizationMiddlewareOptions>>().Value;

        if (options.EnableResponseBuffering)
        {
            app.UseMiddleware<BogowareLocalizationMiddleware>();
        }

        return app;
    }
}
