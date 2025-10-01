// File: StatusForm.cs
using DarkModeForms;
namespace MinimalFirewall
{
    public partial class StatusForm : Form
    {
        private readonly DarkModeCS dm;
        private bool _isMarquee = true;

        public StatusForm(string title)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            this.Text = title;
            this.statusLabel.Text = title;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressLabel.Text = "Starting...";
        }

        public void UpdateStatus(string message)
        {
            this.statusLabel.Text = message;
            Application.DoEvents();
        }

        public void UpdateProgress(int percentage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateProgress(percentage));
                return;
            }

            if (_isMarquee && percentage > 0)
            {
                _isMarquee = false;
                progressBar.Style = System.Windows.Forms.ProgressBarStyle.Blocks;
            }

            progressBar.Value = Math.Clamp(percentage, 0, 100);
            progressLabel.Text = $"{percentage}%";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Owner != null)
            {
                Location = new Point(Owner.Location.X + (Owner.Width - Width) / 2,
                                       Owner.Location.Y + (Owner.Height - Height) / 2);
            }
        }

        public void Complete(string message)
        {
            this.statusLabel.Text = message;
            this.progressBar.Visible = false;
            this.progressLabel.Visible = false;
            this.okButton.Visible = true;
            this.Text = "Scan Complete";
            this.okButton.Focus();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}