---
sidebar_position: 1
title: JSON Templates
---

# Working with JSON Templates

This guide covers how to create, organize, and load JSON template files for localization.

## Template Structure

Each JSON file is a flat key-value dictionary:

```json
{
  "Namespace.ClassName": "Template with {PropertyName} placeholders"
}
```

Keys must match the `Type.FullName` of the localizable type exactly.

## JSONC Support

Template files support **JSONC** (JSON with Comments). You can use single-line and multi-line comments as well as trailing commas — useful for annotating templates by module, domain, or translation context:

```jsonc
{
  // ── Orders Module ──────────────────────────────────────
  "MyApp.Orders.OrderStatus": "Order #{OrderNumber} is {Status}",
  "MyApp.Orders.OrderShippedNotification": "Your order #{OrderNumber} has been shipped to {Address}",

  // ── Payments Module ────────────────────────────────────
  "MyApp.Payments.PaymentDeclined": "Payment of {Amount} was declined: {Reason}",
  "MyApp.Payments.RefundIssued": "A refund of {Amount} has been issued to {Method}",

  // ── Validation Errors ──────────────────────────────────
  // These are shared across modules
  "MyApp.Validation.RequiredFieldError": "'{FieldName}' is required",
  "MyApp.Validation.MaxLengthError": "'{FieldName}' must not exceed {MaxLength} characters",
  "MyApp.Validation.InvalidEmailError": "'{FieldName}' is not a valid email address",
}
```

## Organizing Templates

### By Culture

Create one file per culture:

```
Resources/
├── localized-messages.json         # Invariant (fallback)
├── localized-messages.en-US.json   # English (US)
├── localized-messages.it-IT.json   # Italian
└── localized-messages.de-DE.json   # German
```

### By Domain

For larger applications, you can split templates across multiple assemblies. Each assembly registers its own templates:

```csharp
// Scan both assemblies in a single call
services.AddLocalization(typeof(OrderError).Assembly, typeof(AuthError).Assembly);
```

### By Use Case with Separate Files

You can also organize templates into separate JSON files by use case within the same assembly — for example, separating user-facing messages from error messages:

```
Resources/
├── error-messages.json             # Validation and system errors
├── error-messages.it.json
├── localized-messages.json         # User-facing labels and notifications
└── localized-messages.it.json
```

`error-messages.json`:
```jsonc
{
  // Validation errors returned in ProblemDetails responses
  "MyApp.Validation.RequiredFieldError": "'{FieldName}' is required",
  "MyApp.Validation.MaxLengthError": "'{FieldName}' must not exceed {MaxLength} characters",

  // System errors
  "MyApp.System.ServiceUnavailable": "The {ServiceName} service is temporarily unavailable",
}
```

`localized-messages.json`:
```jsonc
{
  // Order status messages shown in API responses
  "MyApp.Orders.OrderStatus": "Order #{OrderNumber} is {Status}",
  "MyApp.Orders.OrderShipped": "Your order #{OrderNumber} has shipped",

  // Notification templates
  "MyApp.Notifications.WelcomeMessage": "Welcome, {UserName}!",
}
```

The default resource patterns (`localized-messages` and `error-messages`) are automatically discovered by `AddFromAssemblyResources`:

```csharp
// Loads both localized-messages.*.json and error-messages.*.json automatically
builder.AddFromAssemblyResources(typeof(Program).Assembly);
```

For custom file names, use `AddFromFile` to load them explicitly:

```csharp
services.AddLocalization(b =>
{
    // Custom file names loaded explicitly with their culture
    b.AddFromFile("Resources/order-messages.json", CultureInfo.InvariantCulture);
    b.AddFromFile("Resources/order-messages.it.json", new CultureInfo("it-IT"));
    b.AddFromFile("Resources/notification-messages.json", CultureInfo.InvariantCulture);
    b.AddFromFile("Resources/notification-messages.it.json", new CultureInfo("it-IT"));
});
```

Or with ASP.NET Core:

```csharp
builder.Services.AddBogowareLocalization(
    registry: b =>
    {
        // Default patterns (embedded resources)
        b.AddFromAssemblyResources(typeof(Program).Assembly);

        // Additional files loaded from disk
        b.AddFromFile("Resources/custom-messages.json", CultureInfo.InvariantCulture);
        b.AddFromFile("Resources/custom-messages.it.json", new CultureInfo("it-IT"));
    });
```

## Embedding Resources

Add the JSON files as embedded resources in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\**\*.json" WithCulture="false" />
</ItemGroup>
```

:::tip
Use `WithCulture="false"` to prevent MSBuild from routing culture-specific files into satellite assemblies. The localization library handles culture resolution internally.
:::

## Placeholder Syntax

Placeholders use the `{PropertyName}` syntax and are replaced with the corresponding public property values from the localizable type:

```csharp
public class TransferError(string from, string to, decimal amount) : ILocalizable
{
    public string From { get; } = from;
    public string To { get; } = to;
    public decimal Amount { get; } = amount;
}
```

```json
{
  "MyApp.Errors.TransferError": "Cannot transfer {Amount} from '{From}' to '{To}'"
}
```
