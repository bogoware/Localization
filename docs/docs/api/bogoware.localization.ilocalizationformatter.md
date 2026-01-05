---
title: "ILocalizationFormatter"
sidebar_position: 99
---

# ILocalizationFormatter

Namespace: Bogoware.Localization

Formats [ILocalizable](./bogoware.localization.ilocalizable) instances and arbitrary values
 into culture-aware human-readable strings using FQDN-keyed template registries.

```csharp
public interface ILocalizationFormatter
```

Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute)

**Remarks:**

The formatter applies the following resolution chain in order:

1. - Self-provider — if the value implements [ILocalizationProvider](./bogoware.localization.ilocalizationprovider), its `Localize` method is called directly.
2. - DI provider — an [ILocalizationProvider&lt;T&gt;](./bogoware.localization.ilocalizationprovider-1) resolved from the service provider for the runtime type.
3. - Registry template — an [ILocalizationRegistry](./bogoware.localization.ilocalizationregistry) template keyed by `Type.FullName`, with `{PropertyName}` placeholders.
4. - Fallback — a generated string in the form `TypeName(Prop=val)`.

For the generic [ILocalizationFormatter.Format(ILocalizable, CultureInfo)](./bogoware.localization.ilocalizationformatter#formatilocalizable-cultureinfo) overload, non-[ILocalizable](./bogoware.localization.ilocalizable) types skip steps 1 and 3,
 falling back to `ToString()` if no DI provider is registered.

## Methods

### **Format(ILocalizable, CultureInfo)**

Formats an [ILocalizable](./bogoware.localization.ilocalizable) using the full resolution chain:
 self-provider, DI provider, registry template, fallback.

```csharp
string Format(ILocalizable value, CultureInfo culture)
```

#### Parameters

`value` [ILocalizable](./bogoware.localization.ilocalizable)<br>
The localizable instance to format.

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The target culture. When , defaults to [CultureInfo.CurrentUICulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentuiculture).

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A culture-aware human-readable string.

### **Format&lt;T&gt;(T, CultureInfo)**

Formats an arbitrary value. If it implements [ILocalizable](./bogoware.localization.ilocalizable),
 delegates to [ILocalizationFormatter.Format(ILocalizable, CultureInfo)](./bogoware.localization.ilocalizationformatter#formatilocalizable-cultureinfo).
 Otherwise tries the DI provider, then `ToString()` fallback.

```csharp
string Format<T>(T value, CultureInfo culture)
```

#### Type Parameters

`T`<br>
The type of value to format.

#### Parameters

`value` T<br>
The value to format.

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The target culture. When , defaults to [CultureInfo.CurrentUICulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentuiculture).

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A culture-aware human-readable string.
