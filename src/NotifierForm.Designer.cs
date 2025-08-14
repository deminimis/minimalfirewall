namespace MinimalFirewall
{
    public partial class NotifierForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button allowButton;
        private System.Windows.Forms.Button blockButton;
        private System.Windows.Forms.Button ignoreButton;

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
            this.infoLabel = new System.Windows.Forms.Label();
            this.appNameLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.allowButton = new System.Windows.Forms.Button();
            this.blockButton = new System.Windows.Forms.Button();
            this.ignoreButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.Location = new System.Drawing.Point(17, 20);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(466, 23);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "Blocked a connection for:";
            // 
            // appNameLabel
            // 
            this.appNameLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.appNameLabel.Location = new System.Drawing.Point(17, 50);
            this.appNameLabel.Name = "appNameLabel";
            this.appNameLabel.Size = new System.Drawing.Size(466, 24);
            this.appNameLabel.TabIndex = 1;
            this.appNameLabel.Text = "Application Name";
            // 
            // pathLabel
            // 
            this.pathLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.pathLabel.Location = new System.Drawing.Point(17, 80);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(466, 23);
            this.pathLabel.TabIndex = 2;
            this.pathLabel.Text = "C:\\Path\\To\\Application.exe";
            // 
            // allowButton
            // 
            this.allowButton.Location = new System.Drawing.Point(21, 120);
            this.allowButton.Name = "allowButton";
            this.allowButton.Size = new System.Drawing.Size(150, 36);
            this.allowButton.TabIndex = 3;
            this.allowButton.Text = "Allow";
            this.allowButton.UseVisualStyleBackColor = true;
            this.allowButton.Click += new System.EventHandler(this.allowButton_Click);
            // 
            // blockButton
            // 
            this.blockButton.Location = new System.Drawing.Point(179, 120);
            this.blockButton.Name = "blockButton";
            this.blockButton.Size = new System.Drawing.Size(150, 36);
            this.blockButton.TabIndex = 4;
            this.blockButton.Text = "Block";
            this.blockButton.UseVisualStyleBackColor = true;
            this.blockButton.Click += new System.EventHandler(this.blockButton_Click);
            // 
            // ignoreButton
            // 
            this.ignoreButton.Location = new System.Drawing.Point(337, 120);
            this.ignoreButton.Name = "ignoreButton";
            this.ignoreButton.Size = new System.Drawing.Size(150, 36);
            this.ignoreButton.TabIndex = 5;
            this.ignoreButton.Text = "Ignore";
            this.ignoreButton.UseVisualStyleBackColor = true;
            this.ignoreButton.Click += new System.EventHandler(this.ignoreButton_Click);
            // 
            // NotifierForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 180);
            this.Controls.Add(this.ignoreButton);
            this.Controls.Add(this.blockButton);
            this.Controls.Add(this.allowButton);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.appNameLabel);
            this.Controls.Add(this.infoLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotifierForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connection Blocked";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
