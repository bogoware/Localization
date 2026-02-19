namespace Bogoware.Localization;

/// <summary>
/// Fluent builder for <see cref="InMemoryLocalizedMessageRegistry"/>.
/// Uses <c>typeof(T).FullName</c> as the FQDN key for each registered template.
/// </summary>
public class InMemoryLocalizedMessageRegistryBuilder
{
    private readonly Dictionary<string, string> _templates = new();

    /// <summary>
    /// Registers a format template for the given type, keyed by its full name.
    /// </summary>
    public InMemoryLocalizedMessageRegistryBuilder Add<T>(string format)
    {
        _templates[typeof(T).FullName!] = format;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="InMemoryLocalizedMessageRegistry"/> with all registered templates.
    /// </summary>
    public InMemoryLocalizedMessageRegistry Build()
    {
        return new InMemoryLocalizedMessageRegistry(new Dictionary<string, string>(_templates));
    }
}
