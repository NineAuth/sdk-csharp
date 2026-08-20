namespace NineAuth.Client.Models;

/// <summary>
/// License status metadata returned upon activation.
/// </summary>
public sealed record LicenseInfo(
    string Status,
    DateTimeOffset? ExpiresAt
);

/// <summary>
/// Result of license key activation and device binding.
/// </summary>
public sealed record ActivateLicenseResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessExpiresAt,
    DateTimeOffset RefreshExpiresAt,
    IReadOnlyList<string> Entitlements,
    LicenseInfo License
);
