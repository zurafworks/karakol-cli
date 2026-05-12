## Summary

Describe the change and the user-facing behavior it affects.

## Type

- [ ] Parser
- [ ] Detection rule
- [ ] Correlation
- [ ] Risk scoring
- [ ] Reporting
- [ ] CLI
- [ ] Persistence
- [ ] Documentation
- [ ] Build/release

## Local-First Impact

- [ ] No external API call added.
- [ ] No telemetry added.
- [ ] No raw log persistence added by default.
- [ ] Sensitive values are not written to logs or reports without masking.

## Verification

- [ ] `dotnet build --no-restore`
- [ ] `dotnet test --no-restore`
- [ ] `dotnet run --no-restore --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose`

## Documentation

- [ ] README/docs updated.
- [ ] Config/policy examples updated if behavior changed.
- [ ] New parser/rule/reporter/provider has tests.
