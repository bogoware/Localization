---
sidebar_position: 1
title: JSON Templates
---

# Working with JSON Templates

This guide covers how to create, organize, and load JSON template files for localization.

## Template Structure

Each JSON file is a flat key-value dictionary:

```json
{
  "Namespace.ClassName": "Template with {PropertyName} placeholders"
}
```

Keys must match the `Type.FullName` of the localizable type exactly.

## Organizing Templates

### By Culture

Create one file per culture:

```
Resources/
├── localized-messages.json         # Invariant (fallback)
├── localized-messages.en-US.json   # English (US)
├── localized-messages.it-IT.json   # Italian
└── localized-messages.de-DE.json   # German
```

### By Domain

For larger applications, you can split templates across multiple assemblies. Each assembly registers its own templates:

```csharp
// In your Orders module
services.AddLocalization(typeof(OrderError).Assembly);

// In your Auth module
services.AddLocalization(typeof(AuthError).Assembly);
```

## Embedding Resources

Add the JSON files as embedded resources in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\localized-messages.*.json" WithCulture="false" />
  <EmbeddedResource Include="Resources\localized-messages.json" WithCulture="false" />
</ItemGroup>
```

:::tip
Use `WithCulture="false"` to prevent MSBuild from routing culture-specific files into satellite assemblies. The localization library handles culture resolution internally.
:::

## Placeholder Syntax

Placeholders use the `{PropertyName}` syntax and are replaced with the corresponding public property values from the localizable type:

```csharp
public class TransferError(string from, string to, decimal amount) : ILocalizable
{
    public string From { get; } = from;
    public string To { get; } = to;
    public decimal Amount { get; } = amount;
}
```

```json
{
  "MyApp.Errors.TransferError": "Cannot transfer {Amount} from '{From}' to '{To}'"
}
```
