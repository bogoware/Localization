namespace Bogoware.Localization;

/// <summary>
/// Wraps a single configuration delegate for the <see cref="JsonLocalizationRegistryBuilder"/>.
/// Multiple instances are registered in DI and collected at resolution time,
/// enabling additive <c>AddLocalization</c> calls.
/// </summary>
internal sealed class LocalizationRegistryAction(Action<JsonLocalizationRegistryBuilder> configure)
{
    internal void Apply(JsonLocalizationRegistryBuilder builder) => configure(builder);
}
