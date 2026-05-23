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
            foldersListBox = new ListBox();
            addButton = new Button();
            removeButton = new Button();
            closeButton = new Button();
            infoLabel = new Label();
            SuspendLayout();
            // 
            // foldersListBox
            // 
            foldersListBox.FormattingEnabled = true;
            foldersListBox.Location = new Point(12, 48);
            foldersListBox.Name = "foldersListBox";
            foldersListBox.Size = new Size(460, 212);
            foldersListBox.TabIndex = 1;
            foldersListBox.SelectedIndexChanged += foldersListBox_SelectedIndexChanged;
            // 
            // addButton
            // 
            addButton.Location = new Point(12, 267);
            addButton.Name = "addButton";
            addButton.Size = new Size(120, 31);
            addButton.TabIndex = 2;
            addButton.Text = "Add Folder...";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += addButton_Click;
            // 
            // removeButton
            // 
            removeButton.Location = new Point(138, 267);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(120, 31);
            removeButton.TabIndex = 3;
            removeButton.Text = "Remove Selected";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += removeButton_Click;
            // 
            // closeButton
            // 
            closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeButton.Location = new Point(397, 267);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 31);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // infoLabel
            // 
            infoLabel.AutoSize = true;
            infoLabel.Location = new Point(12, 10);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(406, 32);
            infoLabel.TabIndex = 0;
            infoLabel.Text = "Apps running from these folders will NOT be auto-allowed,\r\neven if they have a trusted digital signature.";
            // 
            // ExcludedFoldersForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 310);
            Controls.Add(infoLabel);
            Controls.Add(foldersListBox);
            Controls.Add(addButton);
            Controls.Add(removeButton);
            Controls.Add(closeButton);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExcludedFoldersForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Excluded Folders for Auto-Allow";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
