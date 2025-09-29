namespace MinimalFirewall
{
    public partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private DarkModeForms.FlatTabControl mainTabControl;
        private System.Windows.Forms.TabPage dashboardTabPage;
        private System.Windows.Forms.TabPage rulesTabPage;
        private System.Windows.Forms.TabPage systemChangesTabPage;
        private System.Windows.Forms.TabPage settingsTabPage;
        private System.Windows.Forms.TabPage groupsTabPage;
        private System.Windows.Forms.TabPage liveConnectionsTabPage;
        private System.Windows.Forms.Button lockdownButton;
        private System.Windows.Forms.Button rescanButton;
        private System.Windows.Forms.ListView rulesListView;
        private System.Windows.Forms.ColumnHeader advIconColumn;
        private System.Windows.Forms.ColumnHeader advNameColumn;
        private System.Windows.Forms.ColumnHeader advStatusColumn;
        private System.Windows.Forms.ColumnHeader advProtocolColumn;
        private System.Windows.Forms.ColumnHeader advLocalPortsColumn;
        private System.Windows.Forms.ColumnHeader advRemotePortsColumn;
        private System.Windows.Forms.ColumnHeader advLocalAddressColumn;
        private System.Windows.Forms.ColumnHeader advRemoteAddressColumn;
        private System.Windows.Forms.ColumnHeader advProgramColumn;
        private System.Windows.Forms.ColumnHeader advServiceColumn;
        private System.Windows.Forms.ColumnHeader advProfilesColumn;
        private System.Windows.Forms.ColumnHeader advGroupingColumn;
        private System.Windows.Forms.ColumnHeader advDescColumn;
        private System.Windows.Forms.Button createRuleButton;
        private System.Windows.Forms.TextBox rulesSearchTextBox;
        private MinimalFirewall.ButtonListView systemChangesListView;
        private System.Windows.Forms.ListView groupsListView;
        private System.Windows.Forms.ColumnHeader groupNameColumn;
        private System.Windows.Forms.Button rebuildBaselineButton;
        private System.Windows.Forms.CheckBox advFilterProgramCheck;
        private System.Windows.Forms.CheckBox advFilterServiceCheck;
        private System.Windows.Forms.CheckBox advFilterUwpCheck;
        private System.Windows.Forms.CheckBox advFilterWildcardCheck;
        private System.Windows.Forms.CheckBox advFilterAdvancedCheck;
        private System.Windows.Forms.CheckBox closeToTraySwitch;
        private System.Windows.Forms.CheckBox startOnStartupSwitch;
        private System.Windows.Forms.CheckBox darkModeSwitch;
        private System.Windows.Forms.CheckBox popupsSwitch;
        private System.Windows.Forms.CheckBox loggingSwitch;
        private System.Windows.Forms.TextBox autoRefreshTextBox;
        private System.Windows.Forms.Button openFirewallButton;
        private System.Windows.Forms.Button checkForUpdatesButton;
        private System.Windows.Forms.LinkLabel helpLink;
        private System.Windows.Forms.LinkLabel reportProblemLink;
        private System.Windows.Forms.LinkLabel forumLink;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.ContextMenuStrip rulesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem allowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowOutboundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowInboundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockOutboundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockInboundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.ToolTip mainToolTip;
        private System.Windows.Forms.ImageList appImageList;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.PictureBox coffeePictureBox;
        private System.Windows.Forms.LinkLabel coffeeLinkLabel;
        private System.Windows.Forms.PictureBox arrowPictureBox;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Panel coffeePanel;
        private System.Windows.Forms.TextBox auditSearchTextBox;
        private System.Windows.Forms.ContextMenuStrip auditContextMenu;
        private System.Windows.Forms.ToolStripMenuItem acceptAllToolStripMenuItem;
        private System.Windows.Forms.Label autoRefreshLabel1;
        private System.Windows.Forms.Label autoRefreshLabel2;
        private System.Windows.Forms.Button advancedRuleButton;
        private System.Windows.Forms.ColumnHeader groupEnabledColumn;
        private System.Windows.Forms.ContextMenuStrip groupsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteGroupToolStripMenuItem;
        private System.Windows.Forms.CheckBox trafficMonitorSwitch;
        private System.Windows.Forms.ListView liveConnectionsListView;
        private System.Windows.Forms.ColumnHeader liveIconColumn;
        private System.Windows.Forms.ColumnHeader processNameColumn;
        private System.Windows.Forms.ColumnHeader localAddressColumn;
        private System.Windows.Forms.ColumnHeader localPortColumn;
        private System.Windows.Forms.ColumnHeader remoteAddressColumn;
        private System.Windows.Forms.ColumnHeader remotePortColumn;
        private System.Windows.Forms.ColumnHeader stateColumn;
        private System.Windows.Forms.ContextMenuStrip liveConnectionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem killProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockRemoteIPToolStripMenuItem;
        private System.Windows.Forms.ImageList appIconList;
        private System.Windows.Forms.CheckBox showAppIconsSwitch;
        private System.Windows.Forms.CheckBox autoAllowSystemTrustedCheck;
        private System.Windows.Forms.Button managePublishersButton;
        private ColumnHeader changeActionColumn;
        private ColumnHeader changeNameColumn;
        private ColumnHeader changeAction2Column;
        private ColumnHeader changeDirectionColumn;
        private ColumnHeader changeProtocolColumn;
        private ColumnHeader changeAppColumn;
        private ColumnHeader changeRemoteAddressColumn;
        private ColumnHeader changeDescColumn;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem createAdvancedRuleToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem copyDetailsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem createAdvancedRuleToolStripMenuItem1;
        private ToolStripMenuItem copyRemoteAddressToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem copyDetailsToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem openFileLocationToolStripMenuItem1;
        private ToolStripMenuItem copyDetailsToolStripMenuItem2;
        private ToolStripMenuItem copyDetailsToolStripMenuItem3;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem openFileLocationToolStripMenuItem2;
        private CheckBox auditAlertsSwitch;
        private System.Windows.Forms.Label liveConnectionsDisabledLabel;
        private DashboardControl dashboardControl1;
        private System.Windows.Forms.Button deleteAllRulesButton;
        private System.Windows.Forms.Button revertFirewallButton;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoRefreshTimer?.Dispose();
                _backgroundTaskService?.Dispose();
                _lockedGreenIcon?.Dispose();
                _unlockedWhiteIcon?.Dispose();
                _refreshWhiteIcon?.Dispose();
                _firewallSentryService?.Dispose();
                _eventListenerService?.Dispose();
                _defaultTrayIcon?.Dispose();
                _unlockedTrayIcon?.Dispose();
                _alertTrayIcon?.Dispose();
                dm?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainTabControl = new DarkModeForms.FlatTabControl();
            dashboardTabPage = new TabPage();
            dashboardControl1 = new DashboardControl();
            logoPictureBox = new PictureBox();
            arrowPictureBox = new PictureBox();
            instructionLabel = new Label();
            rulesTabPage = new TabPage();
            advFilterAdvancedCheck = new CheckBox();
            advancedRuleButton = new Button();
            advFilterWildcardCheck = new CheckBox();
            advFilterUwpCheck = new CheckBox();
            advFilterServiceCheck = new CheckBox();
            advFilterProgramCheck = new CheckBox();
            rulesSearchTextBox = new TextBox();
            createRuleButton = new Button();
            rulesListView = new ListView();
            advIconColumn = new ColumnHeader();
            advNameColumn = new ColumnHeader();
            advStatusColumn = new ColumnHeader();
            advProtocolColumn = new ColumnHeader();
            advLocalPortsColumn = new ColumnHeader();
            advRemotePortsColumn = new ColumnHeader();
            advLocalAddressColumn = new ColumnHeader();
            advRemoteAddressColumn = new ColumnHeader();
            advProgramColumn = new ColumnHeader();
            advServiceColumn = new ColumnHeader();
            advProfilesColumn = new ColumnHeader();
            advGroupingColumn = new ColumnHeader();
            advDescColumn = new ColumnHeader();
            rulesContextMenu = new ContextMenuStrip(components);
            allowToolStripMenuItem = new ToolStripMenuItem();
            allowOutboundToolStripMenuItem = new ToolStripMenuItem();
            allowInboundToolStripMenuItem = new ToolStripMenuItem();
            allowAllToolStripMenuItem = new ToolStripMenuItem();
            blockToolStripMenuItem = new ToolStripMenuItem();
            blockOutboundToolStripMenuItem = new ToolStripMenuItem();
            blockInboundToolStripMenuItem = new ToolStripMenuItem();
            blockAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            deleteRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem = new ToolStripMenuItem();
            copyDetailsToolStripMenuItem2 = new ToolStripMenuItem();
            systemChangesTabPage = new TabPage();
            auditSearchTextBox = new TextBox();
            rebuildBaselineButton = new Button();
            systemChangesListView = new MinimalFirewall.ButtonListView();
            changeActionColumn = new ColumnHeader();
            changeNameColumn = new ColumnHeader();
            changeAction2Column = new ColumnHeader();
            changeDirectionColumn = new ColumnHeader();
            changeProtocolColumn = new ColumnHeader();
            changeAppColumn = new ColumnHeader();
            changeRemoteAddressColumn = new ColumnHeader();
            changeDescColumn = new ColumnHeader();
            auditContextMenu = new ContextMenuStrip(components);
            acceptAllToolStripMenuItem = new ToolStripMenuItem();
            copyDetailsToolStripMenuItem3 = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem2 = new ToolStripMenuItem();
            groupsTabPage = new TabPage();
            groupsListView = new ListView();
            groupNameColumn = new ColumnHeader();
            groupEnabledColumn = new ColumnHeader();
            groupsContextMenu = new ContextMenuStrip(components);
            deleteGroupToolStripMenuItem = new ToolStripMenuItem();
            liveConnectionsTabPage = new TabPage();
            liveConnectionsDisabledLabel = new Label();
            liveConnectionsListView = new ListView();
            liveIconColumn = new ColumnHeader();
            processNameColumn = new ColumnHeader();
            localAddressColumn = new ColumnHeader();
            localPortColumn = new ColumnHeader();
            remoteAddressColumn = new ColumnHeader();
            remotePortColumn = new ColumnHeader();
            stateColumn = new ColumnHeader();
            liveConnectionsContextMenu = new ContextMenuStrip(components);
            createAdvancedRuleToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            killProcessToolStripMenuItem = new ToolStripMenuItem();
            blockRemoteIPToolStripMenuItem = new ToolStripMenuItem();
            copyRemoteAddressToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem1 = new ToolStripMenuItem();
            copyDetailsToolStripMenuItem1 = new ToolStripMenuItem();
            settingsTabPage = new TabPage();
            deleteAllRulesButton = new Button();
            revertFirewallButton = new Button();
            auditAlertsSwitch = new CheckBox();
            managePublishersButton = new Button();
            autoAllowSystemTrustedCheck = new CheckBox();
            showAppIconsSwitch = new CheckBox();
            trafficMonitorSwitch = new CheckBox();
            autoRefreshLabel1 = new Label();
            autoRefreshLabel2 = new Label();
            coffeePanel = new Panel();
            coffeeLinkLabel = new LinkLabel();
            coffeePictureBox = new PictureBox();
            versionLabel = new Label();
            checkForUpdatesButton = new Button();
            openFirewallButton = new Button();
            forumLink = new LinkLabel();
            reportProblemLink = new LinkLabel();
            helpLink = new LinkLabel();
            autoRefreshTextBox = new TextBox();
            loggingSwitch = new CheckBox();
            popupsSwitch = new CheckBox();
            darkModeSwitch = new CheckBox();
            startOnStartupSwitch = new CheckBox();
            closeToTraySwitch = new CheckBox();
            appImageList = new ImageList(components);
            lockdownButton = new Button();
            rescanButton = new Button();
            mainToolTip = new ToolTip(components);
            appIconList = new ImageList(components);
            mainTabControl.SuspendLayout();
            dashboardTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            logoPictureBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)arrowPictureBox).BeginInit();
            rulesTabPage.SuspendLayout();
            rulesContextMenu.SuspendLayout();
            systemChangesTabPage.SuspendLayout();
            auditContextMenu.SuspendLayout();
            groupsTabPage.SuspendLayout();
            groupsContextMenu.SuspendLayout();
            liveConnectionsTabPage.SuspendLayout();
            liveConnectionsContextMenu.SuspendLayout();
            settingsTabPage.SuspendLayout();
            coffeePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).BeginInit();
            SuspendLayout();
            // 
            // mainTabControl
            // 
            mainTabControl.Alignment = TabAlignment.Left;
            mainTabControl.BorderColor = SystemColors.ControlDark;
            mainTabControl.Controls.Add(dashboardTabPage);
            mainTabControl.Controls.Add(rulesTabPage);
            mainTabControl.Controls.Add(systemChangesTabPage);
            mainTabControl.Controls.Add(groupsTabPage);
            mainTabControl.Controls.Add(liveConnectionsTabPage);
            mainTabControl.Controls.Add(settingsTabPage);
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabControl.ImageList = appImageList;
            mainTabControl.ItemSize = new Size(70, 120);
            mainTabControl.LineColor = SystemColors.Highlight;
            mainTabControl.Location = new Point(0, 0);
            mainTabControl.Margin = new Padding(3, 4, 3, 4);
            mainTabControl.Multiline = true;
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedForeColor = SystemColors.HighlightText;
            mainTabControl.SelectedIndex = 0;
            mainTabControl.SelectTabColor = SystemColors.ControlLight;
            mainTabControl.Size = new Size(1143, 933);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.TabColor = SystemColors.ControlLight;
            mainTabControl.TabIndex = 0;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            // 
            // dashboardTabPage
            // 
            dashboardTabPage.Controls.Add(logoPictureBox);
            dashboardTabPage.Controls.Add(this.dashboardControl1);
            dashboardTabPage.ImageIndex = 7;
            dashboardTabPage.Location = new Point(124, 4);
            dashboardTabPage.Margin = new Padding(3, 4, 3, 4);
            dashboardTabPage.Name = "dashboardTabPage";
            dashboardTabPage.Padding = new Padding(3, 4, 3, 4);
            dashboardTabPage.Size = new Size(1015, 925);
            dashboardTabPage.TabIndex = 0;
            dashboardTabPage.Text = "Dashboard";
            dashboardTabPage.UseVisualStyleBackColor = true;
            // 
            // dashboardControl1
            // 
            this.dashboardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardControl1.Location = new System.Drawing.Point(3, 4);
            this.dashboardControl1.Name = "dashboardControl1";
            this.dashboardControl1.Size = new System.Drawing.Size(1009, 917);
            this.dashboardControl1.TabIndex = 2;
            // 
            // logoPictureBox
            // 
            logoPictureBox.Controls.Add(arrowPictureBox);
            logoPictureBox.Controls.Add(instructionLabel);
            logoPictureBox.Dock = DockStyle.Fill;
            logoPictureBox.Location = new Point(3, 4);
            logoPictureBox.Margin = new Padding(3, 4, 3, 4);
            logoPictureBox.Name = "logoPictureBox";
            logoPictureBox.Size = new Size(1009, 917);
            logoPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            logoPictureBox.TabIndex = 1;
            logoPictureBox.TabStop = false;
            // 
            // arrowPictureBox
            // 
            arrowPictureBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            arrowPictureBox.BackColor = Color.Transparent;
            arrowPictureBox.Location = new Point(23, 829);
            arrowPictureBox.Margin = new Padding(3, 4, 3, 4);
            arrowPictureBox.Name = "arrowPictureBox";
            arrowPictureBox.Size = new Size(69, 53);
            arrowPictureBox.TabIndex = 3;
            arrowPictureBox.TabStop = false;
            arrowPictureBox.Paint += ArrowPictureBox_Paint;
            // 
            // instructionLabel
            // 
            instructionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            instructionLabel.AutoSize = true;
            instructionLabel.BackColor = Color.Transparent;
            instructionLabel.Font = new Font("Segoe UI", 9F);
            instructionLabel.Location = new Point(23, 789);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(304, 20);
            instructionLabel.TabIndex = 2;
            instructionLabel.Text = "Press the lock key to initiate firewall defense.";
            // 
            // rulesTabPage
            // 
            rulesTabPage.Controls.Add(advFilterAdvancedCheck);
            rulesTabPage.Controls.Add(advancedRuleButton);
            rulesTabPage.Controls.Add(advFilterWildcardCheck);
            rulesTabPage.Controls.Add(advFilterUwpCheck);
            rulesTabPage.Controls.Add(advFilterServiceCheck);
            rulesTabPage.Controls.Add(advFilterProgramCheck);
            rulesTabPage.Controls.Add(rulesSearchTextBox);
            rulesTabPage.Controls.Add(createRuleButton);
            rulesTabPage.Controls.Add(rulesListView);
            rulesTabPage.ImageIndex = 5;
            rulesTabPage.Location = new Point(124, 4);
            rulesTabPage.Margin = new Padding(3, 4, 3, 4);
            rulesTabPage.Name = "rulesTabPage";
            rulesTabPage.Padding = new Padding(3, 4, 3, 4);
            rulesTabPage.Size = new Size(1015, 925);
            rulesTabPage.TabIndex = 1;
            rulesTabPage.Text = "Rules";
            rulesTabPage.UseVisualStyleBackColor = true;
            // 
            // advFilterAdvancedCheck
            // 
            advFilterAdvancedCheck.AutoSize = true;
            advFilterAdvancedCheck.Checked = true;
            advFilterAdvancedCheck.CheckState = CheckState.Checked;
            advFilterAdvancedCheck.Location = new Point(639, 20);
            advFilterAdvancedCheck.Margin = new Padding(3, 4, 3, 4);
            advFilterAdvancedCheck.Name = "advFilterAdvancedCheck";
            advFilterAdvancedCheck.Size = new System.Drawing.Size(97, 24);
            advFilterAdvancedCheck.TabIndex = 6;
            advFilterAdvancedCheck.Text = "Advanced";
            advFilterAdvancedCheck.UseVisualStyleBackColor = true;
            advFilterAdvancedCheck.CheckedChanged += AdvFilter_CheckedChanged;
            // 
            // advancedRuleButton
            // 
            advancedRuleButton.Location = new Point(157, 8);
            advancedRuleButton.Margin = new Padding(3, 4, 3, 4);
            advancedRuleButton.Name = "advancedRuleButton";
            advancedRuleButton.Size = new Size(143, 48);
            advancedRuleButton.TabIndex = 1;
            advancedRuleButton.Text = "Advanced Rule";
            advancedRuleButton.UseVisualStyleBackColor = true;
            advancedRuleButton.Click += AdvancedRuleButton_Click;
            // 
            // advFilterWildcardCheck
            // 
            advFilterWildcardCheck.AutoSize = true;
            advFilterWildcardCheck.Checked = true;
            advFilterWildcardCheck.CheckState = CheckState.Checked;
            advFilterWildcardCheck.Location = new Point(544, 20);
            advFilterWildcardCheck.Margin = new Padding(3, 4, 3, 4);
            advFilterWildcardCheck.Name = "advFilterWildcardCheck";
            advFilterWildcardCheck.Size = new System.Drawing.Size(91, 24);
            advFilterWildcardCheck.TabIndex = 5;
            advFilterWildcardCheck.Text = "Wildcard";
            advFilterWildcardCheck.UseVisualStyleBackColor = true;
            advFilterWildcardCheck.CheckedChanged += AdvFilter_CheckedChanged;
            // 
            // advFilterUwpCheck
            // 
            advFilterUwpCheck.AutoSize = true;
            advFilterUwpCheck.Checked = true;
            advFilterUwpCheck.CheckState = CheckState.Checked;
            advFilterUwpCheck.Location = new Point(477, 20);
            advFilterUwpCheck.Margin = new Padding(3, 4, 3, 4);
            advFilterUwpCheck.Name = "advFilterUwpCheck";
            advFilterUwpCheck.Size = new System.Drawing.Size(63, 24);
            advFilterUwpCheck.TabIndex = 4;
            advFilterUwpCheck.Text = "UWP";
            advFilterUwpCheck.UseVisualStyleBackColor = true;
            advFilterUwpCheck.CheckedChanged += AdvFilter_CheckedChanged;
            // 
            // advFilterServiceCheck
            // 
            advFilterServiceCheck.AutoSize = true;
            advFilterServiceCheck.Checked = true;
            advFilterServiceCheck.CheckState = CheckState.Checked;
            advFilterServiceCheck.Location = new Point(395, 20);
            advFilterServiceCheck.Margin = new Padding(3, 4, 3, 4);
            advFilterServiceCheck.Name = "advFilterServiceCheck";
            advFilterServiceCheck.Size = new System.Drawing.Size(78, 24);
            advFilterServiceCheck.TabIndex = 3;
            advFilterServiceCheck.Text = "Service";
            advFilterServiceCheck.UseVisualStyleBackColor = true;
            advFilterServiceCheck.CheckedChanged += AdvFilter_CheckedChanged;
            // 
            // advFilterProgramCheck
            // 
            advFilterProgramCheck.AutoSize = true;
            advFilterProgramCheck.Checked = true;
            advFilterProgramCheck.CheckState = CheckState.Checked;
            advFilterProgramCheck.Location = new Point(306, 20);
            advFilterProgramCheck.Margin = new Padding(3, 4, 3, 4);
            advFilterProgramCheck.Name = "advFilterProgramCheck";
            advFilterProgramCheck.Size = new System.Drawing.Size(85, 24);
            advFilterProgramCheck.TabIndex = 2;
            advFilterProgramCheck.Text = "Program";
            advFilterProgramCheck.UseVisualStyleBackColor = true;
            advFilterProgramCheck.CheckedChanged += AdvFilter_CheckedChanged;
            // 
            // rulesSearchTextBox
            // 
            rulesSearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rulesSearchTextBox.Location = new Point(704, 17);
            rulesSearchTextBox.Margin = new Padding(3, 4, 3, 4);
            rulesSearchTextBox.Name = "rulesSearchTextBox";
            rulesSearchTextBox.PlaceholderText = "Search rules...";
            rulesSearchTextBox.Size = new Size(285, 27);
            rulesSearchTextBox.TabIndex = 7;
            rulesSearchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // createRuleButton
            // 
            createRuleButton.Location = new Point(7, 8);
            createRuleButton.Margin = new Padding(3, 4, 3, 4);
            createRuleButton.Name = "createRuleButton";
            createRuleButton.Size = new Size(143, 48);
            createRuleButton.TabIndex = 0;
            createRuleButton.Text = "Create Rule";
            createRuleButton.UseVisualStyleBackColor = true;
            createRuleButton.Click += CreateRuleButton_Click;
            // 
            // rulesListView
            // 
            rulesListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rulesListView.BorderStyle = BorderStyle.None;
            rulesListView.Columns.AddRange(new ColumnHeader[] { advIconColumn, advNameColumn, advStatusColumn, advProtocolColumn, advLocalPortsColumn, advRemotePortsColumn, advLocalAddressColumn, advRemoteAddressColumn, advProgramColumn, advServiceColumn, advProfilesColumn, advGroupingColumn, advDescColumn });
            rulesListView.ContextMenuStrip = rulesContextMenu;
            rulesListView.FullRowSelect = true;
            rulesListView.Location = new Point(7, 60);
            rulesListView.Margin = new Padding(3, 4, 3, 4);
            rulesListView.Name = "rulesListView";
            rulesListView.OwnerDraw = true;
            rulesListView.Size = new Size(983, 855);
            rulesListView.TabIndex = 8;
            rulesListView.UseCompatibleStateImageBehavior = false;
            rulesListView.View = View.Details;
            rulesListView.VirtualMode = true;
            rulesListView.ColumnClick += ListView_ColumnClick;
            rulesListView.DrawItem += RulesListView_DrawItem;
            rulesListView.DrawSubItem += RulesListView_DrawSubItem;
            rulesListView.MouseClick += RulesListView_MouseClick;
            rulesListView.MouseLeave += ListView_MouseLeave;
            rulesListView.MouseMove += ListView_MouseMove;
            rulesListView.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(ListView_RetrieveVirtualItem);
            // 
            // advIconColumn
            // 
            advIconColumn.Text = "";
            advIconColumn.Width = 32;
            // 
            // advNameColumn
            // 
            advNameColumn.Text = "Name";
            advNameColumn.Width = 180;
            // 
            // advStatusColumn
            // 
            advStatusColumn.Text = "Action";
            advStatusColumn.Width = 150;
            // 
            // advProtocolColumn
            // 
            advProtocolColumn.Text = "Protocol";
            advProtocolColumn.Width = 70;
            // 
            // advLocalPortsColumn
            // 
            advLocalPortsColumn.Text = "Local Ports";
            advLocalPortsColumn.Width = 100;
            // 
            // advRemotePortsColumn
            // 
            advRemotePortsColumn.Text = "Remote Ports";
            advRemotePortsColumn.Width = 100;
            // 
            // advLocalAddressColumn
            // 
            advLocalAddressColumn.Text = "Local Address";
            advLocalAddressColumn.Width = 120;
            // 
            // advRemoteAddressColumn
            // 
            advRemoteAddressColumn.Text = "Remote Address";
            advRemoteAddressColumn.Width = 120;
            // 
            // advProgramColumn
            // 
            advProgramColumn.Text = "Program";
            advProgramColumn.Width = 200;
            // 
            // advServiceColumn
            // 
            advServiceColumn.Text = "Service";
            advServiceColumn.Width = 150;
            // 
            // advProfilesColumn
            // 
            advProfilesColumn.Text = "Profiles";
            advProfilesColumn.Width = 100;
            // 
            // advGroupingColumn
            // 
            advGroupingColumn.Text = "Group";
            advGroupingColumn.Width = 150;
            // 
            // advDescColumn
            // 
            advDescColumn.Text = "Description";
            advDescColumn.Width = 300;
            // 
            // rulesContextMenu
            // 
            rulesContextMenu.ImageScalingSize = new Size(20, 20);
            rulesContextMenu.Items.AddRange(new ToolStripItem[] { allowToolStripMenuItem, blockToolStripMenuItem, toolStripSeparator1, deleteRuleToolStripMenuItem, toolStripSeparator2, openFileLocationToolStripMenuItem, copyDetailsToolStripMenuItem2 });
            rulesContextMenu.Name = "rulesContextMenu";
            rulesContextMenu.Size = new Size(203, 142);
            rulesContextMenu.Opening += ContextMenu_Opening;
            // 
            // allowToolStripMenuItem
            // 
            allowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allowOutboundToolStripMenuItem, allowInboundToolStripMenuItem, allowAllToolStripMenuItem });
            allowToolStripMenuItem.Name = "allowToolStripMenuItem";
            allowToolStripMenuItem.Size = new Size(202, 24);
            allowToolStripMenuItem.Text = "Allow";
            // 
            // allowOutboundToolStripMenuItem
            // 
            allowOutboundToolStripMenuItem.Name = "allowOutboundToolStripMenuItem";
            allowOutboundToolStripMenuItem.Size = new Size(159, 26);
            allowOutboundToolStripMenuItem.Tag = "Allow (Outbound)";
            allowOutboundToolStripMenuItem.Text = "Outbound";
            allowOutboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // allowInboundToolStripMenuItem
            // 
            allowInboundToolStripMenuItem.Name = "allowInboundToolStripMenuItem";
            allowInboundToolStripMenuItem.Size = new Size(159, 26);
            allowInboundToolStripMenuItem.Tag = "Allow (Inbound)";
            allowInboundToolStripMenuItem.Text = "Inbound";
            allowInboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // allowAllToolStripMenuItem
            // 
            allowAllToolStripMenuItem.Name = "allowAllToolStripMenuItem";
            allowAllToolStripMenuItem.Size = new Size(159, 26);
            allowAllToolStripMenuItem.Tag = "Allow (All)";
            allowAllToolStripMenuItem.Text = "All";
            allowAllToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockToolStripMenuItem
            // 
            blockToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { blockOutboundToolStripMenuItem, blockInboundToolStripMenuItem, blockAllToolStripMenuItem });
            blockToolStripMenuItem.Name = "blockToolStripMenuItem";
            blockToolStripMenuItem.Size = new Size(202, 24);
            blockToolStripMenuItem.Text = "Block";
            // 
            // blockOutboundToolStripMenuItem
            // 
            blockOutboundToolStripMenuItem.Name = "blockOutboundToolStripMenuItem";
            blockOutboundToolStripMenuItem.Size = new Size(159, 26);
            blockOutboundToolStripMenuItem.Tag = "Block (Outbound)";
            blockOutboundToolStripMenuItem.Text = "Outbound";
            blockOutboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockInboundToolStripMenuItem
            // 
            blockInboundToolStripMenuItem.Name = "blockInboundToolStripMenuItem";
            blockInboundToolStripMenuItem.Size = new Size(159, 26);
            blockInboundToolStripMenuItem.Tag = "Block (Inbound)";
            blockInboundToolStripMenuItem.Text = "Inbound";
            blockInboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockAllToolStripMenuItem
            // 
            blockAllToolStripMenuItem.Name = "blockAllToolStripMenuItem";
            blockAllToolStripMenuItem.Size = new Size(159, 26);
            blockAllToolStripMenuItem.Tag = "Block (All)";
            blockAllToolStripMenuItem.Text = "All";
            blockAllToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(199, 6);
            // 
            // deleteRuleToolStripMenuItem
            // 
            deleteRuleToolStripMenuItem.Name = "deleteRuleToolStripMenuItem";
            deleteRuleToolStripMenuItem.Size = new Size(202, 24);
            deleteRuleToolStripMenuItem.Text = "Delete Rule(s)";
            deleteRuleToolStripMenuItem.Click += DeleteRuleMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(199, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            openFileLocationToolStripMenuItem.Size = new Size(202, 24);
            openFileLocationToolStripMenuItem.Text = "Open File Location";
            openFileLocationToolStripMenuItem.Click += OpenFileLocationMenuItem_Click;
            // 
            // copyDetailsToolStripMenuItem2
            // 
            copyDetailsToolStripMenuItem2.Name = "copyDetailsToolStripMenuItem2";
            copyDetailsToolStripMenuItem2.Size = new Size(202, 24);
            copyDetailsToolStripMenuItem2.Text = "Copy Details";
            copyDetailsToolStripMenuItem2.Click += copyDetailsToolStripMenuItem_Click;
            // 
            // systemChangesTabPage
            // 
            systemChangesTabPage.Controls.Add(auditSearchTextBox);
            systemChangesTabPage.Controls.Add(rebuildBaselineButton);
            systemChangesTabPage.Controls.Add(systemChangesListView);
            systemChangesTabPage.ImageIndex = 3;
            systemChangesTabPage.Location = new Point(124, 4);
            systemChangesTabPage.Margin = new Padding(3, 4, 3, 4);
            systemChangesTabPage.Name = "systemChangesTabPage";
            systemChangesTabPage.Size = new Size(1015, 925);
            systemChangesTabPage.TabIndex = 2;
            systemChangesTabPage.Text = "Audit";
            systemChangesTabPage.UseVisualStyleBackColor = true;
            // 
            // auditSearchTextBox
            // 
            auditSearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            auditSearchTextBox.Location = new Point(707, 27);
            auditSearchTextBox.Margin = new Padding(3, 4, 3, 4);
            auditSearchTextBox.Name = "auditSearchTextBox";
            auditSearchTextBox.PlaceholderText = "Search changes...";
            auditSearchTextBox.Size = new Size(285, 27);
            auditSearchTextBox.TabIndex = 3;
            auditSearchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // rebuildBaselineButton
            // 
            rebuildBaselineButton.Location = new Point(3, 17);
            rebuildBaselineButton.Margin = new Padding(3, 4, 3, 4);
            rebuildBaselineButton.Name = "rebuildBaselineButton";
            rebuildBaselineButton.Size = new Size(173, 48);
            rebuildBaselineButton.TabIndex = 2;
            rebuildBaselineButton.Text = "Rebuild Baseline";
            rebuildBaselineButton.Click += RebuildBaselineButton_Click;
            // 
            // systemChangesListView
            // 
            systemChangesListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            systemChangesListView.BorderStyle = BorderStyle.None;
            systemChangesListView.Columns.AddRange(new ColumnHeader[] { changeActionColumn, changeNameColumn, changeAction2Column, changeDirectionColumn, changeProtocolColumn, changeAppColumn, changeRemoteAddressColumn, changeDescColumn });
            systemChangesListView.ContextMenuStrip = auditContextMenu;
            systemChangesListView.DarkMode = null;
            systemChangesListView.FullRowSelect = true;
            systemChangesListView.Location = new Point(3, 77);
            systemChangesListView.Margin = new Padding(3, 4, 3, 4);
            systemChangesListView.Name = "systemChangesListView";
            systemChangesListView.Size = new Size(990, 837);
            systemChangesListView.TabIndex = 1;
            systemChangesListView.UseCompatibleStateImageBehavior = false;
            systemChangesListView.View = View.Details;
            systemChangesListView.ViewMode = MinimalFirewall.ButtonListView.Mode.Audit;
            systemChangesListView.ColumnClick += ListView_ColumnClick;
            // 
            // changeActionColumn
            // 
            changeActionColumn.Text = "Action";
            changeActionColumn.Width = 300;
            // 
            // changeNameColumn
            // 
            changeNameColumn.Text = "Rule Name";
            changeNameColumn.Width = 200;
            // 
            // changeAction2Column
            // 
            changeAction2Column.Text = "Action";
            changeAction2Column.Width = 70;
            // 
            // changeDirectionColumn
            // 
            changeDirectionColumn.Text = "Direction";
            changeDirectionColumn.Width = 80;
            // 
            // changeProtocolColumn
            // 
            changeProtocolColumn.Text = "Protocol";
            // 
            // changeAppColumn
            // 
            changeAppColumn.Text = "Application";
            changeAppColumn.Width = 200;
            // 
            // changeRemoteAddressColumn
            // 
            changeRemoteAddressColumn.Text = "Remote Address";
            changeRemoteAddressColumn.Width = 120;
            // 
            // changeDescColumn
            // 
            changeDescColumn.Text = "Description";
            changeDescColumn.Width = 250;
            // 
            // auditContextMenu
            // 
            auditContextMenu.ImageScalingSize = new Size(20, 20);
            auditContextMenu.Items.AddRange(new ToolStripItem[] { acceptAllToolStripMenuItem, copyDetailsToolStripMenuItem3, toolStripSeparator10, openFileLocationToolStripMenuItem2 });
            auditContextMenu.Name = "auditContextMenu";
            auditContextMenu.Size = new Size(207, 88);
            auditContextMenu.Opening += ContextMenu_Opening;
            // 
            // acceptAllToolStripMenuItem
            // 
            acceptAllToolStripMenuItem.Name = "acceptAllToolStripMenuItem";
            acceptAllToolStripMenuItem.Size = new Size(206, 24);
            acceptAllToolStripMenuItem.Text = "Accept All Changes";
            acceptAllToolStripMenuItem.Click += AcceptAllToolStripMenuItem_Click;
            // 
            // copyDetailsToolStripMenuItem3
            // 
            copyDetailsToolStripMenuItem3.Name = "copyDetailsToolStripMenuItem3";
            copyDetailsToolStripMenuItem3.Size = new Size(206, 24);
            copyDetailsToolStripMenuItem3.Text = "Copy Details";
            copyDetailsToolStripMenuItem3.Click += copyDetailsToolStripMenuItem_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(203, 6);
            // 
            // openFileLocationToolStripMenuItem2
            // 
            openFileLocationToolStripMenuItem2.Name = "openFileLocationToolStripMenuItem2";
            openFileLocationToolStripMenuItem2.Size = new Size(206, 24);
            openFileLocationToolStripMenuItem2.Text = "Open File Location";
            openFileLocationToolStripMenuItem2.Click += OpenFileLocationMenuItem_Click;
            // 
            // groupsTabPage
            // 
            groupsTabPage.Controls.Add(groupsListView);
            groupsTabPage.ImageIndex = 5;
            groupsTabPage.Location = new Point(124, 4);
            groupsTabPage.Margin = new Padding(3, 4, 3, 4);
            groupsTabPage.Name = "groupsTabPage";
            groupsTabPage.Padding = new Padding(3, 4, 3, 4);
            groupsTabPage.Size = new Size(1015, 925);
            groupsTabPage.TabIndex = 5;
            groupsTabPage.Text = "Groups";
            groupsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupsListView
            // 
            groupsListView.BorderStyle = BorderStyle.None;
            groupsListView.Columns.AddRange(new ColumnHeader[] { groupNameColumn, groupEnabledColumn });
            groupsListView.ContextMenuStrip = groupsContextMenu;
            groupsListView.Dock = DockStyle.Fill;
            groupsListView.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupsListView.FullRowSelect = true;
            groupsListView.Location = new Point(3, 4);
            groupsListView.Margin = new Padding(3, 4, 3, 4);
            groupsListView.Name = "groupsListView";
            groupsListView.OwnerDraw = true;
            groupsListView.Size = new Size(1009, 917);
            groupsListView.TabIndex = 0;
            groupsListView.UseCompatibleStateImageBehavior = false;
            groupsListView.View = View.Details;
            groupsListView.ColumnClick += ListView_ColumnClick;
            groupsListView.DrawItem += GroupsListView_DrawItem;
            groupsListView.DrawSubItem += GroupsListView_DrawSubItem;
            groupsListView.MouseClick += GroupsListView_MouseClick;
            groupsListView.MouseLeave += ListView_MouseLeave;
            groupsListView.MouseMove += ListView_MouseMove;
            // 
            // groupNameColumn
            // 
            groupNameColumn.Text = "Group Name";
            groupNameColumn.Width = 400;
            // 
            // groupEnabledColumn
            // 
            groupEnabledColumn.Text = "Enabled";
            groupEnabledColumn.Width = 120;
            // 
            // groupsContextMenu
            // 
            groupsContextMenu.ImageScalingSize = new Size(20, 20);
            groupsContextMenu.Items.AddRange(new ToolStripItem[] { deleteGroupToolStripMenuItem });
            groupsContextMenu.Name = "groupsContextMenu";
            groupsContextMenu.Size = new Size(177, 28);
            // 
            // deleteGroupToolStripMenuItem
            // 
            deleteGroupToolStripMenuItem.Name = "deleteGroupToolStripMenuItem";
            deleteGroupToolStripMenuItem.Size = new Size(176, 24);
            deleteGroupToolStripMenuItem.Text = "Delete Group...";
            deleteGroupToolStripMenuItem.Click += DeleteGroupToolStripMenuItem_Click;
            // 
            // liveConnectionsTabPage
            // 
            liveConnectionsTabPage.Controls.Add(liveConnectionsDisabledLabel);
            liveConnectionsTabPage.Controls.Add(liveConnectionsListView);
            liveConnectionsTabPage.ImageIndex = 10;
            liveConnectionsTabPage.Location = new Point(124, 4);
            liveConnectionsTabPage.Margin = new Padding(3, 4, 3, 4);
            liveConnectionsTabPage.Name = "liveConnectionsTabPage";
            liveConnectionsTabPage.Padding = new Padding(3, 4, 3, 4);
            liveConnectionsTabPage.Size = new Size(1015, 925);
            liveConnectionsTabPage.TabIndex = 6;
            liveConnectionsTabPage.Text = "Live Connections";
            liveConnectionsTabPage.UseVisualStyleBackColor = true;
            // 
            // liveConnectionsDisabledLabel
            // 
            liveConnectionsDisabledLabel.Dock = DockStyle.Fill;
            liveConnectionsDisabledLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            liveConnectionsDisabledLabel.Location = new Point(3, 4);
            liveConnectionsDisabledLabel.Name = "liveConnectionsDisabledLabel";
            liveConnectionsDisabledLabel.Size = new Size(1009, 917);
            liveConnectionsDisabledLabel.TabIndex = 1;
            liveConnectionsDisabledLabel.Text = "Please turn on Live Connections in the Settings tab.";
            liveConnectionsDisabledLabel.TextAlign = ContentAlignment.MiddleCenter;
            liveConnectionsDisabledLabel.Visible = false;
            // 
            // liveConnectionsListView
            // 
            liveConnectionsListView.BorderStyle = BorderStyle.None;
            liveConnectionsListView.Columns.AddRange(new ColumnHeader[] { liveIconColumn, processNameColumn, localAddressColumn, localPortColumn, remoteAddressColumn, remotePortColumn, stateColumn });
            liveConnectionsListView.ContextMenuStrip = liveConnectionsContextMenu;
            liveConnectionsListView.Dock = DockStyle.Fill;
            liveConnectionsListView.FullRowSelect = true;
            liveConnectionsListView.Location = new Point(3, 4);
            liveConnectionsListView.Margin = new Padding(3, 4, 3, 4);
            liveConnectionsListView.Name = "liveConnectionsListView";
            liveConnectionsListView.OwnerDraw = true;
            liveConnectionsListView.Size = new Size(1009, 917);
            liveConnectionsListView.TabIndex = 0;
            liveConnectionsListView.UseCompatibleStateImageBehavior = false;
            liveConnectionsListView.View = View.Details;
            liveConnectionsListView.VirtualMode = true;
            liveConnectionsListView.ColumnClick += ListView_ColumnClick;
            liveConnectionsListView.DrawItem += LiveConnectionsListView_DrawItem;
            liveConnectionsListView.DrawSubItem += LiveConnectionsListView_DrawSubItem;
            liveConnectionsListView.MouseClick += LiveConnectionsListView_MouseClick;
            liveConnectionsListView.MouseLeave += ListView_MouseLeave;
            liveConnectionsListView.MouseMove += ListView_MouseMove;
            liveConnectionsListView.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(ListView_RetrieveVirtualItem);
            // 
            // liveIconColumn
            // 
            liveIconColumn.Text = "";
            liveIconColumn.Width = 32;
            // 
            // processNameColumn
            // 
            processNameColumn.Text = "Process Name";
            processNameColumn.Width = 200;
            // 
            // localAddressColumn
            // 
            localAddressColumn.Text = "Local Address";
            localAddressColumn.Width = 150;
            // 
            // localPortColumn
            // 
            localPortColumn.Text = "Local Port";
            localPortColumn.Width = 80;
            // 
            // remoteAddressColumn
            // 
            remoteAddressColumn.Text = "Remote Address";
            remoteAddressColumn.Width = 150;
            // 
            // remotePortColumn
            // 
            remotePortColumn.Text = "Remote Port";
            remotePortColumn.Width = 80;
            // 
            // stateColumn
            // 
            stateColumn.Text = "State";
            stateColumn.Width = 100;
            // 
            // liveConnectionsContextMenu
            // 
            liveConnectionsContextMenu.ImageScalingSize = new Size(20, 20);
            liveConnectionsContextMenu.Items.AddRange(new ToolStripItem[] { createAdvancedRuleToolStripMenuItem1, toolStripSeparator9, killProcessToolStripMenuItem, blockRemoteIPToolStripMenuItem, copyRemoteAddressToolStripMenuItem, toolStripSeparator8, openFileLocationToolStripMenuItem1, copyDetailsToolStripMenuItem1 });
            liveConnectionsContextMenu.Name = "liveConnectionsContextMenu";
            liveConnectionsContextMenu.Size = new Size(228, 166);
            liveConnectionsContextMenu.Opening += ContextMenu_Opening;
            // 
            // createAdvancedRuleToolStripMenuItem1
            // 
            createAdvancedRuleToolStripMenuItem1.Name = "createAdvancedRuleToolStripMenuItem1";
            createAdvancedRuleToolStripMenuItem1.Size = new Size(227, 24);
            createAdvancedRuleToolStripMenuItem1.Text = "Create Advanced Rule...";
            createAdvancedRuleToolStripMenuItem1.Click += createAdvancedRuleToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(224, 6);
            // 
            // killProcessToolStripMenuItem
            // 
            killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
            killProcessToolStripMenuItem.Size = new Size(227, 24);
            killProcessToolStripMenuItem.Text = "Kill Process";
            killProcessToolStripMenuItem.Click += KillProcessToolStripMenuItem_Click;
            // 
            // blockRemoteIPToolStripMenuItem
            // 
            blockRemoteIPToolStripMenuItem.Name = "blockRemoteIPToolStripMenuItem";
            blockRemoteIPToolStripMenuItem.Size = new Size(227, 24);
            blockRemoteIPToolStripMenuItem.Text = "Block Remote IP";
            blockRemoteIPToolStripMenuItem.Click += BlockRemoteIPToolStripMenuItem_Click;
            // 
            // copyRemoteAddressToolStripMenuItem
            // 
            copyRemoteAddressToolStripMenuItem.Name = "copyRemoteAddressToolStripMenuItem";
            copyRemoteAddressToolStripMenuItem.Size = new Size(227, 24);
            copyRemoteAddressToolStripMenuItem.Text = "Copy Remote Address";
            copyRemoteAddressToolStripMenuItem.Click += copyRemoteAddressToolStripMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(224, 6);
            // 
            // openFileLocationToolStripMenuItem1
            // 
            this.openFileLocationToolStripMenuItem1.Name = "openFileLocationToolStripMenuItem1";
            this.openFileLocationToolStripMenuItem1.Size = new System.Drawing.Size(227, 24);
            this.openFileLocationToolStripMenuItem1.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem1.Click += new System.EventHandler(this.OpenFileLocationMenuItem_Click);
            // 
            // copyDetailsToolStripMenuItem1
            // 
            copyDetailsToolStripMenuItem1.Name = "copyDetailsToolStripMenuItem1";
            copyDetailsToolStripMenuItem1.Size = new Size(227, 24);
            copyDetailsToolStripMenuItem1.Text = "Copy Details";
            copyDetailsToolStripMenuItem1.Click += copyDetailsToolStripMenuItem_Click;
            // 
            // settingsTabPage
            // 
            settingsTabPage.Controls.Add(deleteAllRulesButton);
            settingsTabPage.Controls.Add(revertFirewallButton);
            settingsTabPage.Controls.Add(auditAlertsSwitch);
            settingsTabPage.Controls.Add(managePublishersButton);
            settingsTabPage.Controls.Add(autoAllowSystemTrustedCheck);
            settingsTabPage.Controls.Add(showAppIconsSwitch);
            settingsTabPage.Controls.Add(trafficMonitorSwitch);
            settingsTabPage.Controls.Add(autoRefreshLabel1);
            settingsTabPage.Controls.Add(autoRefreshLabel2);
            settingsTabPage.Controls.Add(coffeePanel);
            settingsTabPage.Controls.Add(versionLabel);
            settingsTabPage.Controls.Add(checkForUpdatesButton);
            settingsTabPage.Controls.Add(openFirewallButton);
            settingsTabPage.Controls.Add(forumLink);
            settingsTabPage.Controls.Add(reportProblemLink);
            settingsTabPage.Controls.Add(helpLink);
            settingsTabPage.Controls.Add(autoRefreshTextBox);
            settingsTabPage.Controls.Add(loggingSwitch);
            settingsTabPage.Controls.Add(popupsSwitch);
            settingsTabPage.Controls.Add(darkModeSwitch);
            settingsTabPage.Controls.Add(startOnStartupSwitch);
            settingsTabPage.Controls.Add(closeToTraySwitch);
            settingsTabPage.ImageIndex = 6;
            settingsTabPage.Location = new Point(124, 4);
            settingsTabPage.Margin = new Padding(3, 4, 3, 4);
            settingsTabPage.Name = "settingsTabPage";
            settingsTabPage.Size = new Size(1015, 925);
            settingsTabPage.TabIndex = 4;
            settingsTabPage.Text = "Settings";
            settingsTabPage.UseVisualStyleBackColor = true;
            settingsTabPage.AutoScroll = true;
            // 
            // deleteAllRulesButton
            // 
            deleteAllRulesButton.Location = new Point(29, 850);
            deleteAllRulesButton.Margin = new Padding(3, 4, 3, 4);
            deleteAllRulesButton.Name = "deleteAllRulesButton";
            deleteAllRulesButton.Size = new Size(240, 37);
            deleteAllRulesButton.TabIndex = 25;
            deleteAllRulesButton.Text = "Delete all Minimal Firewall rules";
            deleteAllRulesButton.UseVisualStyleBackColor = true;
            deleteAllRulesButton.Click += deleteAllRulesButton_Click;
            // 
            // revertFirewallButton
            // 
            revertFirewallButton.Location = new Point(280, 850);
            revertFirewallButton.Margin = new Padding(3, 4, 3, 4);
            revertFirewallButton.Name = "revertFirewallButton";
            revertFirewallButton.Size = new Size(240, 37);
            revertFirewallButton.TabIndex = 26;
            revertFirewallButton.Text = "Revert Windows Firewall";
            revertFirewallButton.UseVisualStyleBackColor = true;
            revertFirewallButton.Click += revertFirewallButton_Click;
            // 
            // auditAlertsSwitch
            // 
            auditAlertsSwitch.AutoSize = true;
            auditAlertsSwitch.Location = new Point(29, 420);
            auditAlertsSwitch.Margin = new Padding(3, 4, 3, 4);
            auditAlertsSwitch.Name = "auditAlertsSwitch";
            auditAlertsSwitch.Size = new System.Drawing.Size(211, 24);
            auditAlertsSwitch.TabIndex = 24;
            auditAlertsSwitch.Text = "Alert on new system rules";
            auditAlertsSwitch.UseVisualStyleBackColor = true;
            // 
            // managePublishersButton
            // 
            managePublishersButton.Location = new Point(29, 500);
            managePublishersButton.Margin = new Padding(3, 4, 3, 4);
            managePublishersButton.Name = "managePublishersButton";
            managePublishersButton.Size = new Size(200, 37);
            managePublishersButton.TabIndex = 23;
            managePublishersButton.Text = "Manage Trusted Publishers";
            managePublishersButton.UseVisualStyleBackColor = true;
            managePublishersButton.Click += managePublishersButton_Click;
            // 
            // autoAllowSystemTrustedCheck
            // 
            autoAllowSystemTrustedCheck.AutoSize = true;
            autoAllowSystemTrustedCheck.Location = new Point(29, 460);
            autoAllowSystemTrustedCheck.Margin = new Padding(3, 4, 3, 4);
            autoAllowSystemTrustedCheck.Name = "autoAllowSystemTrustedCheck";
            autoAllowSystemTrustedCheck.Size = new System.Drawing.Size(276, 24);
            autoAllowSystemTrustedCheck.TabIndex = 22;
            autoAllowSystemTrustedCheck.Text = "Auto-allow apps trusted by Windows";
            autoAllowSystemTrustedCheck.UseVisualStyleBackColor = true;
            // 
            // showAppIconsSwitch
            // 
            showAppIconsSwitch.AutoSize = true;
            showAppIconsSwitch.Location = new Point(29, 380);
            showAppIconsSwitch.Margin = new Padding(3, 4, 3, 4);
            showAppIconsSwitch.Name = "showAppIconsSwitch";
            showAppIconsSwitch.Size = new System.Drawing.Size(177, 24);
            showAppIconsSwitch.TabIndex = 21;
            showAppIconsSwitch.Text = "Show application icons";
            showAppIconsSwitch.UseVisualStyleBackColor = true;
            showAppIconsSwitch.CheckedChanged += ShowAppIconsSwitch_CheckedChanged;
            // 
            // trafficMonitorSwitch
            // 
            trafficMonitorSwitch.AutoSize = true;
            trafficMonitorSwitch.Location = new Point(29, 347);
            trafficMonitorSwitch.Margin = new Padding(3, 4, 3, 4);
            trafficMonitorSwitch.Name = "trafficMonitorSwitch";
            trafficMonitorSwitch.Size = new System.Drawing.Size(191, 24);
            trafficMonitorSwitch.TabIndex = 20;
            trafficMonitorSwitch.Text = "Enable Live Connections";
            trafficMonitorSwitch.UseVisualStyleBackColor = true;
            trafficMonitorSwitch.CheckedChanged += TrafficMonitorSwitch_CheckedChanged;
            // 
            // autoRefreshLabel1
            // 
            autoRefreshLabel1.AutoSize = true;
            autoRefreshLabel1.Location = new Point(29, 299);
            autoRefreshLabel1.Name = "autoRefreshLabel1";
            autoRefreshLabel1.Size = new System.Drawing.Size(117, 20);
            autoRefreshLabel1.TabIndex = 18;
            autoRefreshLabel1.Text = "List refresh time:";
            // 
            // autoRefreshLabel2
            // 
            autoRefreshLabel2.AutoSize = true;
            autoRefreshLabel2.Location = new Point(251, 299);
            autoRefreshLabel2.Name = "autoRefreshLabel2";
            autoRefreshLabel2.Size = new System.Drawing.Size(61, 20);
            autoRefreshLabel2.TabIndex = 19;
            autoRefreshLabel2.Text = "minutes";
            // 
            // coffeePanel
            // 
            coffeePanel.BackColor = Color.Transparent;
            coffeePanel.Controls.Add(coffeeLinkLabel);
            coffeePanel.Controls.Add(coffeePictureBox);
            coffeePanel.Cursor = Cursors.Hand;
            coffeePanel.Location = new Point(21, 720);
            coffeePanel.Margin = new Padding(3, 4, 3, 4);
            coffeePanel.Name = "coffeePanel";
            coffeePanel.Size = new Size(434, 107);
            coffeePanel.TabIndex = 17;
            coffeePanel.Click += CoffeeLink_Click;
            // 
            // coffeeLinkLabel
            // 
            coffeeLinkLabel.ActiveLinkColor = Color.DodgerBlue;
            coffeeLinkLabel.AutoSize = true;
            coffeeLinkLabel.Location = new Point(69, 24);
            coffeeLinkLabel.MaximumSize = new Size(366, 0);
            coffeeLinkLabel.Name = "coffeeLinkLabel";
            coffeeLinkLabel.Size = new System.Drawing.Size(335, 20);
            coffeeLinkLabel.TabIndex = 15;
            coffeeLinkLabel.TabStop = true;
            coffeeLinkLabel.Tag = "https://www.buymeacoffee.com/deminimis";
            coffeeLinkLabel.Text = "Support my caffeine addiction if you like this app";
            coffeeLinkLabel.Click += CoffeeLink_Click;
            // 
            // coffeePictureBox
            // 
            coffeePictureBox.Cursor = Cursors.Hand;
            coffeePictureBox.Location = new Point(0, 0);
            coffeePictureBox.Margin = new Padding(3, 4, 3, 4);
            coffeePictureBox.Name = "coffeePictureBox";
            coffeePictureBox.Size = new Size(62, 72);
            coffeePictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            coffeePictureBox.TabIndex = 13;
            coffeePictureBox.TabStop = false;
            coffeePictureBox.Click += CoffeeLink_Click;
            coffeePictureBox.MouseEnter += CoffeePictureBox_MouseEnter;
            coffeePictureBox.MouseLeave += CoffeePictureBox_MouseLeave;
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Font = new Font("Segoe UI", 9F);
            versionLabel.Location = new Point(223, 620);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(57, 20);
            versionLabel.TabIndex = 12;
            versionLabel.Text = "Version";
            // 
            // checkForUpdatesButton
            // 
            checkForUpdatesButton.Location = new Point(29, 610);
            checkForUpdatesButton.Margin = new Padding(3, 4, 3, 4);
            checkForUpdatesButton.Name = "checkForUpdatesButton";
            checkForUpdatesButton.Size = new Size(183, 37);
            checkForUpdatesButton.TabIndex = 11;
            checkForUpdatesButton.Text = "Check for Updates";
            checkForUpdatesButton.Click += CheckForUpdatesButton_Click;
            // 
            // openFirewallButton
            // 
            openFirewallButton.Location = new Point(29, 570);
            openFirewallButton.Margin = new Padding(3, 4, 3, 4);
            openFirewallButton.Name = "openFirewallButton";
            openFirewallButton.Size = new Size(183, 37);
            openFirewallButton.TabIndex = 10;
            openFirewallButton.Text = "Open Windows Firewall";
            openFirewallButton.Click += OpenFirewallButton_Click;
            // 
            // forumLink
            // 
            forumLink.AutoSize = true;
            forumLink.Location = new Point(29, 660);
            forumLink.Name = "forumLink";
            forumLink.Size = new System.Drawing.Size(140, 20);
            forumLink.TabIndex = 9;
            forumLink.TabStop = true;
            forumLink.Tag = "https://github.com/deminimis/minimalfirewall/discussions";
            forumLink.Text = "Forum / Discussions";
            forumLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // reportProblemLink
            // 
            reportProblemLink.AutoSize = true;
            reportProblemLink.Location = new Point(29, 680);
            reportProblemLink.Name = "reportProblemLink";
            reportProblemLink.Size = new System.Drawing.Size(126, 20);
            reportProblemLink.TabIndex = 8;
            reportProblemLink.TabStop = true;
            reportProblemLink.Tag = "https://github.com/deminimis/minimalfirewall/issues";
            reportProblemLink.Text = "Report a Problem";
            reportProblemLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // helpLink
            // 
            helpLink.AutoSize = true;
            helpLink.Location = new Point(29, 700);
            helpLink.Name = "helpLink";
            helpLink.Size = new System.Drawing.Size(158, 20);
            helpLink.TabIndex = 7;
            helpLink.TabStop = true;
            helpLink.Tag = "https://github.com/deminimis/minimalfirewall";
            helpLink.Text = "Help / Documentation";
            helpLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // autoRefreshTextBox
            // 
            autoRefreshTextBox.Location = new Point(171, 293);
            autoRefreshTextBox.Margin = new Padding(3, 4, 3, 4);
            autoRefreshTextBox.MaxLength = 3;
            autoRefreshTextBox.Name = "autoRefreshTextBox";
            autoRefreshTextBox.Size = new Size(68, 27);
            autoRefreshTextBox.TabIndex = 5;
            autoRefreshTextBox.Text = "10";
            // 
            // loggingSwitch
            // 
            loggingSwitch.AutoSize = true;
            loggingSwitch.Location = new Point(29, 247);
            loggingSwitch.Margin = new Padding(3, 4, 3, 4);
            loggingSwitch.Name = "loggingSwitch";
            loggingSwitch.Size = new System.Drawing.Size(132, 24);
            loggingSwitch.TabIndex = 4;
            loggingSwitch.Text = "Enable logging";
            loggingSwitch.UseVisualStyleBackColor = true;
            // 
            // popupsSwitch
            // 
            popupsSwitch.AutoSize = true;
            popupsSwitch.Location = new Point(29, 200);
            popupsSwitch.Margin = new Padding(3, 4, 3, 4);
            popupsSwitch.Name = "popupsSwitch";
            popupsSwitch.Size = new System.Drawing.Size(216, 24);
            popupsSwitch.TabIndex = 3;
            popupsSwitch.Text = "Enable pop-up notifications";
            popupsSwitch.UseVisualStyleBackColor = true;
            popupsSwitch.CheckedChanged += PopupsSwitch_CheckedChanged;
            // 
            // darkModeSwitch
            // 
            darkModeSwitch.AutoSize = true;
            darkModeSwitch.Location = new Point(29, 153);
            darkModeSwitch.Margin = new Padding(3, 4, 3, 4);
            darkModeSwitch.Name = "darkModeSwitch";
            darkModeSwitch.Size = new System.Drawing.Size(105, 24);
            darkModeSwitch.TabIndex = 2;
            darkModeSwitch.Text = "Dark Mode";
            darkModeSwitch.UseVisualStyleBackColor = true;
            darkModeSwitch.CheckedChanged += DarkModeSwitch_CheckedChanged;
            // 
            // startOnStartupSwitch
            // 
            startOnStartupSwitch.AutoSize = true;
            startOnStartupSwitch.Location = new Point(29, 107);
            startOnStartupSwitch.Margin = new Padding(3, 4, 3, 4);
            startOnStartupSwitch.Name = "startOnStartupSwitch";
            startOnStartupSwitch.Size = new System.Drawing.Size(159, 24);
            startOnStartupSwitch.TabIndex = 1;
            startOnStartupSwitch.Text = "Start with Windows";
            startOnStartupSwitch.UseVisualStyleBackColor = true;
            startOnStartupSwitch.CheckedChanged += startOnStartupSwitch_CheckedChanged;
            // 
            // closeToTraySwitch
            // 
            closeToTraySwitch.AutoSize = true;
            closeToTraySwitch.Checked = true;
            closeToTraySwitch.CheckState = CheckState.Checked;
            closeToTraySwitch.Location = new Point(29, 60);
            closeToTraySwitch.Margin = new Padding(3, 4, 3, 4);
            closeToTraySwitch.Name = "closeToTraySwitch";
            closeToTraySwitch.Size = new System.Drawing.Size(114, 24);
            closeToTraySwitch.TabIndex = 0;
            closeToTraySwitch.Text = "Close to tray";
            closeToTraySwitch.UseVisualStyleBackColor = true;
            // 
            // appImageList
            // 
            appImageList.ColorDepth = ColorDepth.Depth32Bit;
            appImageList.ImageStream = (ImageListStreamer)resources.GetObject("appImageList.ImageStream");
            appImageList.TransparentColor = Color.Transparent;
            appImageList.Images.SetKeyName(0, "coffee.png");
            appImageList.Images.SetKeyName(1, "refresh.png");
            appImageList.Images.SetKeyName(2, "rules.png");
            appImageList.Images.SetKeyName(3, "system_changes.png");
            appImageList.Images.SetKeyName(4, "locked.png");
            appImageList.Images.SetKeyName(5, "advanced.png");
            appImageList.Images.SetKeyName(6, "settings.png");
            appImageList.Images.SetKeyName(7, "dashboard.png");
            appImageList.Images.SetKeyName(8, "unlocked.png");
            appImageList.Images.SetKeyName(9, "logo.png");
            appImageList.Images.SetKeyName(10, "antenna.png");
            // 
            // lockdownButton
            // 
            lockdownButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lockdownButton.BackColor = Color.Transparent;
            lockdownButton.FlatAppearance.BorderColor = SystemColors.Control;
            lockdownButton.FlatAppearance.BorderSize = 2;
            lockdownButton.FlatStyle = FlatStyle.Flat;
            lockdownButton.Location = new Point(74, 869);
            lockdownButton.Margin = new Padding(3, 4, 3, 4);
            lockdownButton.Name = "lockdownButton";
            lockdownButton.Size = new Size(46, 48);
            lockdownButton.TabIndex = 3;
            lockdownButton.UseVisualStyleBackColor = false;
            lockdownButton.Click += ToggleLockdownButton_Click;
            lockdownButton.MouseEnter += LockdownButton_MouseEnter;
            lockdownButton.MouseLeave += LockdownButton_MouseLeave;
            // 
            // rescanButton
            // 
            rescanButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            rescanButton.BackColor = Color.Transparent;
            rescanButton.FlatAppearance.BorderColor = SystemColors.Control;
            rescanButton.FlatAppearance.BorderSize = 2;
            rescanButton.FlatStyle = FlatStyle.Flat;
            rescanButton.Location = new Point(17, 869);
            rescanButton.Margin = new Padding(3, 4, 3, 4);
            rescanButton.Name = "rescanButton";
            rescanButton.Size = new Size(46, 48);
            rescanButton.TabIndex = 1;
            rescanButton.UseVisualStyleBackColor = false;
            rescanButton.Click += RescanButton_Click;
            rescanButton.MouseEnter += RescanButton_MouseEnter;
            rescanButton.MouseLeave += RescanButton_MouseLeave;
            // 
            // auditContextMenu
            // 
            auditContextMenu.ImageScalingSize = new Size(20, 20);
            auditContextMenu.Items.AddRange(new ToolStripItem[] { acceptAllToolStripMenuItem });
            auditContextMenu.Name = "auditContextMenu";
            auditContextMenu.Size = new Size(207, 52);
            // 
            // appIconList
            // 
            appIconList.ColorDepth = ColorDepth.Depth32Bit;
            appIconList.ImageSize = new Size(16, 16);
            appIconList.TransparentColor = Color.Transparent;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1143, 933);
            Controls.Add(rescanButton);
            Controls.Add(lockdownButton);
            Controls.Add(mainTabControl);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            FormClosing += MainForm_FormClosing;
            mainTabControl.ResumeLayout(false);
            dashboardTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            logoPictureBox.ResumeLayout(false);
            logoPictureBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)arrowPictureBox).EndInit();
            rulesTabPage.ResumeLayout(false);
            rulesTabPage.PerformLayout();
            rulesContextMenu.ResumeLayout(false);
            systemChangesTabPage.ResumeLayout(false);
            systemChangesTabPage.PerformLayout();
            auditContextMenu.ResumeLayout(false);
            groupsTabPage.ResumeLayout(false);
            groupsContextMenu.ResumeLayout(false);
            liveConnectionsTabPage.ResumeLayout(false);
            liveConnectionsContextMenu.ResumeLayout(false);
            settingsTabPage.ResumeLayout(false);
            settingsTabPage.PerformLayout();
            coffeePanel.ResumeLayout(false);
            coffeePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).EndInit();
            ResumeLayout(false);
        }
        #endregion
    }
}