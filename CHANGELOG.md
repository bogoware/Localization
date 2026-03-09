# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.5.0] - 2026-03-09

### Added
- `JsonLocalizationRegistryBuilder.LoadFromJson` convenience method for loading raw JSON strings directly on the builder without bypassing it
- Circular reference detection in `LocalizationFormatter` — nested `ILocalizable` cycles now throw `LocalizationFormattingException` instead of causing a `StackOverflowException`

### Changed
- **Breaking:** `JsonLocalizationRegistryBuilder` is now single-use — calling `Build()` marks the builder as consumed; subsequent `Build()` or `AddFrom*` calls throw `InvalidOperationException`
- DI registration pattern replaced: `AddLocalization` no longer scans `ServiceDescriptor.ImplementationInstance` to find an internal configurator; instead, each call registers an individual action wrapper collected via `GetServices<>` at resolution time, making it resilient to DI container decoration and shimming

### Removed
- `LocalizationRegistryConfigurator` internal class (replaced by `LocalizationRegistryAction`)

## [0.4.0] - 2026-02-20

### Added
- Recursive/nested localization — when a property value implements `ILocalizable`, `FormatTemplate` and `BuildFallback` now format it through the full resolution chain instead of calling `ToString()`
- Culture propagation through nested `ILocalizable` formatting

### Changed
- `FormatTemplate` and `BuildFallback` converted from `internal static` to `private` instance methods (not part of the public API surface)

## [0.3.0] - 2026-02-20

### Added
- Additive `AddLocalization` — multiple calls now accumulate configuration delegates instead of replacing the registry; later templates override earlier ones on a per-key/per-culture basis
- `AddFromAssemblyTree` method for scanning an assembly and all its transitive referenced assemblies with topological ordering (dependencies first, root last)
- `[LoggerMessage]` source-generated logging throughout the library: resource loading, assembly scanning, template override warnings, and resolution chain tracing
- Override detection: `Warning`-level log when a template key is replaced, including source descriptor for diagnostics
- `source` parameter on `LoadFromJson` for tracking template origin (`assembly:Name:Resource`, `file:path`, `inline`)

### Changed
- **Breaking:** `AddFromAssemblyResources` renamed to `AddFromAssembly`
- **Breaking:** `AddFromLoadedAssemblies` signature changed from `params string[] prefixes` to `string[] prefixes, params string[] patterns`
- `JsonLocalizationRegistry` now accepts an optional `ILogger?` constructor parameter for diagnostic logging

## [0.2.0] - 2026-02-20

### Added
- New `Bogoware.Localization.AspNetCore` package — per-request culture resolution, automatic JSON response localization, and ProblemDetails support for ASP.NET Core applications
- CI/CD publishing for the AspNetCore package alongside the core library

## [0.1.1] - 2026-02-20

### Added
- Custom exception hierarchy (`LocalizationException`, `LocalizationConfigurationException`, `LocalizationSerializationException`)
- Struct `ILocalizable` support with `Nullable<T>` handling
- JSON serialization converters for nullable struct localizable types

### Fixed
- Cached reflection in template formatting for improved performance
- Deterministic enumerable handling in property extraction
- Culture validation for registry lookups

## [0.1.0] - 2026-02-19

### Added
- Initial release of Bogoware.Localization library
- FQDN-keyed template system mapping `Type.FullName` to localized format strings with `{PropertyName}` placeholders
- Culture fallback chain: exact culture → parent culture → invariant culture
- Provider resolution chain: self → DI → registry → fallback → default format
- JSON-based localization registry with embedded resource support
- In-memory localization registry for testing
- DI integration via `IServiceCollection` extension methods
- Comprehensive XML documentation

[Unreleased]: https://github.com/bogoware/Localization/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/bogoware/Localization/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/bogoware/Localization/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/bogoware/Localization/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/bogoware/Localization/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/bogoware/Localization/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/bogoware/Localization/releases/tag/v0.1.0
