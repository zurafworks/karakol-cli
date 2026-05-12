# Karakol Commercial Readiness and 10/10 Engineering Plan

This plan defines the work needed to move Karakol from working MVP/internal beta to a professional open-source security product that can credibly target 10/10 across architecture, folder structure, detection quality, reporting, release readiness, and operational trust.

## Current Position

Karakol currently has:

- Working .NET 9 solution.
- Clean modular folder structure.
- CLI scan flow.
- JSON, Markdown, and offline HTML reports.
- Rule-based detection and basic correlation.
- Config and policy basics.
- Tests across core modules.
- Local dotnet tool packaging.
- Open-source preparation files.

The remaining gap is not "does it run"; it is "can strangers use, trust, extend, and maintain it without private context."

## Target Scorecard

| Area | Target | Required Evidence |
| --- | ---: | --- |
| Architecture | 10/10 | Enforced dependency rules, ADRs, module boundaries, plugin contracts, no accidental concrete coupling |
| Folder structure | 10/10 | Context-based docs, feature folders, test parity, no catch-all files, consistent namespaces |
| Domain model | 10/10 | Strong invariants, factories, value objects, domain tests, no framework dependencies |
| Application layer | 10/10 | CQRS use cases, MediatR behaviors, validation, cancellation, error mapping, orchestration tests |
| CLI UX | 10/10 | Consistent commands, help text, progress, quiet/verbose, exit codes, install docs |
| Parser quality | 10/10 | Real-world fixtures, edge-case matrix, parser confidence tests, failed-line diagnostics |
| Detection quality | 10/10 | Malicious/benign corpus, false-positive controls, explainable evidence, rule tuning docs |
| Correlation | 10/10 | Bounded state, window tests, source-IP behavior, memory benchmarks |
| Risk scoring | 10/10 | Calibrated formula, component explanations, policy modifiers, regression tests |
| Reporting | 10/10 | Professional HTML, stable JSON schema, encoded output, logo, snapshot tests |
| Performance | 10/10 | 100k/1M/5M benchmarks, memory ceilings, streaming guarantees |
| Security posture | 10/10 | Local-first proof, masking tests, path safety, secure release process |
| Packaging/release | 10/10 | CI, NuGet package, global tool install, changelog, signed release plan |
| Open source readiness | 10/10 | License, contribution docs, code of conduct, security policy, issue templates |
| Analytics storage | 10/10 | DuckDB provider behind abstraction, local-only persistence, analytics benchmarks |

## Phase A - Open Source Foundation

Tasks:

- Keep `LICENSE`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md` at repo root.
- Keep `.github/workflows/ci.yml` as the minimum build/test/package gate.
- Add issue templates for bug reports and feature requests.
- Replace package placeholder repository URLs before public release if the GitHub owner differs.
- Add release notes for every tag.

Done criteria:

- New contributor can build and test from README only.
- CI runs on pull requests.
- Security reporting path is clear.

## Phase B - Parser Hardening

Tasks:

- Build fixture folders for real-world Nginx, Apache, SSH, auth, JSON, CSV, and generic logs.
- Add edge cases: IPv6, quoted URLs, missing referrer, `-` response size, escaped quotes, upstream status, proxy IP headers, malformed timestamps.
- Add parse-failure reason taxonomy.
- Add parser confidence regression tests.
- Add corpus tests for at least 200 benign and 200 suspicious lines.

Done criteria:

- Parser coverage and failed-line ratio are predictable.
- Unknown lines degrade to generic parser without crashing.
- Real-world fixtures are documented and safe to redistribute.

## Phase C - Detection Quality

Tasks:

- Maintain malicious and benign corpora per rule.
- Add false-positive tests for search queries, documentation URLs, normal encoded values, package names, and admin pages used internally.
- Add severity tuning notes per category.
- Add rule option model for thresholds and pattern groups.
- Add rule documentation generated from rule metadata.

Done criteria:

- Every rule has malicious, benign, encoded, and masked-output tests.
- Evidence always explains field, value, matched pattern, and recommended action.
- Rule IDs are stable.

## Phase D - Performance and Memory

Tasks:

- Add benchmark fixtures for 100k, 1M, and 5M lines.
- Track elapsed time, allocations, peak working set, parsed events, and failed lines.
- Add optional CI performance smoke category.
- Add local BenchmarkDotNet project after baseline stabilizes.
- Document expected hardware assumptions.

Done criteria:

- Streaming reader does not load whole files.
- Correlation state remains bounded.
- `MaxLines` behavior is tested and documented.

## Phase E - CI/CD and Release

Tasks:

- CI must run restore, build, test, pack, and sample scan.
- Add release workflow after repository owner is final.
- Add NuGet publish workflow with manual approval.
- Add versioning rules: SemVer, changelog required, tag required.
- Add package signing plan if required by distribution channel.

Done criteria:

- Pull request cannot merge without green CI.
- Release candidate can be produced with repeatable commands.

## Phase F - Installer and Distribution

Tasks:

- Keep `dotnet tool` as primary distribution.
- Document local install, global install, update, and uninstall.
- Add Windows PowerShell smoke commands.
- Add Linux/macOS shell smoke commands.
- Evaluate native single-file binaries after CLI stabilizes.

Done criteria:

- Users can install with `dotnet tool install`.
- Version command reads package metadata.
- Packaged tool includes `karakol_logo.png` for offline reports.

## Phase G - Serilog Integration

Tasks:

- Add Serilog packages centrally.
- Configure console sink for CLI diagnostics.
- Configure optional file sink only when requested.
- Ensure sensitive values are not logged.
- Add logging tests around redaction-sensitive paths.

Done criteria:

- Application behaviors log request lifecycle.
- CLI verbose mode can surface diagnostics.
- Quiet mode remains quiet.

## Phase H - Reporting UX

Tasks:

- Keep HTML single-file and offline.
- Embed Karakol logo as a data URI.
- Maintain XSS-safe encoding.
- Add snapshot/string tests for report sections.
- Improve print stylesheet and responsive behavior.
- Add report visual regression in a later browser-based test stage.

Done criteria:

- HTML report is readable for executives and engineers.
- JSON schema remains stable for automation.
- Markdown remains suitable for CI comments.

## Phase I - Config and Policy

Tasks:

- Keep schema docs under `docs/configuration`.
- Keep examples under `samples/config`.
- Add validation for unknown severity, unknown report format, and invalid paths.
- Add per-rule policy options.
- Add schema JSON files after config surface stabilizes.

Done criteria:

- User can copy example files and run scan without editing code.
- Invalid config returns controlled errors.

## Phase J - DuckDB Local OLAP

Tasks:

- Add `Karakol.Persistence.DuckDb` as an optional provider.
- Store scan facts, findings, evidence, and timeline buckets.
- Do not store raw logs by default.
- Add local analytics queries for trends and top offenders.
- Add performance tests for large historical datasets.

Done criteria:

- Core scan works without DuckDB.
- DuckDB persistence is local-only and opt-in.
- Analytics queries are documented and tested.

## Phase K - Final 10/10 Gate

Release can be considered professional-grade when:

- CI is mandatory.
- All public docs are accurate.
- Sample reports look production-ready.
- Parser/detection corpora are representative.
- Local-first guarantee is tested and documented.
- New parser/rule/reporter/provider can be added by following docs.
- Release package installs and runs on Windows, Linux, and macOS.
- No known critical security handling gaps remain.
