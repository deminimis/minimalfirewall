// File: NotifierForm.Designer.cs
namespace MinimalFirewall
{
    partial class NotifierForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button allowButton;
        private System.Windows.Forms.Button blockButton;
        private System.Windows.Forms.Button ignoreButton;
        private System.Windows.Forms.Button tempAllowButton;
        private System.Windows.Forms.ContextMenuStrip tempAllowContextMenu;
        private System.Windows.Forms.Button createWildcardButton;
        private System.Windows.Forms.CheckBox trustPublisherCheckBox;
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
            this.components = new System.ComponentModel.Container();
            this.infoLabel = new System.Windows.Forms.Label();
            this.appNameLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.allowButton = new System.Windows.Forms.Button();
            this.blockButton = new System.Windows.Forms.Button();
            this.ignoreButton = new System.Windows.Forms.Button();
            this.tempAllowButton = new System.Windows.Forms.Button();
            this.tempAllowContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createWildcardButton = new System.Windows.Forms.Button();
            this.trustPublisherCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(532, 20);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "Blocked a connection for:";
            // 
            // appNameLabel
            // 
            this.appNameLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.appNameLabel.Location = new System.Drawing.Point(12, 38);
            this.appNameLabel.Name = "appNameLabel";
            this.appNameLabel.Size = new System.Drawing.Size(532, 21);
            this.appNameLabel.TabIndex = 1;
            this.appNameLabel.Text = "Application Name";
            // 
            // pathLabel
            // 
            this.pathLabel.AutoEllipsis = true;
            this.pathLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.pathLabel.Location = new System.Drawing.Point(12, 65);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(532, 20);
            this.pathLabel.TabIndex = 2;
            this.pathLabel.Text = "C:\\Path\\To\\Application.exe";
            // 
            // allowButton
            // 
            this.allowButton.Location = new System.Drawing.Point(16, 104);
            this.allowButton.Name = "allowButton";
            this.allowButton.Size = new System.Drawing.Size(120, 31);
            this.allowButton.TabIndex = 3;
            this.allowButton.Text = "Allow";
            this.allowButton.UseVisualStyleBackColor = true;
            this.allowButton.Click += new System.EventHandler(this.allowButton_Click);
            // 
            // blockButton
            // 
            this.blockButton.Location = new System.Drawing.Point(296, 104);
            this.blockButton.Name = "blockButton";
            this.blockButton.Size = new System.Drawing.Size(120, 31);
            this.blockButton.TabIndex = 4;
            this.blockButton.Text = "Block";
            this.blockButton.UseVisualStyleBackColor = true;
            this.blockButton.Click += new System.EventHandler(this.blockButton_Click);
            // 
            // ignoreButton
            // 
            this.ignoreButton.Location = new System.Drawing.Point(424, 104);
            this.ignoreButton.Name = "ignoreButton";
            this.ignoreButton.Size = new System.Drawing.Size(120, 31);
            this.ignoreButton.TabIndex = 5;
            this.ignoreButton.Text = "Ignore";
            this.ignoreButton.UseVisualStyleBackColor = true;
            this.ignoreButton.Click += new System.EventHandler(this.ignoreButton_Click);
            // 
            // tempAllowButton
            // 
            this.tempAllowButton.Location = new System.Drawing.Point(144, 104);
            this.tempAllowButton.Name = "tempAllowButton";
            this.tempAllowButton.Size = new System.Drawing.Size(144, 31);
            this.tempAllowButton.TabIndex = 6;
            this.tempAllowButton.Text = "Allow Temporarily ▼";
            this.tempAllowButton.UseVisualStyleBackColor = true;
            this.tempAllowButton.Click += new System.EventHandler(this.tempAllowButton_Click);
            // 
            // tempAllowContextMenu
            // 
            this.tempAllowContextMenu.Name = "tempAllowContextMenu";
            this.tempAllowContextMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // createWildcardButton
            // 
            this.createWildcardButton.Location = new System.Drawing.Point(16, 148);
            this.createWildcardButton.Name = "createWildcardButton";
            this.createWildcardButton.Size = new System.Drawing.Size(170, 31);
            this.createWildcardButton.TabIndex = 7;
            this.createWildcardButton.Text = "Create Wildcard Rule...";
            this.createWildcardButton.UseVisualStyleBackColor = true;
            this.createWildcardButton.Click += new System.EventHandler(this.createWildcardButton_Click);
            // 
            // trustPublisherCheckBox
            // 
            this.trustPublisherCheckBox.AutoSize = true;
            this.trustPublisherCheckBox.Location = new System.Drawing.Point(16, 185);
            this.trustPublisherCheckBox.Name = "trustPublisherCheckBox";
            this.trustPublisherCheckBox.Size = new System.Drawing.Size(138, 17);
            this.trustPublisherCheckBox.TabIndex = 8;
            this.trustPublisherCheckBox.Text = "Always trust publisher";
            this.trustPublisherCheckBox.UseVisualStyleBackColor = true;
            this.trustPublisherCheckBox.Visible = false;
            // 
            // NotifierForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 225);
            this.Controls.Add(this.trustPublisherCheckBox);
            this.Controls.Add(this.createWildcardButton);
            this.Controls.Add(this.tempAllowButton);
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
            this.PerformLayout();
        }

        #endregion
    }
}