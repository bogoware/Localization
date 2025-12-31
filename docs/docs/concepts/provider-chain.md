---
sidebar_position: 3
title: Provider Chain
---

# Provider Resolution Chain

The `ILocalizationFormatter` resolves localized strings through a chain of providers, stopping at the first one that returns a result.

## Resolution Order

1. **Self-provider** — If the type implements `ILocalizationProvider`, its `Localize(culture)` method is called directly
2. **DI provider** — If an `ILocalizationProvider<T>` for the specific type is registered in the DI container, it is used
3. **Registry template** — The `ILocalizationRegistry` is queried using the type's `FullName` as key
4. **Fallback message** — A configured fallback format is used
5. **Default format** — `TypeName(Prop1=val1, Prop2=val2)` — a diagnostic representation

## Self-Provider

The highest priority. Implement `ILocalizationProvider` directly on your type:

```csharp
public class SpecialError : ILocalizable, ILocalizationProvider
{
    public string Localize(CultureInfo culture) => "Special error message";
}
```

## DI Provider

Register a provider for a specific type in the DI container:

```csharp
public class OrderErrorProvider : ILocalizationProvider<OrderError>
{
    public string? Localize(OrderError instance, CultureInfo culture)
    {
        // Custom localization logic
        return $"Order {instance.OrderId} failed: {instance.Reason}";
    }
}

// Registration
services.AddSingleton<ILocalizationProvider<OrderError>, OrderErrorProvider>();
```

## Registry Template

The standard approach. Templates are loaded from JSON files into the `ILocalizationRegistry`:

```json
{
  "MyApp.Errors.OrderError": "Order #{OrderId} failed: {Reason}"
}
```

## Default Format

When no other provider returns a result, the formatter produces a diagnostic string:

```
OrderError(OrderId=12345, Reason=Payment declined)
```

This ensures that the formatter always returns a meaningful string, even without configured templates.
