# Karakol Architecture

Karakol follows a Clean Architecture oriented modular monolith. The CLI is an adapter; parsing, detection, correlation, risk scoring, reporting, and ML abstractions live in separate projects.

The first executable flow is:

```text
CLI -> ScanLogFileCommand -> Parser -> Detection Rules -> Correlation -> Risk -> Reports -> Terminal Summary
```

See `docs/planning/karakol-mvp-operasyon-plani.md` for the detailed operational plan.

## Current Source Organization

The codebase now avoids catch-all `Core.cs` files. Each subsystem is split by responsibility:

- `Karakol.Domain`: enums, IDs, events, findings, risk, scans, sources, and value objects.
- `Karakol.Application`: scan command, handler, validation helpers, runtime composition, and scan orchestration helpers.
- `Karakol.Parsing`: input reader, parser abstractions, format detection, parser registry, and parser implementations.
- `Karakol.Detection`: rule abstractions, execution context, rule registry, single-event engine, and rule families.
- `Karakol.Correlation`: correlation engine and correlation rules.
- `Karakol.Risk`: risk scoring abstractions and default implementation.
- `Karakol.Reporting`: report abstractions, registry, sanitization, and writer implementations.
- `Karakol.Cli`: thin CLI bootstrap, commands, settings, exit-code mapping, and renderers.

## Current Reference Graph

The current MVP keeps a temporary CLI composition root while package-free bootstrapping remains in place:

```text
Karakol.Cli
  -> Karakol.Application
  -> Karakol.Contracts
  -> Karakol.Infrastructure
  -> Karakol.Parsing
  -> Karakol.Detection
  -> Karakol.Correlation
  -> Karakol.Risk
  -> Karakol.Reporting
  -> Karakol.ML

Karakol.Infrastructure
  -> Karakol.Application
  -> Karakol.Domain
  -> Karakol.Parsing
  -> Karakol.Shared

Karakol.Application
  -> Karakol.Domain
  -> Karakol.Contracts
  -> Karakol.Parsing
  -> Karakol.Detection
  -> Karakol.Correlation
  -> Karakol.Risk
  -> Karakol.Reporting
  -> Karakol.ML
  -> Karakol.Shared
```

Important boundary note: `Karakol.Application` no longer references `Karakol.Infrastructure`. File metadata access is represented by `ILogSourceFactory` in Application and implemented by Infrastructure.

`Karakol.Cli.Composition.KarakolRuntime` is intentionally temporary. It should be replaced by `Microsoft.Extensions.DependencyInjection` registrations during the MediatR/Spectre integration phase.
