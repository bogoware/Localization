---
title: "ILocalizationProvider<T>"
sidebar_position: 99
---

# ILocalizationProvider&lt;T&gt;

Namespace: Bogoware.Localization

External DI provider that can localize instances of .
 No constraint on T — any type can have an external localization provider.

```csharp
public interface ILocalizationProvider<T>
```

#### Type Parameters

`T`<br>
The type this provider can localize.

Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute)

**Remarks:**

Register implementations in the DI container to provide localization for types that
 do not (or cannot) implement [ILocalizationProvider](./bogoware.localization.ilocalizationprovider) themselves.
 This is the second step in the resolution chain, after self-providers and before
 registry template lookup.

Multiple providers can coexist for different types. The formatter resolves the
 provider for the runtime type of the value being formatted.

## Methods

### **Localize(T, CultureInfo)**

Produces a localized string representation of the given `value`.

```csharp
string Localize(T value, CultureInfo culture)
```

#### Parameters

`value` T<br>
The instance to localize.

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The target culture. When , defaults to
 [CultureInfo.CurrentUICulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentuiculture) at the discretion of the implementor.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A culture-aware human-readable string for `value`.
