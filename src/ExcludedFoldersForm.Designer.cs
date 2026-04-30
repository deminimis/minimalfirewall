namespace MinimalFirewall
{
    partial class ExcludedFoldersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox foldersListBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label infoLabel;

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
            this.foldersListBox = new System.Windows.Forms.ListBox();
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // infoLabel
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(400, 30);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "Apps running from these folders will NOT be auto-allowed,\r\neven if they have a trusted digital signature.";

            // foldersListBox
            this.foldersListBox.FormattingEnabled = true;
            this.foldersListBox.ItemHeight = 15;
            this.foldersListBox.Location = new System.Drawing.Point(12, 45);
            this.foldersListBox.Name = "foldersListBox";
            this.foldersListBox.Size = new System.Drawing.Size(460, 199);
            this.foldersListBox.TabIndex = 1;
            this.foldersListBox.SelectedIndexChanged += new System.EventHandler(this.foldersListBox_SelectedIndexChanged);

            // addButton
            this.addButton.Location = new System.Drawing.Point(12, 250);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(120, 29);
            this.addButton.TabIndex = 2;
            this.addButton.Text = "Add Folder...";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);

            // removeButton
            this.removeButton.Location = new System.Drawing.Point(138, 250);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(120, 29);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove Selected";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);

            // closeButton
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.closeButton.Location = new System.Drawing.Point(397, 250);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 29);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);

            // ExcludedFoldersForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 291);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.foldersListBox);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExcludedFoldersForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Excluded Folders for Auto-Allow";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
