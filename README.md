# Bogoware.Localization

![Nuget](https://img.shields.io/nuget/dt/Bogoware.Localization?logo=nuget&style=plastic) ![Nuget](https://img.shields.io/nuget/v/Bogoware.Localization?style=plastic) [![Documentation](https://img.shields.io/badge/docs-online-blue)](https://bogoware.github.io/Localization/)

Lightweight, FQDN-keyed localization library for .NET with DI support, culture fallback chains, and pluggable providers.

**Supported Platforms:** .NET 8 | .NET 10

[Documentation](https://bogoware.github.io/Localization/) | [CHANGELOG](./CHANGELOG.md) | [NuGet Package](https://www.nuget.org/packages/Bogoware.Localization)

## Features

- **FQDN-keyed templates** — map any type's `FullName` to a localized format string with property placeholders
- **Culture fallback chain** — exact culture → parent culture → invariant culture
- **Resolution chain** — self-provider → DI provider → registry template → fallback message → `TypeName(Prop=val)` format
- **JSON registry** — load templates from embedded resources, files, or raw JSON strings
- **In-memory registry** — simple dictionary-backed registry for testing
- **DI integration** — `IServiceCollection.AddLocalization()` extension methods
- **Zero dependencies** — only depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Installation

```bash
dotnet add package Bogoware.Localization
```

## Quick Start

### 1. Define localizable types

```csharp
using Bogoware.Localization;

// Marker interface — opt-in for localization
public class RequiredFieldError(string fieldName) : ILocalizable
{
    public string FieldName { get; } = fieldName;
}
```

### 2. Create JSON templates

**`localized-messages.en-US.json`** (embedded resource):
```json
{
  "MyApp.Errors.RequiredFieldError": "'{FieldName}' is required"
}
```

**`localized-messages.it-IT.json`** (embedded resource):
```json
{
  "MyApp.Errors.RequiredFieldError": "'{FieldName}' è obbligatorio"
}
```

### 3. Register in DI

```csharp
services.AddLocalization(typeof(RequiredFieldError).Assembly);
```

### 4. Format

```csharp
var formatter = serviceProvider.GetRequiredService<ILocalizationFormatter>();
var error = new RequiredFieldError("Email");

// Uses CultureInfo.CurrentUICulture by default
var message = formatter.Format(error);
// → "'Email' is required" (en-US)
// → "'Email' è obbligatorio" (it-IT)
```

## Provider Resolution Chain

The formatter resolves localized strings in this order:

1. **Self-provider** — if the type implements `ILocalizationProvider`, calls `Localize(culture)`
2. **DI provider** — if an `ILocalizationProvider<T>` is registered in the container
3. **Registry template** — looks up `Type.FullName` in the `ILocalizationRegistry`
4. **Fallback message** — uses the default fallback format
5. **Default fallback** — `TypeName(Prop1=val1, Prop2=val2)`

## License

[MIT](./LICENSE)
