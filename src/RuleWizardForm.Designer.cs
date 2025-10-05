// File: C:/Users/anon/PROGRAMMING/C#/SimpleFirewall/VS Minimal Firewall/MinimalFirewall-NET8/MinimalFirewall-WindowsStore/RuleWizardForm.Designer.cs
namespace MinimalFirewall
{
    partial class RuleWizardForm
    {
        private System.ComponentModel.IContainer components = null;
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
            this.pnlSelection = new System.Windows.Forms.Panel();
            this.restrictAppButton = new System.Windows.Forms.Button();
            this.blockDeviceButton = new System.Windows.Forms.Button();
            this.allowFileShareButton = new System.Windows.Forms.Button();
            this.blockServiceButton = new System.Windows.Forms.Button();
            this.advancedRuleButton = new System.Windows.Forms.Button();
            this.wildcardRuleButton = new System.Windows.Forms.Button();
            this.portRuleButton = new System.Windows.Forms.Button();
            this.programRuleButton = new System.Windows.Forms.Button();
            this.pnlGetProgram = new System.Windows.Forms.Panel();
            this.browseButton = new System.Windows.Forms.Button();
            this.programPathTextBox = new System.Windows.Forms.TextBox();
            this.pnlGetPorts = new System.Windows.Forms.Panel();
            this.portsProgramPathTextBox = new System.Windows.Forms.TextBox();
            this.portsBrowseButton = new System.Windows.Forms.Button();
            this.restrictToProgramCheckBox = new System.Windows.Forms.CheckBox();
            this.portsTextBox = new System.Windows.Forms.TextBox();
            this.portsLabel = new System.Windows.Forms.Label();
            this.pnlGetProtocol = new System.Windows.Forms.Panel();
            this.bothProtocolRadioButton = new System.Windows.Forms.RadioButton();
            this.udpRadioButton = new System.Windows.Forms.RadioButton();
            this.tcpRadioButton = new System.Windows.Forms.RadioButton();
            this.pnlSummary = new System.Windows.Forms.Panel();
            this.summaryLabel = new System.Windows.Forms.Label();
            this.pnlGetName = new System.Windows.Forms.Panel();
            this.ruleNameTextBox = new System.Windows.Forms.TextBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.topPanel = new System.Windows.Forms.Panel();
            this.mainHeaderLabel = new System.Windows.Forms.Label();
            this.pnlGetAction = new System.Windows.Forms.Panel();
            this.blockActionRadioButton = new System.Windows.Forms.RadioButton();
            this.allowActionRadioButton = new System.Windows.Forms.RadioButton();
            this.pnlGetDirection = new System.Windows.Forms.Panel();
            this.bothDirRadioButton = new System.Windows.Forms.RadioButton();
            this.inboundRadioButton = new System.Windows.Forms.RadioButton();
            this.outboundRadioButton = new System.Windows.Forms.RadioButton();
            this.pnlGetService = new System.Windows.Forms.Panel();
            this.serviceNameTextBox = new System.Windows.Forms.TextBox();
            this.serviceListBox = new System.Windows.Forms.ListBox();
            this.serviceInstructionLabel = new System.Windows.Forms.Label();
            this.pnlGetFileShareIP = new System.Windows.Forms.Panel();
            this.fileShareIpTextBox = new System.Windows.Forms.TextBox();
            this.fileShareWarningLabel = new System.Windows.Forms.Label();
            this.pnlGetBlockDeviceIP = new System.Windows.Forms.Panel();
            this.blockDeviceIpTextBox = new System.Windows.Forms.TextBox();
            this.pnlGetRestrictApp = new System.Windows.Forms.Panel();
            this.restrictAppPathTextBox = new System.Windows.Forms.TextBox();
            this.restrictAppBrowseButton = new System.Windows.Forms.Button();
            this.pnlSelection.SuspendLayout();
            this.pnlGetProgram.SuspendLayout();
            this.pnlGetPorts.SuspendLayout();
            this.pnlGetProtocol.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            this.pnlGetName.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.pnlGetAction.SuspendLayout();
            this.pnlGetDirection.SuspendLayout();
            this.pnlGetService.SuspendLayout();
            this.pnlGetFileShareIP.SuspendLayout();
            this.pnlGetBlockDeviceIP.SuspendLayout();
            this.pnlGetRestrictApp.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSelection
            // 
            this.pnlSelection.Controls.Add(this.restrictAppButton);
            this.pnlSelection.Controls.Add(this.blockDeviceButton);
            this.pnlSelection.Controls.Add(this.allowFileShareButton);
            this.pnlSelection.Controls.Add(this.blockServiceButton);
            this.pnlSelection.Controls.Add(this.advancedRuleButton);
            this.pnlSelection.Controls.Add(this.wildcardRuleButton);
            this.pnlSelection.Controls.Add(this.portRuleButton);
            this.pnlSelection.Controls.Add(this.programRuleButton);
            this.pnlSelection.Location = new System.Drawing.Point(0, 58);
            this.pnlSelection.Name = "pnlSelection";
            this.pnlSelection.Size = new System.Drawing.Size(534, 340);
            this.pnlSelection.TabIndex = 0;
            // 
            // restrictAppButton
            // 
            this.restrictAppButton.Location = new System.Drawing.Point(50, 246);
            this.restrictAppButton.Name = "restrictAppButton";
            this.restrictAppButton.Size = new System.Drawing.Size(434, 28);
            this.restrictAppButton.TabIndex = 7;
            this.restrictAppButton.Text = "Restrict an App to My Local Network Only";
            this.restrictAppButton.UseVisualStyleBackColor = true;
            this.restrictAppButton.Click += new System.EventHandler(this.restrictAppButton_Click);
            // 
            // blockDeviceButton
            // 
            this.blockDeviceButton.Location = new System.Drawing.Point(50, 208);
            this.blockDeviceButton.Name = "blockDeviceButton";
            this.blockDeviceButton.Size = new System.Drawing.Size(434, 28);
            this.blockDeviceButton.TabIndex = 6;
            this.blockDeviceButton.Text = "Block a Specific Device on My Network";
            this.blockDeviceButton.UseVisualStyleBackColor = true;
            this.blockDeviceButton.Click += new System.EventHandler(this.blockDeviceButton_Click);
            // 
            // allowFileShareButton
            // 
            this.allowFileShareButton.Location = new System.Drawing.Point(50, 170);
            this.allowFileShareButton.Name = "allowFileShareButton";
            this.allowFileShareButton.Size = new System.Drawing.Size(434, 28);
            this.allowFileShareButton.TabIndex = 5;
            this.allowFileShareButton.Text = "Allow Another PC to Access My Files";
            this.allowFileShareButton.UseVisualStyleBackColor = true;
            this.allowFileShareButton.Click += new System.EventHandler(this.allowFileShareButton_Click);
            // 
            // blockServiceButton
            // 
            this.blockServiceButton.Location = new System.Drawing.Point(50, 132);
            this.blockServiceButton.Name = "blockServiceButton";
            this.blockServiceButton.Size = new System.Drawing.Size(434, 28);
            this.blockServiceButton.TabIndex = 4;
            this.blockServiceButton.Text = "Block a Windows Service";
            this.blockServiceButton.UseVisualStyleBackColor = true;
            this.blockServiceButton.Click += new System.EventHandler(this.blockServiceButton_Click);
            // 
            // advancedRuleButton
            // 
            this.advancedRuleButton.Location = new System.Drawing.Point(50, 284);
            this.advancedRuleButton.Name = "advancedRuleButton";
            this.advancedRuleButton.Size = new System.Drawing.Size(434, 28);
            this.advancedRuleButton.TabIndex = 3;
            this.advancedRuleButton.Text = "Create a Custom Advanced Rule...";
            this.advancedRuleButton.UseVisualStyleBackColor = true;
            this.advancedRuleButton.Click += new System.EventHandler(this.advancedRuleButton_Click);
            // 
            // wildcardRuleButton
            // 
            this.wildcardRuleButton.Location = new System.Drawing.Point(50, 94);
            this.wildcardRuleButton.Name = "wildcardRuleButton";
            this.wildcardRuleButton.Size = new System.Drawing.Size(434, 28);
            this.wildcardRuleButton.TabIndex = 2;
            this.wildcardRuleButton.Text = "Create a Wildcard Rule for a Folder...";
            this.wildcardRuleButton.UseVisualStyleBackColor = true;
            this.wildcardRuleButton.Click += new System.EventHandler(this.wildcardRuleButton_Click);
            // 
            // portRuleButton
            // 
            this.portRuleButton.Location = new System.Drawing.Point(50, 56);
            this.portRuleButton.Name = "portRuleButton";
            this.portRuleButton.Size = new System.Drawing.Size(434, 28);
            this.portRuleButton.TabIndex = 1;
            this.portRuleButton.Text = "Open a Port";
            this.portRuleButton.UseVisualStyleBackColor = true;
            this.portRuleButton.Click += new System.EventHandler(this.portRuleButton_Click);
            // 
            // programRuleButton
            // 
            this.programRuleButton.Location = new System.Drawing.Point(50, 18);
            this.programRuleButton.Name = "programRuleButton";
            this.programRuleButton.Size = new System.Drawing.Size(434, 28);
            this.programRuleButton.TabIndex = 0;
            this.programRuleButton.Text = "Allow or Block a Program";
            this.programRuleButton.UseVisualStyleBackColor = true;
            this.programRuleButton.Click += new System.EventHandler(this.programRuleButton_Click);
            // 
            // pnlGetProgram
            // 
            this.pnlGetProgram.Controls.Add(this.browseButton);
            this.pnlGetProgram.Controls.Add(this.programPathTextBox);
            this.pnlGetProgram.Location = new System.Drawing.Point(0, 58);
            this.pnlGetProgram.Name = "pnlGetProgram";
            this.pnlGetProgram.Size = new System.Drawing.Size(534, 340);
            this.pnlGetProgram.TabIndex = 1;
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(422, 149);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(90, 23);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // programPathTextBox
            // 
            this.programPathTextBox.Location = new System.Drawing.Point(23, 149);
            this.programPathTextBox.Name = "programPathTextBox";
            this.programPathTextBox.Size = new System.Drawing.Size(393, 23);
            this.programPathTextBox.TabIndex = 0;
            this.programPathTextBox.PlaceholderText = "Path to application executable";
            // 
            // pnlGetPorts
            // 
            this.pnlGetPorts.Controls.Add(this.portsProgramPathTextBox);
            this.pnlGetPorts.Controls.Add(this.portsBrowseButton);
            this.pnlGetPorts.Controls.Add(this.restrictToProgramCheckBox);
            this.pnlGetPorts.Controls.Add(this.portsTextBox);
            this.pnlGetPorts.Controls.Add(this.portsLabel);
            this.pnlGetPorts.Location = new System.Drawing.Point(0, 58);
            this.pnlGetPorts.Name = "pnlGetPorts";
            this.pnlGetPorts.Size = new System.Drawing.Size(534, 340);
            this.pnlGetPorts.TabIndex = 2;
            // 
            // portsProgramPathTextBox
            // 
            this.portsProgramPathTextBox.Location = new System.Drawing.Point(62, 234);
            this.portsProgramPathTextBox.Name = "portsProgramPathTextBox";
            this.portsProgramPathTextBox.Size = new System.Drawing.Size(354, 23);
            this.portsProgramPathTextBox.TabIndex = 3;
            this.portsProgramPathTextBox.Visible = false;
            // 
            // portsBrowseButton
            // 
            this.portsBrowseButton.Location = new System.Drawing.Point(422, 234);
            this.portsBrowseButton.Name = "portsBrowseButton";
            this.portsBrowseButton.Size = new System.Drawing.Size(90, 23);
            this.portsBrowseButton.TabIndex = 4;
            this.portsBrowseButton.Text = "Browse...";
            this.portsBrowseButton.UseVisualStyleBackColor = true;
            this.portsBrowseButton.Visible = false;
            this.portsBrowseButton.Click += new System.EventHandler(this.portsBrowseButton_Click);
            // 
            // restrictToProgramCheckBox
            // 
            this.restrictToProgramCheckBox.AutoSize = true;
            this.restrictToProgramCheckBox.Location = new System.Drawing.Point(117, 209);
            this.restrictToProgramCheckBox.Name = "restrictToProgramCheckBox";
            this.restrictToProgramCheckBox.Size = new System.Drawing.Size(217, 19);
            this.restrictToProgramCheckBox.TabIndex = 2;
            this.restrictToProgramCheckBox.Text = "Also restrict this rule to a program?";
            this.restrictToProgramCheckBox.UseVisualStyleBackColor = true;
            this.restrictToProgramCheckBox.CheckedChanged += new System.EventHandler(this.restrictToProgramCheckBox_CheckedChanged);
            // 
            // portsTextBox
            // 
            this.portsTextBox.Location = new System.Drawing.Point(117, 122);
            this.portsTextBox.Name = "portsTextBox";
            this.portsTextBox.Size = new System.Drawing.Size(300, 23);
            this.portsTextBox.TabIndex = 0;
            // 
            // portsLabel
            // 
            this.portsLabel.AutoSize = true;
            this.portsLabel.Location = new System.Drawing.Point(117, 148);
            this.portsLabel.Name = "portsLabel";
            this.portsLabel.Size = new System.Drawing.Size(161, 15);
            this.portsLabel.TabIndex = 1;
            this.portsLabel.Text = "e.g., 80, 443 or 27015-27030";
            // 
            // pnlGetProtocol
            // 
            this.pnlGetProtocol.Controls.Add(this.bothProtocolRadioButton);
            this.pnlGetProtocol.Controls.Add(this.udpRadioButton);
            this.pnlGetProtocol.Controls.Add(this.tcpRadioButton);
            this.pnlGetProtocol.Location = new System.Drawing.Point(0, 58);
            this.pnlGetProtocol.Name = "pnlGetProtocol";
            this.pnlGetProtocol.Size = new System.Drawing.Size(534, 340);
            this.pnlGetProtocol.TabIndex = 3;
            // 
            // bothProtocolRadioButton
            // 
            this.bothProtocolRadioButton.AutoSize = true;
            this.bothProtocolRadioButton.Location = new System.Drawing.Point(230, 200);
            this.bothProtocolRadioButton.Name = "bothProtocolRadioButton";
            this.bothProtocolRadioButton.Size = new System.Drawing.Size(76, 19);
            this.bothProtocolRadioButton.TabIndex = 2;
            this.bothProtocolRadioButton.Text = "TCP & UDP";
            this.bothProtocolRadioButton.UseVisualStyleBackColor = true;
            // 
            // udpRadioButton
            // 
            this.udpRadioButton.AutoSize = true;
            this.udpRadioButton.Location = new System.Drawing.Point(230, 165);
            this.udpRadioButton.Name = "udpRadioButton";
            this.udpRadioButton.Size = new System.Drawing.Size(48, 19);
            this.udpRadioButton.TabIndex = 1;
            this.udpRadioButton.Text = "UDP";
            this.udpRadioButton.UseVisualStyleBackColor = true;
            // 
            // tcpRadioButton
            // 
            this.tcpRadioButton.AutoSize = true;
            this.tcpRadioButton.Checked = true;
            this.tcpRadioButton.Location = new System.Drawing.Point(230, 130);
            this.tcpRadioButton.Name = "tcpRadioButton";
            this.tcpRadioButton.Size = new System.Drawing.Size(44, 19);
            this.tcpRadioButton.TabIndex = 0;
            this.tcpRadioButton.TabStop = true;
            this.tcpRadioButton.Text = "TCP";
            this.tcpRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlSummary
            // 
            this.pnlSummary.Controls.Add(this.summaryLabel);
            this.pnlSummary.Location = new System.Drawing.Point(0, 58);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Size = new System.Drawing.Size(534, 340);
            this.pnlSummary.TabIndex = 4;
            // 
            // summaryLabel
            // 
            this.summaryLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.summaryLabel.Location = new System.Drawing.Point(23, 80);
            this.summaryLabel.Name = "summaryLabel";
            this.summaryLabel.Size = new System.Drawing.Size(489, 200);
            this.summaryLabel.TabIndex = 0;
            this.summaryLabel.Text = "Summary Text";
            // 
            // pnlGetName
            // 
            this.pnlGetName.Controls.Add(this.ruleNameTextBox);
            this.pnlGetName.Location = new System.Drawing.Point(0, 58);
            this.pnlGetName.Name = "pnlGetName";
            this.pnlGetName.Size = new System.Drawing.Size(534, 340);
            this.pnlGetName.TabIndex = 5;
            // 
            // ruleNameTextBox
            // 
            this.ruleNameTextBox.Location = new System.Drawing.Point(117, 149);
            this.ruleNameTextBox.Name = "ruleNameTextBox";
            this.ruleNameTextBox.Size = new System.Drawing.Size(300, 23);
            this.ruleNameTextBox.TabIndex = 0;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.cancelButton);
            this.bottomPanel.Controls.Add(this.nextButton);
            this.bottomPanel.Controls.Add(this.backButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 401);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(534, 60);
            this.bottomPanel.TabIndex = 6;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(422, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 36);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(316, 12);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(100, 36);
            this.nextButton.TabIndex = 1;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // backButton
            // 
            this.backButton.Location = new System.Drawing.Point(210, 12);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(100, 36);
            this.backButton.TabIndex = 0;
            this.backButton.Text = "< Back";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.mainHeaderLabel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(534, 55);
            this.topPanel.TabIndex = 7;
            // 
            // mainHeaderLabel
            // 
            this.mainHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.mainHeaderLabel.Location = new System.Drawing.Point(0, 0);
            this.mainHeaderLabel.Name = "mainHeaderLabel";
            this.mainHeaderLabel.Size = new System.Drawing.Size(534, 55);
            this.mainHeaderLabel.TabIndex = 0;
            this.mainHeaderLabel.Text = "Header Label";
            this.mainHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlGetAction
            // 
            this.pnlGetAction.Controls.Add(this.blockActionRadioButton);
            this.pnlGetAction.Controls.Add(this.allowActionRadioButton);
            this.pnlGetAction.Location = new System.Drawing.Point(0, 58);
            this.pnlGetAction.Name = "pnlGetAction";
            this.pnlGetAction.Size = new System.Drawing.Size(534, 340);
            this.pnlGetAction.TabIndex = 8;
            // 
            // blockActionRadioButton
            // 
            this.blockActionRadioButton.AutoSize = true;
            this.blockActionRadioButton.Location = new System.Drawing.Point(230, 182);
            this.blockActionRadioButton.Name = "blockActionRadioButton";
            this.blockActionRadioButton.Size = new System.Drawing.Size(53, 19);
            this.blockActionRadioButton.TabIndex = 1;
            this.blockActionRadioButton.Text = "Block";
            this.blockActionRadioButton.UseVisualStyleBackColor = true;
            // 
            // allowActionRadioButton
            // 
            this.allowActionRadioButton.AutoSize = true;
            this.allowActionRadioButton.Checked = true;
            this.allowActionRadioButton.Location = new System.Drawing.Point(230, 147);
            this.allowActionRadioButton.Name = "allowActionRadioButton";
            this.allowActionRadioButton.Size = new System.Drawing.Size(54, 19);
            this.allowActionRadioButton.TabIndex = 0;
            this.allowActionRadioButton.TabStop = true;
            this.allowActionRadioButton.Text = "Allow";
            this.allowActionRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlGetDirection
            // 
            this.pnlGetDirection.Controls.Add(this.bothDirRadioButton);
            this.pnlGetDirection.Controls.Add(this.inboundRadioButton);
            this.pnlGetDirection.Controls.Add(this.outboundRadioButton);
            this.pnlGetDirection.Location = new System.Drawing.Point(0, 58);
            this.pnlGetDirection.Name = "pnlGetDirection";
            this.pnlGetDirection.Size = new System.Drawing.Size(534, 340);
            this.pnlGetDirection.TabIndex = 9;
            // 
            // bothDirRadioButton
            // 
            this.bothDirRadioButton.AutoSize = true;
            this.bothDirRadioButton.Location = new System.Drawing.Point(230, 200);
            this.bothDirRadioButton.Name = "bothDirRadioButton";
            this.bothDirRadioButton.Size = new System.Drawing.Size(50, 19);
            this.bothDirRadioButton.TabIndex = 2;
            this.bothDirRadioButton.Text = "Both";
            this.bothDirRadioButton.UseVisualStyleBackColor = true;
            // 
            // inboundRadioButton
            // 
            this.inboundRadioButton.AutoSize = true;
            this.inboundRadioButton.Location = new System.Drawing.Point(230, 165);
            this.inboundRadioButton.Name = "inboundRadioButton";
            this.inboundRadioButton.Size = new System.Drawing.Size(70, 19);
            this.inboundRadioButton.TabIndex = 1;
            this.inboundRadioButton.Text = "Inbound";
            this.inboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // outboundRadioButton
            // 
            this.outboundRadioButton.AutoSize = true;
            this.outboundRadioButton.Checked = true;
            this.outboundRadioButton.Location = new System.Drawing.Point(230, 130);
            this.outboundRadioButton.Name = "outboundRadioButton";
            this.outboundRadioButton.Size = new System.Drawing.Size(78, 19);
            this.outboundRadioButton.TabIndex = 0;
            this.outboundRadioButton.TabStop = true;
            this.outboundRadioButton.Text = "Outbound";
            this.outboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlGetService
            // 
            this.pnlGetService.Controls.Add(this.serviceNameTextBox);
            this.pnlGetService.Controls.Add(this.serviceListBox);
            this.pnlGetService.Controls.Add(this.serviceInstructionLabel);
            this.pnlGetService.Location = new System.Drawing.Point(0, 58);
            this.pnlGetService.Name = "pnlGetService";
            this.pnlGetService.Size = new System.Drawing.Size(534, 340);
            this.pnlGetService.TabIndex = 10;
            // 
            // serviceNameTextBox
            // 
            this.serviceNameTextBox.Location = new System.Drawing.Point(23, 290);
            this.serviceNameTextBox.Name = "serviceNameTextBox";
            this.serviceNameTextBox.Size = new System.Drawing.Size(489, 23);
            this.serviceNameTextBox.TabIndex = 2;
            this.serviceNameTextBox.PlaceholderText = "Or enter service name (e.g. DiagTrack)";
            // 
            // serviceListBox
            // 
            this.serviceListBox.FormattingEnabled = true;
            this.serviceListBox.ItemHeight = 15;
            this.serviceListBox.Location = new System.Drawing.Point(23, 40);
            this.serviceListBox.Name = "serviceListBox";
            this.serviceListBox.Size = new System.Drawing.Size(489, 244);
            this.serviceListBox.TabIndex = 1;
            // 
            // serviceInstructionLabel
            // 
            this.serviceInstructionLabel.AutoSize = true;
            this.serviceInstructionLabel.Location = new System.Drawing.Point(23, 12);
            this.serviceInstructionLabel.Name = "serviceInstructionLabel";
            this.serviceInstructionLabel.Size = new System.Drawing.Size(306, 15);
            this.serviceInstructionLabel.TabIndex = 0;
            this.serviceInstructionLabel.Text = "Select a service from the list below, or enter its name.";
            // 
            // pnlGetFileShareIP
            // 
            this.pnlGetFileShareIP.Controls.Add(this.fileShareIpTextBox);
            this.pnlGetFileShareIP.Controls.Add(this.fileShareWarningLabel);
            this.pnlGetFileShareIP.Location = new System.Drawing.Point(0, 58);
            this.pnlGetFileShareIP.Name = "pnlGetFileShareIP";
            this.pnlGetFileShareIP.Size = new System.Drawing.Size(534, 340);
            this.pnlGetFileShareIP.TabIndex = 11;
            // 
            // fileShareIpTextBox
            // 
            this.fileShareIpTextBox.Location = new System.Drawing.Point(117, 180);
            this.fileShareIpTextBox.Name = "fileShareIpTextBox";
            this.fileShareIpTextBox.PlaceholderText = "e.g., 192.168.1.50";
            this.fileShareIpTextBox.Size = new System.Drawing.Size(300, 23);
            this.fileShareIpTextBox.TabIndex = 1;
            // 
            // fileShareWarningLabel
            // 
            this.fileShareWarningLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.fileShareWarningLabel.ForeColor = System.Drawing.Color.Red;
            this.fileShareWarningLabel.Location = new System.Drawing.Point(23, 60);
            this.fileShareWarningLabel.Name = "fileShareWarningLabel";
            this.fileShareWarningLabel.Size = new System.Drawing.Size(489, 84);
            this.fileShareWarningLabel.TabIndex = 0;
            this.fileShareWarningLabel.Text = "Warning: Opening port 445 for file sharing can be a security risk. Ensure you tr" +
    "ust the computer at the IP address you are about to enter and that your network" +
    " is secure.";
            this.fileShareWarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlGetBlockDeviceIP
            // 
            this.pnlGetBlockDeviceIP.Controls.Add(this.blockDeviceIpTextBox);
            this.pnlGetBlockDeviceIP.Location = new System.Drawing.Point(0, 58);
            this.pnlGetBlockDeviceIP.Name = "pnlGetBlockDeviceIP";
            this.pnlGetBlockDeviceIP.Size = new System.Drawing.Size(534, 340);
            this.pnlGetBlockDeviceIP.TabIndex = 12;
            // 
            // blockDeviceIpTextBox
            // 
            this.blockDeviceIpTextBox.Location = new System.Drawing.Point(117, 149);
            this.blockDeviceIpTextBox.Name = "blockDeviceIpTextBox";
            this.blockDeviceIpTextBox.PlaceholderText = "e.g., 192.168.1.101";
            this.blockDeviceIpTextBox.Size = new System.Drawing.Size(300, 23);
            this.blockDeviceIpTextBox.TabIndex = 0;
            // 
            // pnlGetRestrictApp
            // 
            this.pnlGetRestrictApp.Controls.Add(this.restrictAppPathTextBox);
            this.pnlGetRestrictApp.Controls.Add(this.restrictAppBrowseButton);
            this.pnlGetRestrictApp.Location = new System.Drawing.Point(0, 58);
            this.pnlGetRestrictApp.Name = "pnlGetRestrictApp";
            this.pnlGetRestrictApp.Size = new System.Drawing.Size(534, 340);
            this.pnlGetRestrictApp.TabIndex = 13;
            // 
            // restrictAppPathTextBox
            // 
            this.restrictAppPathTextBox.Location = new System.Drawing.Point(23, 149);
            this.restrictAppPathTextBox.Name = "restrictAppPathTextBox";
            this.restrictAppPathTextBox.PlaceholderText = "Path to application executable";
            this.restrictAppPathTextBox.Size = new System.Drawing.Size(393, 23);
            this.restrictAppPathTextBox.TabIndex = 1;
            // 
            // restrictAppBrowseButton
            // 
            this.restrictAppBrowseButton.Location = new System.Drawing.Point(422, 149);
            this.restrictAppBrowseButton.Name = "restrictAppBrowseButton";
            this.restrictAppBrowseButton.Size = new System.Drawing.Size(90, 23);
            this.restrictAppBrowseButton.TabIndex = 2;
            this.restrictAppBrowseButton.Text = "Browse...";
            this.restrictAppBrowseButton.UseVisualStyleBackColor = true;
            this.restrictAppBrowseButton.Click += new System.EventHandler(this.restrictAppBrowseButton_Click);
            // 
            // RuleWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(534, 461);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.pnlSelection);
            this.Controls.Add(this.pnlGetRestrictApp);
            this.Controls.Add(this.pnlGetBlockDeviceIP);
            this.Controls.Add(this.pnlGetFileShareIP);
            this.Controls.Add(this.pnlGetService);
            this.Controls.Add(this.pnlGetName);
            this.Controls.Add(this.pnlSummary);
            this.Controls.Add(this.pnlGetProtocol);
            this.Controls.Add(this.pnlGetPorts);
            this.Controls.Add(this.pnlGetProgram);
            this.Controls.Add(this.pnlGetAction);
            this.Controls.Add(this.pnlGetDirection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuleWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Rule";
            this.pnlSelection.ResumeLayout(false);
            this.pnlGetProgram.ResumeLayout(false);
            this.pnlGetProgram.PerformLayout();
            this.pnlGetPorts.ResumeLayout(false);
            this.pnlGetPorts.PerformLayout();
            this.pnlGetProtocol.ResumeLayout(false);
            this.pnlGetProtocol.PerformLayout();
            this.pnlSummary.ResumeLayout(false);
            this.pnlGetName.ResumeLayout(false);
            this.pnlGetName.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.pnlGetAction.ResumeLayout(false);
            this.pnlGetAction.PerformLayout();
            this.pnlGetDirection.ResumeLayout(false);
            this.pnlGetDirection.PerformLayout();
            this.pnlGetService.ResumeLayout(false);
            this.pnlGetService.PerformLayout();
            this.pnlGetFileShareIP.ResumeLayout(false);
            this.pnlGetFileShareIP.PerformLayout();
            this.pnlGetBlockDeviceIP.ResumeLayout(false);
            this.pnlGetBlockDeviceIP.PerformLayout();
            this.pnlGetRestrictApp.ResumeLayout(false);
            this.pnlGetRestrictApp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSelection;
        private System.Windows.Forms.Button advancedRuleButton;
        private System.Windows.Forms.Button wildcardRuleButton;
        private System.Windows.Forms.Button portRuleButton;
        private System.Windows.Forms.Button programRuleButton;
        private System.Windows.Forms.Panel pnlGetProgram;
        private System.Windows.Forms.Panel pnlGetPorts;
        private System.Windows.Forms.Panel pnlGetProtocol;
        private System.Windows.Forms.Panel pnlSummary;
        private System.Windows.Forms.Panel pnlGetName;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label mainHeaderLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox programPathTextBox;
        private System.Windows.Forms.TextBox portsTextBox;
        private System.Windows.Forms.Label portsLabel;
        private System.Windows.Forms.RadioButton bothProtocolRadioButton;
        private System.Windows.Forms.RadioButton udpRadioButton;
        private System.Windows.Forms.RadioButton tcpRadioButton;
        private System.Windows.Forms.TextBox ruleNameTextBox;
        private System.Windows.Forms.Label summaryLabel;
        private System.Windows.Forms.Panel pnlGetAction;
        private System.Windows.Forms.RadioButton blockActionRadioButton;
        private System.Windows.Forms.RadioButton allowActionRadioButton;
        private System.Windows.Forms.Panel pnlGetDirection;
        private System.Windows.Forms.RadioButton bothDirRadioButton;
        private System.Windows.Forms.RadioButton inboundRadioButton;
        private System.Windows.Forms.RadioButton outboundRadioButton;
        private System.Windows.Forms.CheckBox restrictToProgramCheckBox;
        private System.Windows.Forms.Button portsBrowseButton;
        private System.Windows.Forms.TextBox portsProgramPathTextBox;
        private Button blockServiceButton;
        private Button allowFileShareButton;
        private Button blockDeviceButton;
        private Button restrictAppButton;
        private Panel pnlGetService;
        private Panel pnlGetFileShareIP;
        private Panel pnlGetBlockDeviceIP;
        private Panel pnlGetRestrictApp;
        private TextBox serviceNameTextBox;
        private ListBox serviceListBox;
        private Label serviceInstructionLabel;
        private TextBox fileShareIpTextBox;
        private Label fileShareWarningLabel;
        private TextBox blockDeviceIpTextBox;
        private TextBox restrictAppPathTextBox;
        private Button restrictAppBrowseButton;
    }
}