# Karakol Documentation

This directory is split by context so contributors can find the right document without scanning unrelated planning material.

## Structure

```text
docs/
  README.md
  architecture/
    architecture.md
    adr/
      0001-local-first-boundary.md
      0002-duckdb-as-optional-olap.md
      0003-cli-as-thin-adapter.md
  planning/
    commercial-readiness-10-10-plan.md
    commercial-readiness-sprint-roadmap.md
    karakol-mvp-operasyon-plani.md
    karakol-kalan-isler-sprint-plani.md
  user-guide/
    cli-usage.md
    detection-rules.md
    report-schema.md
    risk-scoring.md
    supported-log-formats.md
  configuration/
    config-policy-schema.md
  data/
    duckdb-olap-plan.md
  operations/
    logging.md
    performance.md
  security/
    security-considerations.md
  release/
    installer-distribution.md
    release-checklist.md
```

## Reading Path

For users:

1. [CLI Usage](user-guide/cli-usage.md)
2. [Supported Log Formats](user-guide/supported-log-formats.md)
3. [Detection Rules](user-guide/detection-rules.md)
4. [Security Considerations](security/security-considerations.md)

For contributors:

1. [Architecture](architecture/architecture.md)
2. [Config and Policy Schema](configuration/config-policy-schema.md)
3. [Report Schema](user-guide/report-schema.md)
4. [Commercial Readiness 10/10 Plan](planning/commercial-readiness-10-10-plan.md)
5. [Commercial Readiness Sprint Roadmap](planning/commercial-readiness-sprint-roadmap.md)
6. [Architecture Decision Records](architecture/adr/0001-local-first-boundary.md)

For maintainers:

1. [Release Checklist](release/release-checklist.md)
2. [Installer and Distribution](release/installer-distribution.md)
3. [Logging](operations/logging.md)
4. [Performance Operations](operations/performance.md)
5. [DuckDB OLAP Plan](data/duckdb-olap-plan.md)
6. [Commercial Readiness Sprint Roadmap](planning/commercial-readiness-sprint-roadmap.md)
7. [Remaining Work Sprint Plan](planning/karakol-kalan-isler-sprint-plani.md)
