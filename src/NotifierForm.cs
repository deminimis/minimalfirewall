using DarkModeForms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace MinimalFirewall
{
    public partial class NotifierForm : Form
    {
        public enum NotifierResult { Ignore, Allow, Block, TemporaryAllow, CreateWildcard }
        public NotifierResult Result { get; set; } = NotifierResult.Ignore;
        public PendingConnectionViewModel PendingConnection { get; private set; }
        public TimeSpan TemporaryDuration { get; private set; }
        public bool TrustPublisher { get; private set; } = false;
        private readonly DarkModeCS dm;

        private readonly string _layoutSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notifier_layout.json");

        public NotifierForm(PendingConnectionViewModel pending, bool isDarkMode)
        {
            InitializeComponent();

            PendingConnection = pending;
            dm = new DarkModeCS(this)
            {
                ColorMode = isDarkMode ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode
            };
            dm.ApplyTheme(isDarkMode);

            if (isDarkMode)
            {
                pathLabel.BackColor = Color.FromArgb(45, 45, 48);
                pathLabel.ForeColor = Color.White;
            }

            string appName = string.IsNullOrEmpty(pending.ServiceName) ? pending.FileName : $"{pending.FileName} ({pending.ServiceName})";
            this.Text = "Connection Blocked";
            infoLabel.Text = $"Blocked a {pending.Direction} connection for:";
            appNameLabel.Text = appName;
            pathLabel.Text = pending.AppPath;

            this.AcceptButton = this.ignoreButton;

            // custom colors for buttons
            Color allowColor = Color.FromArgb(204, 255, 204);
            Color blockColor = Color.FromArgb(255, 204, 204);

            allowButton.BackColor = allowColor;
            blockButton.BackColor = blockColor;

            allowButton.ForeColor = Color.Black;
            blockButton.ForeColor = Color.Black;

            allowButton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(allowColor, 0.1f);
            blockButton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(blockColor, 0.1f);
            allowButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(allowColor, 0.2f);
            blockButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(blockColor, 0.2f);

            SetupTempAllowMenu();
            SetupTrustPublisherCheckBox();
        }

        // remember window position
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (File.Exists(_layoutSettingsPath))
                {
                    string json = File.ReadAllText(_layoutSettingsPath);
                    var settings = JsonSerializer.Deserialize<NotifierLayoutSettings>(json);

                    if (settings != null)
                    {
                        if (settings.Width >= this.MinimumSize.Width && settings.Height >= this.MinimumSize.Height)
                        {
                            this.Size = new Size(settings.Width, settings.Height);
                        }

                        // Check if the saved location is actually visible on screen
                        Point savedLoc = new Point(settings.X, settings.Y);
                        bool isVisible = false;
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.WorkingArea.Contains(savedLoc))
                            {
                                isVisible = true;
                                break;
                            }
                        }

                        if (isVisible)
                        {
                            this.StartPosition = FormStartPosition.Manual;
                            this.Location = savedLoc;
                        }
                    }
                }
            }
            catch { }
        }

        // save window position on close
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    var settings = new NotifierLayoutSettings
                    {
                        X = this.Location.X,
                        Y = this.Location.Y,
                        Width = this.Size.Width,
                        Height = this.Size.Height
                    };

                    string json = JsonSerializer.Serialize(settings);
                    File.WriteAllText(_layoutSettingsPath, json);
                }
            }
            catch { }

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

        private void SetupTrustPublisherCheckBox()
        {
            if (SignatureValidationService.GetPublisherInfo(PendingConnection.AppPath, out var publisherName) && publisherName != null)
            {
                if (publisherName.Length > 30)
                {
                    publisherName = publisherName.Substring(0, 30) + "...";
                }
                trustPublisherCheckBox.Text = $"Trust: {publisherName}";
                trustPublisherCheckBox.Visible = true;
            }
            else
            {
                trustPublisherCheckBox.Visible = false;
            }
        }

        private void SetTemporaryAllow(TimeSpan duration)
        {
            Result = NotifierResult.TemporaryAllow;
            TemporaryDuration = duration;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void allowButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Allow;
            TrustPublisher = trustPublisherCheckBox.Visible && trustPublisherCheckBox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void blockButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Block;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ignoreButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Ignore;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void tempAllowButton_Click(object sender, EventArgs e)
        {
            tempAllowContextMenu.Show(tempAllowButton, new Point(0, tempAllowButton.Height));
        }

        private void createWildcardButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.CreateWildcard;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void copyDetailsButton_Click(object sender, EventArgs e)
        {
            try
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Pending Connection");
                details.AppendLine($"Application: {PendingConnection.FileName}");
                details.AppendLine($"Path: {PendingConnection.AppPath}");
                details.AppendLine($"Service: {PendingConnection.ServiceName}");
                details.AppendLine($"Direction: {PendingConnection.Direction}");
                Clipboard.SetText(details.ToString());

                copyDetailsButton.Text = "✓";

                var t = new System.Windows.Forms.Timer();
                t.Interval = 2000;
                t.Tick += (s, args) =>
                {
                    copyDetailsButton.Text = "📋";
                    t.Stop();
                    t.Dispose();
                };
                t.Start();
            }
            catch
            {
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