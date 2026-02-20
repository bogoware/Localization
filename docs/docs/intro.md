---
sidebar_position: 1
slug: /
title: Introduction
---

# Bogoware.Localization

![Nuget](https://img.shields.io/nuget/dt/Bogoware.Localization?logo=nuget&style=plastic) ![Nuget](https://img.shields.io/nuget/v/Bogoware.Localization?style=plastic)

_Lightweight, FQDN-keyed localization library for .NET with DI support, culture fallback chains, and pluggable providers._

**Supported Platforms:** .NET 8 | .NET 10

[Changelog](./changelog) | [NuGet Package](https://www.nuget.org/packages/Bogoware.Localization) | [GitHub Repository](https://github.com/bogoware/Localization)

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

- **FQDN-keyed templates** — map any type's `FullName` to a localized format string with property placeholders
- **Culture fallback chain** — exact culture → parent culture → invariant culture
- **Resolution chain** — self-provider → DI provider → registry template → fallback message → default format
- **JSON registry** — load templates from embedded resources, files, or raw JSON strings
- **DI integration** — `IServiceCollection.AddLocalization()` extension methods
- **ASP.NET Core integration** — per-request culture resolution, JSON response localization, and ProblemDetails support via `Bogoware.Localization.AspNetCore`
- **Zero DDD dependencies** — only depends on `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Next Steps

- **[Installation](./getting-started/installation)** — Setup and configuration
- **[Localizable Types](./concepts/localizable-types)** — How to make types localizable
- **[Provider Chain](./concepts/provider-chain)** — Understanding the resolution chain
- **[ASP.NET Core Integration](./guides/aspnetcore-integration)** — Per-request culture, automatic JSON localization, and ProblemDetails
- **[API Reference](./api)** — Complete API documentation
