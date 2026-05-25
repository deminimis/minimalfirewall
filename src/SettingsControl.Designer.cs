namespace MinimalFirewall
{
    partial class SettingsControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            deleteAllRulesButton = new DarkModeForms.ThemedButton();
            revertFirewallButton = new DarkModeForms.ThemedButton();
            auditAlertsSwitch = new CheckBox();
            managePublishersButton = new DarkModeForms.ThemedButton();
            autoAllowWhitelistedPublishersCheck = new CheckBox();
            autoAllowSystemSignedAppsCheck = new CheckBox();
            viewTrustedCertsButton = new DarkModeForms.ThemedButton();
            excludedFoldersButton = new DarkModeForms.ThemedButton();
            showAppIconsSwitch = new CheckBox();
            trafficMonitorSwitch = new CheckBox();
            autoRefreshLabel1 = new DarkModeForms.ThemedLabel();
            autoRefreshLabel2 = new DarkModeForms.ThemedLabel();
            coffeePanel = new DarkModeForms.ThemedPanel();
            coffeePictureBox = new PictureBox();
            coffeeLinkLabel = new LinkLabel();
            versionLabel = new DarkModeForms.ThemedLabel();
            checkForUpdatesButton = new DarkModeForms.ThemedButton();
            openFirewallButton = new DarkModeForms.ThemedButton();
            openAppDataButton = new DarkModeForms.ThemedButton();
            forumLink = new LinkLabel();
            reportProblemLink = new LinkLabel();
            helpLink = new LinkLabel();
            autoRefreshTextBox = new TextBox();
            loggingSwitch = new CheckBox();
            popupsSwitch = new CheckBox();
            darkModeSwitch = new CheckBox();
            autoThemeSwitch = new CheckBox();
            startOnStartupSwitch = new CheckBox();
            closeToTraySwitch = new CheckBox();
            mainSettingsPanel = new DarkModeForms.ThemedPanel();
            exportDiagnosticButton = new DarkModeForms.ThemedButton();
            importReplaceButton = new DarkModeForms.ThemedButton();
            importMergeButton = new DarkModeForms.ThemedButton();
            exportRulesButton = new DarkModeForms.ThemedButton();
            cleanUpOrphanedRulesButton = new DarkModeForms.ThemedButton();
            dividerPanel1 = new DarkModeForms.ThemedPanel();
            dividerPanel2 = new DarkModeForms.ThemedPanel();
            coffeePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).BeginInit();
            mainSettingsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // deleteAllRulesButton
            // 
            deleteAllRulesButton.FlatAppearance.BorderSize = 0;
            deleteAllRulesButton.FlatStyle = FlatStyle.Flat;
            deleteAllRulesButton.Location = new Point(245, 501);
            deleteAllRulesButton.Margin = new Padding(4, 3, 4, 3);
            deleteAllRulesButton.Name = "deleteAllRulesButton";
            deleteAllRulesButton.Size = new Size(210, 30);
            deleteAllRulesButton.TabIndex = 25;
            deleteAllRulesButton.Text = "Delete all Minimal Firewall rules";
            deleteAllRulesButton.UseVisualStyleBackColor = true;
            deleteAllRulesButton.Click += DeleteAllRulesButton_Click;
            // 
            // revertFirewallButton
            // 
            revertFirewallButton.FlatAppearance.BorderSize = 0;
            revertFirewallButton.FlatStyle = FlatStyle.Flat;
            revertFirewallButton.Location = new Point(465, 501);
            revertFirewallButton.Margin = new Padding(4, 3, 4, 3);
            revertFirewallButton.Name = "revertFirewallButton";
            revertFirewallButton.Size = new Size(210, 30);
            revertFirewallButton.TabIndex = 26;
            revertFirewallButton.Text = "Revert Windows Firewall";
            revertFirewallButton.UseVisualStyleBackColor = true;
            revertFirewallButton.Click += RevertFirewallButton_Click;
            // 
            // auditAlertsSwitch
            // 
            auditAlertsSwitch.AutoSize = true;
            auditAlertsSwitch.Location = new Point(306, 123);
            auditAlertsSwitch.Margin = new Padding(4, 3, 4, 3);
            auditAlertsSwitch.Name = "auditAlertsSwitch";
            auditAlertsSwitch.Size = new Size(201, 20);
            auditAlertsSwitch.TabIndex = 24;
            auditAlertsSwitch.Text = "Alert on new system rules";
            auditAlertsSwitch.UseVisualStyleBackColor = true;
            // 
            // managePublishersButton
            // 
            managePublishersButton.FlatAppearance.BorderSize = 0;
            managePublishersButton.FlatStyle = FlatStyle.Flat;
            managePublishersButton.Location = new Point(306, 301);
            managePublishersButton.Margin = new Padding(4, 3, 4, 3);
            managePublishersButton.Name = "managePublishersButton";
            managePublishersButton.Size = new Size(225, 30);
            managePublishersButton.TabIndex = 23;
            managePublishersButton.Text = "Manage Trusted Publishers";
            managePublishersButton.UseVisualStyleBackColor = true;
            managePublishersButton.Click += ManagePublishersButton_Click;
            // 
            // autoAllowWhitelistedPublishersCheck
            // 
            autoAllowWhitelistedPublishersCheck.AutoSize = true;
            autoAllowWhitelistedPublishersCheck.Location = new Point(24, 304);
            autoAllowWhitelistedPublishersCheck.Margin = new Padding(4, 3, 4, 3);
            autoAllowWhitelistedPublishersCheck.Name = "autoAllowWhitelistedPublishersCheck";
            autoAllowWhitelistedPublishersCheck.Size = new Size(229, 20);
            autoAllowWhitelistedPublishersCheck.TabIndex = 22;
            autoAllowWhitelistedPublishersCheck.Text = "Auto-allow trusted publishers";
            autoAllowWhitelistedPublishersCheck.UseVisualStyleBackColor = true;
            // 
            // autoAllowSystemSignedAppsCheck
            // 
            autoAllowSystemSignedAppsCheck.AutoSize = true;
            autoAllowSystemSignedAppsCheck.Location = new Point(24, 341);
            autoAllowSystemSignedAppsCheck.Margin = new Padding(4, 3, 4, 3);
            autoAllowSystemSignedAppsCheck.Name = "autoAllowSystemSignedAppsCheck";
            autoAllowSystemSignedAppsCheck.Size = new Size(264, 20);
            autoAllowSystemSignedAppsCheck.TabIndex = 36;
            autoAllowSystemSignedAppsCheck.Text = "Auto-allow apps trusted by Windows";
            autoAllowSystemSignedAppsCheck.UseVisualStyleBackColor = true;
            // 
            // viewTrustedCertsButton
            // 
            viewTrustedCertsButton.FlatAppearance.BorderSize = 0;
            viewTrustedCertsButton.FlatStyle = FlatStyle.Flat;
            viewTrustedCertsButton.Location = new Point(306, 338);
            viewTrustedCertsButton.Margin = new Padding(4, 3, 4, 3);
            viewTrustedCertsButton.Name = "viewTrustedCertsButton";
            viewTrustedCertsButton.Size = new Size(225, 30);
            viewTrustedCertsButton.TabIndex = 34;
            viewTrustedCertsButton.Text = "View Trusted CAs...";
            viewTrustedCertsButton.UseVisualStyleBackColor = true;
            viewTrustedCertsButton.Click += ViewTrustedCertsButton_Click;
            // 
            // excludedFoldersButton
            // 
            excludedFoldersButton.FlatAppearance.BorderSize = 0;
            excludedFoldersButton.FlatStyle = FlatStyle.Flat;
            excludedFoldersButton.Location = new Point(24, 379);
            excludedFoldersButton.Margin = new Padding(4, 3, 4, 3);
            excludedFoldersButton.Name = "excludedFoldersButton";
            excludedFoldersButton.Size = new Size(225, 30);
            excludedFoldersButton.TabIndex = 35;
            excludedFoldersButton.Text = "Auto-Allow Exclusions";
            excludedFoldersButton.UseVisualStyleBackColor = true;
            excludedFoldersButton.Click += ExcludedFoldersButton_Click;
            // 
            // showAppIconsSwitch
            // 
            showAppIconsSwitch.AutoSize = true;
            showAppIconsSwitch.Location = new Point(306, 85);
            showAppIconsSwitch.Margin = new Padding(4, 3, 4, 3);
            showAppIconsSwitch.Name = "showAppIconsSwitch";
            showAppIconsSwitch.Size = new Size(180, 20);
            showAppIconsSwitch.TabIndex = 21;
            showAppIconsSwitch.Text = "Show application icons";
            showAppIconsSwitch.UseVisualStyleBackColor = true;
            showAppIconsSwitch.CheckedChanged += ShowAppIconsSwitch_CheckedChanged;
            // 
            // trafficMonitorSwitch
            // 
            trafficMonitorSwitch.AutoSize = true;
            trafficMonitorSwitch.Location = new Point(306, 48);
            trafficMonitorSwitch.Margin = new Padding(4, 3, 4, 3);
            trafficMonitorSwitch.Name = "trafficMonitorSwitch";
            trafficMonitorSwitch.Size = new Size(187, 20);
            trafficMonitorSwitch.TabIndex = 20;
            trafficMonitorSwitch.Text = "Enable Live Connections";
            trafficMonitorSwitch.UseVisualStyleBackColor = true;
            trafficMonitorSwitch.CheckedChanged += TrafficMonitorSwitch_CheckedChanged;
            // 
            // autoRefreshLabel1
            // 
            autoRefreshLabel1.AutoSize = true;
            autoRefreshLabel1.Location = new Point(24, 239);
            autoRefreshLabel1.Margin = new Padding(4, 0, 4, 0);
            autoRefreshLabel1.Name = "autoRefreshLabel1";
            autoRefreshLabel1.Size = new Size(133, 16);
            autoRefreshLabel1.TabIndex = 18;
            autoRefreshLabel1.Text = "List refresh time:";
            // 
            // autoRefreshLabel2
            // 
            autoRefreshLabel2.AutoSize = true;
            autoRefreshLabel2.Location = new Point(230, 239);
            autoRefreshLabel2.Margin = new Padding(4, 0, 4, 0);
            autoRefreshLabel2.Name = "autoRefreshLabel2";
            autoRefreshLabel2.Size = new Size(56, 16);
            autoRefreshLabel2.TabIndex = 19;
            autoRefreshLabel2.Text = "minutes";
            // 
            // coffeePanel
            // 
            coffeePanel.BackColor = Color.Transparent;
            coffeePanel.Controls.Add(coffeePictureBox);
            coffeePanel.Cursor = Cursors.Hand;
            coffeePanel.Location = new Point(18, 688);
            coffeePanel.Margin = new Padding(4, 3, 4, 3);
            coffeePanel.Name = "coffeePanel";
            coffeePanel.Size = new Size(380, 64);
            coffeePanel.TabIndex = 17;
            coffeePanel.Click += CoffeeLink_Click;
            // 
            // coffeePictureBox
            // 
            coffeePictureBox.Cursor = Cursors.Hand;
            coffeePictureBox.Location = new Point(0, 0);
            coffeePictureBox.Margin = new Padding(4, 3, 4, 3);
            coffeePictureBox.Name = "coffeePictureBox";
            coffeePictureBox.Size = new Size(54, 58);
            coffeePictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            coffeePictureBox.TabIndex = 13;
            coffeePictureBox.TabStop = false;
            coffeePictureBox.Click += CoffeeLink_Click;
            coffeePictureBox.MouseEnter += CoffeePictureBox_MouseEnter;
            coffeePictureBox.MouseLeave += CoffeePictureBox_MouseLeave;
            // 
            // coffeeLinkLabel
            // 
            coffeeLinkLabel.ActiveLinkColor = Color.DodgerBlue;
            coffeeLinkLabel.AutoSize = true;
            coffeeLinkLabel.Location = new Point(85, 709);
            coffeeLinkLabel.Margin = new Padding(4, 0, 4, 0);
            coffeeLinkLabel.MaximumSize = new Size(320, 0);
            coffeeLinkLabel.Name = "coffeeLinkLabel";
            coffeeLinkLabel.Size = new Size(301, 32);
            coffeeLinkLabel.TabIndex = 15;
            coffeeLinkLabel.TabStop = true;
            coffeeLinkLabel.Tag = "https://www.buymeacoffee.com/deminimis";
            coffeeLinkLabel.Text = "Support my caffeine addiction if you like this app";
            coffeeLinkLabel.Click += CoffeeLink_Click;
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Font = new Font("Segoe UI", 9F);
            versionLabel.Location = new Point(450, 645);
            versionLabel.Margin = new Padding(4, 0, 4, 0);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(45, 15);
            versionLabel.TabIndex = 12;
            versionLabel.Text = "Version";
            // 
            // checkForUpdatesButton
            // 
            checkForUpdatesButton.FlatAppearance.BorderSize = 0;
            checkForUpdatesButton.FlatStyle = FlatStyle.Flat;
            checkForUpdatesButton.Location = new Point(245, 597);
            checkForUpdatesButton.Margin = new Padding(4, 3, 4, 3);
            checkForUpdatesButton.Name = "checkForUpdatesButton";
            checkForUpdatesButton.Size = new Size(210, 30);
            checkForUpdatesButton.TabIndex = 11;
            checkForUpdatesButton.Text = "Check for Updates";
            checkForUpdatesButton.Click += CheckForUpdatesButton_Click;
            // 
            // openFirewallButton
            // 
            openFirewallButton.FlatAppearance.BorderSize = 0;
            openFirewallButton.FlatStyle = FlatStyle.Flat;
            openFirewallButton.Location = new Point(24, 453);
            openFirewallButton.Margin = new Padding(4, 3, 4, 3);
            openFirewallButton.Name = "openFirewallButton";
            openFirewallButton.Size = new Size(210, 30);
            openFirewallButton.TabIndex = 10;
            openFirewallButton.Text = "Open Windows Firewall";
            openFirewallButton.Click += OpenFirewallButton_Click;
            // 
            // openAppDataButton
            // 
            openAppDataButton.FlatAppearance.BorderSize = 0;
            openAppDataButton.FlatStyle = FlatStyle.Flat;
            openAppDataButton.Location = new Point(245, 453);
            openAppDataButton.Margin = new Padding(4, 3, 4, 3);
            openAppDataButton.Name = "openAppDataButton";
            openAppDataButton.Size = new Size(210, 30);
            openAppDataButton.TabIndex = 32;
            openAppDataButton.Text = "Open %LocalAppData%";
            openAppDataButton.UseVisualStyleBackColor = true;
            openAppDataButton.Click += OpenAppDataButton_Click;
            // 
            // forumLink
            // 
            forumLink.AutoSize = true;
            forumLink.Location = new Point(24, 645);
            forumLink.Margin = new Padding(4, 0, 4, 0);
            forumLink.Name = "forumLink";
            forumLink.Size = new Size(140, 16);
            forumLink.TabIndex = 9;
            forumLink.TabStop = true;
            forumLink.Tag = "https://github.com/deminimis/minimalfirewall/discussions";
            forumLink.Text = "Forum / Discussions";
            forumLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // reportProblemLink
            // 
            reportProblemLink.AutoSize = true;
            reportProblemLink.Location = new Point(160, 645);
            reportProblemLink.Margin = new Padding(4, 0, 4, 0);
            reportProblemLink.Name = "reportProblemLink";
            reportProblemLink.Size = new Size(119, 16);
            reportProblemLink.TabIndex = 8;
            reportProblemLink.TabStop = true;
            reportProblemLink.Tag = "https://github.com/deminimis/minimalfirewall/issues";
            reportProblemLink.Text = "Report a Problem";
            reportProblemLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // helpLink
            // 
            helpLink.AutoSize = true;
            helpLink.Location = new Point(290, 645);
            helpLink.Margin = new Padding(4, 0, 4, 0);
            helpLink.Name = "helpLink";
            helpLink.Size = new Size(147, 16);
            helpLink.TabIndex = 7;
            helpLink.TabStop = true;
            helpLink.Tag = "https://github.com/deminimis/minimalfirewall";
            helpLink.Text = "Help / Documentation";
            helpLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // autoRefreshTextBox
            // 
            autoRefreshTextBox.Location = new Point(166, 235);
            autoRefreshTextBox.Margin = new Padding(4, 3, 4, 3);
            autoRefreshTextBox.MaxLength = 3;
            autoRefreshTextBox.Name = "autoRefreshTextBox";
            autoRefreshTextBox.Size = new Size(60, 23);
            autoRefreshTextBox.TabIndex = 5;
            autoRefreshTextBox.Text = "10";
            // 
            // loggingSwitch
            // 
            loggingSwitch.AutoSize = true;
            loggingSwitch.Location = new Point(24, 197);
            loggingSwitch.Margin = new Padding(4, 3, 4, 3);
            loggingSwitch.Name = "loggingSwitch";
            loggingSwitch.Size = new Size(124, 20);
            loggingSwitch.TabIndex = 4;
            loggingSwitch.Text = "Enable logging";
            loggingSwitch.UseVisualStyleBackColor = true;
            // 
            // popupsSwitch
            // 
            popupsSwitch.AutoSize = true;
            popupsSwitch.Location = new Point(24, 160);
            popupsSwitch.Margin = new Padding(4, 3, 4, 3);
            popupsSwitch.Name = "popupsSwitch";
            popupsSwitch.Size = new Size(215, 20);
            popupsSwitch.TabIndex = 3;
            popupsSwitch.Text = "Enable pop-up notifications";
            popupsSwitch.UseVisualStyleBackColor = true;
            popupsSwitch.CheckedChanged += PopupsSwitch_CheckedChanged;
            // 
            // darkModeSwitch
            // 
            darkModeSwitch.AutoSize = true;
            darkModeSwitch.Location = new Point(24, 123);
            darkModeSwitch.Margin = new Padding(4, 3, 4, 3);
            darkModeSwitch.Name = "darkModeSwitch";
            darkModeSwitch.Size = new Size(89, 20);
            darkModeSwitch.TabIndex = 2;
            darkModeSwitch.Text = "Dark Mode";
            darkModeSwitch.UseVisualStyleBackColor = true;
            darkModeSwitch.CheckedChanged += DarkModeSwitch_CheckedChanged;
            // 
            // autoThemeSwitch
            // 
            autoThemeSwitch.AutoSize = true;
            autoThemeSwitch.Location = new Point(127, 123);
            autoThemeSwitch.Margin = new Padding(4, 3, 4, 3);
            autoThemeSwitch.Name = "autoThemeSwitch";
            autoThemeSwitch.Size = new Size(96, 20);
            autoThemeSwitch.TabIndex = 33;
            autoThemeSwitch.Text = "Auto Theme";
            autoThemeSwitch.UseVisualStyleBackColor = true;
            autoThemeSwitch.CheckedChanged += AutoThemeSwitch_CheckedChanged;
            // 
            // startOnStartupSwitch
            // 
            startOnStartupSwitch.AutoSize = true;
            startOnStartupSwitch.Location = new Point(24, 85);
            startOnStartupSwitch.Margin = new Padding(4, 3, 4, 3);
            startOnStartupSwitch.Name = "startOnStartupSwitch";
            startOnStartupSwitch.Size = new Size(152, 20);
            startOnStartupSwitch.TabIndex = 1;
            startOnStartupSwitch.Text = "Start with Windows";
            startOnStartupSwitch.UseVisualStyleBackColor = true;
            startOnStartupSwitch.CheckedChanged += StartOnStartupSwitch_CheckedChanged;
            // 
            // closeToTraySwitch
            // 
            closeToTraySwitch.AutoSize = true;
            closeToTraySwitch.Checked = true;
            closeToTraySwitch.CheckState = CheckState.Checked;
            closeToTraySwitch.Location = new Point(24, 48);
            closeToTraySwitch.Margin = new Padding(4, 3, 4, 3);
            closeToTraySwitch.Name = "closeToTraySwitch";
            closeToTraySwitch.Size = new Size(117, 20);
            closeToTraySwitch.TabIndex = 0;
            closeToTraySwitch.Text = "Close to tray";
            closeToTraySwitch.UseVisualStyleBackColor = true;
            // 
            // mainSettingsPanel
            // 
            mainSettingsPanel.AutoScroll = true;
            mainSettingsPanel.Controls.Add(coffeeLinkLabel);
            mainSettingsPanel.Controls.Add(exportDiagnosticButton);
            mainSettingsPanel.Controls.Add(importReplaceButton);
            mainSettingsPanel.Controls.Add(importMergeButton);
            mainSettingsPanel.Controls.Add(exportRulesButton);
            mainSettingsPanel.Controls.Add(cleanUpOrphanedRulesButton);
            mainSettingsPanel.Controls.Add(deleteAllRulesButton);
            mainSettingsPanel.Controls.Add(revertFirewallButton);
            mainSettingsPanel.Controls.Add(auditAlertsSwitch);
            mainSettingsPanel.Controls.Add(managePublishersButton);
            mainSettingsPanel.Controls.Add(autoAllowWhitelistedPublishersCheck);
            mainSettingsPanel.Controls.Add(autoAllowSystemSignedAppsCheck);
            mainSettingsPanel.Controls.Add(viewTrustedCertsButton);
            mainSettingsPanel.Controls.Add(excludedFoldersButton);
            mainSettingsPanel.Controls.Add(showAppIconsSwitch);
            mainSettingsPanel.Controls.Add(trafficMonitorSwitch);
            mainSettingsPanel.Controls.Add(autoRefreshLabel1);
            mainSettingsPanel.Controls.Add(autoRefreshLabel2);
            mainSettingsPanel.Controls.Add(coffeePanel);
            mainSettingsPanel.Controls.Add(versionLabel);
            mainSettingsPanel.Controls.Add(checkForUpdatesButton);
            mainSettingsPanel.Controls.Add(openFirewallButton);
            mainSettingsPanel.Controls.Add(openAppDataButton);
            mainSettingsPanel.Controls.Add(forumLink);
            mainSettingsPanel.Controls.Add(reportProblemLink);
            mainSettingsPanel.Controls.Add(helpLink);
            mainSettingsPanel.Controls.Add(autoRefreshTextBox);
            mainSettingsPanel.Controls.Add(loggingSwitch);
            mainSettingsPanel.Controls.Add(popupsSwitch);
            mainSettingsPanel.Controls.Add(darkModeSwitch);
            mainSettingsPanel.Controls.Add(autoThemeSwitch);
            mainSettingsPanel.Controls.Add(startOnStartupSwitch);
            mainSettingsPanel.Controls.Add(closeToTraySwitch);
            mainSettingsPanel.Controls.Add(dividerPanel1);
            mainSettingsPanel.Controls.Add(dividerPanel2);
            mainSettingsPanel.Dock = DockStyle.Fill;
            mainSettingsPanel.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            mainSettingsPanel.Location = new Point(0, 0);
            mainSettingsPanel.Margin = new Padding(4, 2, 4, 2);
            mainSettingsPanel.Name = "mainSettingsPanel";
            mainSettingsPanel.Size = new Size(888, 740);
            mainSettingsPanel.TabIndex = 27;
            // 
            // exportDiagnosticButton
            // 
            exportDiagnosticButton.FlatAppearance.BorderSize = 0;
            exportDiagnosticButton.FlatStyle = FlatStyle.Flat;
            exportDiagnosticButton.Location = new Point(24, 597);
            exportDiagnosticButton.Margin = new Padding(4, 3, 4, 3);
            exportDiagnosticButton.Name = "exportDiagnosticButton";
            exportDiagnosticButton.Size = new Size(210, 30);
            exportDiagnosticButton.TabIndex = 32;
            exportDiagnosticButton.Text = "Export Diagnostic Package...";
            exportDiagnosticButton.UseVisualStyleBackColor = true;
            exportDiagnosticButton.Click += ExportDiagnosticButton_Click;
            // 
            // importReplaceButton
            // 
            importReplaceButton.FlatAppearance.BorderSize = 0;
            importReplaceButton.FlatStyle = FlatStyle.Flat;
            importReplaceButton.Location = new Point(465, 549);
            importReplaceButton.Margin = new Padding(4, 3, 4, 3);
            importReplaceButton.Name = "importReplaceButton";
            importReplaceButton.Size = new Size(210, 30);
            importReplaceButton.TabIndex = 30;
            importReplaceButton.Text = "Import && Replace Rules...";
            importReplaceButton.UseVisualStyleBackColor = true;
            importReplaceButton.Click += ImportReplaceButton_Click;
            // 
            // importMergeButton
            // 
            importMergeButton.FlatAppearance.BorderSize = 0;
            importMergeButton.FlatStyle = FlatStyle.Flat;
            importMergeButton.Location = new Point(245, 549);
            importMergeButton.Margin = new Padding(4, 3, 4, 3);
            importMergeButton.Name = "importMergeButton";
            importMergeButton.Size = new Size(210, 30);
            importMergeButton.TabIndex = 29;
            importMergeButton.Text = "Import && Add Rules...";
            importMergeButton.UseVisualStyleBackColor = true;
            importMergeButton.Click += ImportMergeButton_Click;
            // 
            // exportRulesButton
            // 
            exportRulesButton.FlatAppearance.BorderSize = 0;
            exportRulesButton.FlatStyle = FlatStyle.Flat;
            exportRulesButton.Location = new Point(24, 549);
            exportRulesButton.Margin = new Padding(4, 3, 4, 3);
            exportRulesButton.Name = "exportRulesButton";
            exportRulesButton.Size = new Size(210, 30);
            exportRulesButton.TabIndex = 28;
            exportRulesButton.Text = "Export Rules...";
            exportRulesButton.UseVisualStyleBackColor = true;
            exportRulesButton.Click += ExportRulesButton_Click;
            // 
            // cleanUpOrphanedRulesButton
            // 
            cleanUpOrphanedRulesButton.FlatAppearance.BorderSize = 0;
            cleanUpOrphanedRulesButton.FlatStyle = FlatStyle.Flat;
            cleanUpOrphanedRulesButton.Location = new Point(24, 501);
            cleanUpOrphanedRulesButton.Margin = new Padding(4, 3, 4, 3);
            cleanUpOrphanedRulesButton.Name = "cleanUpOrphanedRulesButton";
            cleanUpOrphanedRulesButton.Size = new Size(210, 30);
            cleanUpOrphanedRulesButton.TabIndex = 27;
            cleanUpOrphanedRulesButton.Text = "Clean Up Orphaned Rules";
            cleanUpOrphanedRulesButton.UseVisualStyleBackColor = true;
            cleanUpOrphanedRulesButton.Click += CleanUpOrphanedRulesButton_Click;
            // 
            // dividerPanel1
            // 
            dividerPanel1.BackColor = SystemColors.ControlDark;
            dividerPanel1.Location = new Point(24, 283);
            dividerPanel1.Margin = new Padding(4, 3, 4, 3);
            dividerPanel1.Name = "dividerPanel1";
            dividerPanel1.Size = new Size(650, 1);
            dividerPanel1.TabIndex = 37;
            // 
            // dividerPanel2
            // 
            dividerPanel2.BackColor = SystemColors.ControlDark;
            dividerPanel2.Location = new Point(24, 432);
            dividerPanel2.Margin = new Padding(4, 3, 4, 3);
            dividerPanel2.Name = "dividerPanel2";
            dividerPanel2.Size = new Size(650, 1);
            dividerPanel2.TabIndex = 38;
            // 
            // SettingsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainSettingsPanel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 2, 4, 2);
            Name = "SettingsControl";
            Size = new Size(888, 740);
            coffeePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).EndInit();
            mainSettingsPanel.ResumeLayout(false);
            mainSettingsPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
        private DarkModeForms.ThemedButton deleteAllRulesButton;
        private DarkModeForms.ThemedButton revertFirewallButton;
        private System.Windows.Forms.CheckBox auditAlertsSwitch;
        private DarkModeForms.ThemedButton managePublishersButton;
        private System.Windows.Forms.CheckBox autoAllowWhitelistedPublishersCheck;
        private System.Windows.Forms.CheckBox autoAllowSystemSignedAppsCheck;
        private DarkModeForms.ThemedButton viewTrustedCertsButton;
        private DarkModeForms.ThemedButton excludedFoldersButton;
        private System.Windows.Forms.CheckBox showAppIconsSwitch;
        private System.Windows.Forms.CheckBox trafficMonitorSwitch;
        private DarkModeForms.ThemedLabel autoRefreshLabel1;
        private DarkModeForms.ThemedLabel autoRefreshLabel2;
        private DarkModeForms.ThemedPanel coffeePanel;
        private System.Windows.Forms.LinkLabel coffeeLinkLabel;
        private System.Windows.Forms.PictureBox coffeePictureBox;
        private DarkModeForms.ThemedLabel versionLabel;
        private DarkModeForms.ThemedButton checkForUpdatesButton;
        private DarkModeForms.ThemedButton openFirewallButton;
        private DarkModeForms.ThemedButton openAppDataButton;
        private System.Windows.Forms.LinkLabel forumLink;
        private System.Windows.Forms.LinkLabel reportProblemLink;
        private System.Windows.Forms.LinkLabel helpLink;
        private System.Windows.Forms.TextBox autoRefreshTextBox;
        private System.Windows.Forms.CheckBox loggingSwitch;
        private System.Windows.Forms.CheckBox popupsSwitch;
        private System.Windows.Forms.CheckBox darkModeSwitch;
        private System.Windows.Forms.CheckBox autoThemeSwitch;
        private System.Windows.Forms.CheckBox startOnStartupSwitch;
        private System.Windows.Forms.CheckBox closeToTraySwitch;
        private DarkModeForms.ThemedPanel mainSettingsPanel;
        private DarkModeForms.ThemedButton cleanUpOrphanedRulesButton;
        private DarkModeForms.ThemedButton importReplaceButton;
        private DarkModeForms.ThemedButton importMergeButton;
        private DarkModeForms.ThemedButton exportRulesButton;
        private DarkModeForms.ThemedButton exportDiagnosticButton;
        private DarkModeForms.ThemedPanel dividerPanel1;
        private DarkModeForms.ThemedPanel dividerPanel2;
    }
}
