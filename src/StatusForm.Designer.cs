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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.statusLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(6, 20);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(388, 23);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Scanning, please wait...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(150, 90);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Visible = false;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(28, 60);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(280, 23);
            this.progressBar.TabIndex = 1;
            // 
            // progressLabel
            // 
            this.progressLabel.Location = new System.Drawing.Point(314, 60);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(59, 23);
            this.progressLabel.TabIndex = 3;
            this.progressLabel.Text = "0%";
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 150);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StatusForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Scanning...";
            this.ResumeLayout(false);

        }

        #endregion
    }
}