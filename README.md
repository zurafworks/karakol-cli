# Karakol

Karakol is a local-first cybersecurity log analysis CLI. It parses server, application, and security logs locally; detects suspicious activity; calculates risk; and writes terminal, JSON, Markdown, and offline HTML reports without sending log data to external services.

## Features

- .NET 9 modular monolith with Clean Architecture boundaries.
- MediatR-based CQRS scan flow.
- Nginx, Apache, SSH/Auth, JSON, CSV, and generic parser support.
- SQLi, XSS, path traversal, scanner bot, sensitive file, suspicious user-agent, and command injection rules.
- Brute-force and basic DoS correlation.
- Risk scoring with component breakdown.
- Config and policy loading.
- Sensitive data masking and HTML encoding.
- Professional single-file offline HTML reports with the Karakol logo embedded.
- Built-in plugin catalog for parsers, rules, reporters, and dummy ML.

## Requirements

- .NET SDK 9.x

## Build

```bash
dotnet restore
dotnet build --no-restore
```

## Test

```bash
dotnet test --no-restore
```

The test suite covers domain invariants, parsers, detection rules, correlation, risk scoring, reporting, config/policy behavior, CLI integration, ML preparation, persistence defaults, and performance smoke scenarios.

## Run

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose
```

Useful commands:

```bash
dotnet run --project src/Karakol.Cli -- rules
dotnet run --project src/Karakol.Cli -- formats
dotnet run --project src/Karakol.Cli -- doctor
dotnet run --project src/Karakol.Cli -- version
```

Config and policy examples are available under `samples/config/`.

## Package

```bash
dotnet pack --no-restore src/Karakol.Cli/Karakol.Cli.csproj -o artifacts/packages
dotnet tool install Karakol.Cli --add-source artifacts/packages --tool-path .tmp-karakol-tool
.tmp-karakol-tool/karakol version
```

Karakol's CLI package is prepared as a `dotnet tool` with command name `karakol`.

## Documentation

Start with [docs/README.md](docs/README.md).

Key documents:

- [Architecture](docs/architecture/architecture.md)
- [Commercial Readiness 10/10 Plan](docs/planning/commercial-readiness-10-10-plan.md)
- [Commercial Readiness Sprint Roadmap](docs/planning/commercial-readiness-sprint-roadmap.md)
- [CLI Usage](docs/user-guide/cli-usage.md)
- [Supported Log Formats](docs/user-guide/supported-log-formats.md)
- [Detection Rules](docs/user-guide/detection-rules.md)
- [Config and Policy Schema](docs/configuration/config-policy-schema.md)
- [DuckDB OLAP Plan](docs/data/duckdb-olap-plan.md)
- [Security Considerations](docs/security/security-considerations.md)
- [Release Checklist](docs/release/release-checklist.md)
- [Installer and Distribution](docs/release/installer-distribution.md)

## Open Source

- License: MIT
- Contributions: see [CONTRIBUTING.md](CONTRIBUTING.md)
- Code of conduct: see [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
- Security policy: see [SECURITY.md](SECURITY.md)

The package metadata currently points to `karakol-security/karakol`. Update the repository owner before publishing if the final GitHub organization or user differs.

## Local-First Guarantee

Karakol does not upload log files, report contents, metadata, or model inputs. MVP ML support is represented by local abstractions and a dummy classifier; no remote model or external API is called.
