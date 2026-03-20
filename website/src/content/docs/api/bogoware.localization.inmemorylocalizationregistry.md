---
title: "InMemoryLocalizationRegistry"
sidebar:
  order: 99
---

# InMemoryLocalizationRegistry

Namespace: Bogoware.Localization

A simple in-memory registry backed by a flat dictionary (FQDN → format pattern).
 Culture-insensitive — always returns the registered template regardless of the requested culture.
 Intended for unit testing scenarios where culture fallback is not under test.

```csharp
public class InMemoryLocalizationRegistry : ILocalizationRegistry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [InMemoryLocalizationRegistry](./bogoware.localization.inmemorylocalizationregistry)<br>
Implements [ILocalizationRegistry](./bogoware.localization.ilocalizationregistry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **TryGetTemplate(String, CultureInfo, String&)**

```csharp
public bool TryGetTemplate(string fqdn, CultureInfo culture, String& template)
```

#### Parameters

`fqdn` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>

`template` [String&](https://docs.microsoft.com/en-us/dotnet/api/system.string&)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
