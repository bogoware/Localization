# Bogoware.Localization — Claude Code Guide

## Build / Test / Pack

```bash
dotnet restore Bogoware.Localization.slnx
dotnet build Bogoware.Localization.slnx
dotnet test Bogoware.Localization.slnx
dotnet pack src/Bogoware.Localization/Bogoware.Localization.csproj --configuration Release
```

SDK version is pinned in `global.json` (.NET 10). Multi-targets `net8.0` and `net10.0`.

## Architecture

Single library + tests:

```
Localization/
├── src/Bogoware.Localization/       # Library source
├── tests/Bogoware.Localization.Tests/ # xUnit tests
├── Bogoware.Localization.slnx
├── Directory.Build.props            # Multi-target net8.0;net10.0
└── Directory.Packages.props         # Central package management
```

## Key Patterns

- **FQDN-keyed templates**: `Type.FullName` maps to localized format strings with `{PropertyName}` placeholders
- **Resolution chain**: self-provider → DI provider → registry template → fallback message → `TypeName(Prop=val)`
- **Culture fallback**: exact culture → parent culture → invariant culture
- **No DDD dependencies**: only `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Abstractions`

## Conventions

- Namespace: `Bogoware.Localization`
- Central package management: versions only in `Directory.Packages.props`
- Test embedded resources use `WithCulture="false"` to prevent MSBuild satellite assembly routing
- Package ID: `Bogoware.Localization`, published to NuGet.org on `v*` tags
