---
title: "ILocalizationProvider"
sidebar:
  order: 99
---

# ILocalizationProvider

Namespace: Bogoware.Localization

Self-localizing types that can produce their own localized string representation.

```csharp
public interface ILocalizationProvider : ILocalizable
```

Implements [ILocalizable](./bogoware.localization.ilocalizable)

**Remarks:**

This is the highest-priority step in the [ILocalizationFormatter](./bogoware.localization.ilocalizationformatter) resolution chain.
 When a type implements both [ILocalizable](./bogoware.localization.ilocalizable) and [ILocalizationProvider](./bogoware.localization.ilocalizationprovider),
 the formatter calls [ILocalizationProvider.Localize(CultureInfo)](./bogoware.localization.ilocalizationprovider#localizecultureinfo) directly, bypassing DI providers, registry templates,
 and fallback formatting entirely.

## Methods

### **Localize(CultureInfo)**

Produces a localized string representation of the current instance.

```csharp
string Localize(CultureInfo culture)
```

#### Parameters

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The target culture. When , defaults to
 [CultureInfo.CurrentUICulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentuiculture) at the discretion of the implementor.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A culture-aware human-readable string.
