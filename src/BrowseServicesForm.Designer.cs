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
            searchTextBox = new TextBox();
            servicesListBox = new ListBox();
            okButton = new Button();
            cancelButton = new Button();
            bottomPanel = new Panel();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // searchTextBox
            // 
            searchTextBox.Dock = DockStyle.Top;
            searchTextBox.Location = new Point(10, 11);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.PlaceholderText = "Search services...";
            searchTextBox.Size = new Size(464, 23);
            searchTextBox.TabIndex = 0;
            searchTextBox.TextChanged += searchTextBox_TextChanged;
            // 
            // servicesListBox
            // 
            servicesListBox.Dock = DockStyle.Fill;
            servicesListBox.FormattingEnabled = true;
            servicesListBox.IntegralHeight = false;
            servicesListBox.Location = new Point(10, 34);
            servicesListBox.Name = "servicesListBox";
            servicesListBox.Size = new Size(464, 383);
            servicesListBox.TabIndex = 1;
            servicesListBox.DoubleClick += servicesListBox_DoubleClick;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(245, 13);
            okButton.Name = "okButton";
            okButton.Size = new Size(100, 38);
            okButton.TabIndex = 0;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(351, 13);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(100, 38);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(cancelButton);
            bottomPanel.Controls.Add(okButton);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(10, 417);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(464, 64);
            bottomPanel.TabIndex = 2;
            // 
            // BrowseServicesForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(484, 481);
            Controls.Add(servicesListBox);
            Controls.Add(searchTextBox);
            Controls.Add(bottomPanel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 317);
            Name = "BrowseServicesForm";
            Padding = new Padding(10, 11, 10, 0);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Browse Services";
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}
