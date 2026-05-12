namespace Karakol.Domain.Enums;

public enum ThreatCategory
{
    Normal,
    BruteForce,
    CredentialStuffing,
    SqlInjection,
    Xss,
    PathTraversal,
    ScannerBot,
    SuspiciousLogin,
    DosAttempt,
    DataExfiltration,
    MalwareCallback,
    SuspiciousUserAgent,
    SensitiveFileAccess,
    DirectoryEnumeration,
    CommandInjection,
    LocalFileInclusion,
    RemoteFileInclusion,
    UnauthorizedAccess,
    UnknownSuspicious
}
