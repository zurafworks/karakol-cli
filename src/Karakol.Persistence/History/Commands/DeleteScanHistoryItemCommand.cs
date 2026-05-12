using Karakol.Domain.Scans;

namespace Karakol.Persistence.History.Commands;

public sealed record DeleteScanHistoryItemCommand(ScanId ScanId);
