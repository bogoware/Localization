---
title: ASP.NET Core Integration
sidebar:
  order: 4
---

# ASP.NET Core Integration

The `Bogoware.Localization.AspNetCore` package wires localization into the ASP.NET Core pipeline transparently. JSON responses containing `ILocalizable` properties are automatically localized based on the request's culture, with zero per-request overhead.

## Architecture

```
Request -> [UseRequestLocalization] -> [Endpoint] -> [JsonSerializer with culture:null] -> Response
                |                                           |
    Sets CurrentUICulture from                  Reads CurrentUICulture at
    Accept-Language/query/cookie                serialize time -> localized output
```

**Layer 1 (primary, zero-overhead):** `JsonSerializerOptions` is configured at startup with `culture: null`. All JSON serialization automatically localizes `ILocalizable` properties using `CultureInfo.CurrentUICulture` at serialize time.

**Layer 2 (opt-in):** Response-buffering middleware for edge cases where raw JSON is written outside the standard pipeline.

## Installation

```bash
dotnet add package Bogoware.Localization.AspNetCore
```

## Quick Start

```csharp
using Bogoware.Localization.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBogowareLocalization(typeof(Program).Assembly);

var app = builder.Build();
app.UseBogowareLocalization();

app.MapGet("/api/orders/{id}", (int id) =>
    Results.Ok(new OrderResponse(id, new OrderStatus(id, "Shipped"), 42.99m)));

app.Run();
```

```bash
# English (default)
curl -H "Accept-Language: en-US" http://localhost:5000/api/orders/42
# -> {"id":42,"status":"Order #42 is Shipped","total":42.99}

# Italian
curl -H "Accept-Language: it-IT" http://localhost:5000/api/orders/42
# -> {"id":42,"status":"L'ordine #42 e Shipped","total":42.99}
```

## Configuration Reference

### `BogowareLocalizationMiddlewareOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SerializationMode` | `LocalizationSerializationMode` | `Auto` | Controls which properties get localized (see [JSON Serialization](./json-serialization#serialization-modes)) |
| `DefaultCulture` | `CultureInfo` | `InvariantCulture` | Fallback culture when no culture is determined from the request |
| `SupportedCultures` | `List<CultureInfo>` | `[]` | Cultures the application supports. Used to configure `RequestLocalizationOptions` |
| `LocalizeProblemDetails` | `bool` | `true` | Localize `ILocalizable` values in `ProblemDetails.Extensions` |
| `EnableDiagnostics` | `bool` | `false` | Enable OpenTelemetry-compatible `ActivitySource` spans |
| `EnableResponseBuffering` | `bool` | `false` | Enable Layer 2 response-buffering middleware |
| `IncludePaths` | `List<string>` | `[]` | Path prefixes for response buffering (empty = all paths) |
| `ExcludePaths` | `List<string>` | `[]` | Path prefixes to skip (takes precedence over `IncludePaths`) |

### Full Customization

```csharp
builder.Services.AddBogowareLocalization(
    registry: b =>
    {
        b.AddFromAssembly(typeof(Program).Assembly);
        b.AddFromFile("path/to/custom-messages.json", new CultureInfo("en-US"));
    },
    middleware: options =>
    {
        options.SerializationMode = LocalizationSerializationMode.Auto;
        options.DefaultCulture = new CultureInfo("en-US");
        options.SupportedCultures.AddRange([
            new CultureInfo("en-US"),
            new CultureInfo("it-IT"),
        ]);
        options.LocalizeProblemDetails = true;
        options.EnableDiagnostics = true;
    });
```

## Registry Configuration

The `registry:` parameter exposes the same `JsonLocalizationRegistryBuilder` from the core library:

```csharp
builder.Services.AddBogowareLocalization(
    registry: b =>
    {
        // Scan embedded resources from one or more assemblies
        b.AddFromAssembly(typeof(Program).Assembly);

        // Load additional JSON files manually
        b.AddFromFile("Resources/custom-messages.json", CultureInfo.InvariantCulture);
        b.AddFromFile("Resources/custom-messages.it.json", new CultureInfo("it-IT"));

        // Scan all loaded assemblies with prefix filter
        b.AddFromLoadedAssemblies(["MyCompany."]);
    });
```

