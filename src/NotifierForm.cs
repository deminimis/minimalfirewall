using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkModeForms;


namespace MinimalFirewall
{
    public partial class NotifierForm : Form
    {
        // Enums and Properties
        public enum NotifierResult { Ignore, Allow, Block, TemporaryAllow, CreateWildcard }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NotifierResult Result { get; set; } = NotifierResult.Ignore;
        public PendingConnectionViewModel? PendingConnection { get; private set; }
        public FirewallRuleChange? RuleChange { get; private set; }
        public TimeSpan TemporaryDuration { get; private set; }
        public bool TrustPublisher { get; private set; } = false;


        // Settings file > window position
        private readonly string _layoutSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notifier_layout.json");

        private void ApplyThemeStyles(bool isDarkMode)
        {
            Theme.Colors = Theme.GetSystemColors(isDarkMode ? 0 : 1);
            Theme.ApplyTitleBarTheme(this.Handle, isDarkMode ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            this.BackColor = Theme.Colors.Background;
            this.ForeColor = Theme.Colors.TextInactive;

            pathLabel.BackColor = Theme.Colors.PathLabelBackground;
            pathLabel.ForeColor = Theme.Colors.TextActive;

            allowButton.BackColor = Theme.Colors.Success;
            blockButton.BackColor = Theme.Colors.Danger;
            allowButton.ForeColor = Color.Black;
            blockButton.ForeColor = Color.Black;

            allowButton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(Theme.Colors.Success, 0.1f);
            blockButton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(Theme.Colors.Danger, 0.1f);
            allowButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Theme.Colors.Success, 0.2f);
            blockButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Theme.Colors.Danger, 0.2f);
        }

        public NotifierForm(FirewallRuleChange rule, bool isDarkMode)
        {
            InitializeComponent();
            RuleChange = rule;

            
            ApplyThemeStyles(isDarkMode);

            string appName = string.IsNullOrEmpty(rule.ApplicationName) ? rule.Name : Path.GetFileName(rule.ApplicationName);
            Text = "New Firewall Rule Detected";
            string actionText = rule.Rule.Status.Equals("Allow", StringComparison.OrdinalIgnoreCase) ? "allow" : "block";
            infoLabel.Text = $"An application just created a firewall rule to {actionText} traffic:";
            pathLabel.Text = string.IsNullOrEmpty(rule.ApplicationName) ? $"Rule Name: {rule.Name}" : rule.ApplicationName;
            pathLabel.WordWrap = false;

            AcceptButton = ignoreButton;

            tempAllowButton?.Visible = false;
            createWildcardButton?.Visible = false;
            trustPublisherCheckBox?.Visible = false;
        }

        public NotifierForm(PendingConnectionViewModel pending, bool isDarkMode)
        {
            InitializeComponent();
            PendingConnection = pending;

            ApplyThemeStyles(isDarkMode);

            string appName = string.IsNullOrEmpty(pending.ServiceName) ? pending.FileName : $"{pending.FileName} ({pending.ServiceName})";
            Text = "Connection Blocked";
            infoLabel.Text = $"Blocked a {pending.Direction} connection for:";
            appNameLabel.Text = appName;
            pathLabel.Text = pending.AppPath;
            pathLabel.WordWrap = false;

            AcceptButton = ignoreButton;

            SetupTempAllowMenu();
        }

        // Restore window position
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set notification above other apps
            TopMost = true;
            Activate();

