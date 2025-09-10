namespace MinimalFirewall
{
    public partial class CreateProgramRuleForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label programListLabel;
        private System.Windows.Forms.RadioButton allowRadio;
        private System.Windows.Forms.RadioButton blockRadio;
        private DarkModeForms.FlatComboBox allowDirectionCombo;
        private DarkModeForms.FlatComboBox blockDirectionCombo;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private GroupBox actionGroupBox;

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
            this.programListLabel = new System.Windows.Forms.Label();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.blockRadio = new System.Windows.Forms.RadioButton();
            this.allowRadio = new System.Windows.Forms.RadioButton();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.blockDirectionCombo = new DarkModeForms.FlatComboBox();
            this.allowDirectionCombo = new DarkModeForms.FlatComboBox();
            this.actionGroupBox.SuspendLayout();
            this.SuspendLayout();

            this.programListLabel.Location = new System.Drawing.Point(23, 80);
            this.programListLabel.Name = "programListLabel";
            this.programListLabel.Size = new System.Drawing.Size(454, 50);
            this.programListLabel.TabIndex = 0;
            this.programListLabel.Text = "Program List";

            this.actionGroupBox.Controls.Add(this.blockDirectionCombo);
            this.actionGroupBox.Controls.Add(this.allowDirectionCombo);
            this.actionGroupBox.Controls.Add(this.blockRadio);
            this.actionGroupBox.Controls.Add(this.allowRadio);
            this.actionGroupBox.Location = new System.Drawing.Point(23, 140);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(454, 150);
            this.actionGroupBox.TabIndex = 1;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";

            this.blockRadio.AutoSize = true;
            this.blockRadio.Location = new System.Drawing.Point(20, 90);
            this.blockRadio.Name = "blockRadio";
            this.blockRadio.Size = new System.Drawing.Size(54, 19);
            this.blockRadio.TabIndex = 1;
            this.blockRadio.TabStop = true;
            this.blockRadio.Text = "Block";
            this.blockRadio.UseVisualStyleBackColor = true;

            this.allowRadio.AutoSize = true;
            this.allowRadio.Checked = true;
            this.allowRadio.Location = new System.Drawing.Point(20, 30);
            this.allowRadio.Name = "allowRadio";
            this.allowRadio.Size = new System.Drawing.Size(55, 19);
            this.allowRadio.TabIndex = 0;
            this.allowRadio.TabStop = true;
            this.allowRadio.Text = "Allow";
            this.allowRadio.UseVisualStyleBackColor = true;

            this.okButton.Location = new System.Drawing.Point(260, 310);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);

            this.cancelButton.Location = new System.Drawing.Point(377, 310);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);

            this.blockDirectionCombo.BorderColor = System.Drawing.Color.Gray;
            this.blockDirectionCombo.ButtonColor = System.Drawing.Color.LightGray;
            this.blockDirectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blockDirectionCombo.FormattingEnabled = true;
            this.blockDirectionCombo.Items.AddRange(new object[] {
            "Outbound",
            "Inbound",
            "All"});
            this.blockDirectionCombo.Location = new System.Drawing.Point(150, 90);
            this.blockDirectionCombo.Name = "blockDirectionCombo";
            this.blockDirectionCombo.Size = new System.Drawing.Size(280, 23);
            this.blockDirectionCombo.TabIndex = 3;

            this.allowDirectionCombo.BorderColor = System.Drawing.Color.Gray;
            this.allowDirectionCombo.ButtonColor = System.Drawing.Color.LightGray;
            this.allowDirectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.allowDirectionCombo.FormattingEnabled = true;
            this.allowDirectionCombo.Items.AddRange(new object[] {
            "Outbound",
            "Inbound",
            "All"});
            this.allowDirectionCombo.Location = new System.Drawing.Point(150, 30);
            this.allowDirectionCombo.Name = "allowDirectionCombo";
            this.allowDirectionCombo.Size = new System.Drawing.Size(280, 23);
            this.allowDirectionCombo.TabIndex = 2;

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 370);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.actionGroupBox);
            this.Controls.Add(this.programListLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateProgramRuleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Program Rule";
            this.actionGroupBox.ResumeLayout(false);
            this.actionGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion
    }
}
