// File: ManagePublishersForm.Designer.cs
namespace MinimalFirewall
{
    partial class ManagePublishersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox publishersListBox;
        private DarkModeForms.ThemedButton removeButton;
        private DarkModeForms.ThemedButton closeButton;
        private DarkModeForms.ThemedLabel infoLabel;
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
            publishersListBox = new ListBox();
            removeButton = new DarkModeForms.ThemedButton();
            closeButton = new DarkModeForms.ThemedButton();
            infoLabel = new DarkModeForms.ThemedLabel();
            SuspendLayout();
            // 
            // publishersListBox
            // 
            publishersListBox.FormattingEnabled = true;
            publishersListBox.Location = new Point(12, 37);
            publishersListBox.Name = "publishersListBox";
            publishersListBox.Size = new Size(460, 212);
            publishersListBox.TabIndex = 0;
            // 
            // removeButton
            // 
            removeButton.Location = new Point(12, 256);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(120, 31);
            removeButton.TabIndex = 1;
            removeButton.Text = "Remove Selected";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += RemoveButton_Click;
            // 
            // closeButton
            // 
            closeButton.Location = new Point(397, 256);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 31);
            closeButton.TabIndex = 2;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // infoLabel
            // 
            infoLabel.AutoSize = true;
            infoLabel.Location = new Point(12, 10);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(462, 16);
            infoLabel.TabIndex = 3;
            infoLabel.Text = "Applications from these publishers will be allowed automatically.";
            // 
            // ManagePublishersForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 300);
            Controls.Add(infoLabel);
            Controls.Add(closeButton);
            Controls.Add(removeButton);
            Controls.Add(publishersListBox);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ManagePublishersForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Trusted Publishers";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
