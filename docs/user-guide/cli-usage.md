# Karakol CLI Usage

Karakol CLI is the first product surface for the local-first log analysis engine. The CLI is intentionally thin: it parses command-line settings, sends Application requests through MediatR, renders terminal output, and maps controlled failures to exit codes.

## Scan

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

Options:

- `--format`: `auto`, `nginx`, `apache`, `auth`, `ssh`, `json`, `csv`, or `generic`.
- `--report`: may be repeated; supported values are `json`, `html`, and `markdown`.
- `--output`: report directory. Default is `reports`.
- `--config`: optional `karakolsettings.json` path.
- `--policy`: optional `karakol.policy.json` path.
- `--max-lines`: limits streamed input processing.
- `--min-severity`: filters findings below `low`, `medium`, `high`, or `critical`.
- `--enable-ml`: enables the local ML stage; MVP uses dummy/no-op behavior unless a provider is registered.
- `--disable-rules`: disables single-event rules.
- `--disable-correlation`: disables correlation rules.
- `--mask-sensitive-data`: masks secrets in report output.
- `--include-raw-samples`: includes sanitized raw samples.
- `--verbose`: prints additional scan/report details.
- `--quiet`: suppresses normal summary output.

## Rules

```bash
dotnet run --project src/Karakol.Cli -- rules
```

Lists built-in rule metadata from the built-in plugin catalog.

## Formats

```bash
dotnet run --project src/Karakol.Cli -- formats
```

Lists parser formats available through runtime registration.

## Doctor

```bash
dotnet run --project src/Karakol.Cli -- doctor
```

Checks the local runtime surface such as report directory access and sample availability.

## Version

```bash
dotnet run --project src/Karakol.Cli -- version
```

Prints the CLI version from the packaged assembly metadata.

## Exit Codes

- `0`: scan succeeded without blocking failure.
- `1`: scan succeeded but a configured fail condition should block the caller.
- `2`: validation or configuration error.
- `3`: file read error.
- `4`: parser error.
- `5`: unexpected error.

## Local-First Contract

The CLI does not upload log lines, reports, metadata, or model inputs. All analysis is performed in-process against local files.
