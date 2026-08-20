namespace NineAuth.Client.Models;

/// <summary>
/// In-memory snapshot of the current session state.
/// Can be exported or restored by the host application for persistence across restarts.
/// </summary>
public sealed record SessionState(
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? AccessExpiresAt,
    DateTimeOffset? RefreshExpiresAt,
    IReadOnlyList<string> Entitlements
);
