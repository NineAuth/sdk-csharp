namespace NineAuth.Client.Models;

/// <summary>
/// Application initialization metadata returned by NineAuth Runtime API.
/// </summary>
public sealed record ApplicationInfo(
    string ApplicationId,
    string Name,
    string Environment
);
