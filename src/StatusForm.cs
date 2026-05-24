using DarkModeForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public partial class StatusForm : Form
    {
        
        protected System.Windows.Forms.Timer _initialLoadTimer;

        private double _fakeProgress;
        private bool _realProgressStarted;

        public StatusForm(string title, AppSettings appSettings)
        {
            InitializeComponent();
            _initialLoadTimer = new System.Windows.Forms.Timer { Interval = 50 };
            ApplyTheme(appSettings);
            SetupInitialState(title);
            ConfigureTimers();
        }

        private void ApplyTheme(AppSettings appSettings)
        {
            bool isAuto = appSettings.Theme == "Auto";
            bool isDark = isAuto ? Theme.IsSystemDarkMode() : appSettings.Theme == "Dark";

            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(this.Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            this.BackColor = Theme.Colors.Background;
            this.ForeColor = Theme.Colors.TextInactive;

            var styler = new ControlStyler(Theme.Colors, isDark);
            styler.ApplyStyle(this);
        }

        private void SetupInitialState(string title)
        {
            Text = title;
            statusLabel.Text = title;
        }

        private void ConfigureTimers()
        {
            _fakeProgress = 0;
            _realProgressStarted = false;
            _initialLoadTimer.Tick += InitialLoadTimer_Tick;
            _initialLoadTimer.Start();
        }

        private void InitialLoadTimer_Tick(object? sender, EventArgs e)
        {
            if (_realProgressStarted)
            {
                _initialLoadTimer.Stop();
                return;
            }


            double increment = 0;

            if (_fakeProgress < 15)
            {
                increment = 1.5; 
            }
            else if (_fakeProgress < 40)
            {
                increment = 0.5; 
            }
            else if (_fakeProgress < 80)
            {
                increment = 0.1; 
            }

            if (_fakeProgress < 90)
            {
                _fakeProgress += increment;
            }

            int visualValue = (int)Math.Min(_fakeProgress, 100);

            // Only update if value actually changed to prevent flicker
            if (progressBar.Value != visualValue)
            {
                progressBar.Value = visualValue;
                progressLabel.Text = $"{visualValue}%";
            }
        }

        public void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(message)));
                return;
            }

            statusLabel.Text = message;
        }

        public void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateProgress(percentage));
                return;
            }


            _realProgressStarted = true;
            _initialLoadTimer.Stop();

            // Ensure we don't jump backward if the fake progress got ahead of the real scan
            int newProgress = Math.Max((int)_fakeProgress, percentage);

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
            if (InvokeRequired)
            {
                Invoke(new Action(() => Complete(message)));
                return;
            }

            statusLabel.Text = message;
            progressBar.Visible = false;
            progressLabel.Visible = false;
            okButton.Visible = true;
            Text = "Scan Complete";
            okButton.Focus();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
