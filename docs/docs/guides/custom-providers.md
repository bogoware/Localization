---
sidebar_position: 2
title: Custom Providers
---

# Custom Providers

While JSON templates cover most use cases, you can implement custom localization providers for more complex scenarios.

## ILocalizationProvider (Self-Provider)

Implement directly on your localizable type for full control:

```csharp
public class DynamicError : ILocalizable, ILocalizationProvider
{
    public string Code { get; init; }

    public string Localize(CultureInfo culture)
    {
        // Load from database, remote service, etc.
        return LoadFromDatabase(Code, culture);
    }
}
```

This is the highest priority in the resolution chain — it bypasses the registry entirely.

## ILocalizationProvider&lt;T&gt; (DI Provider)

Register a provider for a specific type via dependency injection:

```csharp
public class RichOrderErrorProvider : ILocalizationProvider<OrderError>
{
    private readonly IOrderRepository _orders;

    public RichOrderErrorProvider(IOrderRepository orders)
    {
        _orders = orders;
    }

    public string? Localize(OrderError instance, CultureInfo culture)
    {
        var order = _orders.Find(instance.OrderId);
        if (order is null) return null; // Fall through to next provider

        return culture.Name switch
        {
            "it-IT" => $"Ordine #{order.Number} fallito: {instance.Reason}",
            _ => $"Order #{order.Number} failed: {instance.Reason}"
        };
    }
}
```

Register in DI:

```csharp
services.AddSingleton<ILocalizationProvider<OrderError>, RichOrderErrorProvider>();
```

:::tip
Return `null` from a DI provider to let the resolution chain continue to the registry or fallback.
:::

## When to Use Custom Providers

| Scenario | Recommended Approach |
|----------|---------------------|
| Static, template-based strings | JSON templates |
| Dynamic content (database, API) | `ILocalizationProvider` |
| Type-specific logic with DI dependencies | `ILocalizationProvider<T>` |
| Generic types (e.g. `Result<T>`) | `ILocalizationProvider` or `ILocalizationProvider<T>` |
| Testing | `InMemoryLocalizationRegistry` |
