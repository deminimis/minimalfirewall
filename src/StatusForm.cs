// StatusForm.cs
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class StatusForm : Form
    {
        private readonly DarkModeCS dm;

        public StatusForm(string title)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            this.Text = title;
            this.statusLabel.Text = "Scanning, please wait...";
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
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
