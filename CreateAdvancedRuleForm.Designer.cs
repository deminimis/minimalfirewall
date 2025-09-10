// CreateAdvancedRuleForm.Designer.cs
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
        private System.Windows.Forms.Label labelProtocol;
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
            this.labelName = new System.Windows.Forms.Label();
            this.ruleNameTextBox = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.enabledCheckBox = new System.Windows.Forms.CheckBox();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.blockRadioButton = new System.Windows.Forms.RadioButton();
            this.allowRadioButton = new System.Windows.Forms.RadioButton();
            this.directionGroupBox = new System.Windows.Forms.GroupBox();
            this.bothDirRadioButton = new System.Windows.Forms.RadioButton();
            this.outboundRadioButton = new System.Windows.Forms.RadioButton();
            this.inboundRadioButton = new System.Windows.Forms.RadioButton();
            this.programGroupBox = new System.Windows.Forms.GroupBox();
            this.serviceNameTextBox = new System.Windows.Forms.TextBox();
            this.labelService = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.programPathTextBox = new System.Windows.Forms.TextBox();
            this.labelProgram = new System.Windows.Forms.Label();
            this.protocolGroupBox = new System.Windows.Forms.GroupBox();
            this.protocolComboBox = new DarkModeForms.FlatComboBox();
            this.labelProtocol = new System.Windows.Forms.Label();
            this.portsGroupBox = new System.Windows.Forms.GroupBox();
            this.remotePortsTextBox = new System.Windows.Forms.TextBox();
            this.labelRemotePorts = new System.Windows.Forms.Label();
            this.localPortsTextBox = new System.Windows.Forms.TextBox();
            this.labelLocalPorts = new System.Windows.Forms.Label();
            this.icmpGroupBox = new System.Windows.Forms.GroupBox();
            this.icmpTypesAndCodesTextBox = new System.Windows.Forms.TextBox();
            this.labelIcmpInfo = new System.Windows.Forms.Label();
            this.scopeGroupBox = new System.Windows.Forms.GroupBox();
            this.remoteAddressTextBox = new System.Windows.Forms.TextBox();
            this.labelRemoteAddress = new System.Windows.Forms.Label();
            this.localAddressTextBox = new System.Windows.Forms.TextBox();
            this.labelLocalAddress = new System.Windows.Forms.Label();
            this.profilesGroupBox = new System.Windows.Forms.GroupBox();
            this.publicCheckBox = new System.Windows.Forms.CheckBox();
            this.privateCheckBox = new System.Windows.Forms.CheckBox();
            this.domainCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.interfaceTypesGroupBox = new System.Windows.Forms.GroupBox();
            this.lanCheckBox = new System.Windows.Forms.CheckBox();
            this.wirelessCheckBox = new System.Windows.Forms.CheckBox();
            this.remoteAccessCheckBox = new System.Windows.Forms.CheckBox();
            this.labelGroup = new System.Windows.Forms.Label();
            this.groupComboBox = new DarkModeForms.FlatComboBox();
            this.addGroupButton = new System.Windows.Forms.Button();
            this.actionGroupBox.SuspendLayout();
            this.directionGroupBox.SuspendLayout();
            this.programGroupBox.SuspendLayout();
            this.protocolGroupBox.SuspendLayout();
            this.portsGroupBox.SuspendLayout();
            this.icmpGroupBox.SuspendLayout();
            this.scopeGroupBox.SuspendLayout();
            this.profilesGroupBox.SuspendLayout();
            this.interfaceTypesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(12, 15);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(39, 15);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // ruleNameTextBox
            // 
            this.ruleNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ruleNameTextBox.Location = new System.Drawing.Point(80, 12);
            this.ruleNameTextBox.Name = "ruleNameTextBox";
            this.ruleNameTextBox.Size = new System.Drawing.Size(692, 23);
            this.ruleNameTextBox.TabIndex = 1;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(12, 44);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(67, 15);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "Description";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.Location = new System.Drawing.Point(80, 41);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(692, 23);
            this.descriptionTextBox.TabIndex = 3;
            // 
            // enabledCheckBox
            // 
            this.enabledCheckBox.AutoSize = true;
            this.enabledCheckBox.Checked = true;
            this.enabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enabledCheckBox.Location = new System.Drawing.Point(15, 75);
            this.enabledCheckBox.Name = "enabledCheckBox";
            this.enabledCheckBox.Size = new System.Drawing.Size(92, 19);
            this.enabledCheckBox.TabIndex = 4;
            this.enabledCheckBox.Text = "Enabled";
            this.enabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // actionGroupBox
            // 
            this.actionGroupBox.Controls.Add(this.blockRadioButton);
            this.actionGroupBox.Controls.Add(this.allowRadioButton);
            this.actionGroupBox.Location = new System.Drawing.Point(15, 100);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(130, 105);
            this.actionGroupBox.TabIndex = 5;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";
            // 
            // blockRadioButton
            // 
            this.blockRadioButton.AutoSize = true;
            this.blockRadioButton.Location = new System.Drawing.Point(15, 60);
            this.blockRadioButton.Name = "blockRadioButton";
            this.blockRadioButton.Size = new System.Drawing.Size(53, 19);
            this.blockRadioButton.TabIndex = 1;
            this.blockRadioButton.Text = "Block";
            this.blockRadioButton.UseVisualStyleBackColor = true;
            // 
            // allowRadioButton
            // 
            this.allowRadioButton.AutoSize = true;
            this.allowRadioButton.Checked = true;
            this.allowRadioButton.Location = new System.Drawing.Point(15, 30);
            this.allowRadioButton.Name = "allowRadioButton";
            this.allowRadioButton.Size = new System.Drawing.Size(54, 19);
            this.allowRadioButton.TabIndex = 0;
            this.allowRadioButton.TabStop = true;
            this.allowRadioButton.Text = "Allow";
            this.allowRadioButton.UseVisualStyleBackColor = true;
            // 
            // directionGroupBox
            // 
            this.directionGroupBox.Controls.Add(this.bothDirRadioButton);
            this.directionGroupBox.Controls.Add(this.outboundRadioButton);
            this.directionGroupBox.Controls.Add(this.inboundRadioButton);
            this.directionGroupBox.Location = new System.Drawing.Point(151, 100);
            this.directionGroupBox.Name = "directionGroupBox";
            this.directionGroupBox.Size = new System.Drawing.Size(130, 105);
            this.directionGroupBox.TabIndex = 6;
            this.directionGroupBox.TabStop = false;
            this.directionGroupBox.Text = "Direction";
            // 
            // bothDirRadioButton
            // 
            this.bothDirRadioButton.AutoSize = true;
            this.bothDirRadioButton.Location = new System.Drawing.Point(15, 72);
            this.bothDirRadioButton.Name = "bothDirRadioButton";
            this.bothDirRadioButton.Size = new System.Drawing.Size(50, 19);
            this.bothDirRadioButton.TabIndex = 2;
            this.bothDirRadioButton.Text = "Both";
            this.bothDirRadioButton.UseVisualStyleBackColor = true;
            // 
            // outboundRadioButton
            // 
            this.outboundRadioButton.AutoSize = true;
            this.outboundRadioButton.Checked = true;
            this.outboundRadioButton.Location = new System.Drawing.Point(15, 47);
            this.outboundRadioButton.Name = "outboundRadioButton";
            this.outboundRadioButton.Size = new System.Drawing.Size(78, 19);
            this.outboundRadioButton.TabIndex = 1;
            this.outboundRadioButton.TabStop = true;
            this.outboundRadioButton.Text = "Outbound";
            this.outboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // inboundRadioButton
            // 
            this.inboundRadioButton.AutoSize = true;
            this.inboundRadioButton.Location = new System.Drawing.Point(15, 22);
            this.inboundRadioButton.Name = "inboundRadioButton";
            this.inboundRadioButton.Size = new System.Drawing.Size(70, 19);
            this.inboundRadioButton.TabIndex = 0;
            this.inboundRadioButton.Text = "Inbound";
            this.inboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // programGroupBox
            // 
            this.programGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.programGroupBox.Controls.Add(this.serviceNameTextBox);
            this.programGroupBox.Controls.Add(this.labelService);
            this.programGroupBox.Controls.Add(this.browseButton);
            this.programGroupBox.Controls.Add(this.programPathTextBox);
            this.programGroupBox.Controls.Add(this.labelProgram);
            this.programGroupBox.Location = new System.Drawing.Point(15, 211);
            this.programGroupBox.Name = "programGroupBox";
            this.programGroupBox.Size = new System.Drawing.Size(757, 90);
            this.programGroupBox.TabIndex = 7;
            this.programGroupBox.TabStop = false;
            this.programGroupBox.Text = "Program / Service";
            // 
            // serviceNameTextBox
            // 
            this.serviceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serviceNameTextBox.Location = new System.Drawing.Point(75, 53);
            this.serviceNameTextBox.Name = "serviceNameTextBox";
            this.serviceNameTextBox.Size = new System.Drawing.Size(576, 23);
            this.serviceNameTextBox.TabIndex = 4;
            // 
            // labelService
            // 
            this.labelService.AutoSize = true;
            this.labelService.Location = new System.Drawing.Point(15, 56);
            this.labelService.Name = "labelService";
            this.labelService.Size = new System.Drawing.Size(44, 15);
            this.labelService.TabIndex = 3;
            this.labelService.Text = "Service";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(657, 22);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(94, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // programPathTextBox
            // 
            this.programPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.programPathTextBox.Location = new System.Drawing.Point(75, 22);
            this.programPathTextBox.Name = "programPathTextBox";
            this.programPathTextBox.Size = new System.Drawing.Size(576, 23);
            this.programPathTextBox.TabIndex = 1;
            // 
            // labelProgram
            // 
            this.labelProgram.AutoSize = true;
            this.labelProgram.Location = new System.Drawing.Point(15, 25);
            this.labelProgram.Name = "labelProgram";
            this.labelProgram.Size = new System.Drawing.Size(53, 15);
            this.labelProgram.TabIndex = 0;
            this.labelProgram.Text = "Program";
            // 
            // protocolGroupBox
            // 
            this.protocolGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.protocolGroupBox.Controls.Add(this.protocolComboBox);
            this.protocolGroupBox.Controls.Add(this.labelProtocol);
            this.protocolGroupBox.Location = new System.Drawing.Point(15, 307);
            this.protocolGroupBox.Name = "protocolGroupBox";
            this.protocolGroupBox.Size = new System.Drawing.Size(757, 60);
            this.protocolGroupBox.TabIndex = 8;
            this.protocolGroupBox.TabStop = false;
            this.protocolGroupBox.Text = "Protocol";
            // 
            // protocolComboBox
            // 
            this.protocolComboBox.BorderColor = System.Drawing.Color.Gray;
            this.protocolComboBox.ButtonColor = System.Drawing.Color.LightGray;
            this.protocolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolComboBox.FormattingEnabled = true;
            this.protocolComboBox.Location = new System.Drawing.Point(75, 22);
            this.protocolComboBox.Name = "protocolComboBox";
            this.protocolComboBox.Size = new System.Drawing.Size(150, 23);
            this.protocolComboBox.TabIndex = 1;
            this.protocolComboBox.SelectedIndexChanged += new System.EventHandler(this.ProtocolComboBox_SelectedIndexChanged);
            // 
            // labelProtocol
            // 
            this.labelProtocol.AutoSize = true;
            this.labelProtocol.Location = new System.Drawing.Point(15, 25);
            this.labelProtocol.Name = "labelProtocol";
            this.labelProtocol.Size = new System.Drawing.Size(52, 15);
            this.labelProtocol.TabIndex = 0;
            this.labelProtocol.Text = "Protocol";
            // 
            // portsGroupBox
            // 
            this.portsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.portsGroupBox.Controls.Add(this.remotePortsTextBox);
            this.portsGroupBox.Controls.Add(this.labelRemotePorts);
            this.portsGroupBox.Controls.Add(this.localPortsTextBox);
            this.portsGroupBox.Controls.Add(this.labelLocalPorts);
            this.portsGroupBox.Location = new System.Drawing.Point(15, 373);
            this.portsGroupBox.Name = "portsGroupBox";
            this.portsGroupBox.Size = new System.Drawing.Size(757, 90);
            this.portsGroupBox.TabIndex = 9;
            this.portsGroupBox.TabStop = false;
            this.portsGroupBox.Text = "Ports";
            this.portsGroupBox.Visible = false;
            // 
            // remotePortsTextBox
            // 
            this.remotePortsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remotePortsTextBox.Location = new System.Drawing.Point(90, 53);
            this.remotePortsTextBox.Name = "remotePortsTextBox";
            this.remotePortsTextBox.Size = new System.Drawing.Size(657, 23);
            this.remotePortsTextBox.TabIndex = 3;
            this.remotePortsTextBox.Text = "*";
            // 
            // labelRemotePorts
            // 
            this.labelRemotePorts.AutoSize = true;
            this.labelRemotePorts.Location = new System.Drawing.Point(6, 56);
            this.labelRemotePorts.Name = "labelRemotePorts";
            this.labelRemotePorts.Size = new System.Drawing.Size(77, 15);
            this.labelRemotePorts.TabIndex = 2;
            this.labelRemotePorts.Text = "Remote Ports";
            // 
            // localPortsTextBox
            // 
            this.localPortsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localPortsTextBox.Location = new System.Drawing.Point(90, 22);
            this.localPortsTextBox.Name = "localPortsTextBox";
            this.localPortsTextBox.Size = new System.Drawing.Size(657, 23);
            this.localPortsTextBox.TabIndex = 1;
            this.localPortsTextBox.Text = "*";
            // 
            // labelLocalPorts
            // 
            this.labelLocalPorts.AutoSize = true;
            this.labelLocalPorts.Location = new System.Drawing.Point(6, 25);
            this.labelLocalPorts.Name = "labelLocalPorts";
            this.labelLocalPorts.Size = new System.Drawing.Size(65, 15);
            this.labelLocalPorts.TabIndex = 0;
            this.labelLocalPorts.Text = "Local Ports";
            // 
            // icmpGroupBox
            // 
            this.icmpGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.icmpGroupBox.Controls.Add(this.icmpTypesAndCodesTextBox);
            this.icmpGroupBox.Controls.Add(this.labelIcmpInfo);
            this.icmpGroupBox.Location = new System.Drawing.Point(15, 373);
            this.icmpGroupBox.Name = "icmpGroupBox";
            this.icmpGroupBox.Size = new System.Drawing.Size(757, 90);
            this.icmpGroupBox.TabIndex = 10;
            this.icmpGroupBox.TabStop = false;
            this.icmpGroupBox.Text = "ICMP Settings";
            this.icmpGroupBox.Visible = false;
            // 
            // icmpTypesAndCodesTextBox
            // 
            this.icmpTypesAndCodesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.icmpTypesAndCodesTextBox.Location = new System.Drawing.Point(110, 22);
            this.icmpTypesAndCodesTextBox.Name = "icmpTypesAndCodesTextBox";
            this.icmpTypesAndCodesTextBox.Size = new System.Drawing.Size(637, 23);
            this.icmpTypesAndCodesTextBox.TabIndex = 1;
            this.icmpTypesAndCodesTextBox.Text = "*";
            // 
            // labelIcmpInfo
            // 
            this.labelIcmpInfo.AutoSize = true;
            this.labelIcmpInfo.Location = new System.Drawing.Point(15, 25);
            this.labelIcmpInfo.Name = "labelIcmpInfo";
            this.labelIcmpInfo.Size = new System.Drawing.Size(89, 15);
            this.labelIcmpInfo.TabIndex = 0;
            this.labelIcmpInfo.Text = "Type:Code (e.g. 8:0)";
            // 
            // scopeGroupBox
            // 
            this.scopeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scopeGroupBox.Controls.Add(this.remoteAddressTextBox);
            this.scopeGroupBox.Controls.Add(this.labelRemoteAddress);
            this.scopeGroupBox.Controls.Add(this.localAddressTextBox);
            this.scopeGroupBox.Controls.Add(this.labelLocalAddress);
            this.scopeGroupBox.Location = new System.Drawing.Point(15, 469);
            this.scopeGroupBox.Name = "scopeGroupBox";
            this.scopeGroupBox.Size = new System.Drawing.Size(757, 90);
            this.scopeGroupBox.TabIndex = 11;
            this.scopeGroupBox.TabStop = false;
            this.scopeGroupBox.Text = "Scope (Addresses)";
            // 
            // remoteAddressTextBox
            // 
            this.remoteAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remoteAddressTextBox.Location = new System.Drawing.Point(105, 53);
            this.remoteAddressTextBox.Name = "remoteAddressTextBox";
            this.remoteAddressTextBox.Size = new System.Drawing.Size(642, 23);
            this.remoteAddressTextBox.TabIndex = 3;
            this.remoteAddressTextBox.Text = "*";
            // 
            // labelRemoteAddress
            // 
            this.labelRemoteAddress.AutoSize = true;
            this.labelRemoteAddress.Location = new System.Drawing.Point(6, 56);
            this.labelRemoteAddress.Name = "labelRemoteAddress";
            this.labelRemoteAddress.Size = new System.Drawing.Size(93, 15);
            this.labelRemoteAddress.TabIndex = 2;
            this.labelRemoteAddress.Text = "Remote Address";
            // 
            // localAddressTextBox
            // 
            this.localAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localAddressTextBox.Location = new System.Drawing.Point(105, 22);
            this.localAddressTextBox.Name = "localAddressTextBox";
            this.localAddressTextBox.Size = new System.Drawing.Size(642, 23);
            this.localAddressTextBox.TabIndex = 1;
            this.localAddressTextBox.Text = "*";
            // 
            // labelLocalAddress
            // 
            this.labelLocalAddress.AutoSize = true;
            this.labelLocalAddress.Location = new System.Drawing.Point(6, 25);
            this.labelLocalAddress.Name = "labelLocalAddress";
            this.labelLocalAddress.Size = new System.Drawing.Size(81, 15);
            this.labelLocalAddress.TabIndex = 0;
            this.labelLocalAddress.Text = "Local Address";
            // 
            // profilesGroupBox
            // 
            this.profilesGroupBox.Controls.Add(this.publicCheckBox);
            this.profilesGroupBox.Controls.Add(this.privateCheckBox);
            this.profilesGroupBox.Controls.Add(this.domainCheckBox);
            this.profilesGroupBox.Location = new System.Drawing.Point(287, 100);
            this.profilesGroupBox.Name = "profilesGroupBox";
            this.profilesGroupBox.Size = new System.Drawing.Size(130, 105);
            this.profilesGroupBox.TabIndex = 12;
            this.profilesGroupBox.TabStop = false;
            this.profilesGroupBox.Text = "Profiles";
            // 
            // publicCheckBox
            // 
            this.publicCheckBox.AutoSize = true;
            this.publicCheckBox.Checked = true;
            this.publicCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.publicCheckBox.Location = new System.Drawing.Point(15, 72);
            this.publicCheckBox.Name = "publicCheckBox";
            this.publicCheckBox.Size = new System.Drawing.Size(59, 19);
            this.publicCheckBox.TabIndex = 2;
            this.publicCheckBox.Text = "Public";
            this.publicCheckBox.UseVisualStyleBackColor = true;
            // 
            // privateCheckBox
            // 
            this.privateCheckBox.AutoSize = true;
            this.privateCheckBox.Checked = true;
            this.privateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.privateCheckBox.Location = new System.Drawing.Point(15, 47);
            this.privateCheckBox.Name = "privateCheckBox";
            this.privateCheckBox.Size = new System.Drawing.Size(62, 19);
            this.privateCheckBox.TabIndex = 1;
            this.privateCheckBox.Text = "Private";
            this.privateCheckBox.UseVisualStyleBackColor = true;
            // 
            // domainCheckBox
            // 
            this.domainCheckBox.AutoSize = true;
            this.domainCheckBox.Checked = true;
            this.domainCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.domainCheckBox.Location = new System.Drawing.Point(15, 22);
            this.domainCheckBox.Name = "domainCheckBox";
            this.domainCheckBox.Size = new System.Drawing.Size(67, 19);
            this.domainCheckBox.TabIndex = 0;
            this.domainCheckBox.Text = "Domain";
            this.domainCheckBox.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(556, 572);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 36);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(672, 572);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // interfaceTypesGroupBox
            // 
            this.interfaceTypesGroupBox.Controls.Add(this.lanCheckBox);
            this.interfaceTypesGroupBox.Controls.Add(this.wirelessCheckBox);
            this.interfaceTypesGroupBox.Controls.Add(this.remoteAccessCheckBox);
            this.interfaceTypesGroupBox.Location = new System.Drawing.Point(423, 100);
            this.interfaceTypesGroupBox.Name = "interfaceTypesGroupBox";
            this.interfaceTypesGroupBox.Size = new System.Drawing.Size(149, 105);
            this.interfaceTypesGroupBox.TabIndex = 15;
            this.interfaceTypesGroupBox.TabStop = false;
            this.interfaceTypesGroupBox.Text = "Interface Types";
            // 
            // lanCheckBox
            // 
            this.lanCheckBox.AutoSize = true;
            this.lanCheckBox.Checked = true;
            this.lanCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lanCheckBox.Location = new System.Drawing.Point(15, 72);
            this.lanCheckBox.Name = "lanCheckBox";
            this.lanCheckBox.Size = new System.Drawing.Size(109, 19);
            this.lanCheckBox.TabIndex = 2;
            this.lanCheckBox.Text = "Wired (LAN)";
            this.lanCheckBox.UseVisualStyleBackColor = true;
            // 
            // wirelessCheckBox
            // 
            this.wirelessCheckBox.AutoSize = true;
            this.wirelessCheckBox.Checked = true;
            this.wirelessCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wirelessCheckBox.Location = new System.Drawing.Point(15, 47);
            this.wirelessCheckBox.Name = "wirelessCheckBox";
            this.wirelessCheckBox.Size = new System.Drawing.Size(68, 19);
            this.wirelessCheckBox.TabIndex = 1;
            this.wirelessCheckBox.Text = "Wireless";
            this.wirelessCheckBox.UseVisualStyleBackColor = true;
            // 
            // remoteAccessCheckBox
            // 
            this.remoteAccessCheckBox.AutoSize = true;
            this.remoteAccessCheckBox.Checked = true;
            this.remoteAccessCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.remoteAccessCheckBox.Location = new System.Drawing.Point(15, 22);
            this.remoteAccessCheckBox.Name = "remoteAccessCheckBox";
            this.remoteAccessCheckBox.Size = new System.Drawing.Size(128, 19);
            this.remoteAccessCheckBox.TabIndex = 0;
            this.remoteAccessCheckBox.Text = "Remote (VPN)";
            this.remoteAccessCheckBox.UseVisualStyleBackColor = true;
            // 
            // labelGroup
            // 
            this.labelGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelGroup.AutoSize = true;
            this.labelGroup.Location = new System.Drawing.Point(12, 582);
            this.labelGroup.Name = "labelGroup";
            this.labelGroup.Size = new System.Drawing.Size(40, 15);
            this.labelGroup.TabIndex = 16;
            this.labelGroup.Text = "Group";
            // 
            // groupComboBox
            // 
            this.groupComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupComboBox.BorderColor = System.Drawing.Color.Gray;
            this.groupComboBox.ButtonColor = System.Drawing.Color.LightGray;
            this.groupComboBox.FormattingEnabled = true;
            this.groupComboBox.Location = new System.Drawing.Point(80, 579);
            this.groupComboBox.Name = "groupComboBox";
            this.groupComboBox.Size = new System.Drawing.Size(350, 23);
            this.groupComboBox.TabIndex = 17;
            // 
            // addGroupButton
            // 
            this.addGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addGroupButton.Location = new System.Drawing.Point(436, 579);
            this.addGroupButton.Name = "addGroupButton";
            this.addGroupButton.Size = new System.Drawing.Size(110, 23);
            this.addGroupButton.TabIndex = 18;
            this.addGroupButton.Text = "Add Group";
            this.addGroupButton.UseVisualStyleBackColor = true;
            this.addGroupButton.Click += new System.EventHandler(this.AddGroupButton_Click);
            // 
            // CreateAdvancedRuleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 620);
            this.Controls.Add(this.addGroupButton);
            this.Controls.Add(this.groupComboBox);
            this.Controls.Add(this.labelGroup);
            this.Controls.Add(this.interfaceTypesGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.profilesGroupBox);
            this.Controls.Add(this.scopeGroupBox);
            this.Controls.Add(this.icmpGroupBox);
            this.Controls.Add(this.portsGroupBox);
            this.Controls.Add(this.protocolGroupBox);
            this.Controls.Add(this.programGroupBox);
            this.Controls.Add(this.directionGroupBox);
            this.Controls.Add(this.actionGroupBox);
            this.Controls.Add(this.enabledCheckBox);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.ruleNameTextBox);
            this.Controls.Add(this.labelName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateAdvancedRuleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Advanced Rule";
            this.actionGroupBox.ResumeLayout(false);
            this.actionGroupBox.PerformLayout();
            this.directionGroupBox.ResumeLayout(false);
            this.directionGroupBox.PerformLayout();
            this.programGroupBox.ResumeLayout(false);
            this.programGroupBox.PerformLayout();
            this.protocolGroupBox.ResumeLayout(false);
            this.protocolGroupBox.PerformLayout();
            this.portsGroupBox.ResumeLayout(false);
            this.portsGroupBox.PerformLayout();
            this.icmpGroupBox.ResumeLayout(false);
            this.icmpGroupBox.PerformLayout();
            this.scopeGroupBox.ResumeLayout(false);
            this.scopeGroupBox.PerformLayout();
            this.profilesGroupBox.ResumeLayout(false);
            this.profilesGroupBox.PerformLayout();
            this.interfaceTypesGroupBox.ResumeLayout(false);
            this.interfaceTypesGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}