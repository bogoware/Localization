---
title: JSON Serialization
sidebar:
  order: 3
---

# JSON Serialization

When DTOs contain `ILocalizable` properties, default JSON serialization emits them as objects with all public properties. The serialization feature converts them to localized strings automatically — so your API responses contain human-readable messages instead of raw property bags.

## Setup

Configure `JsonSerializerOptions` with the `AddLocalization()` extension method:

```csharp
using Bogoware.Localization.Serialization;

var formatter = serviceProvider.GetRequiredService<ILocalizationFormatter>();

var options = new JsonSerializerOptions { WriteIndented = true };
options.AddLocalization(formatter);
```

The method returns the same `options` instance for fluent chaining.

## Serialization Modes

The `mode` parameter controls which properties get localized:

| Mode | Behavior | Best for |
|------|----------|----------|
| **Explicit** | Only properties marked with `[Localize]` | Full control, maximum performance |
| **Auto** (default) | All `ILocalizable` properties + `[Localize]`-marked ones | Most applications |
| **Exhaustive** | Every property attempted via `ILocalizationFormatter.Format<T>()` | Debugging and testing |

### Explicit Mode

Only properties decorated with `[Localize]` are localized. Everything else serializes normally:

```csharp
options.AddLocalization(formatter, LocalizationSerializationMode.Explicit);
```

```csharp
public class ApiResponse
{
    public int Code { get; set; }

    [Localize]
    public RequiredFieldError? UserMessage { get; set; }

    // No attribute -> serialized as object
    public RequiredFieldError? InternalError { get; set; }
}
```

```json
{
  "Code": 400,
  "UserMessage": "'Email' is required",
  "InternalError": { "FieldName": "Email" }
}
```

### Auto Mode (Default)

All `ILocalizable` properties are localized automatically. Use `[Localize]` to opt in non-`ILocalizable` types (that have a registered DI provider), and `[DoNotLocalize]` to opt out specific properties:

```csharp
options.AddLocalization(formatter); // Auto is the default
```

```json
{
  "StatusCode": 400,
  "Error": "'Email' is required",
  "Warnings": ["'Name' is required", "'Bio' must not exceed 200 characters"]
}
```

### Exhaustive Mode

Attempts localization on every property. Falls back to default serialization for types without a registered provider. Primitive types (int, string, etc.) are never localized.

```csharp
options.AddLocalization(formatter, LocalizationSerializationMode.Exhaustive);
```

:::caution
**Performance:** Exhaustive mode calls `ILocalizationFormatter.Format<T>()` on every property, which involves DI service resolution and reflection. Use only for debugging or testing — not recommended for production workloads.
:::

## Attributes

### `[Localize]`

Opts a property into localization. In **Explicit** mode, this is the only way to localize a property. In **Auto** mode, use it to localize non-`ILocalizable` types that have a registered `ILocalizationProvider<T>`:

```csharp
public class OrderResponse
{
    public RequiredFieldError? Error { get; set; }        // Auto-localized (ILocalizable)

    [Localize]
    public OrderConfirmation? Confirmation { get; set; }  // Localized via DI provider
}
```

### `[DoNotLocalize]`

Prevents localization of a property in **Auto** and **Exhaustive** modes. The property serializes as a normal JSON object:

```csharp
public class DebugResponse
{
    public RequiredFieldError? Error { get; set; }        // Localized

    [DoNotLocalize]
    public RequiredFieldError? RawError { get; set; }     // Serialized as object
}
```

```json
{
  "Error": "'Email' is required",
  "RawError": { "FieldName": "Email" }
}
```

## Collections

Properties typed as `IEnumerable<ILocalizable>` (including `List<ILocalizable>`) serialize as arrays of localized strings:

```csharp
public class ValidationResult
{
    public int StatusCode { get; set; }
    public List<ILocalizable>? Warnings { get; set; }
}
```

```json
{
  "StatusCode": 400,
  "Warnings": [
    "'Name' is required",
    "'Bio' must not exceed 200 characters"
  ]
}
```

Null collections serialize as `null`.

## Culture

By default, `CultureInfo.CurrentUICulture` is evaluated at serialization time. This is the correct behavior for ASP.NET apps where culture is set per-request via middleware.

To lock serialization to a specific culture, pass it explicitly:

```csharp
options.AddLocalization(formatter, culture: new CultureInfo("it-IT"));
```

:::tip
In ASP.NET, leave `culture` as `null` (the default) so that request-level culture middleware controls the output language automatically.
:::

## ASP.NET Core Integration

For ASP.NET Core applications, use the dedicated `Bogoware.Localization.AspNetCore` package which configures everything automatically — per-request culture resolution, JSON serialization, and ProblemDetails localization:

```csharp
builder.Services.AddBogowareLocalization(typeof(Program).Assembly);

var app = builder.Build();
app.UseBogowareLocalization();
```

See the [ASP.NET Core Integration](./aspnetcore-integration) guide for full configuration options, culture resolution, ProblemDetails support, and coexistence with framework localization.

## Important Notes

- Serialization is **write-only** — deserializing a localized JSON back into the original DTO will throw `NotSupportedException`, since the localized string cannot be reversed into a typed object.
- Null `ILocalizable` properties serialize as JSON `null`.
- See [localizable types](../concepts/localizable-types) for how to define types that participate in localization, and the [provider chain](../concepts/provider-chain) for the full resolution order.
