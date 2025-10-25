// File: WildcardCreatorForm.Designer.cs
namespace MinimalFirewall
{
    public partial class WildcardCreatorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox folderPathTextBox;
        private System.Windows.Forms.TextBox exeNameTextBox;
        private System.Windows.Forms.RadioButton allowRadio;
        private System.Windows.Forms.RadioButton blockRadio;
        private DarkModeForms.FlatComboBox allowDirectionCombo;
        private DarkModeForms.FlatComboBox blockDirectionCombo;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox actionGroupBox;
        private System.Windows.Forms.Label instructionLabel;
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
            this.browseButton = new System.Windows.Forms.Button();
            this.folderPathTextBox = new System.Windows.Forms.TextBox();
            this.exeNameTextBox = new System.Windows.Forms.TextBox();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.blockDirectionCombo = new DarkModeForms.FlatComboBox();
            this.allowDirectionCombo = new DarkModeForms.FlatComboBox();
            this.blockRadio = new System.Windows.Forms.RadioButton();
            this.allowRadio = new System.Windows.Forms.RadioButton();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.actionGroupBox.SuspendLayout();
            this.SuspendLayout();

            this.browseButton.Location = new System.Drawing.Point(377, 120);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(100, 23);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);

            this.folderPathTextBox.Location = new System.Drawing.Point(23, 120);
            this.folderPathTextBox.Name = "folderPathTextBox";
            this.folderPathTextBox.Size = new System.Drawing.Size(347, 23);
            this.folderPathTextBox.TabIndex = 1;
            this.folderPathTextBox.PlaceholderText = "Enter folder path";

            this.exeNameTextBox.Location = new System.Drawing.Point(23, 170);
            this.exeNameTextBox.Name = "exeNameTextBox";
            this.exeNameTextBox.Size = new System.Drawing.Size(454, 23);
            this.exeNameTextBox.TabIndex = 2;
            this.exeNameTextBox.PlaceholderText = "Optional: Filter by .exe name (e.g., svchost.exe or vs_*.exe)";

            this.actionGroupBox.Controls.Add(this.blockDirectionCombo);
            this.actionGroupBox.Controls.Add(this.allowDirectionCombo);
            this.actionGroupBox.Controls.Add(this.blockRadio);
            this.actionGroupBox.Controls.Add(this.allowRadio);
            this.actionGroupBox.Location = new System.Drawing.Point(23, 220);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(454, 150);
            this.actionGroupBox.TabIndex = 3;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";

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

            this.okButton.Location = new System.Drawing.Point(260, 390);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);

            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(377, 390);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);

            this.instructionLabel.Location = new System.Drawing.Point(23, 20);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(454, 80);
            this.instructionLabel.TabIndex = 6;
            this.instructionLabel.Text = @"Enter a folder path below, or use the Browse button. The rule will apply to all matching executables within that folder and its subfolders.

For temporary folders, you can type in environment variables directly. Common examples: %APPDATA% ; %Temp% ;  %LOCALAPPDATA%\Temp ";

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 450);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.actionGroupBox);
            this.Controls.Add(this.exeNameTextBox);
            this.Controls.Add(this.folderPathTextBox);
            this.Controls.Add(this.browseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WildcardCreatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Wildcard Rule";
            this.actionGroupBox.ResumeLayout(false);
            this.actionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}