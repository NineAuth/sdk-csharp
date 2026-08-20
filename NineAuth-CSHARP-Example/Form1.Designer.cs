#nullable enable

namespace NineAuth_CSHARP_Example
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;
        private Label statusLabel = null!;
        private TextBox emailTextBox = null!;
        private TextBox passwordTextBox = null!;
        private TextBox licenseKeyTextBox = null!;
        private Button loginButton = null!;
        private Button registerButton = null!;
        private Button activateButton = null!;
        private Button resetDeviceButton = null!;
        private Button logoutButton = null!;
        private Panel protectedPanel = null!;
        private Label accessLabel = null!;
        private TabControl mainTabControl = null!;
        private TabPage accountTab = null!;
        private TabPage licenseTab = null!;
        private TabPage protectedTab = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            statusLabel = new Label();
            emailTextBox = new TextBox();
            passwordTextBox = new TextBox();
            licenseKeyTextBox = new TextBox();
            loginButton = new Button();
            registerButton = new Button();
            activateButton = new Button();
            resetDeviceButton = new Button();
            logoutButton = new Button();
            protectedPanel = new Panel();
            accessLabel = new Label();
            mainTabControl = new TabControl();
            accountTab = new TabPage();
            licenseTab = new TabPage();
            protectedTab = new TabPage();
            titleLabel = new Label();
            protectedPanel.SuspendLayout();
            mainTabControl.SuspendLayout();
            accountTab.SuspendLayout();
            licenseTab.SuspendLayout();
            protectedTab.SuspendLayout();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.ForeColor = Color.DimGray;
            statusLabel.Location = new Point(33, 66);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(130, 15);
            statusLabel.TabIndex = 1;
            statusLabel.Text = "A inicializar NineAuth…";
            // 
            // emailTextBox
            // 
            emailTextBox.Location = new Point(25, 30);
            emailTextBox.Name = "emailTextBox";
            emailTextBox.PlaceholderText = "Email";
            emailTextBox.Size = new Size(350, 23);
            emailTextBox.TabIndex = 0;
            // 
            // passwordTextBox
            // 
            passwordTextBox.Location = new Point(25, 68);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.PasswordChar = '●';
            passwordTextBox.PlaceholderText = "Palavra-passe";
            passwordTextBox.Size = new Size(350, 23);
            passwordTextBox.TabIndex = 1;
            // 
            // licenseKeyTextBox
            // 
            licenseKeyTextBox.Location = new Point(25, 30);
            licenseKeyTextBox.Name = "licenseKeyTextBox";
            licenseKeyTextBox.PlaceholderText = "Chave de licença";
            licenseKeyTextBox.Size = new Size(350, 23);
            licenseKeyTextBox.TabIndex = 0;
            // 
            // loginButton
            // 
            loginButton.Location = new Point(25, 109);
            loginButton.Name = "loginButton";
            loginButton.Size = new Size(168, 34);
            loginButton.TabIndex = 2;
            loginButton.Text = "Entrar";
            loginButton.Click += loginButton_Click;
            // 
            // registerButton
            // 
            registerButton.Location = new Point(207, 109);
            registerButton.Name = "registerButton";
            registerButton.Size = new Size(168, 34);
            registerButton.TabIndex = 3;
            registerButton.Text = "Criar conta";
            registerButton.Click += registerButton_Click;
            // 
            // activateButton
            // 
            activateButton.Location = new Point(25, 70);
            activateButton.Name = "activateButton";
            activateButton.Size = new Size(350, 34);
            activateButton.TabIndex = 1;
            activateButton.Text = "Ativar licença neste dispositivo / Login";
            activateButton.Click += activateButton_Click;
            // 
            // resetDeviceButton
            // 
            resetDeviceButton.Location = new Point(25, 118);
            resetDeviceButton.Name = "resetDeviceButton";
            resetDeviceButton.Size = new Size(170, 30);
            resetDeviceButton.TabIndex = 2;
            resetDeviceButton.Text = "Reset HWID";
            resetDeviceButton.Click += resetDeviceButton_Click;
            // 
            // logoutButton
            // 
            logoutButton.Location = new Point(205, 118);
            logoutButton.Name = "logoutButton";
            logoutButton.Size = new Size(170, 30);
            logoutButton.TabIndex = 3;
            logoutButton.Text = "Terminar sessão";
            logoutButton.Click += logoutButton_Click;
            // 
            // protectedPanel
            // 
            protectedPanel.BackColor = Color.FromArgb(232, 248, 238);
            protectedPanel.Controls.Add(accessLabel);
            protectedPanel.Dock = DockStyle.Fill;
            protectedPanel.Location = new Point(0, 0);
            protectedPanel.Name = "protectedPanel";
            protectedPanel.Size = new Size(712, 242);
            protectedPanel.TabIndex = 0;
            // 
            // accessLabel
            // 
            accessLabel.Dock = DockStyle.Fill;
            accessLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            accessLabel.Location = new Point(0, 0);
            accessLabel.Name = "accessLabel";
            accessLabel.Size = new Size(712, 242);
            accessLabel.TabIndex = 0;
            accessLabel.Text = "Área protegida\r\n\r\nAtive uma licença para continuar.";
            accessLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(accountTab);
            mainTabControl.Controls.Add(licenseTab);
            mainTabControl.Controls.Add(protectedTab);
            mainTabControl.Location = new Point(30, 96);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(720, 270);
            mainTabControl.TabIndex = 2;
            // 
            // accountTab
            // 
            accountTab.Controls.Add(emailTextBox);
            accountTab.Controls.Add(passwordTextBox);
            accountTab.Controls.Add(loginButton);
            accountTab.Controls.Add(registerButton);
            accountTab.Location = new Point(4, 24);
            accountTab.Name = "accountTab";
            accountTab.Padding = new Padding(10);
            accountTab.Size = new Size(712, 242);
            accountTab.TabIndex = 0;
            accountTab.Text = "Conta";
            // 
            // licenseTab
            // 
            licenseTab.Controls.Add(licenseKeyTextBox);
            licenseTab.Controls.Add(activateButton);
            licenseTab.Controls.Add(resetDeviceButton);
            licenseTab.Controls.Add(logoutButton);
            licenseTab.Location = new Point(4, 24);
            licenseTab.Name = "licenseTab";
            licenseTab.Padding = new Padding(10);
            licenseTab.Size = new Size(712, 242);
            licenseTab.TabIndex = 1;
            licenseTab.Text = "Licença";
            // 
            // protectedTab
            // 
            protectedTab.Controls.Add(protectedPanel);
            protectedTab.Enabled = false;
            protectedTab.Location = new Point(4, 24);
            protectedTab.Name = "protectedTab";
            protectedTab.Size = new Size(712, 242);
            protectedTab.TabIndex = 2;
            protectedTab.Text = "Área protegida";
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.Location = new Point(30, 24);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(478, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Exemplo NineAuth — Acesso por licença";
            // 
            // Form1
            // 
            AcceptButton = loginButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(790, 400);
            Controls.Add(titleLabel);
            Controls.Add(statusLabel);
            Controls.Add(mainTabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NineAuth Login";
            Load += Form1_Load;
            protectedPanel.ResumeLayout(false);
            mainTabControl.ResumeLayout(false);
            accountTab.ResumeLayout(false);
            accountTab.PerformLayout();
            licenseTab.ResumeLayout(false);
            licenseTab.PerformLayout();
            protectedTab.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private Label titleLabel;
    }
}
