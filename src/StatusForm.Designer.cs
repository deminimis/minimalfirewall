// File: StatusForm.Designer.cs
namespace MinimalFirewall
{
    public partial class StatusForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.Button okButton;

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
            statusLabel = new Label();
            okButton = new Button();
            progressBar = new ProgressBar();
            progressLabel = new Label();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Location = new Point(7, 21);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(443, 25);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Scanning, please wait...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom;
            okButton.Location = new Point(171, 96);
            okButton.Name = "okButton";
            okButton.Size = new Size(114, 38);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Visible = false;
            okButton.Click += okButton_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(32, 64);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(320, 25);
            progressBar.TabIndex = 1;
            // 
            // progressLabel
            // 
            progressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressLabel.Location = new Point(359, 64);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(67, 25);
            progressLabel.TabIndex = 3;
            progressLabel.Text = "0%";
            progressLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // StatusForm
            // 
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(457, 160);
            Controls.Add(progressLabel);
            Controls.Add(okButton);
            Controls.Add(progressBar);
            Controls.Add(statusLabel);
            Font = new Font("Georgia", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
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