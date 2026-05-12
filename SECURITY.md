# Security Policy

Karakol is a local-first security tool. Security reports should avoid exposing private logs, credentials, exploit payloads against real systems, or customer data.

## Supported Versions

| Version | Supported |
| --- | --- |
| `0.1.x` | Yes, pre-release/internal beta support |

## Reporting a Vulnerability

After the public GitHub repository is created, enable GitHub Private Vulnerability Reporting and use that channel.

Until that channel is configured:

- Do not open public issues with exploit details.
- Contact the maintainer privately.
- Share minimal reproduction data.
- Redact IPs, tokens, session identifiers, and real customer data.

## Scope

Security issues include:

- Report XSS or unsafe HTML output.
- Sensitive data leakage in reports or logs.
- Path traversal or unsafe file writes.
- Unexpected network calls.
- Unsafe model or plugin loading behavior.

## Local-First Requirement

Any contribution that sends logs, findings, metadata, or model input to a remote service must be rejected unless it is explicitly designed as an opt-in future integration and documented as outside the default local-first mode.
