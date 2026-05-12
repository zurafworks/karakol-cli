# ADR 0003 - CLI as Thin Adapter

## Status

Accepted

## Context

Karakol's first interface is CLI, but the analysis engine must later support agent, dashboard, CI, extension, and enterprise runner surfaces.

## Decision

The CLI layer is an adapter only. It can parse command settings, call MediatR/Application use cases, render terminal output, and map exit codes. It must not contain parser logic, detection logic, risk scoring, persistence policy, or report business rules.

## Consequences

- Application remains the orchestration boundary.
- Domain remains framework-independent.
- New frontends can reuse the same Application layer.
- CLI tests focus on command behavior and exit codes, not business rule internals.
