# Release Checklist

This checklist is the release gate for Karakol MVP candidates. The same commands are mirrored by `.github/workflows/ci.yml` and `.github/workflows/release.yml`.

## Version Gate

- Versioning uses SemVer.
- Git tags use `vMAJOR.MINOR.PATCH`, for example `v0.1.0`.
- `CHANGELOG.md` must include the release section before publishing.
- The release workflow accepts the package version without the leading `v`.

## Build Gate

```bash
dotnet build --no-restore
```

Expected result: `0 warning / 0 error`.

## Test Gate

```bash
dotnet test --no-restore
```

Expected result: all unit, integration, security, reporting, ML, persistence, and performance smoke tests pass.

## Sample Scan Gate

```bash
dotnet run --no-restore --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose
```

Expected result:

- CLI exits with code `0`.
- SQLi, XSS, path traversal, scanner bot, and sensitive file findings are shown or represented in reports.
- JSON and HTML reports are written to `reports/`.

## Packaging Gate

```bash
dotnet pack --no-restore src/Karakol.Cli/Karakol.Cli.csproj -o artifacts/packages
```

Expected result: a local `Karakol.Cli.0.1.0.nupkg` package is produced.

## Local Tool Smoke

```bash
dotnet tool install Karakol.Cli --add-source artifacts/packages --tool-path .tmp-karakol-tool
.tmp-karakol-tool/karakol version
```

Expected result: the installed tool prints version information. Remove `.tmp-karakol-tool` after the smoke check.

## CI Gate

The pull request CI must pass:

- restore
- build
- test
- pack
- sample scan
- package artifact upload
- sample report artifact upload

The CI matrix currently covers:

- `ubuntu-latest`
- `windows-latest`
- `macos-latest`

## Manual Release Workflow

Use `.github/workflows/release.yml`.

Inputs:

- `version`: release version without leading `v`.
- `publish_nuget`: when `true`, pushes package to NuGet.

Required repository secret:

- `NUGET_API_KEY`

NuGet publish is manual and opt-in. Keep `publish_nuget=false` for dry-run release candidates.

## Release Notes Gate

Each release note must state:

- Karakol is local-first.
- No external API, telemetry, or cloud dependency is used by the MVP.
- Reports may contain sensitive incident data and should be handled as confidential artifacts.
- ML is local-only and dummy/no-op unless an explicit local provider is added.
