---
sidebar_position: 1
title: JSON Templates
---

# Working with JSON Templates

This guide covers how to create, organize, and load JSON template files for localization.

## Template Structure

Each JSON file is a flat key-value dictionary where keys are fully-qualified type names and values are format strings:

```json
{
  "Namespace.ClassName": "Template with {PropertyName} placeholders"
}
```

Keys must match the `Type.FullName` of the localizable type exactly.

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

## Generic Types

The library uses `Type.FullName` as the template key. For **generic types**, .NET produces verbose, assembly-qualified names that include backtick notation and full type argument details:

```
MyApp.GenericResult`1[[System.String, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
```

This means using generic types as JSON template keys is technically possible but impractical — the key is long, brittle (tied to runtime version), and hard to maintain.

### InMemory Registry (Transparent)

`InMemoryLocalizationRegistryBuilder.Add<T>()` handles generic types transparently because both registration and lookup use the same `Type.FullName`:

```csharp
// Both sides resolve the same FQDN key automatically
builder.Add<Result<OrderError>>("Order failed: {Message} (code {Code})");
```

### JSON Templates (Not Recommended)

For JSON-based localization, generic types require the exact `Type.FullName` string as the key, which is unwieldy and fragile. Instead, prefer one of these alternatives:

- **Use concrete (non-generic) wrapper types** — e.g. `OrderResult` instead of `Result<Order>`
- **Implement `ILocalizationProvider` on the type itself** (self-provider) for full control over localization
- **Register an `ILocalizationProvider<T>` via DI** for external formatting logic

See the [Custom Providers guide](./custom-providers.md) for implementation details on both provider approaches

:::tip
If you must use a generic type key in JSON, you can discover the exact key at runtime via `typeof(MyType<MyArg>).FullName` and copy it into your template file.
:::

## JSONC Support

Template files support **JSONC** (JSON with Comments), so you can annotate templates by module, domain, or translation context without affecting parsing.

### Supported Features

| Feature | Syntax | Example |
|---|---|---|
| Single-line comments | `// comment` | `// Orders Module` |
| Block comments | `/* comment */` | `/* Shared validation */` |
| Inline comments | after a value | `"key": "value", // note` |
| Trailing commas | last item in object | `"key": "value",` ← allowed |

### Multi-Module Example

A single JSONC file can use comments as section headers to keep related templates grouped:

```jsonc
{
  // ── Orders Module ──────────────────────────────────────
  "MyApp.Orders.OrderStatus": "Order #{OrderNumber} is {Status}",
  "MyApp.Orders.OrderShippedNotification": "Your order #{OrderNumber} has been shipped to {Address}",

  // ── Payments Module ────────────────────────────────────
  "MyApp.Payments.PaymentDeclined": "Payment of {Amount} was declined: {Reason}",
  "MyApp.Payments.RefundIssued": "A refund of {Amount} has been issued to {Method}",

  /*
   * ── Validation Errors ──────────────────────────────────
   * Shared across modules. Used in ProblemDetails responses.
   */
  "MyApp.Validation.RequiredFieldError": "'{FieldName}' is required",
  "MyApp.Validation.MaxLengthError": "'{FieldName}' must not exceed {MaxLength} characters",
  "MyApp.Validation.InvalidEmailError": "'{FieldName}' is not a valid email address",
}
```

:::tip Editor support
If your editor doesn't highlight comments in `.json` files, rename the file to `.jsonc` or set the file association to "JSON with Comments" in your editor settings. The library reads both extensions identically.
:::

## Organizing Templates

### Single File with Module Sections

The simplest approach: keep all templates in one JSONC file and use comments as section dividers (as shown above). This works well for small-to-medium projects where a single file stays manageable.

### By Use Case

For larger applications, split templates into separate files by category. The library ships with two default resource name patterns — `localized-messages` and `error-messages` — that are discovered automatically:

```
Resources/
├── error-messages.json               # Validation and system errors
├── error-messages.it-IT.json
├── localized-messages.json           # User-facing labels and notifications
└── localized-messages.it-IT.json
```

`error-messages.json`:
```jsonc
{
  // Validation errors returned in ProblemDetails responses
  "MyApp.Validation.RequiredFieldError": "'{FieldName}' is required",
  "MyApp.Validation.MaxLengthError": "'{FieldName}' must not exceed {MaxLength} characters",

  // System errors
  "MyApp.System.ServiceUnavailable": "The {ServiceName} service is temporarily unavailable",
}
```

`localized-messages.json`:
```jsonc
{
  // Order status messages shown in API responses
  "MyApp.Orders.OrderStatus": "Order #{OrderNumber} is {Status}",
  "MyApp.Orders.OrderShipped": "Your order #{OrderNumber} has shipped",

  // Notification templates
  "MyApp.Notifications.WelcomeMessage": "Welcome, {UserName}!",
}
```

Both files are loaded automatically with a single call:

```csharp
// Loads both localized-messages.*.json and error-messages.*.json
builder.AddFromAssemblyResources(typeof(Program).Assembly);
```

You can also add **custom file categories** (e.g. `notification-messages`) by supplying custom patterns:

```csharp
builder.AddFromAssemblyResources(typeof(Program).Assembly,
    "localized-messages", "error-messages", "notification-messages");
```

### By Culture

Create one file per culture. The library determines culture from the filename segment before `.json`:

```
Resources/
├── localized-messages.json           # Invariant (fallback)
├── localized-messages.en-US.json     # English (US)
├── localized-messages.it-IT.json     # Italian
└── localized-messages.de-DE.json     # German
```

The culture fallback chain is: **exact culture → parent culture → invariant culture**. For example, a request for `it-IT` will try `it-IT`, then `it`, then invariant.

### By Assembly / Domain

For multi-project solutions, each assembly can own its own templates. Register them individually or scan by prefix:

```csharp
// Shorthand: uses default resource patterns (localized-messages, error-messages)
services.AddLocalization(typeof(OrderError).Assembly, typeof(AuthError).Assembly);
```

Or use `AddFromLoadedAssemblies` to scan all loaded assemblies whose name starts with given prefixes:

```csharp
services.AddLocalization(builder =>
{
    // Scans all loaded assemblies whose name starts with "MyApp."
    builder.AddFromLoadedAssemblies("MyApp.");
});
```

Assemblies are loaded in dependency order (fewest references first), so base libraries provide defaults and higher-level assemblies can override specific keys.

## Combining Multiple Sources

You can mix embedded resources and filesystem files in the same registration. Templates are merged with **last-loaded-wins** semantics: if the same key appears in multiple sources, the value from the last builder call takes precedence.

### Override Layering Example

A common pattern is to ship base templates inside a shared library assembly and let the host application override specific keys at runtime:

```csharp
services.AddLocalization(builder =>
{
    // Layer 1: base templates from the shared library (embedded resources)
    builder.AddFromAssemblyResources(typeof(SharedLib.Marker).Assembly);

    // Layer 2: base templates from this application
    builder.AddFromAssemblyResources(typeof(Program).Assembly);

    // Layer 3: runtime overrides loaded from disk (e.g. customer-specific wording)
    builder.AddFromFile("Localization/overrides.json", CultureInfo.InvariantCulture);
    builder.AddFromFile("Localization/overrides.it-IT.json", new CultureInfo("it-IT"));
});
```

Because `overrides.json` is loaded last, any keys it defines will replace the embedded versions. Keys it does _not_ define remain untouched.

## Loading Templates

The `JsonLocalizationRegistryBuilder` provides several methods for loading templates. All methods return the builder instance for chaining.

### AddFromAssemblyResources

Loads embedded resources from an assembly whose names contain one of the given patterns (or the defaults `localized-messages` and `error-messages`). Culture is extracted automatically from the filename.

```csharp
// Use default patterns
builder.AddFromAssemblyResources(typeof(Program).Assembly);

// Use custom patterns
builder.AddFromAssemblyResources(typeof(Program).Assembly,
    "localized-messages", "error-messages", "notification-messages");
```

### AddFromFile

Loads a single JSON file from the filesystem. You must specify the culture explicitly:

```csharp
builder.AddFromFile("Resources/custom-messages.json", CultureInfo.InvariantCulture);
builder.AddFromFile("Resources/custom-messages.it-IT.json", new CultureInfo("it-IT"));
```

### AddFromLoadedAssemblies

Scans all currently loaded assemblies whose `AssemblyName` starts with one of the given prefixes and loads their embedded resources using the default patterns. At least one prefix must be provided:

```csharp
// Load templates from all "MyApp.*" assemblies
builder.AddFromLoadedAssemblies("MyApp.");

// Multiple prefixes
builder.AddFromLoadedAssemblies("MyApp.", "MyCompany.Shared.");
```

### AddFromAssemblies

Loads embedded resources from an explicit collection of assemblies, with optional custom patterns:

```csharp
var assemblies = new[] { typeof(OrderError).Assembly, typeof(AuthError).Assembly };
builder.AddFromAssemblies(assemblies);

// With custom patterns
builder.AddFromAssemblies(assemblies, "localized-messages", "domain-messages");
```

### ASP.NET Core Integration

With the `Bogoware.Localization.AspNetCore` package, use `AddBogowareLocalization` which combines registry configuration with middleware setup:

```csharp
builder.Services.AddBogowareLocalization(
    registry: b =>
    {
        b.AddFromAssemblyResources(typeof(Program).Assembly);
        b.AddFromFile("Localization/overrides.json", CultureInfo.InvariantCulture);
    });
```

Or the shorthand overload for assembly-only setups:

```csharp
builder.Services.AddBogowareLocalization(typeof(Program).Assembly);
```

See the [ASP.NET Core Integration guide](./aspnetcore-integration.md) for full details on middleware configuration.

### Builder Chaining

All builder methods return the builder instance, so calls can be chained fluently:

```csharp
services.AddLocalization(builder => builder
    .AddFromLoadedAssemblies("MyApp.")
    .AddFromAssemblyResources(typeof(Program).Assembly, "notification-messages")
    .AddFromFile("Localization/overrides.json", CultureInfo.InvariantCulture)
    .AddFromFile("Localization/overrides.it-IT.json", new CultureInfo("it-IT")));
```

## Embedding Resources

Add the JSON files as embedded resources in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\**\*.json" WithCulture="false" />
</ItemGroup>
```

:::tip
Use `WithCulture="false"` to prevent MSBuild from routing culture-specific files into satellite assemblies. The localization library handles culture resolution internally.
:::
