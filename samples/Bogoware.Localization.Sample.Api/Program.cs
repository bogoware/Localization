using System.Globalization;
using Bogoware.Localization;
using Bogoware.Localization.AspNetCore;
using Bogoware.Localization.Sample.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Bogoware Localization Setup ──────────────────────────────────────
builder.Services.AddBogowareLocalization(
    registry: b => b.AddFromAssembly(typeof(Program).Assembly),
    middleware: options =>
    {
        options.DefaultCulture = new CultureInfo("en-US");
        options.SupportedCultures.AddRange([
            new CultureInfo("en-US"),
            new CultureInfo("it-IT"),
        ]);
        options.LocalizeProblemDetails = true;
    });

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseBogowareLocalization();

// ── Endpoints ────────────────────────────────────────────────────────

app.MapGet("/api/orders/{id:int}", (int id) =>
{
    var order = new OrderResponse(
        Id: id,
        Status: new OrderStatus(id, "Shipped"),
        Total: 42.99m);

    return Results.Ok(order);
});

app.MapPost("/api/orders", (OrderResponse? _) =>
{
    // Simulate validation errors
    var errors = new Dictionary<string, object?>
    {
        ["errors"] = new ILocalizable[]
        {
            new RequiredFieldError("CustomerName"),
            new InvalidEmailError("Email"),
            new MaxLengthError("Notes", 500),
        }
    };

    return Results.Problem(
        title: "Validation failed",
        statusCode: StatusCodes.Status400BadRequest,
        extensions: errors);
});

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/api/text", () => Results.Text("plain text response"));

app.Run();

// Make Program accessible for WebApplicationFactory in tests
public partial class Program;
