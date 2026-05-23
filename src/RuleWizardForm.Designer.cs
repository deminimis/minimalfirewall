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
            pnlSelection = new Panel();
            restrictAppButton = new Button();
            blockDeviceButton = new Button();
            allowFileShareButton = new Button();
            blockServiceButton = new Button();
            advancedRuleButton = new Button();
            wildcardRuleButton = new Button();
            portRuleButton = new Button();
            batchProgramRuleButton = new Button();
            programRuleButton = new Button();
            pnlGetProgram = new Panel();
            browseButton = new Button();
            programPathTextBox = new TextBox();
            pnlGetFolder = new Panel();
            dllCheckBox = new CheckBox();
            exeCheckBox = new CheckBox();
            batchBrowseFolderButton = new Button();
            batchFolderPathTextBox = new TextBox();
            pnlGetPorts = new Panel();
            portsProgramPathTextBox = new TextBox();
            portsBrowseButton = new Button();
            restrictToProgramCheckBox = new CheckBox();
            portsTextBox = new TextBox();
            portsLabel = new Label();
            pnlGetProtocol = new Panel();
            bothProtocolRadioButton = new RadioButton();
            udpRadioButton = new RadioButton();
            tcpRadioButton = new RadioButton();
            pnlSummary = new Panel();
            summaryLabel = new Label();
            pnlGetName = new Panel();
            ruleNameTextBox = new TextBox();
            bottomPanel = new Panel();
            cancelButton = new Button();
            nextButton = new Button();
            backButton = new Button();
            topPanel = new Panel();
            mainHeaderLabel = new Label();
            pnlGetAction = new Panel();
            blockActionRadioButton = new RadioButton();
            allowActionRadioButton = new RadioButton();
            pnlGetDirection = new Panel();
            bothDirRadioButton = new RadioButton();
            inboundRadioButton = new RadioButton();
            outboundRadioButton = new RadioButton();
            pnlGetService = new Panel();
            serviceNameTextBox = new TextBox();
            serviceListBox = new ListBox();
            serviceInstructionLabel = new Label();
            pnlGetFileShareIP = new Panel();
            fileShareIpTextBox = new TextBox();
            fileShareWarningLabel = new Label();
            pnlGetBlockDeviceIP = new Panel();
            blockDeviceIpTextBox = new TextBox();
            pnlGetRestrictApp = new Panel();
            restrictAppPathTextBox = new TextBox();
            restrictAppBrowseButton = new Button();
            pnlSelection.SuspendLayout();
            pnlGetProgram.SuspendLayout();
            pnlGetFolder.SuspendLayout();
            pnlGetPorts.SuspendLayout();
            pnlGetProtocol.SuspendLayout();
            pnlSummary.SuspendLayout();
            pnlGetName.SuspendLayout();
            bottomPanel.SuspendLayout();
            topPanel.SuspendLayout();
            pnlGetAction.SuspendLayout();
            pnlGetDirection.SuspendLayout();
            pnlGetService.SuspendLayout();
            pnlGetFileShareIP.SuspendLayout();
            pnlGetBlockDeviceIP.SuspendLayout();
            pnlGetRestrictApp.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSelection
            // 
            pnlSelection.Controls.Add(restrictAppButton);
            pnlSelection.Controls.Add(blockDeviceButton);
            pnlSelection.Controls.Add(allowFileShareButton);
            pnlSelection.Controls.Add(blockServiceButton);
            pnlSelection.Controls.Add(advancedRuleButton);
            pnlSelection.Controls.Add(wildcardRuleButton);
            pnlSelection.Controls.Add(portRuleButton);
            pnlSelection.Controls.Add(batchProgramRuleButton);
            pnlSelection.Controls.Add(programRuleButton);
            pnlSelection.Location = new Point(0, 62);
            pnlSelection.Name = "pnlSelection";
            pnlSelection.Size = new Size(534, 363);
            pnlSelection.TabIndex = 0;
            // 
            // restrictAppButton
            // 
            restrictAppButton.Location = new Point(50, 265);
            restrictAppButton.Name = "restrictAppButton";
            restrictAppButton.Size = new Size(434, 30);
            restrictAppButton.TabIndex = 7;
            restrictAppButton.Text = "Restrict an App to My Local Network Only";
            restrictAppButton.UseVisualStyleBackColor = true;
            restrictAppButton.Click += restrictAppButton_Click;
            // 
            // blockDeviceButton
            // 
            blockDeviceButton.Location = new Point(50, 228);
            blockDeviceButton.Name = "blockDeviceButton";
            blockDeviceButton.Size = new Size(434, 30);
            blockDeviceButton.TabIndex = 6;
            blockDeviceButton.Text = "Block a Specific Device on My Network";
            blockDeviceButton.UseVisualStyleBackColor = true;
            blockDeviceButton.Click += blockDeviceButton_Click;
            // 
            // allowFileShareButton
            // 
            allowFileShareButton.Location = new Point(50, 192);
            allowFileShareButton.Name = "allowFileShareButton";
            allowFileShareButton.Size = new Size(434, 30);
            allowFileShareButton.TabIndex = 5;
            allowFileShareButton.Text = "Allow Another PC to Access My Files";
            allowFileShareButton.UseVisualStyleBackColor = true;
            allowFileShareButton.Click += allowFileShareButton_Click;
            // 
            // blockServiceButton
            // 
            blockServiceButton.Location = new Point(50, 156);
            blockServiceButton.Name = "blockServiceButton";
            blockServiceButton.Size = new Size(434, 30);
            blockServiceButton.TabIndex = 4;
            blockServiceButton.Text = "Block a Windows Service";
            blockServiceButton.UseVisualStyleBackColor = true;
            blockServiceButton.Click += blockServiceButton_Click;
            // 
            // advancedRuleButton
            // 
            advancedRuleButton.Location = new Point(50, 301);
            advancedRuleButton.Name = "advancedRuleButton";
            advancedRuleButton.Size = new Size(434, 30);
            advancedRuleButton.TabIndex = 3;
            advancedRuleButton.Text = "Create a Custom Advanced Rule...";
            advancedRuleButton.UseVisualStyleBackColor = true;
            advancedRuleButton.Click += advancedRuleButton_Click;
            // 
            // wildcardRuleButton
            // 
            wildcardRuleButton.Location = new Point(50, 119);
            wildcardRuleButton.Name = "wildcardRuleButton";
            wildcardRuleButton.Size = new Size(434, 30);
            wildcardRuleButton.TabIndex = 2;
            wildcardRuleButton.Text = "Create a Wildcard Rule for a Folder...";
            wildcardRuleButton.UseVisualStyleBackColor = true;
            wildcardRuleButton.Click += wildcardRuleButton_Click;
            // 
            // portRuleButton
            // 
            portRuleButton.Location = new Point(50, 83);
            portRuleButton.Name = "portRuleButton";
            portRuleButton.Size = new Size(434, 30);
            portRuleButton.TabIndex = 1;
            portRuleButton.Text = "Open a Port";
            portRuleButton.UseVisualStyleBackColor = true;
            portRuleButton.Click += portRuleButton_Click;
            // 
            // batchProgramRuleButton
            // 
            batchProgramRuleButton.Location = new Point(50, 47);
            batchProgramRuleButton.Name = "batchProgramRuleButton";
            batchProgramRuleButton.Size = new Size(434, 30);
            batchProgramRuleButton.TabIndex = 8;
            batchProgramRuleButton.Text = "Allow or Block All Programs in a Folder";
            batchProgramRuleButton.UseVisualStyleBackColor = true;
            batchProgramRuleButton.Click += batchProgramRuleButton_Click;
            // 
            // programRuleButton
            // 
            programRuleButton.Location = new Point(50, 11);
            programRuleButton.Name = "programRuleButton";
            programRuleButton.Size = new Size(434, 30);
            programRuleButton.TabIndex = 0;
            programRuleButton.Text = "Allow or Block a Program";
            programRuleButton.UseVisualStyleBackColor = true;
            programRuleButton.Click += programRuleButton_Click;
            // 
            // pnlGetProgram
            // 
            pnlGetProgram.Controls.Add(browseButton);
            pnlGetProgram.Controls.Add(programPathTextBox);
            pnlGetProgram.Location = new Point(0, 62);
            pnlGetProgram.Name = "pnlGetProgram";
            pnlGetProgram.Size = new Size(534, 363);
            pnlGetProgram.TabIndex = 1;
            // 
            // browseButton
            // 
            browseButton.Location = new Point(422, 159);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(90, 25);
            browseButton.TabIndex = 1;
            browseButton.Text = "Browse...";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // programPathTextBox
            // 
            programPathTextBox.Location = new Point(23, 159);
            programPathTextBox.Name = "programPathTextBox";
            programPathTextBox.PlaceholderText = "Path to application executable";
            programPathTextBox.Size = new Size(393, 23);
            programPathTextBox.TabIndex = 0;
            // 
            // pnlGetFolder
            // 
            pnlGetFolder.Controls.Add(dllCheckBox);
            pnlGetFolder.Controls.Add(exeCheckBox);
            pnlGetFolder.Controls.Add(batchBrowseFolderButton);
            pnlGetFolder.Controls.Add(batchFolderPathTextBox);
            pnlGetFolder.Location = new Point(0, 62);
            pnlGetFolder.Name = "pnlGetFolder";
            pnlGetFolder.Size = new Size(534, 363);
            pnlGetFolder.TabIndex = 14;
            // 
            // dllCheckBox
            // 
            dllCheckBox.AutoSize = true;
            dllCheckBox.Location = new Point(112, 190);
            dllCheckBox.Name = "dllCheckBox";
            dllCheckBox.Size = new Size(96, 20);
            dllCheckBox.TabIndex = 3;
            dllCheckBox.Text = ".dll files";
            dllCheckBox.UseVisualStyleBackColor = true;
            // 
            // exeCheckBox
            // 
            exeCheckBox.AutoSize = true;
            exeCheckBox.Checked = true;
            exeCheckBox.CheckState = CheckState.Checked;
            exeCheckBox.Location = new Point(23, 190);
            exeCheckBox.Name = "exeCheckBox";
            exeCheckBox.Size = new Size(96, 20);
            exeCheckBox.TabIndex = 2;
            exeCheckBox.Text = ".exe files";
            exeCheckBox.UseVisualStyleBackColor = true;
            // 
            // batchBrowseFolderButton
            // 
            batchBrowseFolderButton.Location = new Point(422, 159);
            batchBrowseFolderButton.Name = "batchBrowseFolderButton";
            batchBrowseFolderButton.Size = new Size(90, 25);
            batchBrowseFolderButton.TabIndex = 1;
            batchBrowseFolderButton.Text = "Browse...";
            batchBrowseFolderButton.UseVisualStyleBackColor = true;
            batchBrowseFolderButton.Click += batchBrowseFolderButton_Click;
            // 
            // batchFolderPathTextBox
            // 
            batchFolderPathTextBox.Location = new Point(23, 159);
            batchFolderPathTextBox.Name = "batchFolderPathTextBox";
            batchFolderPathTextBox.PlaceholderText = "Path to folder";
            batchFolderPathTextBox.Size = new Size(393, 23);
            batchFolderPathTextBox.TabIndex = 0;
            // 
            // pnlGetPorts
            // 
            pnlGetPorts.Controls.Add(portsProgramPathTextBox);
            pnlGetPorts.Controls.Add(portsBrowseButton);
            pnlGetPorts.Controls.Add(restrictToProgramCheckBox);
            pnlGetPorts.Controls.Add(portsTextBox);
            pnlGetPorts.Controls.Add(portsLabel);
            pnlGetPorts.Location = new Point(0, 62);
            pnlGetPorts.Name = "pnlGetPorts";
            pnlGetPorts.Size = new Size(534, 363);
            pnlGetPorts.TabIndex = 2;
            // 
            // portsProgramPathTextBox
            // 
            portsProgramPathTextBox.Location = new Point(62, 250);
            portsProgramPathTextBox.Name = "portsProgramPathTextBox";
            portsProgramPathTextBox.Size = new Size(354, 23);
            portsProgramPathTextBox.TabIndex = 3;
            portsProgramPathTextBox.Visible = false;
            // 
            // portsBrowseButton
            // 
            portsBrowseButton.Location = new Point(422, 250);
            portsBrowseButton.Name = "portsBrowseButton";
            portsBrowseButton.Size = new Size(90, 25);
            portsBrowseButton.TabIndex = 4;
            portsBrowseButton.Text = "Browse...";
            portsBrowseButton.UseVisualStyleBackColor = true;
            portsBrowseButton.Visible = false;
            portsBrowseButton.Click += portsBrowseButton_Click;
            // 
            // restrictToProgramCheckBox
            // 
            restrictToProgramCheckBox.AutoSize = true;
            restrictToProgramCheckBox.Location = new Point(117, 223);
            restrictToProgramCheckBox.Name = "restrictToProgramCheckBox";
            restrictToProgramCheckBox.Size = new Size(194, 20);
            restrictToProgramCheckBox.TabIndex = 2;
            restrictToProgramCheckBox.Text = "Restrict rule to program";
            restrictToProgramCheckBox.UseVisualStyleBackColor = true;
            restrictToProgramCheckBox.CheckedChanged += restrictToProgramCheckBox_CheckedChanged;
            // 
            // portsTextBox
            // 
            portsTextBox.Location = new Point(117, 130);
            portsTextBox.Name = "portsTextBox";
            portsTextBox.Size = new Size(300, 23);
            portsTextBox.TabIndex = 0;
            // 
            // portsLabel
            // 
            portsLabel.AutoSize = true;
            portsLabel.Location = new Point(117, 158);
            portsLabel.Name = "portsLabel";
            portsLabel.Size = new Size(203, 16);
            portsLabel.TabIndex = 1;
            portsLabel.Text = "e.g., 80, 443 or 27015-27030";
            // 
            // pnlGetProtocol
            // 
            pnlGetProtocol.Controls.Add(bothProtocolRadioButton);
            pnlGetProtocol.Controls.Add(udpRadioButton);
            pnlGetProtocol.Controls.Add(tcpRadioButton);
            pnlGetProtocol.Location = new Point(0, 62);
            pnlGetProtocol.Name = "pnlGetProtocol";
            pnlGetProtocol.Size = new Size(534, 363);
            pnlGetProtocol.TabIndex = 3;
            // 
            // bothProtocolRadioButton
            // 
            bothProtocolRadioButton.AutoSize = true;
            bothProtocolRadioButton.Location = new Point(230, 213);
            bothProtocolRadioButton.Name = "bothProtocolRadioButton";
            bothProtocolRadioButton.Size = new Size(81, 20);
            bothProtocolRadioButton.TabIndex = 2;
            bothProtocolRadioButton.Text = "TCP & UDP";
            bothProtocolRadioButton.UseVisualStyleBackColor = true;
            // 
            // udpRadioButton
            // 
            udpRadioButton.AutoSize = true;
            udpRadioButton.Location = new Point(230, 176);
            udpRadioButton.Name = "udpRadioButton";
            udpRadioButton.Size = new Size(46, 20);
            udpRadioButton.TabIndex = 1;
            udpRadioButton.Text = "UDP";
            udpRadioButton.UseVisualStyleBackColor = true;
            // 
            // tcpRadioButton
            // 
            tcpRadioButton.AutoSize = true;
            tcpRadioButton.Checked = true;
            tcpRadioButton.Location = new Point(230, 139);
            tcpRadioButton.Name = "tcpRadioButton";
            tcpRadioButton.Size = new Size(46, 20);
            tcpRadioButton.TabIndex = 0;
            tcpRadioButton.TabStop = true;
            tcpRadioButton.Text = "TCP";
            tcpRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlSummary
            // 
            pnlSummary.Controls.Add(summaryLabel);
            pnlSummary.Location = new Point(0, 62);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Size = new Size(534, 363);
            pnlSummary.TabIndex = 4;
            // 
            // summaryLabel
            // 
            summaryLabel.Font = new Font("Segoe UI", 10F);
            summaryLabel.Location = new Point(23, 85);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(489, 213);
            summaryLabel.TabIndex = 0;
            summaryLabel.Text = "Summary Text";
            // 
            // pnlGetName
            // 
            pnlGetName.Controls.Add(ruleNameTextBox);
            pnlGetName.Location = new Point(0, 62);
            pnlGetName.Name = "pnlGetName";
            pnlGetName.Size = new Size(534, 363);
            pnlGetName.TabIndex = 5;
            // 
            // ruleNameTextBox
            // 
            ruleNameTextBox.Location = new Point(117, 159);
            ruleNameTextBox.Name = "ruleNameTextBox";
            ruleNameTextBox.Size = new Size(300, 23);
            ruleNameTextBox.TabIndex = 0;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(cancelButton);
            bottomPanel.Controls.Add(nextButton);
            bottomPanel.Controls.Add(backButton);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 428);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(534, 64);
            bottomPanel.TabIndex = 6;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(422, 13);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(100, 38);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // nextButton
            // 
            nextButton.Location = new Point(316, 13);
            nextButton.Name = "nextButton";
            nextButton.Size = new Size(100, 38);
            nextButton.TabIndex = 1;
            nextButton.Text = "Next";
            nextButton.UseVisualStyleBackColor = true;
            nextButton.Click += nextButton_Click;
            // 
            // backButton
            // 
            backButton.Location = new Point(210, 13);
            backButton.Name = "backButton";
            backButton.Size = new Size(100, 38);
            backButton.TabIndex = 0;
            backButton.Text = "< Back";
            backButton.UseVisualStyleBackColor = true;
            backButton.Click += backButton_Click;
            // 
            // topPanel
            // 
            topPanel.Controls.Add(mainHeaderLabel);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(534, 59);
            topPanel.TabIndex = 7;
            // 
            // mainHeaderLabel
            // 
            mainHeaderLabel.Dock = DockStyle.Fill;
            mainHeaderLabel.Font = new Font("Segoe UI", 12F);
            mainHeaderLabel.Location = new Point(0, 0);
            mainHeaderLabel.Name = "mainHeaderLabel";
            mainHeaderLabel.Size = new Size(534, 59);
            mainHeaderLabel.TabIndex = 0;
            mainHeaderLabel.Text = "Header Label";
            mainHeaderLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlGetAction
            // 
            pnlGetAction.Controls.Add(blockActionRadioButton);
            pnlGetAction.Controls.Add(allowActionRadioButton);
            pnlGetAction.Location = new Point(0, 62);
            pnlGetAction.Name = "pnlGetAction";
            pnlGetAction.Size = new Size(534, 363);
            pnlGetAction.TabIndex = 8;
            // 
            // blockActionRadioButton
            // 
            blockActionRadioButton.AutoSize = true;
            blockActionRadioButton.Location = new Point(230, 194);
            blockActionRadioButton.Name = "blockActionRadioButton";
            blockActionRadioButton.Size = new Size(60, 20);
            blockActionRadioButton.TabIndex = 1;
            blockActionRadioButton.Text = "Block";
            blockActionRadioButton.UseVisualStyleBackColor = true;
            // 
            // allowActionRadioButton
            // 
            allowActionRadioButton.AutoSize = true;
            allowActionRadioButton.Checked = true;
            allowActionRadioButton.Location = new Point(230, 157);
            allowActionRadioButton.Name = "allowActionRadioButton";
            allowActionRadioButton.Size = new Size(60, 20);
            allowActionRadioButton.TabIndex = 0;
            allowActionRadioButton.TabStop = true;
            allowActionRadioButton.Text = "Allow";
            allowActionRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlGetDirection
            // 
            pnlGetDirection.Controls.Add(bothDirRadioButton);
            pnlGetDirection.Controls.Add(inboundRadioButton);
            pnlGetDirection.Controls.Add(outboundRadioButton);
            pnlGetDirection.Location = new Point(0, 62);
            pnlGetDirection.Name = "pnlGetDirection";
            pnlGetDirection.Size = new Size(534, 363);
            pnlGetDirection.TabIndex = 9;
            // 
            // bothDirRadioButton
            // 
            bothDirRadioButton.AutoSize = true;
            bothDirRadioButton.Location = new Point(230, 213);
            bothDirRadioButton.Name = "bothDirRadioButton";
            bothDirRadioButton.Size = new Size(53, 20);
            bothDirRadioButton.TabIndex = 2;
            bothDirRadioButton.Text = "Both";
            bothDirRadioButton.UseVisualStyleBackColor = true;
            // 
            // inboundRadioButton
            // 
            inboundRadioButton.AutoSize = true;
            inboundRadioButton.Location = new Point(230, 176);
            inboundRadioButton.Name = "inboundRadioButton";
            inboundRadioButton.Size = new Size(74, 20);
            inboundRadioButton.TabIndex = 1;
            inboundRadioButton.Text = "Inbound";
            inboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // outboundRadioButton
            // 
            outboundRadioButton.AutoSize = true;
            outboundRadioButton.Checked = true;
            outboundRadioButton.Location = new Point(230, 139);
            outboundRadioButton.Name = "outboundRadioButton";
            outboundRadioButton.Size = new Size(81, 20);
            outboundRadioButton.TabIndex = 0;
            outboundRadioButton.TabStop = true;
            outboundRadioButton.Text = "Outbound";
            outboundRadioButton.UseVisualStyleBackColor = true;
            // 
            // pnlGetService
            // 
            pnlGetService.Controls.Add(serviceNameTextBox);
            pnlGetService.Controls.Add(serviceListBox);
            pnlGetService.Controls.Add(serviceInstructionLabel);
            pnlGetService.Location = new Point(0, 62);
            pnlGetService.Name = "pnlGetService";
            pnlGetService.Size = new Size(534, 363);
            pnlGetService.TabIndex = 10;
            // 
            // serviceNameTextBox
            // 
            serviceNameTextBox.Location = new Point(23, 309);
            serviceNameTextBox.Name = "serviceNameTextBox";
            serviceNameTextBox.PlaceholderText = "Or enter service name (e.g. DiagTrack)";
            serviceNameTextBox.Size = new Size(489, 23);
            serviceNameTextBox.TabIndex = 2;
            // 
            // serviceListBox
            // 
            serviceListBox.FormattingEnabled = true;
            serviceListBox.Location = new Point(23, 43);
            serviceListBox.Name = "serviceListBox";
            serviceListBox.Size = new Size(489, 260);
            serviceListBox.TabIndex = 1;
            // 
            // serviceInstructionLabel
            // 
            serviceInstructionLabel.AutoSize = true;
            serviceInstructionLabel.Location = new Point(23, 13);
            serviceInstructionLabel.Name = "serviceInstructionLabel";
            serviceInstructionLabel.Size = new Size(399, 16);
            serviceInstructionLabel.TabIndex = 0;
            serviceInstructionLabel.Text = "Select a service from the list below, or enter its name.";
            // 
            // pnlGetFileShareIP
            // 
            pnlGetFileShareIP.Controls.Add(fileShareIpTextBox);
            pnlGetFileShareIP.Controls.Add(fileShareWarningLabel);
            pnlGetFileShareIP.Location = new Point(0, 62);
            pnlGetFileShareIP.Name = "pnlGetFileShareIP";
            pnlGetFileShareIP.Size = new Size(534, 363);
            pnlGetFileShareIP.TabIndex = 11;
            // 
            // fileShareIpTextBox
            // 
            fileShareIpTextBox.Location = new Point(117, 192);
            fileShareIpTextBox.Name = "fileShareIpTextBox";
            fileShareIpTextBox.PlaceholderText = "e.g., 192.168.1.50";
            fileShareIpTextBox.Size = new Size(300, 23);
            fileShareIpTextBox.TabIndex = 1;
            // 
            // fileShareWarningLabel
            // 
            fileShareWarningLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            fileShareWarningLabel.ForeColor = Color.Red;
            fileShareWarningLabel.Location = new Point(23, 64);
            fileShareWarningLabel.Name = "fileShareWarningLabel";
            fileShareWarningLabel.Size = new Size(489, 90);
            fileShareWarningLabel.TabIndex = 0;
            fileShareWarningLabel.Text = "Warning: Opening port 445 for file sharing can be a security risk. Ensure you trust the computer at the IP address you are about to enter and that your network is secure.";
            fileShareWarningLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlGetBlockDeviceIP
            // 
            pnlGetBlockDeviceIP.Controls.Add(blockDeviceIpTextBox);
            pnlGetBlockDeviceIP.Location = new Point(0, 62);
            pnlGetBlockDeviceIP.Name = "pnlGetBlockDeviceIP";
            pnlGetBlockDeviceIP.Size = new Size(534, 363);
            pnlGetBlockDeviceIP.TabIndex = 12;
            // 
            // blockDeviceIpTextBox
            // 
            blockDeviceIpTextBox.Location = new Point(117, 159);
            blockDeviceIpTextBox.Name = "blockDeviceIpTextBox";
            blockDeviceIpTextBox.PlaceholderText = "e.g., 192.168.1.101";
            blockDeviceIpTextBox.Size = new Size(300, 23);
            blockDeviceIpTextBox.TabIndex = 0;
            // 
            // pnlGetRestrictApp
            // 
            pnlGetRestrictApp.Controls.Add(restrictAppPathTextBox);
            pnlGetRestrictApp.Controls.Add(restrictAppBrowseButton);
            pnlGetRestrictApp.Location = new Point(0, 62);
            pnlGetRestrictApp.Name = "pnlGetRestrictApp";
            pnlGetRestrictApp.Size = new Size(534, 363);
            pnlGetRestrictApp.TabIndex = 13;
            // 
            // restrictAppPathTextBox
            // 
            restrictAppPathTextBox.Location = new Point(23, 159);
            restrictAppPathTextBox.Name = "restrictAppPathTextBox";
            restrictAppPathTextBox.PlaceholderText = "Path to application executable";
            restrictAppPathTextBox.Size = new Size(393, 23);
            restrictAppPathTextBox.TabIndex = 1;
            // 
            // restrictAppBrowseButton
            // 
            restrictAppBrowseButton.Location = new Point(422, 159);
            restrictAppBrowseButton.Name = "restrictAppBrowseButton";
            restrictAppBrowseButton.Size = new Size(90, 25);
            restrictAppBrowseButton.TabIndex = 2;
            restrictAppBrowseButton.Text = "Browse...";
            restrictAppBrowseButton.UseVisualStyleBackColor = true;
            restrictAppBrowseButton.Click += restrictAppBrowseButton_Click;
            // 
            // RuleWizardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(534, 492);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
            Controls.Add(pnlGetFolder);
            Controls.Add(pnlSelection);
            Controls.Add(pnlGetRestrictApp);
            Controls.Add(pnlGetBlockDeviceIP);
            Controls.Add(pnlGetFileShareIP);
            Controls.Add(pnlGetService);
            Controls.Add(pnlGetName);
            Controls.Add(pnlSummary);
            Controls.Add(pnlGetProtocol);
            Controls.Add(pnlGetPorts);
            Controls.Add(pnlGetProgram);
            Controls.Add(pnlGetAction);
            Controls.Add(pnlGetDirection);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RuleWizardForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create New Rule";
            pnlSelection.ResumeLayout(false);
            pnlGetProgram.ResumeLayout(false);
            pnlGetProgram.PerformLayout();
            pnlGetFolder.ResumeLayout(false);
            pnlGetFolder.PerformLayout();
            pnlGetPorts.ResumeLayout(false);
            pnlGetPorts.PerformLayout();
            pnlGetProtocol.ResumeLayout(false);
            pnlGetProtocol.PerformLayout();
            pnlSummary.ResumeLayout(false);
            pnlGetName.ResumeLayout(false);
            pnlGetName.PerformLayout();
            bottomPanel.ResumeLayout(false);
            topPanel.ResumeLayout(false);
            pnlGetAction.ResumeLayout(false);
            pnlGetAction.PerformLayout();
            pnlGetDirection.ResumeLayout(false);
            pnlGetDirection.PerformLayout();
            pnlGetService.ResumeLayout(false);
            pnlGetService.PerformLayout();
            pnlGetFileShareIP.ResumeLayout(false);
            pnlGetFileShareIP.PerformLayout();
            pnlGetBlockDeviceIP.ResumeLayout(false);
            pnlGetBlockDeviceIP.PerformLayout();
            pnlGetRestrictApp.ResumeLayout(false);
            pnlGetRestrictApp.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSelection;
        private System.Windows.Forms.Button advancedRuleButton;
        private System.Windows.Forms.Button wildcardRuleButton;
        private System.Windows.Forms.Button portRuleButton;
        private System.Windows.Forms.Button programRuleButton;
        private System.Windows.Forms.Button batchProgramRuleButton;
        private System.Windows.Forms.Panel pnlGetProgram;
        private System.Windows.Forms.Panel pnlGetFolder;
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
        private System.Windows.Forms.Button batchBrowseFolderButton;
        private System.Windows.Forms.TextBox batchFolderPathTextBox;
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
        private CheckBox dllCheckBox;
        private CheckBox exeCheckBox;
    }
}

