# ADR 0002 - DuckDB as Optional Local OLAP

## Status

Accepted

## Context

Karakol needs future scan history, trend analysis, and aggregate analytics. SQLite is appropriate for simple metadata, but analytical questions over findings and timelines are a better fit for local OLAP storage.

## Decision

DuckDB will be introduced as an optional persistence provider behind Karakol persistence abstractions.

DuckDB must not be required for single-file scan, report generation, parser execution, rule execution, or CI smoke tests.

## Consequences

- `Karakol.Persistence.DuckDb` will be a separate module.
- Raw log storage remains disabled by default.
- The core scan path remains streaming-first and persistence-independent.
- Analytics queries can evolve without polluting Domain or CLI layers.
