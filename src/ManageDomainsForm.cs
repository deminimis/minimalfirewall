using System;
using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public class ManageDomainsForm : Form
    {
        private TextBox domainsTextBox;
        private Button okButton;
        private Button cancelButton;
        private Label instructionsLabel;

        public string Domains => domainsTextBox.Text.Trim();

        // Accept the rule action (Allow/Block) to dynamically set the text
        public ManageDomainsForm(string currentDomains, string ruleAction)
        {
            InitializeComponent();
            domainsTextBox.Text = currentDomains;

            bool isAllow = ruleAction.Equals("Allow", StringComparison.OrdinalIgnoreCase);
            string ruleType = isAllow ? "Allow" : "Block";

            this.Text = $"Manage Domains ({ruleType})";
            this.instructionsLabel.Text = $"Restrict this {ruleType} rule to ONLY the following domains (leave blank for Any):\nSeparate by commas (e.g., example.com, test.com):";
        }

        private void InitializeComponent()
        {
            this.domainsTextBox = new TextBox();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.instructionsLabel = new Label();
            this.SuspendLayout();

            // instructionsLabel
            this.instructionsLabel.AutoSize = true;
            this.instructionsLabel.Location = new Point(12, 15);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new Size(340, 30);

            // domainsTextBox
            this.domainsTextBox.Location = new Point(15, 50);
            this.domainsTextBox.Name = "domainsTextBox";
            this.domainsTextBox.Size = new Size(350, 23);

            // okButton
            this.okButton.Location = new Point(209, 85);
            this.okButton.Name = "okButton";
            this.okButton.Size = new Size(75, 25);
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            // cancelButton
            this.cancelButton.Location = new Point(290, 85);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(75, 25);
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // ManageDomainsForm
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new Size(384, 125);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.domainsTextBox);
            this.Controls.Add(this.instructionsLabel);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManageDomainsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
