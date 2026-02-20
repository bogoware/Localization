# Bogoware.Localization - Project Overview

## Purpose
.NET localization library using FQDN-keyed templates with `{PropertyName}` placeholders for culture-aware string formatting. Includes JSON serialization support.

## Tech Stack
- .NET 10 SDK (multi-targets net8.0 and net10.0)
- xUnit for testing
- System.Text.Json for serialization
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions

## Key Commands
- `dotnet restore Bogoware.Localization.slnx`
- `dotnet build Bogoware.Localization.slnx`
- `dotnet test Bogoware.Localization.slnx`
- `dotnet pack src/Bogoware.Localization/Bogoware.Localization.csproj --configuration Release`

## Structure
- `src/Bogoware.Localization/` - Core library source
- `src/Bogoware.Localization/Serialization/` - JSON serialization converters and modifiers
- `src/Bogoware.Localization.AspNetCore/` - ASP.NET Core integration package
- `samples/Bogoware.Localization.Sample.Api/` - Sample ASP.NET Core API
- `tests/Bogoware.Localization.Tests/` - Core library xUnit tests
- `tests/Bogoware.Localization.AspNetCore.Tests/` - ASP.NET Core integration tests
- `docs/` - Docusaurus documentation site

## Key Patterns
- Resolution chain: self-provider -> DI provider -> registry template -> fallback
- Culture fallback: exact -> parent -> invariant
- Three serialization modes: Auto, Explicit, Exhaustive
- ASP.NET Core: Two-layer approach (Layer 1: PostConfigure on JsonOptions, Layer 2: opt-in response buffering)
- `AddBogowareLocalization` / `UseBogowareLocalization` for ASP.NET Core setup
