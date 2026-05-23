using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    partial class NotifierForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.TextBox pathLabel;
        private System.Windows.Forms.CheckBox trustPublisherCheckBox;
        private System.Windows.Forms.Button createWildcardButton;
        private System.Windows.Forms.Button copyDetailsButton;
        private System.Windows.Forms.Button allowButton;
        private System.Windows.Forms.Button tempAllowButton;
        private System.Windows.Forms.Button blockButton;
        private System.Windows.Forms.Button ignoreButton;
        private System.Windows.Forms.ContextMenuStrip tempAllowContextMenu;

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
            components = new System.ComponentModel.Container();
            infoLabel = new Label();
            appNameLabel = new Label();
            pathLabel = new TextBox();
            trustPublisherCheckBox = new CheckBox();
            createWildcardButton = new Button();
            copyDetailsButton = new Button();
            allowButton = new Button();
            tempAllowButton = new Button();
            blockButton = new Button();
            ignoreButton = new Button();
            tempAllowContextMenu = new ContextMenuStrip(components);
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // infoLabel
            // 
            infoLabel.AutoSize = true;
            infoLabel.Location = new Point(14, 11);
            infoLabel.Margin = new Padding(4, 0, 4, 0);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(182, 16);
            infoLabel.TabIndex = 0;
            infoLabel.Text = "Blocked a connection for:";
            // 
            // appNameLabel
            // 
            appNameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            appNameLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            appNameLabel.Location = new Point(4, 32);
            appNameLabel.Margin = new Padding(4, 0, 4, 0);
            appNameLabel.Name = "appNameLabel";
            appNameLabel.Size = new Size(330, 29);
            appNameLabel.TabIndex = 1;
            appNameLabel.Text = "Application Name";
            // 
            // pathLabel
            // 
            pathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pathLabel.BackColor = SystemColors.Control;
            pathLabel.BorderStyle = BorderStyle.None;
            pathLabel.Font = new Font("Segoe UI", 8F);
            pathLabel.Location = new Point(4, 64);
            pathLabel.Margin = new Padding(4, 3, 4, 3);
            pathLabel.Multiline = true;
            pathLabel.Name = "pathLabel";
            pathLabel.ReadOnly = true;
            pathLabel.ScrollBars = ScrollBars.Vertical;
            pathLabel.Size = new Size(330, 58);
            pathLabel.TabIndex = 2;
            pathLabel.Text = "C:\\Path\\To\\Application.exe";
            // 
            // trustPublisherCheckBox
            // 
            trustPublisherCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            trustPublisherCheckBox.AutoSize = true;
            trustPublisherCheckBox.Location = new Point(72, 137);
            trustPublisherCheckBox.Margin = new Padding(4, 3, 4, 3);
            trustPublisherCheckBox.Name = "trustPublisherCheckBox";
            trustPublisherCheckBox.Size = new Size(180, 20);
            trustPublisherCheckBox.TabIndex = 3;
            trustPublisherCheckBox.Text = "Always trust publisher";
            trustPublisherCheckBox.UseVisualStyleBackColor = true;
            trustPublisherCheckBox.Visible = false;
            // 
            // createWildcardButton
            // 
            createWildcardButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            createWildcardButton.FlatStyle = FlatStyle.Flat;
            createWildcardButton.Location = new Point(8, 217);
            createWildcardButton.Margin = new Padding(4, 3, 4, 3);
            createWildcardButton.Name = "createWildcardButton";
            createWildcardButton.Size = new Size(165, 37);
            createWildcardButton.TabIndex = 4;
            createWildcardButton.Text = "Create Wildcard Rule...";
            createWildcardButton.UseVisualStyleBackColor = true;
            createWildcardButton.Click += CreateWildcardButton_Click;
            // 
            // copyDetailsButton
            // 
            copyDetailsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            copyDetailsButton.Location = new Point(18, 128);
            copyDetailsButton.Margin = new Padding(4, 3, 4, 3);
            copyDetailsButton.Name = "copyDetailsButton";
            copyDetailsButton.Size = new Size(46, 34);
            copyDetailsButton.TabIndex = 5;
            copyDetailsButton.Text = "📋";
            copyDetailsButton.TextImageRelation = TextImageRelation.TextAboveImage;
            toolTip1.SetToolTip(copyDetailsButton, "Copy Details");
            copyDetailsButton.UseVisualStyleBackColor = true;
            copyDetailsButton.Click += CopyDetailsButton_Click;
            // 
            // allowButton
            // 
            allowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            allowButton.FlatStyle = FlatStyle.Flat;
            allowButton.Location = new Point(8, 169);
            allowButton.Margin = new Padding(4, 3, 4, 3);
            allowButton.Name = "allowButton";
            allowButton.Size = new Size(98, 37);
            allowButton.TabIndex = 6;
            allowButton.Text = "Allow";
            allowButton.UseVisualStyleBackColor = true;
            allowButton.Click += AllowButton_Click;
            // 
            // tempAllowButton
            // 
            tempAllowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            tempAllowButton.FlatStyle = FlatStyle.Flat;
            tempAllowButton.Location = new Point(192, 217);
            tempAllowButton.Margin = new Padding(4, 3, 4, 3);
            tempAllowButton.Name = "tempAllowButton";
            tempAllowButton.Size = new Size(165, 37);
            tempAllowButton.TabIndex = 7;
            tempAllowButton.Text = "Allow Temporarily ▼";
            tempAllowButton.UseVisualStyleBackColor = true;
            tempAllowButton.Click += TempAllowButton_Click;
            // 
            // blockButton
            // 
            blockButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            blockButton.FlatStyle = FlatStyle.Flat;
            blockButton.Location = new Point(114, 169);
            blockButton.Margin = new Padding(4, 3, 4, 3);
            blockButton.Name = "blockButton";
            blockButton.Size = new Size(98, 37);
            blockButton.TabIndex = 8;
            blockButton.Text = "Block";
            blockButton.UseVisualStyleBackColor = true;
            blockButton.Click += BlockButton_Click;
            // 
            // ignoreButton
            // 
            ignoreButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ignoreButton.FlatStyle = FlatStyle.Flat;
            ignoreButton.Location = new Point(259, 169);
            ignoreButton.Margin = new Padding(4, 3, 4, 3);
            ignoreButton.Name = "ignoreButton";
            ignoreButton.Size = new Size(98, 37);
            ignoreButton.TabIndex = 9;
            ignoreButton.Text = "Ignore";
            ignoreButton.UseVisualStyleBackColor = true;
            ignoreButton.Click += IgnoreButton_Click;
            // 
            // tempAllowContextMenu
            // 
            tempAllowContextMenu.Name = "tempAllowContextMenu";
            tempAllowContextMenu.Size = new Size(61, 4);
            // 
            // toolTip1
            // 
            toolTip1.IsBalloon = true;
            // 
            // NotifierForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(365, 272);
            Controls.Add(ignoreButton);
            Controls.Add(blockButton);
            Controls.Add(tempAllowButton);
            Controls.Add(allowButton);
            Controls.Add(copyDetailsButton);
            Controls.Add(createWildcardButton);
            Controls.Add(trustPublisherCheckBox);
            Controls.Add(pathLabel);
            Controls.Add(appNameLabel);
            Controls.Add(infoLabel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(365, 269);
            Name = "NotifierForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Connection Blocked";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ToolTip toolTip1;
    }
}
