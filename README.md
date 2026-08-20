<div align="center">
  <img src="https://github.com/Deedzera/NineAuth/blob/main/assets/capa-sdk-cpp.png" alt="NineAuth C# SDK" width="120" height="120" />
  <br /><br />
  <p><strong>NineAuth C# (.NET) SDK &amp; Reference Implementation</strong></p>
  <p>Official client library and ready-to-run desktop example for .NET 8, .NET Framework 4.8+, Unity, WinForms, and WPF applications.</p>
  <br />
  <a href="https://nineauth.xyz">Website</a>
  &nbsp;&middot;&nbsp;
  <a href="https://nineauth.xyz/docs">Documentation</a>
  &nbsp;&middot;&nbsp;
  <a href="https://nineauth.xyz/dashboard">Dashboard</a>
</div>

---

## 📋 Overview

The **NineAuth C# SDK** provides a type-safe, asynchronous client for integrating server-side authentication, hardware ID (HWID) locking, license activation, and entitlement enforcement into Windows desktop software, launchers, and .NET applications.

### Key Capabilities

- 🔐 **User Authentication:** Argon2id credential verification with automatic session token caching in memory.
- 💻 **Hardware Fingerprint Binding (HWID):** Native Windows `MachineGuid` SHA-256 hashing to enforce seat limits.
- 🎟️ **License Key Activation:** Bind customer licenses to specific devices with automatic expiration enforcement.
- ⚡ **Entitlement Verification:** Zero-overhead permission checks for feature gating.
- 🔄 **Automated Anti-Replay:** 128-bit cryptographic nonces and UTC ISO 8601 timestamps generated automatically for every sensitive request.
- 🛡️ **Zero Exception Crashes:** Structured exception hierarchy (`NineAuthException`) with typed error codes.

---

## 🛠️ Requirements & Compatibility

- **Runtimes:** .NET 8.0+, .NET 7.0, .NET 6.0, or .NET Framework 4.8+
- **Frameworks:** Windows Forms (WinForms), WPF, Avalonia, Console CLI, Unity 2021+
- **IDE:** Visual Studio 2022 (Community / Professional / Enterprise) or JetBrains Rider

---

## 🚀 Quick Start (5 Minutes)

### 1. Configure your Application ID

