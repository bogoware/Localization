---
title: Nested Localization
sidebar:
  order: 6
---

# Nested Localization

When a localizable type has a property that is itself `ILocalizable`, the formatter recurses automatically — each nested value goes through the full resolution chain.

## Example: Payment Validation

Define two error types where one references the other:

```csharp
public class InvalidCurrencyError(string currencyCode) : ILocalizable
{
    public string CurrencyCode { get; } = currencyCode;
}

public class PaymentValidationError(
    string fieldName,
    InvalidCurrencyError detail) : ILocalizable
{
    public string FieldName { get; } = fieldName;
    public InvalidCurrencyError Detail { get; } = detail;
}
```

Register templates in your JSON resource:

```json
{
  "MyApp.InvalidCurrencyError": "'{CurrencyCode}' is not a valid currency",
  "MyApp.PaymentValidationError": "Payment field '{FieldName}' is invalid: {Detail}"
}
```

Format the outer error:

```csharp
var error = new PaymentValidationError("Amount", new InvalidCurrencyError("XYZ"));
var message = formatter.Format(error);
// -> "Payment field 'Amount' is invalid: 'XYZ' is not a valid currency"
```

The `{Detail}` placeholder is resolved by formatting the nested `InvalidCurrencyError` through the same resolution chain.

## How It Works

When the formatter encounters a `{PropertyName}` placeholder whose value implements `ILocalizable`, it calls `Format()` recursively instead of `ToString()`. The full [resolution chain](../concepts/provider-chain) applies to each nested value:

1. Self-provider (`ILocalizationProvider`)
2. DI provider (`ILocalizationProvider<T>`)
3. Registry template
4. Fallback (`TypeName(Prop=val)`)

## Fallback Behavior

If the inner type has no template, the fallback representation is used:

```csharp
// No template registered for InvalidCurrencyError
formatter.Format(error);
// -> "Payment field 'Amount' is invalid: InvalidCurrencyError(CurrencyCode=XYZ)"
```

The fallback itself also recurses — if a fallback property is `ILocalizable`, it gets formatted through the resolution chain too.

:::tip
Culture is propagated through the entire nesting chain. A single `Format(error, culture)` call ensures every nested `ILocalizable` is formatted with the same culture.
:::