See [JSON Templates](./json-templates) for the template file format.

## Culture Resolution

Culture is resolved using ASP.NET Core's built-in `UseRequestLocalization()` middleware. The providers are consulted in order:

1. **Query string** — `?culture=it-IT`
2. **Cookie** — `.AspNetCore.Culture`
3. **Accept-Language header** — `Accept-Language: it-IT`

If no provider matches a supported culture, `DefaultCulture` is used.

```csharp
options.DefaultCulture = new CultureInfo("en-US");
options.SupportedCultures.AddRange([
    new CultureInfo("en-US"),
    new CultureInfo("it-IT"),
    new CultureInfo("de-DE"),
]);
```

## ProblemDetails Integration

When `LocalizeProblemDetails` is `true` (the default), `ILocalizable` values in `ProblemDetails.Extensions` are automatically formatted:

```csharp
app.MapPost("/api/orders", () =>
{
    var errors = new Dictionary<string, object?>
    {
        ["errors"] = new ILocalizable[]
        {
            new RequiredFieldError("CustomerName"),
            new InvalidEmailError("Email"),
        }
    };

    return Results.Problem(
        title: "Validation failed",
        statusCode: 400,
        extensions: errors);
});
```

```bash
curl -X POST -H "Accept-Language: it-IT" http://localhost:5000/api/orders
```

```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": [
    "'CustomerName' e obbligatorio",
    "'Email' non e un indirizzo email valido"
  ]
}
```

:::note
Title and Detail remain developer-controlled — only `Extensions` values are localized.
:::

## Coexistence with Framework Localization

Both systems share the same culture resolution through `RequestLocalizationOptions`:

```csharp
// Framework localization (IStringLocalizer<T>)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Bogoware localization (ILocalizationFormatter)
builder.Services.AddBogowareLocalization(typeof(Program).Assembly);

var app = builder.Build();
app.UseBogowareLocalization(); // calls UseRequestLocalization() — shared by both
```

Both `IStringLocalizer<T>` and `ILocalizationFormatter` resolve the same `CurrentUICulture` per request.

## Diagnostics & Tracing

Enable OpenTelemetry-compatible diagnostics:

```csharp
options.EnableDiagnostics = true;
```

Wire with OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(BogowareLocalizationActivitySource.SourceName));
```

**Activity Source:** `Bogoware.Localization.AspNetCore`

| Operation Name | Tags |
|---|---|
| `bogoware.localization.culture_resolution` | `bogoware.localization.culture`, `bogoware.localization.culture_source` |
| `bogoware.localization.response` | `bogoware.localization.path` |

## Response Buffering (Advanced)

For edge cases where raw JSON is written outside the standard pipeline, enable Layer 2:

```csharp
options.EnableResponseBuffering = true;
options.IncludePaths.Add("/api/");
options.ExcludePaths.Add("/api/health");
```

:::caution
Response buffering adds per-request overhead (read, parse, re-serialize). Most applications should NOT need this — Layer 1 (PostConfigure on `JsonOptions`) handles standard minimal API and MVC responses automatically.
:::

## Complete Example

```csharp
using System.Globalization;
using Bogoware.Localization;
using Bogoware.Localization.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBogowareLocalization(
    registry: b => b.AddFromAssembly(typeof(Program).Assembly),
    middleware: options =>
    {
        options.DefaultCulture = new CultureInfo("en-US");
        options.SupportedCultures.AddRange([
            new CultureInfo("en-US"),
            new CultureInfo("it-IT"),
        ]);
    });

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseBogowareLocalization();

app.MapGet("/api/orders/{id:int}", (int id) =>
    Results.Ok(new OrderResponse(id, new OrderStatus(id, "Shipped"), 42.99m)));

app.Run();

// Models
namespace MyApp;
public record OrderStatus(int OrderNumber, string Status) : ILocalizable;
public record OrderResponse(int Id, OrderStatus Status, decimal Total);
```

### Embedded Resources

`Resources/localized-messages.json` (invariant):
```json
{
  "MyApp.OrderStatus": "Order #{OrderNumber} is {Status}"
}
```

`Resources/localized-messages.it.json`:
```json
{
  "MyApp.OrderStatus": "L'ordine #{OrderNumber} e {Status}"
}
```
