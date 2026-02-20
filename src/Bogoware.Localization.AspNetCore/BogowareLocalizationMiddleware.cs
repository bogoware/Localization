using System.Text.Json;
using Bogoware.Localization.AspNetCore.Diagnostics;
using Bogoware.Localization.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bogoware.Localization.AspNetCore;

/// <summary>
/// Layer 2 response-buffering middleware that intercepts JSON responses
/// and re-serializes them with localization applied.
/// <para>
/// Most applications do <strong>not</strong> need this middleware — the standard
/// <c>IPostConfigureOptions&lt;JsonOptions&gt;</c> (Layer 1) handles localization
/// transparently for minimal APIs and MVC. Enable this only for edge cases
/// where raw JSON is written outside the standard pipeline.
/// </para>
/// </summary>
internal sealed class BogowareLocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BogowareLocalizationMiddlewareOptions _options;
    private readonly JsonSerializerOptions _serializerOptions;

    public BogowareLocalizationMiddleware(
        RequestDelegate next,
        ILocalizationFormatter formatter,
        IOptions<BogowareLocalizationMiddlewareOptions> options)
    {
        _next = next;
        _options = options.Value;
        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _serializerOptions.AddLocalization(formatter, _options.SerializationMode, culture: null);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (!ShouldProcess(path))
        {
            await _next(context);
            return;
        }

        using var activity = _options.EnableDiagnostics
            ? BogowareLocalizationActivitySource.StartResponse(path)
            : null;

        // Buffer the response body
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            buffer.Seek(0, SeekOrigin.Begin);

            // Only process JSON responses
            var contentType = context.Response.ContentType;
            if (contentType is not null && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var doc = await JsonDocument.ParseAsync(buffer);

                    context.Response.Body = originalBody;
                    // Clear content-length since re-serialization may change size
                    context.Response.Headers.Remove("Content-Length");
                    await JsonSerializer.SerializeAsync(originalBody, doc.RootElement, _serializerOptions);
                }
                catch (JsonException)
                {
                    // Not valid JSON — pass through as-is
                    context.Response.Body = originalBody;
                    buffer.Seek(0, SeekOrigin.Begin);
                    await buffer.CopyToAsync(originalBody);
                }
            }
            else
            {
                context.Response.Body = originalBody;
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private bool ShouldProcess(string path)
    {
        // Check excludes first (takes precedence)
        foreach (var exclude in _options.ExcludePaths)
        {
            if (path.StartsWith(exclude, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // If includes are specified, path must match at least one
        if (_options.IncludePaths.Count > 0)
        {
            foreach (var include in _options.IncludePaths)
            {
                if (path.StartsWith(include, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        return true;
    }
}