            try
            {
                if (File.Exists(_layoutSettingsPath))
                {
                    string json = File.ReadAllText(_layoutSettingsPath);
                    var settings = JsonSerializer.Deserialize<NotifierLayoutSettings>(json);

                    if (settings != null)
                    {
                        // Restore Size
                        if (settings.Width >= MinimumSize.Width && settings.Height >= MinimumSize.Height)
                        {
                            Size = new Size(settings.Width, settings.Height);
                        }

                        // Restore Location only if visible on current screens
                        var savedLoc = new Point(settings.X, settings.Y);
                        var targetRect = new Rectangle(savedLoc, Size);
                        bool isVisible = false;

                        // Check intersection to ensure window isn't lost off-screen
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.WorkingArea.IntersectsWith(targetRect))
                            {
                                isVisible = true;
                                break;
                            }
                        }

                        if (isVisible)
                        {
                            StartPosition = FormStartPosition.Manual;
                            Location = savedLoc;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] NotifierForm.OnLoad failed to restore layout: {ex.Message}");
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                if (PendingConnection == null)
                {
                    return;
                }

                string? publisherName = null;
                bool hasInfo = await Task.Run(() => SignatureValidationService.IsSignatureTrusted(PendingConnection.AppPath, out publisherName));

                if (hasInfo && !string.IsNullOrEmpty(publisherName))
                {
                    if (publisherName.Length > 30)
                    {
                        publisherName = string.Concat(publisherName.AsSpan(0, 30), "...");
                    }
                    trustPublisherCheckBox.Text = $"Trust: {publisherName}";
                    trustPublisherCheckBox.Visible = true;
                }
                else
                {
                    trustPublisherCheckBox.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] NotifierForm.OnShown failed to load publisher info: {ex.Message}");
                trustPublisherCheckBox.Visible = false;
            }
        }

        // Window Closing: Save position
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (WindowState == FormWindowState.Normal)
                {
                    var settings = new NotifierLayoutSettings
                    {
                        X = Location.X,
                        Y = Location.Y,
                        Width = Size.Width,
                        Height = Size.Height
                    };

                    string json = JsonSerializer.Serialize(settings);
                    File.WriteAllText(_layoutSettingsPath, json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] NotifierForm.OnFormClosing failed to save layout: {ex.Message}");
            }

            base.OnFormClosing(e);
        }

        private void SetupTempAllowMenu()
        {
            tempAllowContextMenu.Items.Add("For 2 minutes").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromMinutes(2));
            tempAllowContextMenu.Items.Add("For 5 minutes").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromMinutes(5));
            tempAllowContextMenu.Items.Add("For 15 minutes").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromMinutes(15));
            tempAllowContextMenu.Items.Add("For 1 hour").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromHours(1));
            tempAllowContextMenu.Items.Add("For 3 hours").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromHours(3));
            tempAllowContextMenu.Items.Add("For 8 hours").Click += (s, e) => SetTemporaryAllow(TimeSpan.FromHours(8));
        }

        private void SetTemporaryAllow(TimeSpan duration)
        {
            Result = NotifierResult.TemporaryAllow;
            TemporaryDuration = duration;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void AllowButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Allow;
            TrustPublisher = trustPublisherCheckBox.Visible && trustPublisherCheckBox.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BlockButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Block;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void IgnoreButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Ignore;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TempAllowButton_Click(object sender, EventArgs e)
        {
            tempAllowContextMenu.Show(tempAllowButton, new Point(0, tempAllowButton.Height));
        }

        private void CreateWildcardButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.CreateWildcard;
            DialogResult = DialogResult.OK;
            Close();
        }

        private async void CopyDetailsButton_Click(object sender, EventArgs e)
        {
            try
            {
                var details = new System.Text.StringBuilder();
                if (PendingConnection == null && RuleChange != null)
                {
                    details.AppendLine($"Type: New Rule Detected");
                    details.AppendLine($"Rule Name: {RuleChange.Name}");
                    details.AppendLine($"Path: {RuleChange.ApplicationName}");
                    details.AppendLine($"Publisher: {RuleChange.Publisher}");
                }
                else if (PendingConnection != null)
                {
                    details.AppendLine($"Type: Pending Connection");
                    details.AppendLine($"Application: {PendingConnection.FileName}");
                    details.AppendLine($"Path: {PendingConnection.AppPath}");
                    details.AppendLine($"PID: {PendingConnection.ProcessId}");
                    if (!string.IsNullOrEmpty(PendingConnection.ProcessOwner))
                    {
                        details.AppendLine($"Owner: {PendingConnection.ProcessOwner}");
                    }

                    if (!string.IsNullOrEmpty(PendingConnection.ParentProcessId))
                    {
                        string parentDisplay = string.IsNullOrEmpty(PendingConnection.ParentProcessName) ?
                            PendingConnection.ParentProcessId : $"{PendingConnection.ParentProcessName} (PID: {PendingConnection.ParentProcessId})";
                        details.AppendLine($"Parent Process: {parentDisplay}");
                    }
                    details.AppendLine($"Service: {PendingConnection.ServiceName}");
                    details.AppendLine($"Direction: {PendingConnection.Direction}");
                    if (!string.IsNullOrEmpty(PendingConnection.CommandLine))
                    {
                        details.AppendLine($"CMD: {PendingConnection.CommandLine}");
                    }
                }
                // clipboard retry logic
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        Clipboard.SetText(details.ToString());
                        break;
                    }
                    catch (ExternalException)
                    {
                        if (i == 4)
                        {
                            throw;
                        }

                        await Task.Delay(50);
                    }
                }

                copyDetailsButton.Text = "✓";

                await Task.Delay(2000);

                if (!IsDisposed && IsHandleCreated)
                {
                    copyDetailsButton.Text = "📋";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] NotifierForm.copyDetailsButton_Click failed: {ex.Message}");
            }
        }

        public class NotifierLayoutSettings
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}
