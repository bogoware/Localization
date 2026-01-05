---
title: "InMemoryLocalizationRegistryBuilder"
sidebar_position: 99
---

# InMemoryLocalizationRegistryBuilder

Namespace: Bogoware.Localization

Fluent builder for [InMemoryLocalizationRegistry](./bogoware.localization.inmemorylocalizationregistry).
 Uses `typeof(T).FullName` as the FQDN key for each registered template.

```csharp
public class InMemoryLocalizationRegistryBuilder
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [InMemoryLocalizationRegistryBuilder](./bogoware.localization.inmemorylocalizationregistrybuilder)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **InMemoryLocalizationRegistryBuilder()**

```csharp
public InMemoryLocalizationRegistryBuilder()
```

## Methods

### **Add&lt;T&gt;(String)**

Registers a format template for the given type, keyed by its full name.

```csharp
public InMemoryLocalizationRegistryBuilder Add<T>(string format)
```

#### Type Parameters

`T`<br>
The type this template describes. Its `FullName` becomes the FQDN key.

#### Parameters

`format` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A format string with `{PropertyName}` placeholders that match public properties on .

#### Returns

[InMemoryLocalizationRegistryBuilder](./bogoware.localization.inmemorylocalizationregistrybuilder)<br>
This builder instance for fluent chaining.

### **Build()**

Builds the [InMemoryLocalizationRegistry](./bogoware.localization.inmemorylocalizationregistry) with all registered templates.

```csharp
public InMemoryLocalizationRegistry Build()
```

#### Returns

[InMemoryLocalizationRegistry](./bogoware.localization.inmemorylocalizationregistry)<br>
A new [InMemoryLocalizationRegistry](./bogoware.localization.inmemorylocalizationregistry) containing all registered templates.
