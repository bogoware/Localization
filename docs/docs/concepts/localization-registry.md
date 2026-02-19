---
sidebar_position: 2
title: Localization Registry
---

# Localization Registry

The `ILocalizationRegistry` stores mappings from type full names to localized template strings, organized by culture.

## JSON Registry

The `JsonLocalizationRegistry` loads templates from JSON files. Templates are typically stored as embedded resources in your assembly.

### JSON Format

Each JSON file contains a flat dictionary mapping type full names to format strings:

```json
{
  "MyApp.Errors.RequiredFieldError": "'{FieldName}' is required",
  "MyApp.Errors.InvalidEmailError": "'{Email}' is not a valid email address"
}
```

### File Naming Convention

JSON files follow the pattern `localized-messages.{culture}.json`:

- `localized-messages.en-US.json` — English (US)
- `localized-messages.it-IT.json` — Italian
- `localized-messages.json` — Invariant culture (fallback)

### Loading from Embedded Resources

Mark your JSON files as embedded resources in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="localized-messages.en-US.json" WithCulture="false" />
  <EmbeddedResource Include="localized-messages.it-IT.json" WithCulture="false" />
</ItemGroup>
```

:::note
The `WithCulture="false"` attribute prevents MSBuild from routing the files into satellite assemblies.
:::

Then register the assembly containing the resources:

```csharp
services.AddLocalization(typeof(MyType).Assembly);
```

## In-Memory Registry

The `InMemoryLocalizationRegistry` is a simple dictionary-backed registry, useful for testing:

```csharp
var registry = new InMemoryLocalizationRegistryBuilder()
    .AddTemplate("en-US", "MyApp.Errors.RequiredFieldError", "'{FieldName}' is required")
    .AddTemplate("it-IT", "MyApp.Errors.RequiredFieldError", "'{FieldName}' è obbligatorio")
    .Build();
```
