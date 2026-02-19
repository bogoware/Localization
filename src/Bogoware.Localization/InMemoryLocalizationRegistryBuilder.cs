namespace Bogoware.Localization;

/// <summary>
/// Fluent builder for <see cref="InMemoryLocalizationRegistry"/>.
/// Uses <c>typeof(T).FullName</c> as the FQDN key for each registered template.
/// </summary>
/// <example>
/// <code>
/// var registry = new InMemoryLocalizationRegistryBuilder()
///     .Add&lt;InvalidEmailError&gt;("The email '{Email}' is not valid.")
///     .Add&lt;NotFoundError&gt;("Resource '{Id}' was not found.")
///     .Build();
/// </code>
/// </example>
public class InMemoryLocalizationRegistryBuilder
{
    private readonly Dictionary<string, string> _templates = new();

    /// <summary>
    /// Registers a format template for the given type, keyed by its full name.
    /// </summary>
    /// <typeparam name="T">The type this template describes. Its <c>FullName</c> becomes the FQDN key.</typeparam>
    /// <param name="format">
    /// A format string with <c>{PropertyName}</c> placeholders that match public properties on <typeparamref name="T"/>.
    /// </param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public InMemoryLocalizationRegistryBuilder Add<T>(string format)
    {
        _templates[typeof(T).FullName!] = format;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="InMemoryLocalizationRegistry"/> with all registered templates.
    /// </summary>
    /// <returns>A new <see cref="InMemoryLocalizationRegistry"/> containing all registered templates.</returns>
    public InMemoryLocalizationRegistry Build()
    {
        return new InMemoryLocalizationRegistry(new Dictionary<string, string>(_templates));
    }
}
