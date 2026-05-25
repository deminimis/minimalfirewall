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
            statusLabel = new DarkModeForms.ThemedLabel();
            okButton = new DarkModeForms.ThemedButton();
            progressBar = new ProgressBar();
            progressLabel = new DarkModeForms.ThemedLabel();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Location = new Point(7, 24);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(443, 28);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Scanning, please wait...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom;
            okButton.Location = new Point(171, 108);
            okButton.Name = "okButton";
            okButton.Size = new Size(114, 43);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Visible = false;
            okButton.Click += okButton_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(32, 72);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(320, 28);
            progressBar.TabIndex = 1;
            // 
            // progressLabel
            // 
            progressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressLabel.Location = new Point(359, 72);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(67, 28);
            progressLabel.TabIndex = 3;
            progressLabel.Text = "0%";
            progressLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // StatusForm
            // 
            AutoScaleDimensions = new SizeF(8F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(457, 180);
            Controls.Add(progressLabel);
            Controls.Add(okButton);
            Controls.Add(progressBar);
            Controls.Add(statusLabel);
            Font = new Font("Roboto Mono", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StatusForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scanning...";
            ResumeLayout(false);

        }

        #endregion
    }
}
