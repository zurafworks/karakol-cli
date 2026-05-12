# Config and Policy Schema

Karakol has two local JSON inputs:

- `karakolsettings.json`: runtime defaults for scanning, rules, ML, reporting, and persistence.
- `karakol.policy.json`: security policy that affects rule selection, severity filtering, trusted IPs, ignored paths, and sensitive paths.

Both files are optional. Missing files use safe defaults.

JSON Schema files are available for editor integration:

- `schemas/karakolsettings.schema.json`
- `schemas/karakol.policy.schema.json`

## Settings Example

See `samples/config/karakolsettings.example.json`.

```json
{
  "scanning": {
    "maxLines": null,
    "streamingBatchSize": 1000,
    "autoDetectFormat": true,
    "maskSensitiveData": true,
    "includeRawSamples": false
  },
  "rules": {
    "enabled": true
  },
  "ml": {
    "enabled": false,
    "modelPath": "./models/karakol-threat-classifier.onnx",
    "minimumConfidence": 0.7
  },
  "reporting": {
    "defaultOutputDirectory": "./reports",
    "defaultFormats": ["json", "html"],
    "includeEvidence": true,
    "includeRawSamples": false
  },
  "persistence": {
    "enabled": false,
    "provider": "DuckDB",
    "connectionString": "Data Source=./data/karakol.duckdb"
  }
}
```

## Policy Example

See `samples/config/karakol.policy.example.json`.

```json
{
  "minimumSeverity": "Medium",
  "enabledRules": [],
  "disabledRules": [],
  "sensitivePaths": ["/admin", "/api/payment", "/api/users/export"],
  "trustedIps": ["127.0.0.1", "10.0.0.0/8"],
  "ignoredPaths": ["/health", "/metrics"]
}
```

## Operational Rules

- Keep config and policy files local.
- Do not store secrets in policy files.
- Prefer `ignoredPaths` for health and metrics endpoints.
- Prefer `trustedIps` for internal scanners and synthetic monitoring.
- Prefer `sensitivePaths` for endpoints that should increase risk even when the matched payload is moderate.

## MVP Limitations

- `rules.enabled` is global; per-rule detailed options are planned.
- DuckDB persistence is planned as the analytics provider; the current MVP keeps persistence disabled by default.
- CIDR matching should be hardened before enterprise release.
