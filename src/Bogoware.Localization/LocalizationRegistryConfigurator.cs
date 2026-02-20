using Microsoft.Extensions.Logging;

namespace Bogoware.Localization;

/// <summary>
/// Accumulates <see cref="JsonLocalizationRegistryBuilder"/> configuration delegates across
/// multiple <c>AddLocalization</c> calls, enabling additive registration.
/// </summary>
internal sealed class LocalizationRegistryConfigurator
{
    private readonly List<Action<JsonLocalizationRegistryBuilder>> _configureActions = [];

    internal void Add(Action<JsonLocalizationRegistryBuilder> configure)
        => _configureActions.Add(configure);

    internal JsonLocalizationRegistry Build(ILogger? logger)
    {
        var builder = new JsonLocalizationRegistryBuilder(logger);
        foreach (var action in _configureActions)
            action(builder);
        return builder.Build();
    }
}
