# Bogoware.Localization

![Nuget](https://img.shields.io/nuget/dt/Bogoware.Localization?logo=nuget&style=plastic) ![Nuget](https://img.shields.io/nuget/v/Bogoware.Localization?style=plastic) [![Documentation](https://img.shields.io/badge/docs-online-blue)](https://bogoware.github.io/Localization/)

A .NET localization library where **each message is a class** — its name is the key, its properties are the placeholders. No magic strings, no naming conventions to memorize, no resource files to keep in sync.

**Supported Platforms:** .NET 8 | .NET 10

[Documentation](https://bogoware.github.io/Localization/) | [CHANGELOG](https://github.com/Bogoware/Localization/blob/rel/prod/CHANGELOG.md) | [NuGet: Core](https://www.nuget.org/packages/Bogoware.Localization) | [NuGet: AspNetCore](https://www.nuget.org/packages/Bogoware.Localization.AspNetCore)

## Why Bogoware.Localization?

Most localization frameworks start with a string key — `"errors.required_field"` — and a separately maintained template — `"{0} is required"`. You pick the key by convention, match positional arguments by hand, and hope everything stays consistent across languages and refactors.

**What if the message itself were a type?**

```csharp
public class RequiredFieldError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}
```

The fully-qualified type name *is* the key. The properties *are* the placeholders. Rename a class and the compiler tells you; rename a property and the template follows. There is nothing to keep in sync by hand.

This is a well-established idea — types as messages — applied to localization. The result is a system that is **type-safe by design** and **simple by default**: one DI call registers everything, culture fallback works automatically, and you can swap providers without touching the rest of your code.

## Key Features

- **Class-based message modeling** — the type name is the key, properties are the placeholders
- **Culture fallback chain** — exact → parent → invariant, automatic
- **Multi-level provider resolution** — self, DI, registry, fallback
- **JSON registry** — load templates from embedded resources, files, or raw strings
- **JSON serialization converters** — localize properties during serialization
- **ASP.NET Core integration** — per-request culture resolution, automatic JSON response localization, and ProblemDetails support via [Bogoware.Localization.AspNetCore](https://www.nuget.org/packages/Bogoware.Localization.AspNetCore)
- **DI integration** — single-call setup via `IServiceCollection`
- **Near-zero dependencies (core)** — only `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Getting Started

```bash
dotnet add package Bogoware.Localization
```

```csharp
// 1. Define a localizable type — this *is* the message
public class RequiredFieldError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}

// 2. Register with one DI call (scans assembly for JSON templates)
services.AddLocalization(typeof(RequiredFieldError).Assembly);

// 3. Format — culture fallback is automatic
var message = formatter.Format(new RequiredFieldError("Email"));
// → "'Email' is required" (en-US)
// → "'Email' è obbligatorio" (it-IT)
```

## Learn More

Visit the [documentation site](https://bogoware.github.io/Localization/) for the Quick Start guide, detailed walkthroughs, and API reference.

## License

[MIT](https://github.com/Bogoware/Localization/blob/rel/prod/LICENSE)
