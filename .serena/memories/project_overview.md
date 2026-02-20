# Bogoware.Localization - Project Overview

## Purpose
.NET localization library using FQDN-keyed templates with `{PropertyName}` placeholders for culture-aware string formatting. Includes JSON serialization support and ASP.NET Core integration.

## Current Version
v0.3.0 (published 2026-02-20)

## Tech Stack
- .NET 10 SDK (multi-targets net8.0 and net10.0)
- xUnit for testing (with AwesomeAssertions)
- System.Text.Json for serialization
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- `[LoggerMessage]` source generators for high-performance logging

## Key Commands
- `dotnet restore Bogoware.Localization.slnx`
- `dotnet build Bogoware.Localization.slnx`
- `dotnet test Bogoware.Localization.slnx`
- `dotnet pack src/Bogoware.Localization/Bogoware.Localization.csproj --configuration Release`
- `dotnet pack src/Bogoware.Localization.AspNetCore/Bogoware.Localization.AspNetCore.csproj --configuration Release`
- `cd docs && npm run build` - Docusaurus documentation site

## Structure
- `src/Bogoware.Localization/` - Core library source
- `src/Bogoware.Localization/Serialization/` - JSON serialization converters and modifiers
- `src/Bogoware.Localization.AspNetCore/` - ASP.NET Core integration package
- `samples/Bogoware.Localization.Sample.Api/` - Sample ASP.NET Core API
- `tests/Bogoware.Localization.Tests/` - Core library xUnit tests (92 tests)
- `tests/Bogoware.Localization.AspNetCore.Tests/` - ASP.NET Core integration tests (20 tests)
- `docs/` - Docusaurus documentation site

## Key Patterns
- Resolution chain: self-provider -> DI provider -> registry template -> fallback
- Culture fallback: exact -> parent -> invariant
- Three serialization modes: Auto, Explicit, Exhaustive
- ASP.NET Core: Two-layer approach (Layer 1: PostConfigure on JsonOptions, Layer 2: opt-in response buffering)
- `AddBogowareLocalization` / `UseBogowareLocalization` for ASP.NET Core setup

## DI Registration (Additive)
- `AddLocalization` is **additive**: multiple calls accumulate configuration delegates via `LocalizationRegistryConfigurator`; all delegates execute in order at resolution time
- Later templates override earlier ones on a per-key/per-culture basis
- Singletons (`ILocalizationRegistry`, `ILocalizationFormatter`) are registered only on the first call

## Assembly Scanning API
- `AddFromAssembly(Assembly, params string[] patterns)` - single assembly scan
- `AddFromAssemblyTree(Assembly, params string[] patterns)` - transitive references with topological ordering (dependencies first, root last)
- `AddFromLoadedAssemblies(string[] prefixes, params string[] patterns)` - AppDomain scan by name prefix
- `AddFromAssemblies(IEnumerable<Assembly>, params string[] patterns)` - explicit ordered list
- `AddFromFile(string path, CultureInfo culture)` - filesystem JSON file
- Default patterns: `["localized-messages", "error-messages"]`

## Logging
- `Log.cs` contains `[LoggerMessage]` source-generated methods (static partial class)
- All `Log.*` calls must be guarded with `if (logger is not null)` due to nullable `ILogger?` and source generator limitations
- `JsonLocalizationRegistry` accepts optional `ILogger?` constructor param; emits `TemplateOverride` warning on key conflicts
- `LoadFromJson` has a `source` parameter (`"assembly:Name:Resource"`, `"file:path"`, `"inline"`) for diagnostic tracking
