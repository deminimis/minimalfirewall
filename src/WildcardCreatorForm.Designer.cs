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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(WildcardCreatorForm));
            browseButton = new Button();
            folderPathTextBox = new TextBox();
            exeNameTextBox = new TextBox();
            actionGroupBox = new GroupBox();
            directionCombo = new DarkModeForms.FlatComboBox();
            blockRadio = new RadioButton();
            allowRadio = new RadioButton();
            okButton = new Button();
            cancelButton = new Button();
            instructionLabel = new Label();
            exeNameNoteLabel = new Label();
            errorProvider1 = new ErrorProvider(components);
            advancedButton = new Button();
            advancedGroupBox = new GroupBox();
            remoteAddressTextBox = new TextBox();
            labelRemoteAddress = new Label();
            remotePortsTextBox = new TextBox();
            labelRemotePorts = new Label();
            localPortsTextBox = new TextBox();
            labelLocalPorts = new Label();
            protocolComboBox = new DarkModeForms.FlatComboBox();
            labelProtocol = new Label();
            actionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            advancedGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // browseButton
            // 
            browseButton.Location = new Point(330, 96);
            browseButton.Margin = new Padding(3, 2, 3, 2);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(88, 23);
            browseButton.TabIndex = 0;
            browseButton.Text = "Browse...";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // folderPathTextBox
            // 
            folderPathTextBox.Location = new Point(20, 96);
            folderPathTextBox.Margin = new Padding(3, 2, 3, 2);
            folderPathTextBox.Name = "folderPathTextBox";
            folderPathTextBox.PlaceholderText = "Enter folder path";
            folderPathTextBox.Size = new Size(304, 23);
            folderPathTextBox.TabIndex = 1;
            // 
            // exeNameTextBox
            // 
            exeNameTextBox.Location = new Point(20, 137);
            exeNameTextBox.Margin = new Padding(3, 2, 3, 2);
            exeNameTextBox.Name = "exeNameTextBox";
            exeNameTextBox.PlaceholderText = "Optional: Filter by .exe name (e.g., svchost.exe or vs_*.exe)";
            exeNameTextBox.Size = new Size(398, 23);
            exeNameTextBox.TabIndex = 2;
            // 
            // actionGroupBox
            // 
            actionGroupBox.Controls.Add(directionCombo);
            actionGroupBox.Controls.Add(blockRadio);
            actionGroupBox.Controls.Add(allowRadio);
            actionGroupBox.Location = new Point(20, 192);
            actionGroupBox.Margin = new Padding(3, 2, 3, 2);
            actionGroupBox.Name = "actionGroupBox";
            actionGroupBox.Padding = new Padding(3, 2, 3, 2);
            actionGroupBox.Size = new Size(397, 119);
            actionGroupBox.TabIndex = 3;
            actionGroupBox.TabStop = false;
            actionGroupBox.Text = "Action";
            // 
            // directionCombo
            // 
            directionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            directionCombo.FormattingEnabled = true;
            directionCombo.Items.AddRange(new object[] { "Outbound", "Inbound", "All" });
            directionCombo.Location = new Point(131, 48);
            directionCombo.Margin = new Padding(3, 2, 3, 2);
            directionCombo.Name = "directionCombo";
            directionCombo.Size = new Size(246, 24);
            directionCombo.TabIndex = 2;
            // 
            // blockRadio
            // 
            blockRadio.AutoSize = true;
            blockRadio.Location = new Point(18, 73);
            blockRadio.Margin = new Padding(3, 2, 3, 2);
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
            allowRadio.Location = new Point(18, 23);
            allowRadio.Margin = new Padding(3, 2, 3, 2);
            allowRadio.Name = "allowRadio";
            allowRadio.Size = new Size(60, 20);
            allowRadio.TabIndex = 0;
            allowRadio.TabStop = true;
            allowRadio.Text = "Allow";
            allowRadio.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(228, 497);
            okButton.Margin = new Padding(3, 2, 3, 2);
            okButton.Name = "okButton";
            okButton.Size = new Size(88, 29);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(330, 497);
            cancelButton.Margin = new Padding(3, 2, 3, 2);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 29);
            cancelButton.TabIndex = 5;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // instructionLabel
            // 
            instructionLabel.Location = new Point(20, 16);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(397, 64);
            instructionLabel.TabIndex = 6;
            instructionLabel.Text = resources.GetString("instructionLabel.Text");
            // 
            // exeNameNoteLabel
            // 
            exeNameNoteLabel.AutoSize = true;
            exeNameNoteLabel.ForeColor = SystemColors.GrayText;
            exeNameNoteLabel.Location = new Point(20, 157);
            exeNameNoteLabel.Name = "exeNameNoteLabel";
            exeNameNoteLabel.Size = new Size(546, 16);
            exeNameNoteLabel.TabIndex = 7;
            exeNameNoteLabel.Text = "If left blank, the rule will apply to all executables in the selected folder.";
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // advancedButton
            // 
            advancedButton.Location = new Point(20, 318);
            advancedButton.Margin = new Padding(3, 2, 3, 2);
            advancedButton.Name = "advancedButton";
            advancedButton.Size = new Size(106, 23);
            advancedButton.TabIndex = 8;
            advancedButton.Text = "Advanced...";
            advancedButton.UseVisualStyleBackColor = true;
            advancedButton.Click += advancedButton_Click;
            // 
            // advancedGroupBox
            // 
            advancedGroupBox.Controls.Add(remoteAddressTextBox);
            advancedGroupBox.Controls.Add(labelRemoteAddress);
            advancedGroupBox.Controls.Add(remotePortsTextBox);
            advancedGroupBox.Controls.Add(labelRemotePorts);
            advancedGroupBox.Controls.Add(localPortsTextBox);
            advancedGroupBox.Controls.Add(labelLocalPorts);
            advancedGroupBox.Controls.Add(protocolComboBox);
            advancedGroupBox.Controls.Add(labelProtocol);
            advancedGroupBox.Location = new Point(20, 346);
            advancedGroupBox.Margin = new Padding(3, 2, 3, 2);
            advancedGroupBox.Name = "advancedGroupBox";
            advancedGroupBox.Padding = new Padding(3, 2, 3, 2);
            advancedGroupBox.Size = new Size(397, 138);
            advancedGroupBox.TabIndex = 9;
            advancedGroupBox.TabStop = false;
            advancedGroupBox.Text = "Advanced Settings";
            advancedGroupBox.Visible = false;
            // 
            // remoteAddressTextBox
            // 
            remoteAddressTextBox.Location = new Point(108, 101);
            remoteAddressTextBox.Margin = new Padding(3, 2, 3, 2);
            remoteAddressTextBox.Name = "remoteAddressTextBox";
            remoteAddressTextBox.Size = new Size(268, 23);
            remoteAddressTextBox.TabIndex = 7;
            remoteAddressTextBox.Text = "*";
            remoteAddressTextBox.Validating += remoteAddressTextBox_Validating;
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(5, 105);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(105, 16);
            labelRemoteAddress.TabIndex = 6;
            labelRemoteAddress.Text = "Remote Address";
            // 
            // remotePortsTextBox
            // 
            remotePortsTextBox.Location = new Point(108, 75);
            remotePortsTextBox.Margin = new Padding(3, 2, 3, 2);
            remotePortsTextBox.Name = "remotePortsTextBox";
            remotePortsTextBox.Size = new Size(268, 23);
            remotePortsTextBox.TabIndex = 5;
            remotePortsTextBox.Text = "*";
            remotePortsTextBox.Validating += ValidatePortTextBox_Validating;
            // 
            // labelRemotePorts
            // 
            labelRemotePorts.AutoSize = true;
            labelRemotePorts.Location = new Point(5, 78);
            labelRemotePorts.Name = "labelRemotePorts";
            labelRemotePorts.Size = new Size(91, 16);
            labelRemotePorts.TabIndex = 4;
            labelRemotePorts.Text = "Remote Ports";
            // 
            // localPortsTextBox
            // 
            localPortsTextBox.Location = new Point(108, 49);
            localPortsTextBox.Margin = new Padding(3, 2, 3, 2);
            localPortsTextBox.Name = "localPortsTextBox";
            localPortsTextBox.Size = new Size(268, 23);
            localPortsTextBox.TabIndex = 3;
            localPortsTextBox.Text = "*";
            localPortsTextBox.Validating += ValidatePortTextBox_Validating;
            // 
            // labelLocalPorts
            // 
            labelLocalPorts.AutoSize = true;
            labelLocalPorts.Location = new Point(5, 51);
            labelLocalPorts.Name = "labelLocalPorts";
            labelLocalPorts.Size = new Size(84, 16);
            labelLocalPorts.TabIndex = 2;
            labelLocalPorts.Text = "Local Ports";
            // 
            // protocolComboBox
            // 
            protocolComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            protocolComboBox.FormattingEnabled = true;
            protocolComboBox.Location = new Point(108, 21);
            protocolComboBox.Margin = new Padding(3, 2, 3, 2);
            protocolComboBox.Name = "protocolComboBox";
            protocolComboBox.Size = new Size(133, 24);
            protocolComboBox.TabIndex = 1;
            // 
            // labelProtocol
            // 
            labelProtocol.AutoSize = true;
            labelProtocol.Location = new Point(5, 23);
            labelProtocol.Name = "labelProtocol";
            labelProtocol.Size = new Size(63, 16);
            labelProtocol.TabIndex = 0;
            labelProtocol.Text = "Protocol";
            // 
            // WildcardCreatorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(438, 535);
            Controls.Add(advancedGroupBox);
            Controls.Add(advancedButton);
            Controls.Add(exeNameNoteLabel);
            Controls.Add(instructionLabel);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(actionGroupBox);
            Controls.Add(exeNameTextBox);
            Controls.Add(folderPathTextBox);
            Controls.Add(browseButton);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WildcardCreatorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Wildcard Rule";
            actionGroupBox.ResumeLayout(false);
            actionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            advancedGroupBox.ResumeLayout(false);
            advancedGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
    }
}
