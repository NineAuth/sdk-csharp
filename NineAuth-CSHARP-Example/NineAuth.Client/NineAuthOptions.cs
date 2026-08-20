namespace NineAuth.Client;

/// <summary>
/// Configuration options for initializing the NineAuthClient.
/// </summary>
public sealed class NineAuthOptions
{
    /// <summary>
    /// The unique UUID of your application in NineAuth.
    /// </summary>
    public required string ApplicationId { get; set; }

    /// <summary>
    /// Base URL of the NineAuth API.
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.nineauth.xyz";

    /// <summary>
    /// Target environment name.
    /// </summary>
    public string Environment { get; set; } = "production";

    /// <summary>
    /// HTTP timeout for runtime API requests.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);
}
