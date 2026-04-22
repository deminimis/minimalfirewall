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
            this.certGrid = new System.Windows.Forms.DataGridView();
            this.closeButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.countLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.certGrid)).BeginInit();
            this.SuspendLayout();

            // infoLabel
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(400, 30);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "These are the root certificate authorities (CAs) trusted by your system.\r\nWhen the auto-allow setting is on, any app signed with a certificate chaining to one of these CAs will be allowed automatically.";

            // certGrid
            this.certGrid.AllowUserToAddRows = false;
            this.certGrid.AllowUserToDeleteRows = false;
            this.certGrid.AllowUserToResizeRows = false;
            this.certGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.certGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.certGrid.Location = new System.Drawing.Point(12, 45);
            this.certGrid.MultiSelect = false;
            this.certGrid.Name = "certGrid";
            this.certGrid.ReadOnly = true;
            this.certGrid.RowHeadersVisible = false;
            this.certGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.certGrid.Size = new System.Drawing.Size(730, 340);
            this.certGrid.TabIndex = 1;

            // countLabel
            this.countLabel.AutoSize = true;
            this.countLabel.Location = new System.Drawing.Point(12, 392);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(200, 15);
            this.countLabel.TabIndex = 2;

            // closeButton
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(667, 420);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 29);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);

            // TrustedCertificatesForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 461);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.certGrid);
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TrustedCertificatesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trusted Root Certificate Authorities";
            ((System.ComponentModel.ISupportInitialize)(this.certGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
