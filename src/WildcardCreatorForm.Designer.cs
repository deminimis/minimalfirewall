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
        private DarkModeForms.FlatComboBox directionCombo;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox actionGroupBox;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Label exeNameNoteLabel;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button advancedButton;
        private System.Windows.Forms.GroupBox advancedGroupBox;
        private DarkModeForms.FlatComboBox protocolComboBox;
        private System.Windows.Forms.Label labelProtocol;
        private System.Windows.Forms.TextBox remotePortsTextBox;
        private System.Windows.Forms.Label labelRemotePorts;
        private System.Windows.Forms.TextBox localPortsTextBox;
        private System.Windows.Forms.Label labelLocalPorts;
        private System.Windows.Forms.TextBox remoteAddressTextBox;
        private System.Windows.Forms.Label labelRemoteAddress;

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
            this.components = new System.ComponentModel.Container();
            this.browseButton = new System.Windows.Forms.Button();
            this.folderPathTextBox = new System.Windows.Forms.TextBox();
            this.exeNameTextBox = new System.Windows.Forms.TextBox();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.directionCombo = new DarkModeForms.FlatComboBox();
            this.blockRadio = new System.Windows.Forms.RadioButton();
            this.allowRadio = new System.Windows.Forms.RadioButton();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.exeNameNoteLabel = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.advancedButton = new System.Windows.Forms.Button();
            this.advancedGroupBox = new System.Windows.Forms.GroupBox();
            this.remoteAddressTextBox = new System.Windows.Forms.TextBox();
            this.labelRemoteAddress = new System.Windows.Forms.Label();
            this.remotePortsTextBox = new System.Windows.Forms.TextBox();
            this.labelRemotePorts = new System.Windows.Forms.Label();
            this.localPortsTextBox = new System.Windows.Forms.TextBox();
            this.labelLocalPorts = new System.Windows.Forms.Label();
            this.protocolComboBox = new DarkModeForms.FlatComboBox();
            this.labelProtocol = new System.Windows.Forms.Label();
            this.actionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.advancedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(377, 120);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(100, 29);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // folderPathTextBox
            // 
            this.folderPathTextBox.Location = new System.Drawing.Point(23, 120);
            this.folderPathTextBox.Name = "folderPathTextBox";
            this.folderPathTextBox.Size = new System.Drawing.Size(347, 27);
            this.folderPathTextBox.TabIndex = 1;
            this.folderPathTextBox.PlaceholderText = "Enter folder path";
            // 
            // exeNameTextBox
            // 
            this.exeNameTextBox.Location = new System.Drawing.Point(23, 170);
            this.exeNameTextBox.Name = "exeNameTextBox";
            this.exeNameTextBox.Size = new System.Drawing.Size(454, 27);
            this.exeNameTextBox.TabIndex = 2;
            this.exeNameTextBox.PlaceholderText = "Optional: Filter by .exe name (e.g., svchost.exe or vs_*.exe)";
            // 
            // actionGroupBox
            // 
            this.actionGroupBox.Controls.Add(this.directionCombo);
            this.actionGroupBox.Controls.Add(this.blockRadio);
            this.actionGroupBox.Controls.Add(this.allowRadio);
            this.actionGroupBox.Location = new System.Drawing.Point(23, 240);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(454, 150);
            this.actionGroupBox.TabIndex = 3;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";
            // 
            // directionCombo
            // 
            this.directionCombo.BorderColor = System.Drawing.Color.Gray;
            this.directionCombo.ButtonColor = System.Drawing.Color.LightGray;
            this.directionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.directionCombo.FormattingEnabled = true;
            this.directionCombo.Items.AddRange(new object[] {
            "Outbound",
            "Inbound",
            "All"});
            this.directionCombo.Location = new System.Drawing.Point(150, 60);
            this.directionCombo.Name = "directionCombo";
            this.directionCombo.Size = new System.Drawing.Size(280, 28);
            this.directionCombo.TabIndex = 2;
            // 
            // blockRadio
            // 
            this.blockRadio.AutoSize = true;
            this.blockRadio.Location = new System.Drawing.Point(20, 90);
            this.blockRadio.Name = "blockRadio";
            this.blockRadio.Size = new System.Drawing.Size(66, 24);
            this.blockRadio.TabIndex = 1;
            this.blockRadio.TabStop = true;
            this.blockRadio.Text = "Block";
            this.blockRadio.UseVisualStyleBackColor = true;
            // 
            // allowRadio
            // 
            this.allowRadio.AutoSize = true;
            this.allowRadio.Checked = true;
            this.allowRadio.Location = new System.Drawing.Point(20, 30);
            this.allowRadio.Name = "allowRadio";
            this.allowRadio.Size = new System.Drawing.Size(68, 24);
            this.allowRadio.TabIndex = 0;
            this.allowRadio.TabStop = true;
            this.allowRadio.Text = "Allow";
            this.allowRadio.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(260, 622);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(377, 622);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // instructionLabel
            // 
            this.instructionLabel.Location = new System.Drawing.Point(23, 20);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(454, 80);
            this.instructionLabel.TabIndex = 6;
            this.instructionLabel.Text = "Enter a folder path below, or use the Browse button. The rule will apply to all " +
    "matching executables within that folder and its subfolders.\r\n\r\nFor temporary f" +
    "olders, you can type in environment variables directly. Common examples: %APPDATA% ; %Temp% ;  %LOCALAPPDATA%\\Temp ";
            // 
            // exeNameNoteLabel
            // 
            this.exeNameNoteLabel.AutoSize = true;
            this.exeNameNoteLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.exeNameNoteLabel.Location = new System.Drawing.Point(23, 196);
            this.exeNameNoteLabel.Name = "exeNameNoteLabel";
            this.exeNameNoteLabel.Size = new System.Drawing.Size(465, 20);
            this.exeNameNoteLabel.TabIndex = 7;
            this.exeNameNoteLabel.Text = "If left blank, the rule will apply to all executables in the selected folder.";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // advancedButton
            // 
            this.advancedButton.Location = new System.Drawing.Point(23, 397);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(121, 29);
            this.advancedButton.TabIndex = 8;
            this.advancedButton.Text = "Advanced...";
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
            // 
            // advancedGroupBox
            // 
            this.advancedGroupBox.Controls.Add(this.remoteAddressTextBox);
            this.advancedGroupBox.Controls.Add(this.labelRemoteAddress);
            this.advancedGroupBox.Controls.Add(this.remotePortsTextBox);
            this.advancedGroupBox.Controls.Add(this.labelRemotePorts);
            this.advancedGroupBox.Controls.Add(this.localPortsTextBox);
            this.advancedGroupBox.Controls.Add(this.labelLocalPorts);
            this.advancedGroupBox.Controls.Add(this.protocolComboBox);
            this.advancedGroupBox.Controls.Add(this.labelProtocol);
            this.advancedGroupBox.Location = new System.Drawing.Point(23, 432);
            this.advancedGroupBox.Name = "advancedGroupBox";
            this.advancedGroupBox.Size = new System.Drawing.Size(454, 172);
            this.advancedGroupBox.TabIndex = 9;
            this.advancedGroupBox.TabStop = false;
            this.advancedGroupBox.Text = "Advanced Settings";
            this.advancedGroupBox.Visible = false;
            // 
            // remoteAddressTextBox
            // 
            this.remoteAddressTextBox.Location = new System.Drawing.Point(124, 127);
            this.remoteAddressTextBox.Name = "remoteAddressTextBox";
            this.remoteAddressTextBox.Size = new System.Drawing.Size(306, 27);
            this.remoteAddressTextBox.TabIndex = 7;
            this.remoteAddressTextBox.Text = "*";
            this.remoteAddressTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.remoteAddressTextBox_Validating);
            // 
            // labelRemoteAddress
            // 
            this.labelRemoteAddress.AutoSize = true;
            this.labelRemoteAddress.Location = new System.Drawing.Point(6, 130);
            this.labelRemoteAddress.Name = "labelRemoteAddress";
            this.labelRemoteAddress.Size = new System.Drawing.Size(117, 20);
            this.labelRemoteAddress.TabIndex = 6;
            this.labelRemoteAddress.Text = "Remote Address";
            // 
            // remotePortsTextBox
            // 
            this.remotePortsTextBox.Location = new System.Drawing.Point(124, 94);
            this.remotePortsTextBox.Name = "remotePortsTextBox";
            this.remotePortsTextBox.Size = new System.Drawing.Size(306, 27);
            this.remotePortsTextBox.TabIndex = 5;
            this.remotePortsTextBox.Text = "*";
            this.remotePortsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.remotePortsTextBox_Validating);
            // 
            // labelRemotePorts
            // 
            this.labelRemotePorts.AutoSize = true;
            this.labelRemotePorts.Location = new System.Drawing.Point(6, 97);
            this.labelRemotePorts.Name = "labelRemotePorts";
            this.labelRemotePorts.Size = new System.Drawing.Size(95, 20);
            this.labelRemotePorts.TabIndex = 4;
            this.labelRemotePorts.Text = "Remote Ports";
            // 
            // localPortsTextBox
            // 
            this.localPortsTextBox.Location = new System.Drawing.Point(124, 61);
            this.localPortsTextBox.Name = "localPortsTextBox";
            this.localPortsTextBox.Size = new System.Drawing.Size(306, 27);
            this.localPortsTextBox.TabIndex = 3;
            this.localPortsTextBox.Text = "*";
            this.localPortsTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.localPortsTextBox_Validating);
            // 
            // labelLocalPorts
            // 
            this.labelLocalPorts.AutoSize = true;
            this.labelLocalPorts.Location = new System.Drawing.Point(6, 64);
            this.labelLocalPorts.Name = "labelLocalPorts";
            this.labelLocalPorts.Size = new System.Drawing.Size(81, 20);
            this.labelLocalPorts.TabIndex = 2;
            this.labelLocalPorts.Text = "Local Ports";
            // 
            // protocolComboBox
            // 
            this.protocolComboBox.BorderColor = System.Drawing.Color.Gray;
            this.protocolComboBox.ButtonColor = System.Drawing.Color.LightGray;
            this.protocolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolComboBox.FormattingEnabled = true;
            this.protocolComboBox.Location = new System.Drawing.Point(124, 27);
            this.protocolComboBox.Name = "protocolComboBox";
            this.protocolComboBox.Size = new System.Drawing.Size(151, 28);
            this.protocolComboBox.TabIndex = 1;
            // 
            // labelProtocol
            // 
            this.labelProtocol.AutoSize = true;
            this.labelProtocol.Location = new System.Drawing.Point(6, 30);
            this.labelProtocol.Name = "labelProtocol";
            this.labelProtocol.Size = new System.Drawing.Size(64, 20);
            this.labelProtocol.TabIndex = 0;
            this.labelProtocol.Text = "Protocol";
            // 
            // WildcardCreatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 670);
            this.Controls.Add(this.advancedGroupBox);
            this.Controls.Add(this.advancedButton);
            this.Controls.Add(this.exeNameNoteLabel);
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
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.advancedGroupBox.ResumeLayout(false);
            this.advancedGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}