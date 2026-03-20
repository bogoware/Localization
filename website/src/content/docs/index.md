---
title: Bogoware.Localization
description: Lightweight, FQDN-keyed localization for .NET
template: splash
hero:
  title: Bogoware.Localization
  tagline: Lightweight, FQDN-keyed localization for .NET
  actions:
    - text: Get Started
      link: /Localization/getting-started/installation/
      icon: right-arrow
    - text: View on NuGet
      link: https://www.nuget.org/packages/Bogoware.Localization
      variant: minimal
---

# Bogoware.Localization

![Nuget](https://img.shields.io/nuget/dt/Bogoware.Localization?logo=nuget&style=plastic) ![Nuget](https://img.shields.io/nuget/v/Bogoware.Localization?style=plastic)

_A .NET localization library where **each message is a class** — its name is the key, its properties are the placeholders._

**Supported Platforms:** .NET 8 | .NET 10

[Changelog](./changelog) | [NuGet Package](https://www.nuget.org/packages/Bogoware.Localization) | [GitHub Repository](https://github.com/bogoware/Localization)

## Why?

String-based localization keys are fragile. You pick `"errors.required_field"` by convention, match `{0}` placeholders by position, and maintain separate resource files that drift out of sync with every refactor.

Bogoware.Localization takes a different approach: **define a class, and you've defined a message**. The fully-qualified type name becomes the lookup key. The properties become the template placeholders. Rename either one and the compiler catches it — no conventions to memorize, no resource files to keep aligned by hand. It's a well-established idea — types as messages — applied to localization.

## Quick Start

Install from NuGet:

```bash
dotnet add package Bogoware.Localization
```

### 1. Define localizable types

```csharp
using Bogoware.Localization;

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

## Key Features

- **Class-based message modeling** — the type name is the key, properties are the placeholders
- **Culture fallback chain** — exact culture → parent culture → invariant culture
- **Resolution chain** — self-provider → DI provider → registry template → fallback format
- **Nested localization** — `ILocalizable` properties are formatted recursively through the full chain
- **JSON registry** — load templates from embedded resources, files, or raw JSON strings
- **JSON serialization converters** — localize properties during serialization
- **DI integration** — `IServiceCollection.AddLocalization()` extension methods
- **ASP.NET Core integration** — per-request culture resolution, JSON response localization, and ProblemDetails support via `Bogoware.Localization.AspNetCore`
- **Near-zero dependencies (core)** — only depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Next Steps

- **[Installation](./getting-started/installation)** — Setup and configuration
- **[Localizable Types](./concepts/localizable-types)** — How to make types localizable
- **[Provider Chain](./concepts/provider-chain)** — Understanding the resolution chain
- **[ASP.NET Core Integration](./guides/aspnetcore-integration)** — Per-request culture, automatic JSON localization, and ProblemDetails
- **[Nested Localization](./guides/nested-localization)** — Recursive formatting for nested `ILocalizable` properties
- **[API Reference](./api)** — Complete API documentation
