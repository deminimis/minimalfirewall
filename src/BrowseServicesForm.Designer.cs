namespace MinimalFirewall
{
    partial class BrowseServicesForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.ListBox servicesListBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel bottomPanel;

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
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.servicesListBox = new System.Windows.Forms.ListBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchTextBox
            // 
            this.searchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchTextBox.Location = new System.Drawing.Point(10, 10);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.PlaceholderText = "Search services...";
            this.searchTextBox.Size = new System.Drawing.Size(464, 23);
            this.searchTextBox.TabIndex = 0;
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            // 
            // servicesListBox
            // 
            this.servicesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.servicesListBox.FormattingEnabled = true;
            this.servicesListBox.IntegralHeight = false;
            this.servicesListBox.ItemHeight = 15;
            this.servicesListBox.Location = new System.Drawing.Point(10, 33);
            this.servicesListBox.Name = "servicesListBox";
            this.servicesListBox.Size = new System.Drawing.Size(464, 358);
            this.servicesListBox.TabIndex = 1;
            this.servicesListBox.DoubleClick += new System.EventHandler(this.servicesListBox_DoubleClick);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(265, 12);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(371, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.cancelButton);
            this.bottomPanel.Controls.Add(this.okButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 391);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(484, 60);
            this.bottomPanel.TabIndex = 2;
            // 
            // BrowseServicesForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 451);
            this.Controls.Add(this.servicesListBox);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.bottomPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "BrowseServicesForm";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Browse Services";
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}