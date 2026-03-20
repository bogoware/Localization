# Bogoware.Localization — Claude Code Guide

## Build / Test / Pack

```bash
dotnet restore Bogoware.Localization.slnx
dotnet build Bogoware.Localization.slnx
dotnet test Bogoware.Localization.slnx
dotnet pack src/Bogoware.Localization/Bogoware.Localization.csproj --configuration Release
dotnet pack src/Bogoware.Localization.AspNetCore/Bogoware.Localization.AspNetCore.csproj --configuration Release
```

SDK pinned in `global.json` (.NET 10.0.102, rollForward: latestFeature). Multi-targets `net8.0` + `net10.0`.

## Architecture

```
Localization/
├── src/Bogoware.Localization/              # Core library (NuGet: Bogoware.Localization)
│   └── Serialization/                      # STJ converters, modifiers, attributes
├── src/Bogoware.Localization.AspNetCore/   # ASP.NET Core integration (NuGet)
├── tests/Bogoware.Localization.Tests/      # Core xUnit unit tests
├── tests/Bogoware.Localization.AspNetCore.Tests/ # Integration tests (WebApplicationFactory)
├── samples/Bogoware.Localization.Sample.Api/     # Sample Minimal API (also integration test host)
├── website/                                # Astro/Starlight documentation site
├── scripts/                                # generate-api-docs.sh, sync-readme.js
├── .github/workflows/                      # build.yml, publish.yml, docs.yml
├── Directory.Build.props                   # Multi-target, nullable, implicit usings, XML docs
└── Directory.Packages.props                # Central package management (all versions here)
```

## Key Patterns

- **FQDN-keyed templates**: `Type.FullName` maps to localized format strings with `{PropertyName}` placeholders
- **Resolution chain (immutable order)**: self-provider → DI provider → registry template → fallback `TypeName(Prop=val)` — never reorder or skip
- **Culture fallback (3-tier)**: exact culture → parent culture → invariant culture — built into `TryGetTemplate`, don't reimplement
- **Additive DI**: multiple `AddLocalization()` calls accumulate via `GetServices<>` — later templates override per key/culture
- **Nested localization**: `ILocalizable` properties formatted recursively through full chain (not `ToString()`)
- **Single-use builder**: `JsonLocalizationRegistryBuilder.Build()` consumes it; reuse throws `InvalidOperationException`
- **Circular reference detection**: nested `ILocalizable` cycles throw `LocalizationFormattingException`
- **ASP.NET Core two layers**: Layer 1 (default) = `IPostConfigureOptions<JsonOptions>` for Minimal API + MVC; Layer 2 (opt-in) = response buffering middleware
- **Serialization modes**: `Explicit` | `Auto` (default) | `Exhaustive` — `[DoNotLocalize]` always wins

## Conventions

- Namespaces: `Bogoware.Localization`, `Bogoware.Localization.AspNetCore`
- Central package management: versions only in `Directory.Packages.props`
- `.csproj` files use `0.0.0-local` — real version injected from git tag via `/p:Version`
- Test embedded resources use `WithCulture="false"` to prevent MSBuild satellite assembly routing
- Naming: `I` prefix (interfaces), `*Builder` suffix, `*Extensions` suffix, `Bogoware*` prefix (ASP.NET types), `*JsonConverter` suffix
- Logging: `[LoggerMessage]` source-generated methods in `Log.cs` only — never string interpolation
- Custom exceptions: `LocalizationConfigurationException`, `LocalizationFormattingException`, `LocalizationSerializationException` — never bare `Exception`
- JSON templates: `*.messages.{culture}.json`; invariant = no culture segment; JSONC supported
- One file per type; serialization in `Serialization/` subfolder
- EditorConfig: C# 4-space indent CRLF; project files 2-space indent CRLF

## Testing

- **Core tests**: direct instantiation with embedded JSON resources, `AwesomeAssertions` fluent syntax (`.Should()`)
- **Integration tests**: `WebApplicationFactory` with `Sample.Api` as test host — don't create separate web apps
- **Scenario-based**: each setup variant gets its own folder + fixture in `Fixtures/`
- Shared test types in `Helpers/TestTypes.cs`
- `[Fact]` for single-case, `[Theory]` for parameterized
- Must pass on both `net8.0` and `net10.0`

## Branch Model & CI/CD

- **`rel/prod`** is the production branch (NOT `main` or `master`)
- Feature branches: `feat/*`, `fix/*`, `refactor/*`
- **`build.yml`**: CI on push/PR to `rel/*` — restore → build → test (both TFMs)
- **`publish.yml`**: On `v*` tag or GitHub Release — build → test → pack → Meziantou validate → NuGet push (pwsh shell, version from git tag)
- **`docs.yml`**: On `rel/prod` push, release, or manual — .NET build → xmldoc2md (net8.0 DLL only) → changelog sync → Astro/Starlight build → GitHub Pages deploy
- Docs site: `https://bogoware.github.io/Localization/` — uses `@bogoware/starlight-theme` (architect mode)
- Docs build: `cd website && pnpm install && pnpm build`
- API docs auto-generated to `website/src/content/docs/api/`
- Starlight frontmatter uses `sidebar: { order: N }` (not Docusaurus `sidebar_position`)
- Docs must build cleanly on every `rel/prod` push — broken docs block the site

## Post-Task Review

Every development task MUST conclude with the following review agents run in parallel:

1. **Code Simplification & Refactoring Review, No Compile Warnings** — Review source code for simplification opportunities, unnecessary complexity, and refactoring suggestions, fix all compile warnings (`feature-dev:code-reviewer` agent scoped to `src/`)
2. **Documentation Coherence & Coverage Review** — Verify documentation covers new/modified features, cross-references are consistent, and changelog is updated (`pr-review-toolkit:comment-analyzer` agent scoped to `docs/` + `README.md`)
3. **Test QA Review** — Assess test quality, coverage gaps, missing edge cases, and confirm all tests pass (`pr-review-toolkit:pr-test-analyzer` agent scoped to `tests/`)

## CHANGELOG Policy

`CHANGELOG.md` tracks only changes that affect the **library itself** (API, behavior, dependencies, bug fixes). Do NOT add entries for:
- Documentation changes (README, docs site, XML comments)
- CI/CD workflow changes (GitHub Actions, build scripts)
- Test-only changes (new tests, test refactors)
- Cosmetic or formatting changes
- Developer tooling or project configuration

## Pre-Release Checklist

Before tagging a new version (`v*`) for publication, ALL of the following MUST be completed:

1. **Update CHANGELOG.md** — Move items from `[Unreleased]` into a new version section following [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format. Include the release date. Update the comparison links at the bottom of the file. Only include library-affecting changes per the CHANGELOG Policy above.
2. **All tests pass** — `dotnet test Bogoware.Localization.slnx` must report 0 failures on both target frameworks.
3. **Zero compile warnings** — `dotnet build Bogoware.Localization.slnx` must produce 0 warnings (NuGet source mapping warnings NU1507 are excluded).
4. **Post-task reviews completed** — The three review agents (code, docs, tests) from the Post-Task Review section must have run and their findings addressed.
5. **Version tag follows SemVer** — Tag format is `vMAJOR.MINOR.PATCH`. Bump rules:
   - **Patch**: bug fixes, test-only changes, documentation
   - **Minor**: new features, backward-compatible additions
   - **Major**: breaking API changes
