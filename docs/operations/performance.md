# Karakol Performance Operations

Karakol's default test suite includes lightweight performance smoke tests. Expensive tests and benchmarks are opt-in so CI remains fast.

## Default Smoke Tests

Run the normal suite:

```bash
dotnet test --no-build
```

The default suite includes:

- 100k-line streaming reader smoke.
- `MaxLines` enforcement.
- Streaming memory growth smoke.
- Correlation related-event bound check.
- Report generation memory smoke.

## Manual 1M-Line Smoke

The 1M-line fixture is gated by `KARAKOL_RUN_LARGE_PERF=1`.

PowerShell:

```powershell
$env:KARAKOL_RUN_LARGE_PERF = "1"
dotnet test tests/Karakol.Performance.Tests/Karakol.Performance.Tests.csproj --filter Category=ManualPerformance
```

Shell:

```bash
KARAKOL_RUN_LARGE_PERF=1 dotnet test tests/Karakol.Performance.Tests/Karakol.Performance.Tests.csproj --filter Category=ManualPerformance
```

Current manual smoke budget:

- Reader must honor `MaxLines=10000` on a generated 1M-line fixture.
- The bounded read must complete within 5 seconds on a normal developer machine.

## Benchmarks

BenchmarkDotNet benchmarks live in `benchmarks/Karakol.Benchmarks`.

```bash
dotnet run -c Release --project benchmarks/Karakol.Benchmarks -- --filter *
```

Initial benchmark coverage:

- Nginx parser throughput and allocation.
- SQL injection rule throughput and allocation.
- Correlation engine throughput and allocation.

Benchmark results are not a CI gate yet. They are for local comparison before and after parser, rule, correlation, or scan pipeline changes.

## Memory Notes

Karakol should keep large-file handling streaming-first:

- Readers must not materialize the whole file.
- Correlation may keep bounded windows and capped related-event references.
- Report generation can materialize report DTO/output, so report memory cost must be evaluated separately from scan ingestion.
