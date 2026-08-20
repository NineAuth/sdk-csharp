namespace NineAuth.Client.Models;

/// <summary>
/// Result of hardware device reset.
/// </summary>
public sealed record ResetDeviceResult(
    bool Success,
    int DeviceResetsRemaining
);
