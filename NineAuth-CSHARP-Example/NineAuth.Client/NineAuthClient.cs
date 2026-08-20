using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NineAuth.Client.Exceptions;
using NineAuth.Client.Models;

namespace NineAuth.Client;

/// <summary>
/// Official .NET client for NineAuth Identity, Access and Licensing Infrastructure.
/// Provides a strongly-typed, zero-local-authorization interface to the NineAuth Runtime API.
/// </summary>
public sealed class NineAuthClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public string ApplicationId { get; }
    public string ApiUrl { get; }
    public string Environment { get; }
    public ApplicationInfo? ApplicationInfo { get; private set; }

    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset? _accessExpiresAt;
    private DateTimeOffset? _refreshExpiresAt;
    private List<string> _entitlements = [];

    /// <summary>
    /// Checks whether the client holds an unexpired access token in memory.
    /// Note: Does not make a network call; checks purely in-memory token expiry.
    /// </summary>
    public bool IsAuthenticated =>
        !string.IsNullOrEmpty(_accessToken) &&
        _accessExpiresAt.HasValue &&
        _accessExpiresAt.Value > DateTimeOffset.UtcNow;

    /// <summary>
    /// Constructs a NineAuthClient with the given options.
    /// Call <see cref="InitializeAsync"/> to verify connection and load application metadata.
    /// </summary>
    public NineAuthClient(NineAuthOptions options, HttpClient? customHttpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.ApplicationId))
        {
            throw new NineAuthException("ApplicationId is required to initialize NineAuthClient", "CONFIG_ERROR");
        }

        ApplicationId = options.ApplicationId;
        ApiUrl = (options.ApiUrl ?? "https://api.nineauth.xyz").TrimEnd('/');
        Environment = string.IsNullOrWhiteSpace(options.Environment) ? "production" : options.Environment;

        if (customHttpClient != null)
        {
            _httpClient = customHttpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiUrl),
                Timeout = options.Timeout > TimeSpan.Zero ? options.Timeout : TimeSpan.FromSeconds(15),
            };
            _ownsHttpClient = true;
        }
    }

    /// <summary>
    /// Static factory to create and initialize a NineAuthClient instance in one step.
    /// </summary>
    public static async Task<NineAuthClient> InitializeAsync(NineAuthOptions options, HttpClient? customHttpClient = null, CancellationToken cancellationToken = default)
    {
        var client = new NineAuthClient(options, customHttpClient);
        await client.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }

    /// <summary>
    /// Verifies application validity against the NineAuth backend and caches application metadata.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<InitResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/applications/init",
            body: null,
            includeAuth: false,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        ApplicationInfo = new ApplicationInfo(
            response.ApplicationId,
            response.Name,
            response.Environment
        );
    }

    // =========================================================================
    // AUTHENTICATION
    // =========================================================================

    /// <summary>
    /// Registers a new end-user account for this application.
    /// </summary>
    public async Task<string> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty.", nameof(password));

        var payload = new
        {
            email,
            password
        };

        var response = await SendRequestAsync<RegisterResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/auth/register",
            payload,
            includeAuth: false,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return response.UserId;
    }

    /// <summary>
    /// Logs in an end-user and stores the issued session tokens in memory.
    /// Includes cryptographic anti-replay protection (timestamp + random nonce).
    /// </summary>
    public async Task LoginAsync(string email, string password, string? deviceFingerprint = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty.", nameof(password));

        var payload = new
        {
            email,
            password,
            device_fingerprint = deviceFingerprint,
            timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            nonce = GenerateNonce()
        };

        var response = await SendRequestAsync<AuthResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/auth/login",
            payload,
            includeAuth: false,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        _accessToken = response.AccessToken;
        _refreshToken = response.RefreshToken;
        _accessExpiresAt = response.AccessExpiresAt;
        _refreshExpiresAt = response.RefreshExpiresAt;
        _entitlements.Clear();
    }

    /// <summary>
    /// Revokes the current session and clears all in-memory credentials.
    /// </summary>
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            ClearSessionMemory();
            return;
        }

        try
        {
            var payload = new
            {
                access_token = _accessToken
            };

            await SendRequestAsync<JsonElement>(
                HttpMethod.Post,
                "/v1/runtime/auth/logout",
                payload,
                includeAuth: false,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
        finally
        {
            ClearSessionMemory();
        }
    }

    // =========================================================================
    // LICENSING & SESSIONS
    // =========================================================================

    /// <summary>
    /// Activates a license key for a hardware device fingerprint.
    /// Includes cryptographic anti-replay protection (timestamp + random nonce).
    /// </summary>
    public async Task<ActivateLicenseResult> ActivateLicenseAsync(string licenseKey, string deviceFingerprint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey)) throw new ArgumentException("License key cannot be empty.", nameof(licenseKey));
        if (string.IsNullOrWhiteSpace(deviceFingerprint)) throw new ArgumentException("Device fingerprint cannot be empty.", nameof(deviceFingerprint));

        var payload = new
        {
            license_key = licenseKey,
            device_fingerprint = deviceFingerprint,
            timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            nonce = GenerateNonce()
        };

        var response = await SendRequestAsync<ActivationResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/licenses/activate",
            payload,
            includeAuth: true,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        _accessToken = response.AccessToken;
        _refreshToken = response.RefreshToken;
        _accessExpiresAt = response.AccessExpiresAt;
        _refreshExpiresAt = response.RefreshExpiresAt;
        _entitlements = response.Entitlements != null ? [.. response.Entitlements] : [];

        return new ActivateLicenseResult(
            response.AccessToken,
            response.RefreshToken,
            response.AccessExpiresAt,
            response.RefreshExpiresAt,
            _entitlements.AsReadOnly(),
            new LicenseInfo(response.License.Status, response.License.ExpiresAt)
        );
    }

    /// <summary>
    /// Validates the active session against the backend.
    /// Updates in-memory entitlements if valid; clears session if invalid.
    /// </summary>
    public async Task<ValidateSessionResult> ValidateSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new NineAuthException("No active session to validate", "NOT_AUTHENTICATED", 401);
        }

        var payload = new
        {
            access_token = _accessToken
        };

        var response = await SendRequestAsync<ValidateSessionResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/sessions/validate",
            payload,
            includeAuth: false,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        if (response.Valid)
        {
            _entitlements = response.Entitlements != null ? [.. response.Entitlements] : [];
            return new ValidateSessionResult(
                true,
                _entitlements.AsReadOnly(),
                response.ExpiresAt,
                response.Reason
            );
        }
        else
        {
            ClearSessionMemory();
            return new ValidateSessionResult(
                false,
                Array.Empty<string>(),
                null,
                response.Reason
            );
        }
    }

    /// <summary>
    /// Refreshes the session using the in-memory refresh token.
    /// </summary>
    public async Task RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            throw new NineAuthException("No refresh token available to refresh session", "NOT_AUTHENTICATED", 401);
        }

        try
        {
            var payload = new
            {
                refresh_token = _refreshToken
            };

            var response = await SendRequestAsync<AuthResponseDto>(
                HttpMethod.Post,
                "/v1/runtime/sessions/refresh",
                payload,
                includeAuth: false,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            _accessToken = response.AccessToken;
            _refreshToken = response.RefreshToken;
            _accessExpiresAt = response.AccessExpiresAt;
            _refreshExpiresAt = response.RefreshExpiresAt;
        }
        catch
        {
            ClearSessionMemory();
            throw;
        }
    }

    /// <summary>
    /// Performs self-service HWID reset for a license, decoupling the specified hardware device.
    /// </summary>
    public async Task<ResetDeviceResult> ResetDeviceAsync(string licenseKey, string deviceFingerprint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey)) throw new ArgumentException("License key cannot be empty.", nameof(licenseKey));
        if (string.IsNullOrWhiteSpace(deviceFingerprint)) throw new ArgumentException("Device fingerprint cannot be empty.", nameof(deviceFingerprint));

        var payload = new
        {
            license_key = licenseKey,
            device_fingerprint = deviceFingerprint
        };

        var response = await SendRequestAsync<ResetDeviceResponseDto>(
            HttpMethod.Post,
            "/v1/runtime/devices/reset",
            payload,
            includeAuth: false,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return new ResetDeviceResult(
            response.Success,
            response.DeviceResetsRemaining
        );
    }

    // =========================================================================
    // ENTITLEMENTS (ZERO LOCAL DECISION LOGIC)
    // =========================================================================

    /// <summary>
    /// Checks whether the in-memory session holds a specific entitlement.
    /// Purely reads from the local in-memory entitlements list (zero network call).
    /// </summary>
    public bool HasEntitlement(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Entitlement key cannot be empty.", nameof(key));
        return _entitlements.Contains(key, StringComparer.Ordinal);
    }

    /// <summary>
    /// Returns a snapshot list of the active entitlements held in memory.
    /// Zero network call.
    /// </summary>
    public IReadOnlyList<string> GetEntitlements()
    {
        return _entitlements.AsReadOnly();
    }

    /// <summary>
    /// Queries the backend server in real-time to check if an entitlement is currently granted.
    /// Always hits the network for critical, fresh entitlement verification.
    /// </summary>
    public async Task<bool> CheckEntitlementAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Entitlement key cannot be empty.", nameof(key));

        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new NineAuthException("No active session to check entitlement", "NOT_AUTHENTICATED", 401);
        }

        var uri = $"/v1/runtime/entitlements/check?key={Uri.EscapeDataString(key)}";
        var response = await SendRequestAsync<CheckEntitlementResponseDto>(
            HttpMethod.Get,
            uri,
            body: null,
            includeAuth: true,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return response.Granted;
    }

    // =========================================================================
    // SESSION EXPORT & RESTORE
    // =========================================================================

    /// <summary>
    /// Exports an in-memory snapshot of the current session state.
    /// </summary>
    public SessionState GetSessionState()
    {
        return new SessionState(
            _accessToken,
            _refreshToken,
            _accessExpiresAt,
            _refreshExpiresAt,
            _entitlements.AsReadOnly()
        );
    }

    /// <summary>
    /// Restores session state from a previously saved snapshot.
    /// </summary>
    public void RestoreSession(SessionState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        _accessToken = state.AccessToken;
        _refreshToken = state.RefreshToken;
        _accessExpiresAt = state.AccessExpiresAt;
        _refreshExpiresAt = state.RefreshExpiresAt;
        _entitlements = state.Entitlements != null ? [.. state.Entitlements] : [];
    }

    // =========================================================================
    // INTERNAL HELPERS
    // =========================================================================

    /// <summary>
    /// Generates a cryptographically secure 16-byte random hexadecimal nonce for anti-replay protection.
    /// </summary>
    private static string GenerateNonce()
    {
        byte[] bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
#if NETSTANDARD2_1
        var sb = new StringBuilder(32);
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("x2"));
        }
        return sb.ToString();
