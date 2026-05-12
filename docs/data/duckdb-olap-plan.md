# DuckDB OLAP Plan

DuckDB is the preferred local OLAP engine for Karakol's post-MVP analytics layer. It should not replace the streaming scan pipeline. It should sit behind persistence/query abstractions and receive normalized scan facts after the core local analysis completes.

## Why DuckDB

- Embedded and local-first.
- Strong analytical SQL over columnar workloads.
- Suitable for scan history, trend analysis, aggregation, and dashboard queries.
- Better fit than SQLite for multi-scan analytics over large event/finding tables.
- Lower operational burden than ClickHouse for a local-first CLI/desktop product.

## Non-Goals

- Do not require DuckDB for single-file scan.
- DuckDB is opt-in; when explicitly enabled, persistence failures fail the scan with a controlled configuration/persistence error so automation does not assume history was saved.
- Do not store raw logs by default.
- Do not introduce cloud sync, telemetry, or remote warehouse dependencies.

## Current Module

```text
src/
  Karakol.Persistence.DuckDb/
    Analytics/
    DuckDb/
    Repository/
    Sanitization/
    Schema/

tests/
  Karakol.Persistence.DuckDb.Tests/
```

The provider is opt-in. Normal scans still run without DuckDB. A scan can persist derived facts with:

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report json --persist-history
```

Or through config:

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --config samples/config/karakolsettings.duckdb.json
```

## Tables

`scan_sessions`

- `scan_id`
- `source_path_hash`
- `source_format`
- `started_at`
- `completed_at`
- `duration_ms`
- `total_lines`
- `parsed_events`
- `failed_lines`
- `overall_risk`

`findings`

- `finding_id`
- `scan_id`
- `category`
- `severity`
- `risk_score`
- `confidence`
- `detector_id`
- `source_ip`
- `target_resource_masked`
- `created_at`

`finding_evidence`

- `finding_id`
- `field`
- `value_masked`
- `matched_pattern`
- `explanation`

`timeline_buckets`

- `scan_id`
- `bucket_start`
- `event_count`
- `finding_count`

## Query Use Cases

- Top attacking IPs across scans.
- Category trend by day/week.
- Severity distribution across repositories or systems.
- Sensitive path probing trend.
- Repeated scanner signatures.
- Parser failure ratio by format.

## Implementation Phases

1. Add `Karakol.Persistence.DuckDb` project. Done.
2. Add `DuckDB.NET.Data.Full` package in central package management. Done.
3. Add connection factory and migration runner. Done.
4. Add projection from `ScanReport` to relational facts. Done.
5. Add repository tests using temporary local DuckDB files. Done.
6. Add optional `--persist-history` behavior. Done.
7. Add dashboard/query commands after storage is stable. Pending.

## Acceptance Criteria

- Single-file scan still works when persistence is disabled.
- DuckDB writes are local only.
- Raw log lines are not stored by default.
- Report generation does not require DuckDB.
- Aggregation queries over 1M findings complete within an agreed local benchmark target.
