// NotifierForm.cs
using DarkModeForms;
using System.Drawing;

namespace MinimalFirewall
{
    public partial class NotifierForm : Form
    {
        public enum NotifierResult { Ignore, Allow, Block, TemporaryAllow, CreateWildcard }
        public NotifierResult Result { get; private set; } = NotifierResult.Ignore;
        public PendingConnectionViewModel PendingConnection { get; private set; }
        public TimeSpan TemporaryDuration { get; private set; }
        public bool TrustPublisher { get; private set; } = false;

        private readonly DarkModeCS dm;

        public NotifierForm(PendingConnectionViewModel pending, bool isDarkMode)
        {
            InitializeComponent();
            PendingConnection = pending;
            dm = new DarkModeCS(this)
            {
                ColorMode = isDarkMode ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode
            };
            dm.ApplyTheme(isDarkMode);
            string appName = string.IsNullOrEmpty(pending.ServiceName) ? pending.FileName : $"{pending.FileName} ({pending.ServiceName})";
            this.Text = "Connection Blocked";
            infoLabel.Text = $"Blocked a {pending.Direction} connection for:";
            appNameLabel.Text = appName;
            pathLabel.Text = pending.AppPath;

            allowButton.Text = $"Allow {pending.Direction}";
            blockButton.Text = $"Block {pending.Direction}";

            // Customize button colors
            allowButton.FlatStyle = FlatStyle.Flat;
            blockButton.FlatStyle = FlatStyle.Flat;

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

            allowButton.FlatAppearance.BorderSize = 0;
            blockButton.FlatAppearance.BorderSize = 0;


            SetupTempAllowMenu();
            SetupTrustPublisherCheckBox();
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
                trustPublisherCheckBox.Text = $"Always trust publisher: {publisherName}";
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
            Result = wildcardCheckBox.Checked ? NotifierResult.CreateWildcard : NotifierResult.Allow;
            TrustPublisher = trustPublisherCheckBox.Visible && trustPublisherCheckBox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void blockButton_Click(object sender, EventArgs e)
        {
            Result = wildcardCheckBox.Checked ? NotifierResult.CreateWildcard : NotifierResult.Block;
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

        private void wildcardCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isWildcard = wildcardCheckBox.Checked;
            tempAllowButton.Visible = !isWildcard;
            ignoreButton.Visible = !isWildcard;
            trustPublisherCheckBox.Visible = !isWildcard && trustPublisherCheckBox.Text.Length > 0;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.TopMost = true;
            this.Activate();
        }
    }
}