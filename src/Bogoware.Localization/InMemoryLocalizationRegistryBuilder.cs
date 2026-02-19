namespace Bogoware.Localization;

/// <summary>
/// Fluent builder for <see cref="InMemoryLocalizationRegistry"/>.
/// Uses <c>typeof(T).FullName</c> as the FQDN key for each registered template.
/// </summary>
public class InMemoryLocalizationRegistryBuilder
{
    private readonly Dictionary<string, string> _templates = new();

    /// <summary>
    /// Registers a format template for the given type, keyed by its full name.
    /// </summary>
    public InMemoryLocalizationRegistryBuilder Add<T>(string format)
    {
        _templates[typeof(T).FullName!] = format;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="InMemoryLocalizationRegistry"/> with all registered templates.
    /// </summary>
    public InMemoryLocalizationRegistry Build()
    {
        return new InMemoryLocalizationRegistry(new Dictionary<string, string>(_templates));
    }
}
