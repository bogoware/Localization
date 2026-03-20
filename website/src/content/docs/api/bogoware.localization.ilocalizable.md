---
title: "ILocalizable"
sidebar:
  order: 99
---


Namespace: Bogoware.Localization

Marker interface — opt-in for localization.
 Any type implementing this interface can be formatted by [ILocalizationFormatter](./bogoware.localization.ilocalizationformatter).

```csharp
public interface ILocalizable
```

**Remarks:**

Implement this interface on types that should participate in the localization pipeline.
 By itself, [ILocalizable](./bogoware.localization.ilocalizable) carries no members — it simply marks a type as eligible
 for the [ILocalizationFormatter](./bogoware.localization.ilocalizationformatter) resolution chain.

For self-localizing types, also implement [ILocalizationProvider](./bogoware.localization.ilocalizationprovider).
 For external localization via dependency injection, register an [ILocalizationProvider&lt;T&gt;](./bogoware.localization.ilocalizationprovider-1).
