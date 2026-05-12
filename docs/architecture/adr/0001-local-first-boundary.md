# ADR 0001 - Local-First Boundary

## Status

Accepted

## Context

Karakol analyzes potentially sensitive server, application, and security logs. Logs can contain IP addresses, usernames, tokens, credentials, session identifiers, internal paths, and incident evidence.

## Decision

The default product mode is local-first:

- No log lines are sent to external services.
- No telemetry is emitted.
- No remote model download is performed.
- Reports are generated locally.
- Persistence is opt-in.

## Consequences

- Integrations that require network access must be explicit and disabled by default.
- Tests and release notes must preserve the local-first guarantee.
- Documentation must warn users that generated reports can still be sensitive local artifacts.
