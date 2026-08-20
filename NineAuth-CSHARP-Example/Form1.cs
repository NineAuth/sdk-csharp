using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using NineAuth.Client;
using NineAuth.Client.Exceptions;

namespace NineAuth_CSHARP_Example;

public partial class Form1 : Form
{
    private const string ApplicationId = "APPLICATION_ID AQUI"; // ← coloca aqui o teu Application ID
    private NineAuthClient? _client;
    private readonly string _deviceFingerprint = CreateDeviceFingerprint();

    public Form1() => InitializeComponent();

    private async void Form1_Load(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            _client = await NineAuthClient.InitializeAsync(new NineAuthOptions
            {
                ApplicationId = ApplicationId,
                Environment = "production"
            });
            SetStatus("Pronto. Entre para ativar a sua licença.");
        });
    }

    private async void loginButton_Click(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            await Client.LoginAsync(emailTextBox.Text.Trim(), passwordTextBox.Text, _deviceFingerprint);
            SetStatus("Sessão iniciada. Introduza a sua chave de licença.");
        });
    }

    private async void registerButton_Click(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            await Client.RegisterAsync(emailTextBox.Text.Trim(), passwordTextBox.Text);
            SetStatus("Conta criada. Agora entre com as mesmas credenciais.");
        });
    }

    private async void activateButton_Click(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            var activation = await Client.ActivateLicenseAsync(licenseKeyTextBox.Text.Trim(), _deviceFingerprint);
            var session = await Client.ValidateSessionAsync();
            if (!session.Valid) throw new NineAuthException(session.Reason ?? "Licença inválida.", "LICENSE_INVALID");

            accessLabel.Text = $"Acesso autorizado\r\n\r\nLicença: {activation.License.Status}\r\nExpira: {activation.License.ExpiresAt?.ToLocalTime().ToString("g") ?? "sem expiração"}\r\n\r\nPermissões: {string.Join(", ", session.Entitlements)}";
            protectedTab.Enabled = true;
            mainTabControl.SelectedTab = protectedTab;
            SetStatus("Licença ativa neste dispositivo.", true);
        });
    }

    private async void resetDeviceButton_Click(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            var result = await Client.ResetDeviceAsync(licenseKeyTextBox.Text.Trim(), _deviceFingerprint);
            SetStatus($"Dispositivo reposto. Restam {result.DeviceResetsRemaining} reposições.");
        });
    }

    private async void logoutButton_Click(object? sender, EventArgs e)
    {
        await RunAsync(async () =>
        {
            await Client.LogoutAsync();
            accessLabel.Text = "Área protegida\r\n\r\nAtive uma licença para continuar.";
            protectedTab.Enabled = false;
            mainTabControl.SelectedTab = accountTab;
            SetStatus("Sessão terminada.");
        });
    }

    private NineAuthClient Client => _client ?? throw new InvalidOperationException("O cliente ainda está a inicializar.");

    private async Task RunAsync(Func<Task> action)
    {
        SetControlsEnabled(false);
        try { await action(); }
        catch (NineAuthException ex) { SetStatus(ex.Message, false); }
        catch (ArgumentException ex) { SetStatus(ex.Message, false); }
        catch (Exception) { SetStatus("Ocorreu um erro inesperado. Tente novamente.", false); }
        finally { SetControlsEnabled(_client is not null); }
    }

    private void SetControlsEnabled(bool enabled)
    {
        foreach (Control control in Controls) control.Enabled = enabled;
        titleLabel.Enabled = true;
        statusLabel.Enabled = true;
    }

    private void SetStatus(string message, bool success = true)
    {
        statusLabel.Text = message;
        statusLabel.ForeColor = success ? Color.SeaGreen : Color.Firebrick;
    }

    private static string CreateDeviceFingerprint()
    {
        // ponytail: stable Windows MachineGuid hash; use a dedicated hardware-ID provider only if stricter binding is required.
        var machineGuid = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", null)?.ToString()
            ?? Environment.MachineName;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(machineGuid)));
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        if (_client is not null)
        {
            try { await _client.LogoutAsync(); }
            catch { /* The local session is discarded regardless of network availability. */ }
            _client.Dispose();
        }
        base.OnFormClosed(e);
    }

}
