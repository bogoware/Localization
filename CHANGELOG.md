# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.1] - 2026-02-20

### Added
- Custom exception hierarchy (`LocalizationException`, `LocalizationConfigurationException`, `LocalizationSerializationException`)
- Struct `ILocalizable` support with `Nullable<T>` handling
- JSON serialization converters for nullable struct localizable types
- Struct attribute interaction and edge case tests (61 total)

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

[Unreleased]: https://github.com/bogoware/Localization/compare/v0.1.1...HEAD
[0.1.1]: https://github.com/bogoware/Localization/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/bogoware/Localization/releases/tag/v0.1.0
