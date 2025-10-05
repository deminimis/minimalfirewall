// File: StatusForm.cs
using DarkModeForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public partial class StatusForm : Form
    {
        private readonly DarkModeCS dm;
        private System.Windows.Forms.Timer _initialLoadTimer;
        private int _fakeProgress;
        private bool _realProgressStarted;

        public StatusForm(string title)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            this.Text = title;
            this.statusLabel.Text = title;
            this.progressLabel.Text = "0%";
            this.progressBar.Value = 0;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Blocks;

            _fakeProgress = 0;
            _realProgressStarted = false;

            _initialLoadTimer = new System.Windows.Forms.Timer
            {
                Interval = 150
            };
            _initialLoadTimer.Tick += InitialLoadTimer_Tick;
            _initialLoadTimer.Start();

            this.FormClosing += (s, e) => _initialLoadTimer?.Dispose();
        }

        private void InitialLoadTimer_Tick(object? sender, EventArgs e)
        {
            _fakeProgress++;
            progressBar.Value = _fakeProgress;
            progressLabel.Text = $"{_fakeProgress}%";

            if (_fakeProgress >= 10)
            {
                _initialLoadTimer.Stop();
            }
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

            if (!_realProgressStarted)
            {
                _realProgressStarted = true;
                _initialLoadTimer.Stop();
            }

            int newProgress = Math.Max(_fakeProgress, percentage);
            progressBar.Value = Math.Clamp(newProgress, 0, 100);
            progressLabel.Text = $"{progressBar.Value}%";
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