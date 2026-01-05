---
title: "JsonLocalizationRegistry"
sidebar_position: 99
---

# JsonLocalizationRegistry

Namespace: Bogoware.Localization

JSON-backed registry for managing localized message templates with culture fallback chain.

```csharp
public class JsonLocalizationRegistry : ILocalizationRegistry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [JsonLocalizationRegistry](./bogoware.localization.jsonlocalizationregistry)<br>
Implements [ILocalizationRegistry](./bogoware.localization.ilocalizationregistry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### **JsonLocalizationRegistry()**

```csharp
public JsonLocalizationRegistry()
```

## Methods

### **LoadFromJson(String, CultureInfo)**

Loads message templates from a JSON or JSONC string (FQDN → template pairs) for the specified culture.

```csharp
public void LoadFromJson(string json, CultureInfo culture)
```

#### Parameters

`json` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A JSON (or JSONC) object mapping fully qualified type names to format template strings
 (e.g. `{ "MyApp.Errors.NotFound": "Resource '{Id}' was not found." }`).
 Both single-line (`//`) and block (`/* */`) comments are accepted, as well as trailing commas.

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The culture these templates belong to. Use [CultureInfo.InvariantCulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture) for the default fallback.

**Remarks:**

When the same FQDN key already exists for the given culture, the new value silently
 overrides the previous one. This merge-on-conflict behavior lets downstream assemblies
 override templates defined by upstream assemblies.

### **TryGetTemplate(String, CultureInfo, String&)**

Attempts to retrieve a template for the given FQDN and culture.

```csharp
public bool TryGetTemplate(string fqdn, CultureInfo culture, String& template)
```

#### Parameters

`fqdn` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The fully qualified type name used as the template key.

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The desired culture for the lookup.

`template` [String&](https://docs.microsoft.com/en-us/dotnet/api/system.string&)<br>
When this method returns , contains the resolved template.
 When , set to .

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
if a matching template was found; otherwise .

**Remarks:**

The lookup follows a 3-tier fallback: exact culture → parent culture → invariant culture
 (empty culture name). The first match wins.
