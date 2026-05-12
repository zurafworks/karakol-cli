# Karakol Logging

Karakol uses Serilog through `Microsoft.Extensions.Logging`. Logging is local-only and never sends telemetry, logs, reports, or scan data to an external service.

## Default Behavior

Default CLI mode keeps terminal output focused on the scan summary and report paths. Diagnostic logs are suppressed unless a warning or error occurs.

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

## Verbose Mode

`--verbose` raises the diagnostic log level to `Information`. Application pipeline events include request start and completion records, but they do not serialize command payloads or config/policy contents.

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report json --verbose
```

## Quiet Mode

`--quiet` suppresses the terminal summary while preserving controlled error output. It is intended for automation where the exit code and generated reports are the primary outputs.

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report json --quiet
```

## Optional File Logging

File logging is opt-in through the `KARAKOL_LOG_FILE` environment variable. This writes diagnostic application logs only; raw log lines are not persisted by the logger.

PowerShell:

```powershell
$env:KARAKOL_LOG_FILE = ".\reports\karakol.log"
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report json --verbose
```

Shell:

```bash
KARAKOL_LOG_FILE=./reports/karakol.log dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report json --verbose
```

## Redaction Rules

Logging rules:

- Do not log command objects by value.
- Do not log config or policy JSON contents.
- Do not log raw log lines.
- Do not log credentials, bearer tokens, API keys, session IDs, or query string secrets.
- Prefer request type, duration, result state, and controlled error codes.

These rules are enforced by pipeline behavior tests that check sensitive command values are not written to logs.
