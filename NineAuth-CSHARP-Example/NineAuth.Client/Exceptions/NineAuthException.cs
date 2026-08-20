namespace NineAuth.Client.Exceptions;

/// <summary>
/// Exception thrown by NineAuthClient when the API returns an error or a
/// network/configuration problem occurs. Always carries a structured error
/// code that maps to the NineAuth API error vocabulary.
/// </summary>
public sealed class NineAuthException : Exception
{
    /// <summary>
    /// Machine-readable error code (e.g. LICENSE_EXPIRED, REPLAY_DETECTED, INVALID_CREDENTIALS).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Optional raw HTTP status code returned by the API, if applicable.
    /// </summary>
    public int? HttpStatus { get; }

    public NineAuthException(string message, string code, int? httpStatus = null)
        : base(message)
    {
        Code = code;
        HttpStatus = httpStatus;
    }

    public override string ToString() =>
        $"NineAuthException [{Code}] (HTTP {HttpStatus?.ToString() ?? "N/A"}): {Message}";
}
