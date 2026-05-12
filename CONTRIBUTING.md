# Contributing

Karakol accepts contributions that preserve the local-first security model and keep the codebase modular.

## Development Setup

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-restore
```

Run the sample scan:

```bash
dotnet run --no-restore --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose
```

## Contribution Rules

- Keep Domain framework-independent.
- Keep CLI as an adapter; business logic belongs in Application/Domain modules.
- Add tests for parser, rule, risk, reporting, or policy changes.
- Do not add cloud calls, telemetry, or remote model downloads.
- Mask or remove sensitive data from fixtures.
- Prefer small pull requests with clear scope.

## Pull Request Checklist

- Build passes.
- Tests pass.
- Sample scan still works.
- Documentation is updated when behavior changes.
- New parser/rule/reporter/provider has at least one test and one docs note.
