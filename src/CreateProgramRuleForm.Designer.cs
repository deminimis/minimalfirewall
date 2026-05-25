// File: CreateProgramRuleForm.Designer.cs
namespace MinimalFirewall
{
    public partial class CreateProgramRuleForm
    {
        private System.ComponentModel.IContainer components = null;
        private DarkModeForms.ThemedLabel programListLabel;
        private System.Windows.Forms.RadioButton allowRadio;
        private System.Windows.Forms.RadioButton blockRadio;
        private System.Windows.Forms.ComboBox allowDirectionCombo;
        private System.Windows.Forms.ComboBox blockDirectionCombo;
        private DarkModeForms.ThemedButton okButton;
        private DarkModeForms.ThemedButton cancelButton;
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
            programListLabel = new DarkModeForms.ThemedLabel();
            actionGroupBox = new GroupBox();
            blockDirectionCombo = new System.Windows.Forms.ComboBox();
            allowDirectionCombo = new System.Windows.Forms.ComboBox();
            blockRadio = new RadioButton();
            allowRadio = new RadioButton();
            okButton = new DarkModeForms.ThemedButton();
            cancelButton = new DarkModeForms.ThemedButton();
            actionGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // programListLabel
            // 
            programListLabel.AutoEllipsis = true;
            programListLabel.Location = new Point(23, 85);
            programListLabel.Name = "programListLabel";
            programListLabel.Size = new Size(454, 53);
            programListLabel.TabIndex = 0;
            programListLabel.Text = "Program List";
            // 
            // actionGroupBox
            // 
            actionGroupBox.Controls.Add(blockDirectionCombo);
            actionGroupBox.Controls.Add(allowDirectionCombo);
            actionGroupBox.Controls.Add(blockRadio);
            actionGroupBox.Controls.Add(allowRadio);
            actionGroupBox.Location = new Point(23, 149);
            actionGroupBox.Name = "actionGroupBox";
            actionGroupBox.Size = new Size(454, 160);
            actionGroupBox.TabIndex = 1;
            actionGroupBox.TabStop = false;
            actionGroupBox.Text = "Action";
            // 
            // blockDirectionCombo
            // 
            blockDirectionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            blockDirectionCombo.FormattingEnabled = true;
            blockDirectionCombo.Items.AddRange(new object[] { "Outbound", "Inbound", "All" });
            blockDirectionCombo.Location = new Point(150, 96);
            blockDirectionCombo.Name = "blockDirectionCombo";
            blockDirectionCombo.Size = new Size(280, 24);
            blockDirectionCombo.TabIndex = 3;
            // 
            // allowDirectionCombo
            // 
            allowDirectionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            allowDirectionCombo.FormattingEnabled = true;
            allowDirectionCombo.Items.AddRange(new object[] { "Outbound", "Inbound", "All" });
            allowDirectionCombo.Location = new Point(150, 32);
            allowDirectionCombo.Name = "allowDirectionCombo";
            allowDirectionCombo.Size = new Size(280, 24);
            allowDirectionCombo.TabIndex = 2;
            // 
            // blockRadio
            // 
            blockRadio.AutoSize = true;
            blockRadio.Location = new Point(20, 96);
            blockRadio.Name = "blockRadio";
            blockRadio.Size = new Size(60, 20);
            blockRadio.TabIndex = 1;
            blockRadio.TabStop = true;
            blockRadio.Text = "Block";
            blockRadio.UseVisualStyleBackColor = true;
            // 
            // allowRadio
            // 
            allowRadio.AutoSize = true;
            allowRadio.Checked = true;
            allowRadio.Location = new Point(20, 32);
            allowRadio.Name = "allowRadio";
            allowRadio.Size = new Size(60, 20);
            allowRadio.TabIndex = 0;
            allowRadio.TabStop = true;
            allowRadio.Text = "Allow";
            allowRadio.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Location = new Point(260, 331);
            okButton.Name = "okButton";
            okButton.Size = new Size(100, 38);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(377, 331);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(100, 38);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // CreateProgramRuleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 395);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(actionGroupBox);
            Controls.Add(programListLabel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateProgramRuleForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Program Rule";
            actionGroupBox.ResumeLayout(false);
            actionGroupBox.PerformLayout();
            ResumeLayout(false);
        }
        #endregion
    }
}
