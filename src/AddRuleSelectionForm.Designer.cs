// File: AddRuleSelectionForm.Designer.cs
namespace MinimalFirewall
{
    public partial class AddRuleSelectionForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button programRuleButton;
        private System.Windows.Forms.Button wildcardRuleButton;

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
            this.programRuleButton = new System.Windows.Forms.Button();
            this.wildcardRuleButton = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.programRuleButton.Location = new System.Drawing.Point(50, 90);
            this.programRuleButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.programRuleButton.Name = "programRuleButton";
            this.programRuleButton.Size = new System.Drawing.Size(250, 40);
            this.programRuleButton.TabIndex = 0;
            this.programRuleButton.Text = "Create Program Rule...";
            this.programRuleButton.UseVisualStyleBackColor = true;
            this.programRuleButton.Click += new System.EventHandler(this.ProgramRuleButton_Click);

            this.wildcardRuleButton.Location = new System.Drawing.Point(50, 150);
            this.wildcardRuleButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.wildcardRuleButton.Name = "wildcardRuleButton";
            this.wildcardRuleButton.Size = new System.Drawing.Size(250, 40);
            this.wildcardRuleButton.TabIndex = 1;
            this.wildcardRuleButton.Text = "Create Wildcard Rule...";
            this.wildcardRuleButton.UseVisualStyleBackColor = true;
            this.wildcardRuleButton.Click += new System.EventHandler(this.WildcardRuleButton_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 250);
            this.Controls.Add(this.wildcardRuleButton);
            this.Controls.Add(this.programRuleButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddRuleSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add New Rule";
            this.ResumeLayout(false);
        }

        #endregion
    }
}