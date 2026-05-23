namespace MinimalFirewall
{
    partial class CreateAdvancedRuleForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox ruleNameTextBox;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.CheckBox enabledCheckBox;
        private System.Windows.Forms.GroupBox actionGroupBox;
        private System.Windows.Forms.RadioButton blockRadioButton;
        private System.Windows.Forms.RadioButton allowRadioButton;
        private System.Windows.Forms.GroupBox directionGroupBox;
        private System.Windows.Forms.RadioButton bothDirRadioButton;
        private System.Windows.Forms.RadioButton outboundRadioButton;
        private System.Windows.Forms.RadioButton inboundRadioButton;
        private System.Windows.Forms.GroupBox programGroupBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox programPathTextBox;
        private System.Windows.Forms.Label labelProgram;
        private System.Windows.Forms.TextBox serviceNameTextBox;
        private System.Windows.Forms.Label labelService;
        private System.Windows.Forms.GroupBox protocolGroupBox;
        private DarkModeForms.FlatComboBox protocolComboBox;
        private System.Windows.Forms.GroupBox portsGroupBox;
        private System.Windows.Forms.TextBox remotePortsTextBox;
        private System.Windows.Forms.Label labelRemotePorts;
        private System.Windows.Forms.TextBox localPortsTextBox;
        private System.Windows.Forms.Label labelLocalPorts;
        private System.Windows.Forms.GroupBox icmpGroupBox;
        private System.Windows.Forms.TextBox icmpTypesAndCodesTextBox;
        private System.Windows.Forms.Label labelIcmpInfo;
        private System.Windows.Forms.GroupBox scopeGroupBox;
        private System.Windows.Forms.TextBox remoteAddressTextBox;
        private System.Windows.Forms.Label labelRemoteAddress;
        private System.Windows.Forms.TextBox localAddressTextBox;
        private System.Windows.Forms.Label labelLocalAddress;
        private System.Windows.Forms.Label labelScopeExample;
        private System.Windows.Forms.GroupBox profilesGroupBox;
        private System.Windows.Forms.CheckBox publicCheckBox;
        private System.Windows.Forms.CheckBox privateCheckBox;
        private System.Windows.Forms.CheckBox domainCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox interfaceTypesGroupBox;
        private System.Windows.Forms.CheckBox lanCheckBox;
        private System.Windows.Forms.CheckBox wirelessCheckBox;
        private System.Windows.Forms.CheckBox remoteAccessCheckBox;
        private System.Windows.Forms.Label labelGroup;
        private DarkModeForms.FlatComboBox groupComboBox;
        private System.Windows.Forms.Button addGroupButton;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label programPathNoteLabel;
        private System.Windows.Forms.Button browseServiceButton;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label labelIcmpExample;

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
            labelName = new Label();
            labelIcmpExample = new Label();
            ruleNameTextBox = new TextBox();
            labelDescription = new Label();
            descriptionTextBox = new TextBox();
            enabledCheckBox = new CheckBox();
            actionGroupBox = new GroupBox();
            blockRadioButton = new RadioButton();
            allowRadioButton = new RadioButton();
            directionGroupBox = new GroupBox();
            bothDirRadioButton = new RadioButton();
            outboundRadioButton = new RadioButton();
            inboundRadioButton = new RadioButton();
            programGroupBox = new GroupBox();
            browseServiceButton = new Button();
            programPathNoteLabel = new Label();
            serviceNameTextBox = new TextBox();
            labelService = new Label();
            browseButton = new Button();
            programPathTextBox = new TextBox();
            labelProgram = new Label();
            protocolGroupBox = new GroupBox();
            protocolComboBox = new DarkModeForms.FlatComboBox();
            portsGroupBox = new GroupBox();
            remotePortsTextBox = new TextBox();
            labelRemotePorts = new Label();
            localPortsTextBox = new TextBox();
            labelLocalPorts = new Label();
            icmpGroupBox = new GroupBox();
            labelIcmpInfo = new Label();
            icmpTypesAndCodesTextBox = new TextBox();
            scopeGroupBox = new GroupBox();
            labelScopeExample = new Label();
            remoteAddressTextBox = new TextBox();
            labelRemoteAddress = new Label();
            localAddressTextBox = new TextBox();
            labelLocalAddress = new Label();
            profilesGroupBox = new GroupBox();
            publicCheckBox = new CheckBox();
            privateCheckBox = new CheckBox();
            domainCheckBox = new CheckBox();
            okButton = new Button();
            cancelButton = new Button();
            interfaceTypesGroupBox = new GroupBox();
            lanCheckBox = new CheckBox();
            wirelessCheckBox = new CheckBox();
            remoteAccessCheckBox = new CheckBox();
            labelGroup = new Label();
            groupComboBox = new DarkModeForms.FlatComboBox();
            addGroupButton = new Button();
            mainPanel = new Panel();
            bottomPanel = new Panel();
            errorProvider1 = new ErrorProvider(components);
            actionGroupBox.SuspendLayout();
            directionGroupBox.SuspendLayout();
            programGroupBox.SuspendLayout();
            protocolGroupBox.SuspendLayout();
            portsGroupBox.SuspendLayout();
            icmpGroupBox.SuspendLayout();
            scopeGroupBox.SuspendLayout();
            profilesGroupBox.SuspendLayout();
            interfaceTypesGroupBox.SuspendLayout();
            mainPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            SuspendLayout();
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(10, 12);
            labelName.Name = "labelName";
            labelName.Size = new Size(35, 16);
            labelName.TabIndex = 0;
            labelName.Text = "Name";
            // 
            // labelIcmpExample
            // 
            labelIcmpExample.AutoSize = true;
            labelIcmpExample.ForeColor = SystemColors.GrayText;
            labelIcmpExample.Location = new Point(94, 42);
            labelIcmpExample.Name = "labelIcmpExample";
            labelIcmpExample.Size = new Size(182, 16);
            labelIcmpExample.TabIndex = 2;
            labelIcmpExample.Text = "(e.g., 8:0, 3:*, 5, or *)";
            // 
            // ruleNameTextBox
            // 
            ruleNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ruleNameTextBox.Location = new Point(51, 10);
            ruleNameTextBox.Margin = new Padding(3, 2, 3, 2);
            ruleNameTextBox.Name = "ruleNameTextBox";
            ruleNameTextBox.Size = new Size(655, 23);
            ruleNameTextBox.TabIndex = 1;
            // 
            // labelDescription
            // 
            labelDescription.AutoSize = true;
            labelDescription.Location = new Point(10, 35);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(84, 16);
            labelDescription.TabIndex = 2;
            labelDescription.Text = "Description";
            // 
            // descriptionTextBox
            // 
            descriptionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            descriptionTextBox.Location = new Point(100, 33);
            descriptionTextBox.Margin = new Padding(3, 2, 3, 2);
            descriptionTextBox.Name = "descriptionTextBox";
            descriptionTextBox.Size = new Size(606, 23);
            descriptionTextBox.TabIndex = 3;
            // 
            // enabledCheckBox
            // 
            enabledCheckBox.AutoSize = true;
            enabledCheckBox.Checked = true;
            enabledCheckBox.CheckState = CheckState.Checked;
            enabledCheckBox.Location = new Point(13, 60);
            enabledCheckBox.Margin = new Padding(3, 2, 3, 2);
            enabledCheckBox.Name = "enabledCheckBox";
            enabledCheckBox.Size = new Size(75, 20);
            enabledCheckBox.TabIndex = 4;
            enabledCheckBox.Text = "Enabled";
            enabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // actionGroupBox
            // 
            actionGroupBox.Controls.Add(blockRadioButton);
            actionGroupBox.Controls.Add(allowRadioButton);
            actionGroupBox.Location = new Point(13, 80);
            actionGroupBox.Margin = new Padding(3, 2, 3, 2);
            actionGroupBox.Name = "actionGroupBox";
            actionGroupBox.Padding = new Padding(3, 2, 3, 2);
            actionGroupBox.Size = new Size(114, 84);
            actionGroupBox.TabIndex = 5;
            actionGroupBox.TabStop = false;
            actionGroupBox.Text = "Action";
            // 
            // blockRadioButton
            // 
            blockRadioButton.AutoSize = true;
            blockRadioButton.Location = new Point(13, 48);
            blockRadioButton.Margin = new Padding(3, 2, 3, 2);
            blockRadioButton.Name = "blockRadioButton";
            blockRadioButton.Size = new Size(60, 20);
            blockRadioButton.TabIndex = 1;
            blockRadioButton.Text = "Block";
            blockRadioButton.UseVisualStyleBackColor = true;
            // 
            // allowRadioButton
            // 
            allowRadioButton.AutoSize = true;
            allowRadioButton.Checked = true;
            allowRadioButton.Location = new Point(13, 23);
            allowRadioButton.Margin = new Padding(3, 2, 3, 2);
            allowRadioButton.Name = "allowRadioButton";
            allowRadioButton.Size = new Size(60, 20);
            allowRadioButton.TabIndex = 0;
            allowRadioButton.TabStop = true;
            allowRadioButton.Text = "Allow";
            allowRadioButton.UseVisualStyleBackColor = true;
            // 
            // directionGroupBox
            // 
            directionGroupBox.Controls.Add(bothDirRadioButton);
            directionGroupBox.Controls.Add(outboundRadioButton);
            directionGroupBox.Controls.Add(inboundRadioButton);
            directionGroupBox.Location = new Point(132, 80);
            directionGroupBox.Margin = new Padding(3, 2, 3, 2);
            directionGroupBox.Name = "directionGroupBox";
            directionGroupBox.Padding = new Padding(3, 2, 3, 2);
            directionGroupBox.Size = new Size(114, 84);
            directionGroupBox.TabIndex = 6;
            directionGroupBox.TabStop = false;
            directionGroupBox.Text = "Direction";
            // 
            // bothDirRadioButton
            // 
            bothDirRadioButton.AutoSize = true;
            bothDirRadioButton.Location = new Point(13, 58);
            bothDirRadioButton.Margin = new Padding(3, 2, 3, 2);
            bothDirRadioButton.Name = "bothDirRadioButton";
            bothDirRadioButton.Size = new Size(53, 20);
            bothDirRadioButton.TabIndex = 2;
            bothDirRadioButton.Text = "Both";
            bothDirRadioButton.UseVisualStyleBackColor = true;
            // 
            // outboundRadioButton
            // 
            outboundRadioButton.AutoSize = true;
            outboundRadioButton.Checked = true;
            outboundRadioButton.Location = new Point(13, 37);
            outboundRadioButton.Margin = new Padding(3, 2, 3, 2);
            outboundRadioButton.Name = "outboundRadioButton";
            outboundRadioButton.Size = new Size(81, 20);
            outboundRadioButton.TabIndex = 1;
            outboundRadioButton.TabStop = true;
            outboundRadioButton.Text = "Outbound";
            outboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // inboundRadioButton
            // 
            inboundRadioButton.AutoSize = true;
            inboundRadioButton.Location = new Point(13, 17);
            inboundRadioButton.Margin = new Padding(3, 2, 3, 2);
            inboundRadioButton.Name = "inboundRadioButton";
            inboundRadioButton.Size = new Size(74, 20);
            inboundRadioButton.TabIndex = 0;
            inboundRadioButton.Text = "Inbound";
            inboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // programGroupBox
            // 
            programGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            programGroupBox.Controls.Add(browseServiceButton);
            programGroupBox.Controls.Add(programPathNoteLabel);
            programGroupBox.Controls.Add(serviceNameTextBox);
            programGroupBox.Controls.Add(labelService);
            programGroupBox.Controls.Add(browseButton);
            programGroupBox.Controls.Add(programPathTextBox);
            programGroupBox.Controls.Add(labelProgram);
            programGroupBox.Location = new Point(13, 169);
            programGroupBox.Margin = new Padding(3, 2, 3, 2);
            programGroupBox.Name = "programGroupBox";
            programGroupBox.Padding = new Padding(3, 2, 3, 2);
            programGroupBox.Size = new Size(692, 112);
            programGroupBox.TabIndex = 7;
            programGroupBox.TabStop = false;
            programGroupBox.Text = "Program and Service";
            programGroupBox.Enter += programGroupBox_Enter;
            // 
            // browseServiceButton
            // 
            browseServiceButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseServiceButton.Location = new Point(605, 66);
            browseServiceButton.Margin = new Padding(3, 2, 3, 2);
            browseServiceButton.Name = "browseServiceButton";
            browseServiceButton.Size = new Size(82, 23);
            browseServiceButton.TabIndex = 7;
            browseServiceButton.Text = "Browse...";
            browseServiceButton.UseVisualStyleBackColor = true;
            browseServiceButton.Click += browseServiceButton_Click;
            // 
            // programPathNoteLabel
            // 
            programPathNoteLabel.AutoSize = true;
            programPathNoteLabel.ForeColor = SystemColors.GrayText;
            programPathNoteLabel.Location = new Point(66, 41);
            programPathNoteLabel.Name = "programPathNoteLabel";
            programPathNoteLabel.Size = new Size(462, 16);
            programPathNoteLabel.TabIndex = 6;
            programPathNoteLabel.Text = "Leave blank to apply the rule to any program hosting the service.";
            // 
            // serviceNameTextBox
            // 
            serviceNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            serviceNameTextBox.Location = new Point(79, 66);
            serviceNameTextBox.Margin = new Padding(3, 2, 3, 2);
            serviceNameTextBox.Name = "serviceNameTextBox";
            serviceNameTextBox.Size = new Size(521, 23);
            serviceNameTextBox.TabIndex = 4;
            // 
            // labelService
            // 
            labelService.AutoSize = true;
            labelService.Location = new Point(13, 68);
            labelService.Name = "labelService";
            labelService.Size = new Size(56, 16);
            labelService.TabIndex = 3;
            labelService.Text = "Service";
            // 
            // browseButton
            // 
            browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseButton.Location = new Point(605, 17);
            browseButton.Margin = new Padding(3, 2, 3, 2);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(82, 23);
            browseButton.TabIndex = 2;
            browseButton.Text = "Browse...";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += BrowseButton_Click;
            // 
            // programPathTextBox
            // 
            programPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            programPathTextBox.Location = new Point(79, 17);
            programPathTextBox.Margin = new Padding(3, 2, 3, 2);
            programPathTextBox.Name = "programPathTextBox";
            programPathTextBox.Size = new Size(521, 23);
            programPathTextBox.TabIndex = 1;
            // 
            // labelProgram
            // 
            labelProgram.AutoSize = true;
            labelProgram.Location = new Point(13, 20);
            labelProgram.Name = "labelProgram";
            labelProgram.Size = new Size(56, 16);
            labelProgram.TabIndex = 0;
            labelProgram.Text = "Program";
            // 
            // protocolGroupBox
            // 
            protocolGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            protocolGroupBox.Controls.Add(protocolComboBox);
            protocolGroupBox.Location = new Point(13, 286);
            protocolGroupBox.Margin = new Padding(3, 2, 3, 2);
            protocolGroupBox.Name = "protocolGroupBox";
            protocolGroupBox.Padding = new Padding(3, 2, 3, 2);
            protocolGroupBox.Size = new Size(692, 48);
            protocolGroupBox.TabIndex = 8;
            protocolGroupBox.TabStop = false;
            protocolGroupBox.Text = "Protocol";
            // 
            // protocolComboBox
            // 
            protocolComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            protocolComboBox.FormattingEnabled = true;
            protocolComboBox.Location = new Point(20, 17);
            protocolComboBox.Margin = new Padding(3, 2, 3, 2);
            protocolComboBox.Name = "protocolComboBox";
            protocolComboBox.Size = new Size(132, 24);
            protocolComboBox.TabIndex = 1;
            protocolComboBox.SelectedIndexChanged += ProtocolComboBox_SelectedIndexChanged;
            // 
            // portsGroupBox
            // 
            portsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            portsGroupBox.Controls.Add(remotePortsTextBox);
            portsGroupBox.Controls.Add(labelRemotePorts);
            portsGroupBox.Controls.Add(localPortsTextBox);
            portsGroupBox.Controls.Add(labelLocalPorts);
            portsGroupBox.Location = new Point(13, 338);
            portsGroupBox.Margin = new Padding(3, 2, 3, 2);
            portsGroupBox.Name = "portsGroupBox";
            portsGroupBox.Padding = new Padding(3, 2, 3, 2);
            portsGroupBox.Size = new Size(692, 73);
            portsGroupBox.TabIndex = 9;
            portsGroupBox.TabStop = false;
            portsGroupBox.Text = "Ports";
            portsGroupBox.Visible = false;
            // 
            // remotePortsTextBox
            // 
            remotePortsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            remotePortsTextBox.Location = new Point(96, 43);
            remotePortsTextBox.Margin = new Padding(3, 2, 3, 2);
            remotePortsTextBox.Name = "remotePortsTextBox";
            remotePortsTextBox.Size = new Size(562, 23);
            remotePortsTextBox.TabIndex = 3;
            remotePortsTextBox.Text = "*";
            remotePortsTextBox.Validating += ValidatePortTextBox_Validating;
            // 
            // labelRemotePorts
            // 
            labelRemotePorts.AutoSize = true;
            labelRemotePorts.Location = new Point(5, 45);
            labelRemotePorts.Name = "labelRemotePorts";
            labelRemotePorts.Size = new Size(91, 16);
            labelRemotePorts.TabIndex = 2;
            labelRemotePorts.Text = "Remote Ports";
            // 
            // localPortsTextBox
            // 
            localPortsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            localPortsTextBox.Location = new Point(96, 17);
            localPortsTextBox.Margin = new Padding(3, 2, 3, 2);
            localPortsTextBox.Name = "localPortsTextBox";
            localPortsTextBox.Size = new Size(562, 23);
            localPortsTextBox.TabIndex = 1;
            localPortsTextBox.Text = "*";
            localPortsTextBox.Validating += ValidatePortTextBox_Validating;
            // 
            // labelLocalPorts
            // 
            labelLocalPorts.AutoSize = true;
            labelLocalPorts.Location = new Point(5, 20);
            labelLocalPorts.Name = "labelLocalPorts";
            labelLocalPorts.Size = new Size(84, 16);
            labelLocalPorts.TabIndex = 0;
            labelLocalPorts.Text = "Local Ports";
            // 
            // icmpGroupBox
            // 
            icmpGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            icmpGroupBox.Controls.Add(labelIcmpInfo);
            icmpGroupBox.Controls.Add(labelIcmpExample);
            icmpGroupBox.Controls.Add(icmpTypesAndCodesTextBox);
            icmpGroupBox.Location = new Point(13, 338);
            icmpGroupBox.Margin = new Padding(3, 2, 3, 2);
            icmpGroupBox.Name = "icmpGroupBox";
            icmpGroupBox.Padding = new Padding(3, 2, 3, 2);
            icmpGroupBox.Size = new Size(692, 73);
            icmpGroupBox.TabIndex = 10;
            icmpGroupBox.TabStop = false;
            icmpGroupBox.Text = "ICMP Settings";
            icmpGroupBox.Visible = false;
            // 
            // labelIcmpInfo
            // 
            labelIcmpInfo.AutoSize = true;
            labelIcmpInfo.Location = new Point(13, 20);
            labelIcmpInfo.Name = "labelIcmpInfo";
            labelIcmpInfo.Size = new Size(70, 16);
            labelIcmpInfo.TabIndex = 0;
            labelIcmpInfo.Text = "Type:Code";
            // 
            // icmpTypesAndCodesTextBox
            // 
            icmpTypesAndCodesTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            icmpTypesAndCodesTextBox.Location = new Point(96, 17);
            icmpTypesAndCodesTextBox.Margin = new Padding(3, 2, 3, 2);
            icmpTypesAndCodesTextBox.Name = "icmpTypesAndCodesTextBox";
            icmpTypesAndCodesTextBox.Size = new Size(562, 23);
            icmpTypesAndCodesTextBox.TabIndex = 1;
            icmpTypesAndCodesTextBox.Text = "*";
            icmpTypesAndCodesTextBox.Validating += icmpTypesAndCodesTextBox_Validating;
            // 
            // scopeGroupBox
            // 
            scopeGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scopeGroupBox.Controls.Add(labelScopeExample);
            scopeGroupBox.Controls.Add(remoteAddressTextBox);
            scopeGroupBox.Controls.Add(labelRemoteAddress);
            scopeGroupBox.Controls.Add(localAddressTextBox);
            scopeGroupBox.Controls.Add(labelLocalAddress);
            scopeGroupBox.Location = new Point(13, 415);
            scopeGroupBox.Margin = new Padding(3, 2, 3, 2);
            scopeGroupBox.Name = "scopeGroupBox";
            scopeGroupBox.Padding = new Padding(3, 2, 3, 2);
            scopeGroupBox.Size = new Size(692, 92);
            scopeGroupBox.TabIndex = 11;
            scopeGroupBox.TabStop = false;
            scopeGroupBox.Text = "Scope (Addresses)";
            // 
            // labelScopeExample
            // 
            labelScopeExample.AutoSize = true;
            labelScopeExample.ForeColor = SystemColors.GrayText;
            labelScopeExample.Location = new Point(89, 68);
            labelScopeExample.Name = "labelScopeExample";
            labelScopeExample.Size = new Size(385, 16);
            labelScopeExample.TabIndex = 4;
            labelScopeExample.Text = "Keywords: LocalSubnet, DNS, DHCP, WINS, DefaultGateway";
            // 
            // remoteAddressTextBox
            // 
            remoteAddressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            remoteAddressTextBox.Location = new Point(119, 43);
            remoteAddressTextBox.Margin = new Padding(3, 2, 3, 2);
            remoteAddressTextBox.Name = "remoteAddressTextBox";
            remoteAddressTextBox.Size = new Size(539, 23);
            remoteAddressTextBox.TabIndex = 3;
            remoteAddressTextBox.Text = "*";
            remoteAddressTextBox.Validating += remoteAddressTextBox_Validating;
            // 
            // labelRemoteAddress
            // 
            labelRemoteAddress.AutoSize = true;
            labelRemoteAddress.Location = new Point(5, 45);
            labelRemoteAddress.Name = "labelRemoteAddress";
            labelRemoteAddress.Size = new Size(105, 16);
            labelRemoteAddress.TabIndex = 2;
            labelRemoteAddress.Text = "Remote Address";
            // 
            // localAddressTextBox
            // 
            localAddressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            localAddressTextBox.Location = new Point(119, 17);
            localAddressTextBox.Margin = new Padding(3, 2, 3, 2);
            localAddressTextBox.Name = "localAddressTextBox";
            localAddressTextBox.Size = new Size(539, 23);
            localAddressTextBox.TabIndex = 1;
            localAddressTextBox.Text = "*";
            localAddressTextBox.Validating += localAddressTextBox_Validating;
            // 
            // labelLocalAddress
            // 
            labelLocalAddress.AutoSize = true;
            labelLocalAddress.Location = new Point(5, 20);
            labelLocalAddress.Name = "labelLocalAddress";
            labelLocalAddress.Size = new Size(98, 16);
            labelLocalAddress.TabIndex = 0;
            labelLocalAddress.Text = "Local Address";
            // 
            // profilesGroupBox
            // 
            profilesGroupBox.Controls.Add(publicCheckBox);
            profilesGroupBox.Controls.Add(privateCheckBox);
            profilesGroupBox.Controls.Add(domainCheckBox);
            profilesGroupBox.Location = new Point(251, 80);
            profilesGroupBox.Margin = new Padding(3, 2, 3, 2);
            profilesGroupBox.Name = "profilesGroupBox";
            profilesGroupBox.Padding = new Padding(3, 2, 3, 2);
            profilesGroupBox.Size = new Size(114, 84);
            profilesGroupBox.TabIndex = 12;
            profilesGroupBox.TabStop = false;
            profilesGroupBox.Text = "Profiles";
            // 
            // publicCheckBox
            // 
            publicCheckBox.AutoSize = true;
            publicCheckBox.Checked = true;
            publicCheckBox.CheckState = CheckState.Checked;
            publicCheckBox.Location = new Point(13, 58);
            publicCheckBox.Margin = new Padding(3, 2, 3, 2);
            publicCheckBox.Name = "publicCheckBox";
            publicCheckBox.Size = new Size(68, 20);
            publicCheckBox.TabIndex = 2;
            publicCheckBox.Text = "Public";
            publicCheckBox.UseVisualStyleBackColor = true;
            // 
            // privateCheckBox
            // 
            privateCheckBox.AutoSize = true;
            privateCheckBox.Checked = true;
            privateCheckBox.CheckState = CheckState.Checked;
            privateCheckBox.Location = new Point(13, 37);
            privateCheckBox.Margin = new Padding(3, 2, 3, 2);
            privateCheckBox.Name = "privateCheckBox";
            privateCheckBox.Size = new Size(75, 20);
            privateCheckBox.TabIndex = 1;
            privateCheckBox.Text = "Private";
            privateCheckBox.UseVisualStyleBackColor = true;
            // 
            // domainCheckBox
            // 
            domainCheckBox.AutoSize = true;
            domainCheckBox.Checked = true;
            domainCheckBox.CheckState = CheckState.Checked;
            domainCheckBox.Location = new Point(13, 17);
            domainCheckBox.Margin = new Padding(3, 2, 3, 2);
            domainCheckBox.Name = "domainCheckBox";
            domainCheckBox.Size = new Size(68, 20);
            domainCheckBox.TabIndex = 0;
            domainCheckBox.Text = "Domain";
            domainCheckBox.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okButton.Location = new Point(538, 6);
            okButton.Margin = new Padding(3, 2, 3, 2);
            okButton.Name = "okButton";
            okButton.Size = new Size(88, 29);
            okButton.TabIndex = 13;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelButton.Location = new Point(632, 6);
            cancelButton.Margin = new Padding(3, 2, 3, 2);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 29);
            cancelButton.TabIndex = 14;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // interfaceTypesGroupBox
            // 
            interfaceTypesGroupBox.Controls.Add(lanCheckBox);
            interfaceTypesGroupBox.Controls.Add(wirelessCheckBox);
            interfaceTypesGroupBox.Controls.Add(remoteAccessCheckBox);
            interfaceTypesGroupBox.Location = new Point(370, 80);
            interfaceTypesGroupBox.Margin = new Padding(3, 2, 3, 2);
            interfaceTypesGroupBox.Name = "interfaceTypesGroupBox";
            interfaceTypesGroupBox.Padding = new Padding(3, 2, 3, 2);
            interfaceTypesGroupBox.Size = new Size(130, 84);
            interfaceTypesGroupBox.TabIndex = 15;
            interfaceTypesGroupBox.TabStop = false;
            interfaceTypesGroupBox.Text = "Interface Types";
            // 
            // lanCheckBox
            // 
            lanCheckBox.AutoSize = true;
            lanCheckBox.Checked = true;
            lanCheckBox.CheckState = CheckState.Checked;
            lanCheckBox.Location = new Point(13, 58);
            lanCheckBox.Margin = new Padding(3, 2, 3, 2);
            lanCheckBox.Name = "lanCheckBox";
            lanCheckBox.Size = new Size(103, 20);
            lanCheckBox.TabIndex = 2;
            lanCheckBox.Text = "Wired (LAN)";
            lanCheckBox.UseVisualStyleBackColor = true;
            // 
            // wirelessCheckBox
            // 
            wirelessCheckBox.AutoSize = true;
            wirelessCheckBox.Checked = true;
            wirelessCheckBox.CheckState = CheckState.Checked;
            wirelessCheckBox.Location = new Point(13, 37);
            wirelessCheckBox.Margin = new Padding(3, 2, 3, 2);
            wirelessCheckBox.Name = "wirelessCheckBox";
            wirelessCheckBox.Size = new Size(82, 20);
            wirelessCheckBox.TabIndex = 1;
            wirelessCheckBox.Text = "Wireless";
            wirelessCheckBox.UseVisualStyleBackColor = true;
            // 
            // remoteAccessCheckBox
            // 
            remoteAccessCheckBox.AutoSize = true;
            remoteAccessCheckBox.Checked = true;
            remoteAccessCheckBox.CheckState = CheckState.Checked;
            remoteAccessCheckBox.Location = new Point(13, 17);
            remoteAccessCheckBox.Margin = new Padding(3, 2, 3, 2);
            remoteAccessCheckBox.Name = "remoteAccessCheckBox";
            remoteAccessCheckBox.Size = new Size(110, 20);
            remoteAccessCheckBox.TabIndex = 0;
            remoteAccessCheckBox.Text = "Remote (VPN)";
            remoteAccessCheckBox.UseVisualStyleBackColor = true;
            // 
            // labelGroup
            // 
            labelGroup.AutoSize = true;
            labelGroup.Location = new Point(10, 13);
            labelGroup.Name = "labelGroup";
            labelGroup.Size = new Size(42, 16);
            labelGroup.TabIndex = 16;
            labelGroup.Text = "Group";
            // 
            // groupComboBox
            // 
            groupComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupComboBox.FormattingEnabled = true;
            groupComboBox.Location = new Point(70, 11);
            groupComboBox.Margin = new Padding(3, 2, 3, 2);
            groupComboBox.Name = "groupComboBox";
            groupComboBox.Size = new Size(337, 24);
            groupComboBox.TabIndex = 17;
            // 
            // addGroupButton
            // 
            addGroupButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addGroupButton.Location = new Point(412, 11);
            addGroupButton.Margin = new Padding(3, 2, 3, 2);
            addGroupButton.Name = "addGroupButton";
            addGroupButton.Size = new Size(96, 23);
            addGroupButton.TabIndex = 18;
            addGroupButton.Text = "Add Group";
            addGroupButton.UseVisualStyleBackColor = true;
            addGroupButton.Click += AddGroupButton_Click;
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.Controls.Add(labelName);
            mainPanel.Controls.Add(ruleNameTextBox);
            mainPanel.Controls.Add(labelDescription);
            mainPanel.Controls.Add(descriptionTextBox);
            mainPanel.Controls.Add(enabledCheckBox);
            mainPanel.Controls.Add(actionGroupBox);
            mainPanel.Controls.Add(directionGroupBox);
            mainPanel.Controls.Add(profilesGroupBox);
            mainPanel.Controls.Add(interfaceTypesGroupBox);
            mainPanel.Controls.Add(programGroupBox);
            mainPanel.Controls.Add(protocolGroupBox);
            mainPanel.Controls.Add(portsGroupBox);
            mainPanel.Controls.Add(icmpGroupBox);
            mainPanel.Controls.Add(scopeGroupBox);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Margin = new Padding(3, 2, 3, 2);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(730, 478);
            mainPanel.TabIndex = 19;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(addGroupButton);
            bottomPanel.Controls.Add(groupComboBox);
            bottomPanel.Controls.Add(labelGroup);
            bottomPanel.Controls.Add(cancelButton);
            bottomPanel.Controls.Add(okButton);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 478);
            bottomPanel.Margin = new Padding(3, 2, 3, 2);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(730, 48);
            bottomPanel.TabIndex = 20;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // CreateAdvancedRuleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 526);
            Controls.Add(mainPanel);
            Controls.Add(bottomPanel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(716, 328);
            Name = "CreateAdvancedRuleForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Advanced Rule";
            actionGroupBox.ResumeLayout(false);
            actionGroupBox.PerformLayout();
            directionGroupBox.ResumeLayout(false);
            directionGroupBox.PerformLayout();
            programGroupBox.ResumeLayout(false);
            programGroupBox.PerformLayout();
            protocolGroupBox.ResumeLayout(false);
            portsGroupBox.ResumeLayout(false);
            portsGroupBox.PerformLayout();
            icmpGroupBox.ResumeLayout(false);
            icmpGroupBox.PerformLayout();
            scopeGroupBox.ResumeLayout(false);
            scopeGroupBox.PerformLayout();
            profilesGroupBox.ResumeLayout(false);
            profilesGroupBox.PerformLayout();
            interfaceTypesGroupBox.ResumeLayout(false);
            interfaceTypesGroupBox.PerformLayout();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            ResumeLayout(false);

        }
        #endregion
    }
}
