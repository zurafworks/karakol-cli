# Changelog

All notable changes to Karakol will be documented in this file.

The format follows a simple release-note structure: Added, Changed, Fixed, Security, and Known Limitations.

## 0.1.0 - Unreleased

### Added

- Local-first CLI scan flow.
- Nginx, Apache, SSH/Auth, JSON, CSV, and generic parser support.
- Rule-based detection for SQLi, XSS, path traversal, scanner bot, sensitive file access, suspicious user agents, and command injection.
- Bounded brute-force and basic DoS correlation.
- Risk scoring with component breakdown.
- JSON, Markdown, and offline HTML reports.
- Config and basic policy loading.
- Built-in plugin registry.
- Dummy local ML classifier abstraction.
- Persistence abstraction with disabled default behavior.
- Optional DuckDB scan history and analytics persistence provider.
- Serilog logging integration with console/file sinks and sensitive payload redaction safeguards.
- JSON Schema files for settings and policy editor support.
- CI workflow covering Windows, Ubuntu, and macOS.
- Packaging metadata for `dotnet tool` distribution.

### Security

- HTML output is encoded.
- Sensitive data masking is enabled for report output.
- No external API, cloud sync, telemetry, or remote model calls are used by the MVP.

### Known Limitations

- SQLite persistence implementation is post-MVP; DuckDB is the preferred local OLAP provider.
- External dynamic plugin loading is post-MVP.
- ONNX inference provider is prepared at abstraction level only.
- Real-world parser coverage should keep expanding before a 1.0 enterprise release.
