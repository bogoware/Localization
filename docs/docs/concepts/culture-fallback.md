---
sidebar_position: 4
title: Culture Fallback
---

# Culture Fallback

When resolving a template from the registry, the localization system walks up the culture hierarchy until it finds a match.

## Fallback Chain

For a request with culture `pt-BR` (Portuguese, Brazil):

1. **Exact match** — look for `pt-BR`
2. **Parent culture** — look for `pt`
3. **Invariant culture** — look for the invariant (empty string) culture

If no template is found at any level, the resolution continues to the next step in the [provider chain](./provider-chain).

## Example

Given these templates:

```json
// localized-messages.en-US.json
{ "MyApp.Greeting": "Hello, {Name}!" }

// localized-messages.en.json
{ "MyApp.Greeting": "Hello, {Name}." }

// localized-messages.json (invariant)
{ "MyApp.Greeting": "Hello {Name}" }
```

| Requested Culture | Resolved Template |
|-------------------|-------------------|
| `en-US` | `"Hello, {Name}!"` |
| `en-GB` | `"Hello, {Name}."` (falls back to `en`) |
| `en` | `"Hello, {Name}."` |
| `fr-FR` | `"Hello {Name}"` (falls back to invariant) |

## Setting the Culture

The formatter uses `CultureInfo.CurrentUICulture` by default. You can also pass an explicit culture:

```csharp
var message = formatter.Format(error, new CultureInfo("it-IT"));
```
