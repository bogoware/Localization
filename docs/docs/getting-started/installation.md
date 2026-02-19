---
sidebar_position: 1
title: Installation
---

# Installation

## Requirements

- .NET 8.0 or .NET 10.0

## Install via NuGet

```bash
dotnet add package Bogoware.Localization
```

Or via the Package Manager Console:

```powershell
Install-Package Bogoware.Localization
```

## Dependencies

Bogoware.Localization has minimal dependencies:

- `Microsoft.Extensions.DependencyInjection.Abstractions` — for DI integration
- `Microsoft.Extensions.Logging.Abstractions` — for optional logging support

## Basic Setup

### 1. Create your localizable types

Define types that implement the `ILocalizable` marker interface:

```csharp
using Bogoware.Localization;

public class ValidationError(string fieldName, string message) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public string Message { get; } = message;
}
```

### 2. Add JSON template files

Create JSON files with your localized templates as embedded resources:

```json
{
  "MyApp.Errors.ValidationError": "Field '{FieldName}': {Message}"
}
```

### 3. Register with DI

```csharp
services.AddLocalization(typeof(ValidationError).Assembly);
```

This registers:
- `ILocalizationRegistry` — loaded from JSON embedded resources in the specified assembly
- `ILocalizationFormatter` — the main formatting service

### 4. Use the formatter

```csharp
public class MyService(ILocalizationFormatter formatter)
{
    public string GetMessage(ILocalizable localizable)
    {
        return formatter.Format(localizable);
    }
}
```
