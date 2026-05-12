# Security Considerations

Karakol is designed as a local-first security tool. Its default behavior must be conservative with user log data because logs often contain credentials, session identifiers, internal hostnames, personal data, and incident evidence.

## Local-First Guarantee

- No log file content is sent to external APIs.
- No telemetry is emitted by the MVP.
- ML classification is local-only. The MVP dummy classifier does not download or call remote models.
- Reports are written to local output paths only.

## Report Safety

- HTML reports are single-file and offline.
- Raw values are HTML encoded before being written.
- JavaScript is not required for report rendering.
- Sensitive data masking runs before report output.
- Raw samples are disabled by default and must be explicitly requested.

## Sensitive Data Masking

The masking layer targets:

- emails
- bearer tokens
- JWT-like values
- long API-key-like tokens
- `password`, `token`, `session`, and authorization query/header values

Example:

```text
/login?username=ali&password=123456
```

becomes:

```text
/login?username=ali&password=******
```

## File And Path Handling

- Input paths are validated before scanning.
- Output directories are normalized and created explicitly.
- Report names are generated to avoid accidental overwrite collisions.
- Directory traversal through report path manipulation is treated as a validation concern.

## Operational Notes

- Keep generated reports out of source control.
- Treat reports as sensitive artifacts.
- Do not enable raw samples in shared CI logs unless the data is known to be safe.
- Prefer policy files for trusted IPs and ignored health-check paths rather than hard-coding exceptions.
