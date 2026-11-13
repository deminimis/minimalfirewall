// File: StatusForm.Designer.cs
namespace MinimalFirewall
{
    public partial class StatusForm
    {
        private System.ComponentModel.IContainer components = null;
        private DarkModeForms.ThemedLabel statusLabel;
        private System.Windows.Forms.ProgressBar progressBar;
        private DarkModeForms.ThemedLabel progressLabel;
        private DarkModeForms.ThemedButton okButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _initialLoadTimer?.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            statusLabel = new DarkModeForms.ThemedLabel
            {
                Name = "statusLabel", AutoSize = true, Dock = DockStyle.Fill,
                Text = "Scanning, please wait...", MaximumSize = new Size(388, 0),
                Margin = new Padding(0, 0, 0, 16)
            };
            progressBar = new ProgressBar { Name = "progressBar", Dock = DockStyle.Fill, MinimumSize = new Size(120, 24), Margin = new Padding(0, 0, 12, 0) };
            progressLabel = new DarkModeForms.ThemedLabel
            {
                Name = "progressLabel", AutoSize = true, Text = "0%", Anchor = AnchorStyles.Left, Margin = Padding.Empty
            };
            okButton = new DarkModeForms.ThemedButton
            {
                Name = "okButton", AutoSize = true, MinimumSize = new Size(90, 32), Text = "OK", Visible = false
            };
            okButton.Click += okButton_Click;
            var progress = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
            progress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            progress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            progress.Controls.Add(progressBar, 0, 0);
            progress.Controls.Add(progressLabel, 1, 0);
            var commands = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 16, 0, 0) };
            commands.Controls.Add(okButton);
            var layout = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, ColumnCount = 1, Padding = new Padding(16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(statusLabel, 0, 0);
            layout.Controls.Add(progress, 0, 1);
            layout.Controls.Add(commands, 0, 2);
            Controls.Add(layout);
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = SystemFonts.MessageBoxFont;
            ClientSize = new Size(420, 150);
            MinimumSize = new Size(360, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StatusForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scanning...";
            layout.SizeChanged += (_, _) => ClientSize = new Size(ClientSize.Width, layout.Height);
        }

        #endregion
    }
}
