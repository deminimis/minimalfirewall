using System;
using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public class ManageDomainsForm : Form
    {
        private TextBox domainsTextBox = null!;
        private Button okButton = null!;
        private Button cancelButton = null!;
        private Label instructionsLabel = null!;

        public string Domains => domainsTextBox.Text.Trim();

        // Accept the rule action (Allow/Block) to dynamically set the text
        public ManageDomainsForm(string currentDomains, string ruleAction)
        {
            InitializeComponent();
            domainsTextBox.Text = currentDomains;

            bool isAllow = ruleAction.Equals("Allow", StringComparison.OrdinalIgnoreCase);
            string ruleType = isAllow ? "Allow" : "Block";

            Text = $"Manage Domains ({ruleType})";
            instructionsLabel.Text = $"Restrict this {ruleType} rule to ONLY the following domains (leave blank for Any):\nSeparate by commas (e.g., example.com, test.com):";
        }

        private void InitializeComponent()
        {
            domainsTextBox = new TextBox();
            okButton = new Button();
            cancelButton = new Button();
            instructionsLabel = new Label();
            SuspendLayout();

            // instructionsLabel
            instructionsLabel.AutoSize = true;
            instructionsLabel.Location = new Point(12, 15);
            instructionsLabel.Name = "instructionsLabel";
            instructionsLabel.Size = new Size(340, 30);

            // domainsTextBox
            domainsTextBox.Location = new Point(15, 50);
            domainsTextBox.Name = "domainsTextBox";
            domainsTextBox.Size = new Size(350, 23);

            // okButton
            okButton.Location = new Point(209, 85);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 25);
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            // cancelButton
            cancelButton.Location = new Point(290, 85);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 25);
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // ManageDomainsForm
            AcceptButton = okButton;
            CancelButton = cancelButton;
            ClientSize = new Size(384, 125);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(domainsTextBox);
            Controls.Add(instructionsLabel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ManageDomainsForm";
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
