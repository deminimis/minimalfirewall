// NotifierForm.cs
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class NotifierForm : Form
    {
        public enum NotifierResult { Ignore, Allow, Block }
        public NotifierResult Result { get; private set; } = NotifierResult.Ignore;
        public PendingConnectionViewModel PendingConnection { get; private set; }
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
        }

        private void allowButton_Click(object sender, EventArgs e)
        {
            Result = NotifierResult.Allow;
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.TopMost = true;
            this.Activate();
        }
    }
}
