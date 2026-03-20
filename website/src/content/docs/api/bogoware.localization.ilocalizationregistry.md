---
title: "ILocalizationRegistry"
sidebar:
  order: 99
---


Namespace: Bogoware.Localization

Represents a contract for managing culture-specific message templates
 associated with fully qualified domain names (FQDNs).

```csharp
public interface ILocalizationRegistry
```

Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute)

**Remarks:**

Template keys use `typeof(T).FullName` as the FQDN convention. Implementations
 typically provide a culture fallback chain: exact culture, parent culture, then invariant culture.

## Methods

### **TryGetTemplate(String, CultureInfo, String&)**

Attempts to retrieve a localized template for the given FQDN and culture.

```csharp
bool TryGetTemplate(string fqdn, CultureInfo culture, String& template)
```

#### Parameters

`fqdn` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The fully qualified type name used as the template key (e.g. `"MyApp.Errors.InvalidEmailError"`).

`culture` [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>
The desired culture for the template lookup.

`template` [String&](https://docs.microsoft.com/en-us/dotnet/api/system.string&)<br>
When this method returns , contains the resolved template string
 with `{PropertyName}` placeholders. When , set to .

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
if a matching template was found; otherwise .
