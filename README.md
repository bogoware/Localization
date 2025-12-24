# Bogoware.Localization

Lightweight, FQDN-keyed localization library for .NET with DI support, culture fallback chains, and pluggable providers.

## Features

- **FQDN-keyed templates** — map any type's `FullName` to a localized format string with property placeholders
- **Culture fallback chain** — exact culture → parent culture → invariant culture
- **Resolution chain** — self-provider → DI provider → registry template → fallback message → `TypeName(Prop=val)` format
- **JSON registry** — load templates from embedded resources, files, or raw JSON strings
- **In-memory registry** — simple dictionary-backed registry for testing
- **DI integration** — `IServiceCollection.AddLocalization()` extension methods
- **Zero DDD dependencies** — only depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Installation

```bash
dotnet add package Bogoware.Localization
```

## Quick Start

### 1. Define localizable types

```csharp
using Bogoware.Localization;

// Marker interface — opt-in for localization
public class RequiredFieldError(string fieldName) : ILocalizableString
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
var formatter = serviceProvider.GetRequiredService<ILocalizedMessageFormatter>();
var error = new RequiredFieldError("Email");

// Uses CultureInfo.CurrentUICulture by default
var message = formatter.Format(error);
// → "'Email' is required" (en-US)
// → "'Email' è obbligatorio" (it-IT)
```

## Provider Resolution Chain

The formatter resolves localized strings in this order:

1. **Self-provider** — if the type implements `ILocalizableStringProvider`, calls `Localize(culture)`
2. **DI provider** — if an `ILocalizableStringProvider<T>` is registered in the container
3. **Registry template** — looks up `Type.FullName` in the `ILocalizedMessageRegistry`
4. **Fallback message** — if the type extends `LocalizedMessage`, uses `FallbackMessage`
5. **Default fallback** — `TypeName(Prop1=val1, Prop2=val2)`

## License

MIT
