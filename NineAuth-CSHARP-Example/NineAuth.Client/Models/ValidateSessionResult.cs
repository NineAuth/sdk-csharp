namespace NineAuth.Client.Models;

/// <summary>
/// Result of session validation check against the backend.
/// </summary>
public sealed record ValidateSessionResult(
    bool Valid,
    IReadOnlyList<string> Entitlements,
    DateTimeOffset? ExpiresAt = null,
    string? Reason = null
);
