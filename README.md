# Bogoware.Localization

![Nuget](https://img.shields.io/nuget/dt/Bogoware.Localization?logo=nuget&style=plastic) ![Nuget](https://img.shields.io/nuget/v/Bogoware.Localization?style=plastic) [![Documentation](https://img.shields.io/badge/docs-online-blue)](https://bogoware.github.io/Localization/)

Type-safe, FQDN-keyed localization for .NET — define localizable types, load JSON templates, and let the formatter resolve culture-aware messages with zero boilerplate.

**Supported Platforms:** .NET 8 | .NET 10

[Documentation](https://bogoware.github.io/Localization/) | [CHANGELOG](https://github.com/Bogoware/Localization/blob/rel/prod/CHANGELOG.md) | [NuGet Package](https://www.nuget.org/packages/Bogoware.Localization)

## Why Bogoware.Localization?

**Type-safe by design.** Templates are keyed by fully-qualified type names and bound to real .NET types — not magic strings. Rename a class and the compiler tells you; property placeholders resolve from the actual object at runtime.

**Zero-config simplicity.** One DI call registers everything. Culture fallback walks from exact culture to parent to invariant automatically. Swap providers, add registries, or supply your own resolution logic without touching the rest of your code.

## Key Features

- **FQDN-keyed templates** — tied to real types (classes and structs), not magic strings
- **Culture fallback chain** — exact → parent → invariant, automatic
- **Multi-level provider resolution** — self, DI, registry, fallback
- **JSON registry** — load from embedded resources, files, or raw strings
- **JSON serialization converters** — localize properties during serialization
- **DI integration** — single-call setup via `IServiceCollection`
- **Near-zero dependencies** — only `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Getting Started

```bash
dotnet add package Bogoware.Localization
```

```csharp
// Define a localizable type
public class RequiredFieldError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}

// Register with one DI call (scans assembly for JSON templates)
services.AddLocalization(typeof(RequiredFieldError).Assembly);

// Format — culture fallback is automatic
var message = formatter.Format(new RequiredFieldError("Email"));
// With JSON templates: "'Email' is required" (en-US), "'Email' è obbligatorio" (it-IT)
```

## Learn More

Visit the [documentation site](https://bogoware.github.io/Localization/) for the Quick Start guide, detailed walkthroughs, and API reference.

## License

[MIT](https://github.com/Bogoware/Localization/blob/rel/prod/LICENSE)