#else
        return Convert.ToHexString(bytes).ToLowerInvariant();
#endif
    }

    private void ClearSessionMemory()
    {
        _accessToken = null;
        _refreshToken = null;
        _accessExpiresAt = null;
        _refreshExpiresAt = null;
        _entitlements.Clear();
    }

    private async Task<TResponse> SendRequestAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body = null,
        bool includeAuth = false,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, path);

        request.Headers.Add("X-Application-Id", ApplicationId);
        request.Headers.Add("X-Environment", Environment);

        if (includeAuth && !string.IsNullOrEmpty(_accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        if (body != null)
        {
            string json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new NineAuthException($"Network error connecting to NineAuth API: {ex.Message}", "NETWORK_ERROR");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new NineAuthException($"Request to NineAuth API timed out: {ex.Message}", "TIMEOUT_ERROR");
        }

#if NETSTANDARD2_1
        string rawResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        string rawResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorCode = "API_ERROR";
            string errorMessage = $"Request failed with status code {(int)httpResponse.StatusCode}";

            try
            {
                var errorDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(rawResponse, JsonOptions);
                if (errorDto?.Error != null)
                {
                    errorCode = errorDto.Error.Code ?? errorCode;
                    errorMessage = errorDto.Error.Message ?? errorMessage;
                }
            }
            catch
            {
                // Fallback to generic message if error payload is not structured JSON
            }

            throw new NineAuthException(errorMessage, errorCode, (int)httpResponse.StatusCode);
        }

        try
        {
            var result = JsonSerializer.Deserialize<TResponse>(rawResponse, JsonOptions);
            if (result == null)
            {
                throw new NineAuthException("API returned empty or invalid JSON response payload", "INVALID_RESPONSE");
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new NineAuthException($"Failed to deserialize API response: {ex.Message}", "DESERIALIZATION_ERROR");
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    // =========================================================================
    // DTOs for JSON serialization
    // =========================================================================

    private sealed record InitResponseDto(
        string ApplicationId,
        string Name,
        string Environment
    );

    private sealed record RegisterResponseDto(
        string UserId
    );

    private sealed record AuthResponseDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessExpiresAt,
        DateTimeOffset RefreshExpiresAt
    );

    private sealed record ActivationResponseDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessExpiresAt,
        DateTimeOffset RefreshExpiresAt,
        List<string>? Entitlements,
        LicenseDto License
    );

    private sealed record LicenseDto(
        string Status,
        DateTimeOffset? ExpiresAt
    );

    private sealed record ValidateSessionResponseDto(
        bool Valid,
        List<string>? Entitlements,
        DateTimeOffset? ExpiresAt,
        string? Reason
    );

    private sealed record CheckEntitlementResponseDto(
        bool Granted
    );

    private sealed record ResetDeviceResponseDto(
        bool Success,
        int DeviceResetsRemaining
    );

    private sealed record ApiErrorResponseDto(
        ErrorDetailsDto? Error
    );

    private sealed record ErrorDetailsDto(
        string? Code,
        string? Message
    );
}
