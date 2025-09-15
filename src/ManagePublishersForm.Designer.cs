// File: ManagePublishersForm.Designer.cs
namespace MinimalFirewall
{
    partial class ManagePublishersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox publishersListBox;
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
            this.publishersListBox = new System.Windows.Forms.ListBox();
            this.removeButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.publishersListBox.FormattingEnabled = true;
            this.publishersListBox.ItemHeight = 15;
            this.publishersListBox.Location = new System.Drawing.Point(12, 35);
            this.publishersListBox.Name = "publishersListBox";
            this.publishersListBox.Size = new System.Drawing.Size(460, 199);
            this.publishersListBox.TabIndex = 0;

            this.removeButton.Location = new System.Drawing.Point(12, 240);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(120, 29);
            this.removeButton.TabIndex = 1;
            this.removeButton.Text = "Remove Selected";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);

            this.closeButton.Location = new System.Drawing.Point(397, 240);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 29);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);

            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(262, 15);
            this.infoLabel.TabIndex = 3;
            this.infoLabel.Text = "Applications from these publishers will be allowed automatically.";

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 281);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.publishersListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManagePublishersForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Trusted Publishers";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}