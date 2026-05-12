# Risk Scoring

Karakol assigns risk at two levels: individual findings and the whole scan. Scores are value objects with a numeric range of `0-100`, severity mapping, component breakdown, and explanation.

## Severity Bands

| Score | Severity |
| --- | --- |
| `0-9` | Info |
| `10-39` | Low |
| `40-69` | Medium |
| `70-89` | High |
| `90-100` | Critical |

## Finding Risk

Base severity scores:

- Info: `5`
- Low: `20`
- Medium: `50`
- High: `75`
- Critical: `95`

Modifiers:

- Lower confidence reduces score.
- Correlation findings may increase score.
- Sensitive paths increase score.
- Trusted IPs reduce score.
- Repeated findings from the same source increase score.
- Multiple categories from the same source increase score.

Each modifier is represented as a risk component so JSON and HTML reports can explain why a score changed.

## Scan Risk

Scan-level risk considers:

- Critical, high, medium, and low finding counts.
- Unique threat category count.
- Repeated source IP concentration.
- Sensitive endpoint findings.
- Parser failure ratio.
- Timeline density and burst behavior.

The result is capped at `100`. The current MVP formula is intentionally simple and test-backed so it can later be replaced by a strategy without changing report contracts.

## Policy Interaction

`trustedIps` and `sensitivePaths` from policy files feed the risk context. `minimumSeverity` affects which findings are returned after scoring.
