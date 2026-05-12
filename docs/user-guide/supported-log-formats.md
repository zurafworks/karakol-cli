# Supported Log Formats

Karakol supports explicit format selection and auto-detection. Auto-detection samples the input, asks registered parsers whether they can parse it, and records confidence metadata in the report.

## Formats

| Format | CLI Value | Status | Notes |
| --- | --- | --- | --- |
| Nginx access | `nginx` | MVP | Parses common combined access lines with method, URL, status, size, referrer, and user agent |
| Apache access | `apache` | MVP | Supports common and combined access log shapes |
| Linux auth | `auth` | MVP | Parses auth messages with hostname, process, user, source IP, and auth event type |
| SSH auth | `ssh` | MVP | Specializes auth parsing for failed and accepted SSH login events |
| JSON security event | `json` | MVP | Supports JSON Lines, single-line JSON arrays, multi-line JSON arrays, optional fields, and unknown metadata |
| CSV security event | `csv` | MVP | Supports quoted fields, common column aliases, and unknown metadata columns |
| Generic regex/raw | `generic` | MVP | Produces normalized events when a specific parser is unavailable |
| Auto | `auto` | MVP | Selects the best registered parser by confidence |

## Parser Output

Parsers produce normalized `SecurityEvent` instances while preserving the raw message. Detection rules use normalized fields such as `Url`, `Path`, `QueryString`, `StatusCode`, `UserAgent`, `Username`, and `SourceIp`.

Structured parsers keep unrecognized JSON properties and CSV columns in `SecurityEvent.Metadata`. This preserves useful context for future reporting, correlation, and analytics without forcing every source-specific field into the core domain model.

## Parser Statistics

Reports include parser metadata such as total lines, parsed events, failed lines, failed ratio, detected format, and format confidence.