Open `Form1.cs` (or your startup configuration) and insert your `ApplicationId` from the [NineAuth Dashboard](https://nineauth.xyz/dashboard):

```csharp
private const string ApplicationId = "app_xxxxxxxxxxxxxxxx";
```

### 2. Initialize the Client

Initialize the SDK once during application startup:

```csharp
using NineAuth.Client;

var client = await NineAuthClient.InitializeAsync(new NineAuthOptions
{
    ApplicationId = "app_xxxxxxxxxxxxxxxx",
    Environment = "production" // or custom BaseUrl
});
```

---

## 💡 Code Recipes & Integration Patterns

### 1. User Registration

Register a new user account directly from your client application:

```csharp
try
{
    await client.RegisterAsync(email: "user@example.com", password: "SecurePassword123!");
    Console.WriteLine("Account created successfully. You can now log in.");
}
catch (NineAuthException ex)
{
    Console.WriteLine($"Registration failed: [{ex.ErrorCode}] {ex.Message}");
}
```

---

### 2. User Login with HWID Binding

Authenticate the user and automatically bind their session to the local hardware fingerprint:

```csharp
// Generate stable device fingerprint
string hwid = GetDeviceFingerprint();

try
{
    var authResult = await client.LoginAsync("user@example.com", "SecurePassword123!", hwid);
    Console.WriteLine($"Logged in as: {authResult.User.Email}");
}
catch (NineAuthException ex)
{
    Console.WriteLine($"Login rejected: [{ex.ErrorCode}] {ex.Message}");
}
```

---

### 3. Activating a License Key

Bind a purchased license key to the current device:

```csharp
try
{
    var activation = await client.ActivateLicenseAsync("NINE-XXXX-XXXX-XXXX", hwid);
    
    Console.WriteLine($"Status: {activation.License.Status}");
    Console.WriteLine($"Expires: {activation.License.ExpiresAt?.ToString("yyyy-MM-dd") ?? "Lifetime"}");
    Console.WriteLine($"Seats Used: {activation.License.UsedSeats}/{activation.License.MaxSeats}");
}
catch (NineAuthException ex)
{
    Console.WriteLine($"Activation failed: [{ex.ErrorCode}] {ex.Message}");
}
```

---

### 4. Validating Session & Checking Entitlements

Validate the current runtime session against the server to ensure the license has not been revoked or expired:

```csharp
var session = await client.ValidateSessionAsync();

if (!session.Valid)
{
    throw new NineAuthException(session.Reason ?? "License invalid.", "LICENSE_INVALID");
}

// Check feature gates (entitlements)
if (session.Entitlements.Contains("pro_features"))
{
    EnableProFeatures();
}
```

---

### 5. Hardware Reset (Transfer License)

If a user replaces their hardware, they can consume a reset quota:

```csharp
try
{
    var result = await client.ResetDeviceAsync("NINE-XXXX-XXXX-XXXX", newHwid);
    Console.WriteLine($"HWID reset successful. Remaining resets: {result.DeviceResetsRemaining}");
}
catch (NineAuthException ex)
{
    Console.WriteLine($"Reset failed: [{ex.ErrorCode}] {ex.Message}");
}
```

---

### 6. Secure Logout & Cleanup

Revoke the active session on the server and dispose local in-memory credentials:

```csharp
await client.LogoutAsync();
client.Dispose();
```

---

## 🔒 Generating Hardware Fingerprints (HWID)

The reference implementation uses a native Windows `MachineGuid` hash (SHA-256) without requiring third-party dependencies:

```csharp
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

public static string GetDeviceFingerprint()
{
    var machineGuid = Registry.GetValue(
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", 
        "MachineGuid", 
        null
    )?.ToString() ?? Environment.MachineName;

    using var sha256 = SHA256.Create();
    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineGuid));
    return Convert.ToHexString(hash);
}
```

> 💡 **Tip:** For stricter anti-tamper security in production, combine CPU serial numbers, motherboard UUID, and disk serial numbers into a single aggregated HMAC hash.

---

## 📁 Project Structure

```text
NineAuth-CSHARP-Example/
├── NineAuth.Client/                 ← Core SDK library
│   ├── Exceptions/
│   │   └── NineAuthException.cs     ← Typed error handling
│   ├── Models/                      ← API payload and response types
│   ├── NineAuthClient.cs            ← Main async HTTP client
│   ├── NineAuthOptions.cs           ← Client configuration
│   └── Polyfills.cs                 ← Backward compatibility shims
├── Form1.cs                         ← WinForms UI implementation & event handlers
├── Form1.Designer.cs                ← Visual UI designer layout
└── Program.cs                       ← WinForms entry point
```

---

## 🛡️ Production Hardening Checklist

When shipping software protected with NineAuth:

1. **Enable Code Obfuscation:** Use tools like ConfuserEx, Obfuscar, or VMProtect on your compiled `.exe` / `.dll` to prevent decompilation.
2. **Never Hardcode Secrets:** Only embed your public `ApplicationId`. Never embed administrative API keys or master credentials in client code.
3. **Periodic Heartbeats:** Call `ValidateSessionAsync()` periodically (e.g. every 5–15 minutes) in a background worker to detect remote revocations.
4. **Clean Session on Exit:** Hook `FormClosed` or `AppDomain.CurrentDomain.ProcessExit` to call `client.LogoutAsync()`.

---

<div align="center">
  <sub>NineAuth C# SDK · <a href="https://nineauth.xyz">nineauth.xyz</a></sub>
</div>
