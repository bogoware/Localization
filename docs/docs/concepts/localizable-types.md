---
sidebar_position: 1
title: Localizable Types
---

# Localizable Types

The `ILocalizable` interface is a marker interface that opts a type into the localization system. Any class implementing `ILocalizable` can be formatted by the `ILocalizationFormatter`.

## Defining a Localizable Type

```csharp
using Bogoware.Localization;

public class InsufficientFundsError(decimal amount, decimal balance) : ILocalizable
{
    public decimal Amount { get; } = amount;
    public decimal Balance { get; } = balance;
}
```

The type's public properties are used as placeholders in localization templates. The template key is the type's `FullName` (namespace + class name).

## Template Mapping

Given the type `MyApp.Errors.InsufficientFundsError`, the registry looks up the key `MyApp.Errors.InsufficientFundsError` and substitutes `{Amount}` and `{Balance}` with the instance's property values.

**Template:**
```json
{
  "MyApp.Errors.InsufficientFundsError": "Cannot withdraw {Amount}: balance is only {Balance}"
}
```

**Output:** `"Cannot withdraw 150.00: balance is only 42.50"`

## Self-Providing Localization

A type can also implement `ILocalizationProvider` to provide its own localized string, bypassing the registry entirely:

```csharp
public class CustomError : ILocalizable, ILocalizationProvider
{
    public string Localize(CultureInfo culture)
    {
        return culture.Name switch
        {
            "it-IT" => "Errore personalizzato",
            _ => "Custom error"
        };
    }
}
```

This is the highest priority in the [provider resolution chain](./provider-chain).
