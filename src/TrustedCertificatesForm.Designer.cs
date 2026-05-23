namespace MinimalFirewall
{
    partial class TrustedCertificatesForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView certGrid;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label countLabel;

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
            certGrid = new DataGridView();
            closeButton = new Button();
            infoLabel = new Label();
            countLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)certGrid).BeginInit();
            SuspendLayout();
            // 
            // certGrid
            // 
            certGrid.AllowUserToAddRows = false;
            certGrid.AllowUserToDeleteRows = false;
            certGrid.AllowUserToResizeRows = false;
            certGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            certGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            certGrid.Location = new Point(12, 72);
            certGrid.MultiSelect = false;
            certGrid.Name = "certGrid";
            certGrid.ReadOnly = true;
            certGrid.RowHeadersVisible = false;
            certGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            certGrid.Size = new Size(730, 350);
            certGrid.TabIndex = 1;
            // 
            // closeButton
            // 
            closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeButton.DialogResult = DialogResult.Cancel;
            closeButton.Location = new Point(667, 448);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 31);
            closeButton.TabIndex = 3;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // infoLabel
            // 
            infoLabel.AutoSize = true;
            infoLabel.Dock = DockStyle.Left;
            infoLabel.Location = new Point(0, 0);
            infoLabel.Margin = new Padding(3, 2, 3, 2);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(588, 32);
            infoLabel.TabIndex = 0;
            infoLabel.Text = "These are the root certificate authorities (CAs) trusted by your system.\r\nWhen auto-allow is on in settings, apps signed with these CAs will be auto-allowed.";
            infoLabel.Click += infoLabel_Click;
            // 
            // countLabel
            // 
            countLabel.AutoSize = true;
            countLabel.Location = new Point(12, 418);
            countLabel.Name = "countLabel";
            countLabel.Size = new Size(0, 16);
            countLabel.TabIndex = 2;
            // 
            // TrustedCertificatesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 492);
            Controls.Add(infoLabel);
            Controls.Add(certGrid);
            Controls.Add(countLabel);
            Controls.Add(closeButton);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TrustedCertificatesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Trusted Root Certificate Authorities";
            ((System.ComponentModel.ISupportInitialize)certGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
