# Detection Rules

Karakol MVP uses rule-based detection as the default engine. Rules are registered through the built-in plugin catalog and execute before optional ML and correlation stages.

## Built-In Single Event Rules

| Rule | Category | Default Severity | Primary Signals |
| --- | --- | --- | --- |
| SQL injection | `SqlInjection` | High | `UNION SELECT`, tautologies, SQL comments, `information_schema`, timing functions, destructive SQL verbs |
| XSS | `Xss` | High | `<script`, `javascript:`, event handlers, `document.cookie`, encoded script payloads |
| Path traversal | `PathTraversal` | High | `../`, `..\\`, encoded traversal, `/etc/passwd`, `win.ini`, system paths |
| Scanner bot | `ScannerBot` | Medium | `/wp-admin`, `/.env`, `/phpmyadmin`, `/actuator`, scanner user agents |
| Sensitive file access | `SensitiveFileAccess` | High | `.env`, `id_rsa`, `appsettings.json`, `.git/config`, backups, credential files |
| Suspicious user agent | `SuspiciousUserAgent` | Medium | empty, very short, scanner signatures, scripted clients |
| Command injection | `CommandInjection` | High | shell separators, command substitution, `bash -c`, `cmd.exe`, `powershell`, download-and-execute patterns |

## Correlation Rules

| Rule | Category | Default Severity | Window Behavior |
| --- | --- | --- | --- |
| Brute force | `BruteForce` | High/Critical | Counts failed login or auth-like failures by source IP in a bounded time window |
| DoS attempt | `DosAttempt` | Medium/High | Counts request bursts by source IP in a short bounded time window |

## Evidence Contract

Each finding should include:

- `field`: PascalCase source field such as `Url`, `Path`, `QueryString`, `UserAgent`, or `RawMessage`.
- `value`: sanitized value used for the match.
- `matchedPattern`: the rule pattern or signal when a concrete pattern caused the finding.
- `explanation`: why the signal matters.

Rule findings must include at least one evidence item unless a rule explicitly documents why evidence cannot be attached. Built-in rule IDs use the `category.variant` convention, for example `sqli.basic`, `xss.basic`, and `scanner-bot.basic`.

## Rule Options

Rule options support:

- Global enable/disable.
- Per-rule disabled IDs.
- Per-rule enabled allow-list IDs.
- Per-rule severity overrides.

Pattern group overrides remain a future extension. The current implementation keeps built-in pattern groups code-owned so rule behavior stays deterministic for the initial open-source release.

## Policy Interaction

Policies can disable rules, restrict enabled rules, define a minimum severity, ignore paths, mark trusted IPs, and add sensitive paths that increase risk scoring.
