---
sidebar_position: 5
title: Additive Configuration
---

# Additive Configuration

## Overview

`AddLocalization` is **additive**: every call appends a configuration delegate to a shared pipeline. All delegates run in registration order when the singleton `ILocalizationRegistry` is first resolved. Later templates override earlier ones on a per-key/per-culture basis.

This lets libraries ship their own default templates while still allowing the host application to override specific messages — without either side knowing about the other at compile time.

## How It Works

```
Call 1 ─► delegate A ──┐
Call 2 ─► delegate B ──┤──► single JsonLocalizationRegistryBuilder ──► ILocalizationRegistry
Call 3 ─► delegate C ──┘
```

1. The first `AddLocalization` call creates an internal `LocalizationRegistryConfigurator` and registers the `ILocalizationRegistry` / `ILocalizationFormatter` singletons once.
2. Each subsequent call finds the existing configurator and adds its delegate.
3. At resolution time, a single `JsonLocalizationRegistryBuilder` runs all delegates in order and calls `Build()`.

Because delegates execute in registration order, later calls override earlier ones when the same FQDN key and culture collide.

## Template Layering Example

Consider an e-commerce solution with a shared library and two consuming APIs:

### Shared library (`Acme.Orders`)

`Resources/error-messages.json`:
```json
{
  "Acme.Orders.OrderNotFound": "Order '{OrderId}' was not found in the system",
  "Acme.Orders.PaymentFailed": "Payment for order '{OrderId}' failed: {Reason}",
  "Acme.Orders.ItemOutOfStock": "Item '{ItemName}' is currently out of stock"
}
```

`Program.cs` (library host):
```csharp
// Library registers its base templates
services.AddLocalization(b => b.AddFromAssembly(typeof(Acme.Orders.OrderNotFound).Assembly));
```

### Customer-facing Web API

Overrides two messages with user-friendly wording:

`Resources/error-messages.json`:
```json
{
  "Acme.Orders.OrderNotFound": "Sorry, we couldn't find order '{OrderId}'",
  "Acme.Orders.PaymentFailed": "Your payment didn't go through. Please try again."
}
```

`Program.cs`:
```csharp
// First: shared library templates (base)
services.AddLocalization(b => b.AddFromAssembly(typeof(Acme.Orders.OrderNotFound).Assembly));

// Second: app-specific overrides (wins on conflict)
services.AddLocalization(b => b.AddFromAssembly(typeof(Program).Assembly));
```

Result: `OrderNotFound` and `PaymentFailed` use the friendly wording; `ItemOutOfStock` keeps the shared library's default.

### Admin API

Keeps the technical defaults from the shared library — no override call needed:

```csharp
services.AddLocalization(b => b.AddFromAssembly(typeof(Acme.Orders.OrderNotFound).Assembly));
```

## Assembly Scanning Methods

The `JsonLocalizationRegistryBuilder` provides three assembly scanning methods:

### `AddFromAssembly`

Scans embedded resources from a **single assembly**.

```csharp
builder.AddFromAssembly(typeof(Program).Assembly);

// With custom patterns
builder.AddFromAssembly(typeof(Program).Assembly, "notification-messages");
```

**When to use:** You know exactly which assembly to scan and want precise control.

### `AddFromAssemblyTree`

Scans the given assembly **and all its transitive referenced assemblies**. Assemblies are loaded in topological order — deepest dependencies first, root last — so the root can override its dependencies' templates.

```csharp
builder.AddFromAssemblyTree(typeof(Program).Assembly);
```

System assemblies (`System.*`, `Microsoft.*`, `netstandard`) are automatically excluded. Assemblies that cannot be loaded are silently skipped.

**When to use:** Convention-based layering where you want dependencies' templates loaded automatically, with the application assembly overriding as needed. Ideal for application entry points.

### `AddFromLoadedAssemblies`

Scans all assemblies currently loaded in the AppDomain whose names start with the given prefixes. Assemblies are sorted by reference count (ascending), so base libraries load first.

```csharp
builder.AddFromLoadedAssemblies(["MyApp.", "MyCompany.Shared."]);

// With custom patterns
builder.AddFromLoadedAssemblies(["MyApp."], "notification-messages");
```

**When to use:** You want to discover assemblies at runtime without explicitly listing them.

### Decision Table

| Scenario | Method |
|----------|--------|
| Single known assembly | `AddFromAssembly` |
| Application root + all its NuGet/project dependencies | `AddFromAssemblyTree` |
| Runtime discovery by naming convention | `AddFromLoadedAssemblies` |
| Explicit ordered list | `AddFromAssemblies` |

## Override Diagnostics

When a template key is overridden, the library emits a **Warning**-level log entry:

```
warn: Bogoware.Localization.JsonLocalizationRegistryBuilder
      Template override: key 'Acme.Orders.OrderNotFound' for culture '' replaced (source: assembly:MyApp:error-messages.json)
```

### Structured Log Fields

| Field | Description |
|-------|-------------|
| `Fqdn` | The fully-qualified type name key being overridden |
| `Culture` | The culture for which the override occurred (empty string = invariant) |
| `Source` | Descriptor of the overriding source |

### Enabling Diagnostic Logging

```csharp
builder.Logging.AddFilter("Bogoware.Localization", LogLevel.Debug);
```

For override warnings only:
```csharp
builder.Logging.AddFilter("Bogoware.Localization", LogLevel.Warning);
```

## Source Tracking

Each template registration carries a `Source` descriptor:

| Registration Method | Source Format |
|---------------------|--------------|
| `AddFromAssembly` | `assembly:{AssemblyName}:{ResourceName}` |
| `AddFromFile` | `file:{Path}` |
| `LoadFromJson` (direct) | `inline` (default) |

## Best Practices

1. **Register base/shared libraries first**, application-specific overrides last.
2. Use `AddFromAssemblyTree` for convention-based layering at application roots.
3. Enable `Warning`-level logging during development to catch unintended overrides.
4. Use `Debug`-level logging to trace which assemblies and resources are loaded.
5. Keep override files small — only include the keys you want to change.
