# Karakol JSON Report Schema

Karakol JSON reports use the explicit `karakol.report.v1` document shape.

Top-level fields:

- `schemaVersion`
- `scanId`
- `toolName`
- `toolVersion`
- `generatedAt`
- `source`
- `summary`
- `findings`
- `recommendations`
- `metadata`

The report writer projects domain objects into report DTOs before serialization. This keeps the JSON contract stable for dashboard, CI and future API consumers while allowing domain models to evolve internally.

Backward compatibility rule: additive fields are allowed within `karakol.report.v1`; breaking shape changes require a new schema version.
