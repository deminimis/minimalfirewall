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
            deleteAllRulesButton = new Button();
            revertFirewallButton = new Button();
            auditAlertsSwitch = new CheckBox();
            managePublishersButton = new Button();
            autoAllowWhitelistedPublishersCheck = new CheckBox();
            autoAllowSystemSignedAppsCheck = new CheckBox();
            autoDisableOsBlockRulesCheck = new CheckBox();
            viewTrustedCertsButton = new Button();
            excludedFoldersButton = new Button();
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
            openAppDataButton = new Button();
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
            mainSettingsPanel = new Panel();
            exportDiagnosticButton = new Button();
            importReplaceButton = new Button();
            importMergeButton = new Button();
            exportRulesButton = new Button();
            cleanUpOrphanedRulesButton = new Button();
            coffeePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).BeginInit();
            mainSettingsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // deleteAllRulesButton
            // 
            deleteAllRulesButton.FlatAppearance.BorderSize = 0;
            deleteAllRulesButton.FlatStyle = FlatStyle.Flat;
            deleteAllRulesButton.Location = new Point(245, 470);
            deleteAllRulesButton.Name = "deleteAllRulesButton";
            deleteAllRulesButton.Size = new Size(210, 28);
            deleteAllRulesButton.TabIndex = 25;
            deleteAllRulesButton.Text = "Delete all Minimal Firewall rules";
            deleteAllRulesButton.UseVisualStyleBackColor = true;
            deleteAllRulesButton.Click += deleteAllRulesButton_Click;
            // 
            // revertFirewallButton
            // 
            revertFirewallButton.FlatAppearance.BorderSize = 0;
            revertFirewallButton.FlatStyle = FlatStyle.Flat;
            revertFirewallButton.Location = new Point(465, 470);
            revertFirewallButton.Name = "revertFirewallButton";
            revertFirewallButton.Size = new Size(210, 28);
            revertFirewallButton.TabIndex = 26;
            revertFirewallButton.Text = "Revert Windows Firewall";
            revertFirewallButton.UseVisualStyleBackColor = true;
            revertFirewallButton.Click += revertFirewallButton_Click;
            // 
            // auditAlertsSwitch
            // 
            auditAlertsSwitch.AutoSize = true;
            auditAlertsSwitch.Location = new Point(306, 115);
            auditAlertsSwitch.Name = "auditAlertsSwitch";
            auditAlertsSwitch.Size = new Size(161, 19);
            auditAlertsSwitch.TabIndex = 24;
            auditAlertsSwitch.Text = "Alert on new system rules";
            auditAlertsSwitch.UseVisualStyleBackColor = true;
            // 
            // managePublishersButton
            // 
            managePublishersButton.FlatAppearance.BorderSize = 0;
            managePublishersButton.FlatStyle = FlatStyle.Flat;
            managePublishersButton.Location = new Point(306, 282);
            managePublishersButton.Name = "managePublishersButton";
            managePublishersButton.Size = new Size(225, 28);
            managePublishersButton.TabIndex = 23;
            managePublishersButton.Text = "Manage Trusted Publishers";
            managePublishersButton.UseVisualStyleBackColor = true;
            managePublishersButton.Click += managePublishersButton_Click;
            // 
            // autoAllowWhitelistedPublishersCheck
            // 
            autoAllowWhitelistedPublishersCheck.AutoSize = true;
            autoAllowWhitelistedPublishersCheck.Location = new Point(25, 285);
            autoAllowWhitelistedPublishersCheck.Name = "autoAllowWhitelistedPublishersCheck";
            autoAllowWhitelistedPublishersCheck.Size = new Size(182, 19);
            autoAllowWhitelistedPublishersCheck.TabIndex = 22;
            autoAllowWhitelistedPublishersCheck.Text = "Auto-allow trusted publishers";
            autoAllowWhitelistedPublishersCheck.UseVisualStyleBackColor = true;
            // 
            // autoAllowSystemSignedAppsCheck
            // 
            autoAllowSystemSignedAppsCheck.AutoSize = true;
            autoAllowSystemSignedAppsCheck.Location = new Point(25, 320);
            autoAllowSystemSignedAppsCheck.Name = "autoAllowSystemSignedAppsCheck";
            autoAllowSystemSignedAppsCheck.Size = new Size(221, 19);
            autoAllowSystemSignedAppsCheck.TabIndex = 36;
            autoAllowSystemSignedAppsCheck.Text = "Auto-allow apps trusted by Windows";
            autoAllowSystemSignedAppsCheck.UseVisualStyleBackColor = true;
            // 
            // autoDisableOsBlockRulesCheck
            // 
            autoDisableOsBlockRulesCheck.AutoSize = true;
            autoDisableOsBlockRulesCheck.Location = new Point(306, 150);
            autoDisableOsBlockRulesCheck.Name = "autoDisableOsBlockRulesCheck";
            autoDisableOsBlockRulesCheck.Size = new Size(218, 19);
            autoDisableOsBlockRulesCheck.TabIndex = 37;
            autoDisableOsBlockRulesCheck.Text = "Automatically disable OS block rules";
            autoDisableOsBlockRulesCheck.UseVisualStyleBackColor = true;
            // 
            // viewTrustedCertsButton
            // 
            viewTrustedCertsButton.FlatAppearance.BorderSize = 0;
            viewTrustedCertsButton.FlatStyle = FlatStyle.Flat;
            viewTrustedCertsButton.Location = new Point(306, 317);
            viewTrustedCertsButton.Name = "viewTrustedCertsButton";
            viewTrustedCertsButton.Size = new Size(225, 28);
            viewTrustedCertsButton.TabIndex = 34;
            viewTrustedCertsButton.Text = "View Trusted CAs...";
            viewTrustedCertsButton.UseVisualStyleBackColor = true;
            viewTrustedCertsButton.Click += viewTrustedCertsButton_Click;
            // 
            // excludedFoldersButton
            // 
            excludedFoldersButton.FlatAppearance.BorderSize = 0;
            excludedFoldersButton.FlatStyle = FlatStyle.Flat;
            excludedFoldersButton.Location = new Point(25, 355);
            excludedFoldersButton.Name = "excludedFoldersButton";
            excludedFoldersButton.Size = new Size(225, 28);
            excludedFoldersButton.TabIndex = 35;
            excludedFoldersButton.Text = "Auto-Allow Exclusions";
            excludedFoldersButton.UseVisualStyleBackColor = true;
            excludedFoldersButton.Click += excludedFoldersButton_Click;
            // 
            // showAppIconsSwitch
            // 
            showAppIconsSwitch.AutoSize = true;
            showAppIconsSwitch.Location = new Point(306, 80);
            showAppIconsSwitch.Name = "showAppIconsSwitch";
            showAppIconsSwitch.Size = new Size(148, 19);
            showAppIconsSwitch.TabIndex = 21;
            showAppIconsSwitch.Text = "Show application icons";
            showAppIconsSwitch.UseVisualStyleBackColor = true;
            showAppIconsSwitch.CheckedChanged += ShowAppIconsSwitch_CheckedChanged;
            // 
            // trafficMonitorSwitch
            // 
            trafficMonitorSwitch.AutoSize = true;
            trafficMonitorSwitch.Location = new Point(306, 45);
            trafficMonitorSwitch.Name = "trafficMonitorSwitch";
            trafficMonitorSwitch.Size = new Size(155, 19);
            trafficMonitorSwitch.TabIndex = 20;
            trafficMonitorSwitch.Text = "Enable Live Connections";
            trafficMonitorSwitch.UseVisualStyleBackColor = true;
            trafficMonitorSwitch.CheckedChanged += TrafficMonitorSwitch_CheckedChanged;
            // 
            // autoRefreshLabel1
            // 
            autoRefreshLabel1.AutoSize = true;
            autoRefreshLabel1.Location = new Point(25, 224);
            autoRefreshLabel1.Name = "autoRefreshLabel1";
            autoRefreshLabel1.Size = new Size(94, 15);
            autoRefreshLabel1.TabIndex = 18;
            autoRefreshLabel1.Text = "List refresh time:";
            // 
            // autoRefreshLabel2
            // 
            autoRefreshLabel2.AutoSize = true;
            autoRefreshLabel2.Location = new Point(200, 224);
            autoRefreshLabel2.Name = "autoRefreshLabel2";
            autoRefreshLabel2.Size = new Size(50, 15);
            autoRefreshLabel2.TabIndex = 19;
            autoRefreshLabel2.Text = "minutes";
            // 
            // coffeePanel
            // 
            coffeePanel.BackColor = Color.Transparent;
            coffeePanel.Controls.Add(coffeePictureBox);
            coffeePanel.Cursor = Cursors.Hand;
            coffeePanel.Location = new Point(18, 645);
            coffeePanel.Name = "coffeePanel";
            coffeePanel.Size = new Size(380, 60);
            coffeePanel.TabIndex = 17;
            coffeePanel.Click += CoffeeLink_Click;
            // 
            // coffeeLinkLabel
            // 
            coffeeLinkLabel.ActiveLinkColor = Color.DodgerBlue;
            coffeeLinkLabel.AutoSize = true;
            coffeeLinkLabel.Location = new Point(85, 665);
            coffeeLinkLabel.MaximumSize = new Size(320, 0);
            coffeeLinkLabel.Name = "coffeeLinkLabel";
            coffeeLinkLabel.Size = new Size(266, 15);
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
            coffeePictureBox.Name = "coffeePictureBox";
            coffeePictureBox.Size = new Size(54, 54);
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
            versionLabel.Location = new Point(450, 605);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(45, 15);
            versionLabel.TabIndex = 12;
            versionLabel.Text = "Version";
            // 
            // checkForUpdatesButton
            // 
            checkForUpdatesButton.FlatAppearance.BorderSize = 0;
            checkForUpdatesButton.FlatStyle = FlatStyle.Flat;
            checkForUpdatesButton.Location = new Point(245, 560);
            checkForUpdatesButton.Name = "checkForUpdatesButton";
            checkForUpdatesButton.Size = new Size(210, 28);
            checkForUpdatesButton.TabIndex = 11;
            checkForUpdatesButton.Text = "Check for Updates";
            checkForUpdatesButton.Click += CheckForUpdatesButton_Click;
            // 
            // openFirewallButton
            // 
            openFirewallButton.FlatAppearance.BorderSize = 0;
            openFirewallButton.FlatStyle = FlatStyle.Flat;
            openFirewallButton.Location = new Point(25, 425);
            openFirewallButton.Name = "openFirewallButton";
            openFirewallButton.Size = new Size(210, 28);
            openFirewallButton.TabIndex = 10;
            openFirewallButton.Text = "Open Windows Firewall";
            openFirewallButton.Click += OpenFirewallButton_Click;
            // 
            // openAppDataButton
            // 
            openAppDataButton.FlatAppearance.BorderSize = 0;
            openAppDataButton.FlatStyle = FlatStyle.Flat;
            openAppDataButton.Location = new Point(245, 425);
            openAppDataButton.Name = "openAppDataButton";
            openAppDataButton.Size = new Size(210, 28);
            openAppDataButton.TabIndex = 32;
            openAppDataButton.Text = "Open %LocalAppData%";
            openAppDataButton.UseVisualStyleBackColor = true;
            openAppDataButton.Click += openAppDataButton_Click;
            // 
            // forumLink
            // 
            forumLink.AutoSize = true;
            forumLink.Location = new Point(25, 605);
            forumLink.Name = "forumLink";
            forumLink.Size = new Size(114, 15);
            forumLink.TabIndex = 9;
            forumLink.TabStop = true;
            forumLink.Tag = "https://github.com/deminimis/minimalfirewall/discussions";
            forumLink.Text = "Forum / Discussions";
            forumLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // reportProblemLink
            // 
            reportProblemLink.AutoSize = true;
            reportProblemLink.Location = new Point(160, 605);
            reportProblemLink.Name = "reportProblemLink";
            reportProblemLink.Size = new Size(99, 15);
            reportProblemLink.TabIndex = 8;
            reportProblemLink.TabStop = true;
            reportProblemLink.Tag = "https://github.com/deminimis/minimalfirewall/issues";
            reportProblemLink.Text = "Report a Problem";
            reportProblemLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // helpLink
            // 
            helpLink.AutoSize = true;
            helpLink.Location = new Point(290, 605);
            helpLink.Name = "helpLink";
            helpLink.Size = new Size(126, 15);
            helpLink.TabIndex = 7;
            helpLink.TabStop = true;
            helpLink.Tag = "https://github.com/deminimis/minimalfirewall";
            helpLink.Text = "Help / Documentation";
            helpLink.LinkClicked += LinkLabel_LinkClicked;
            // 
            // autoRefreshTextBox
            // 
            autoRefreshTextBox.Location = new Point(136, 224);
            autoRefreshTextBox.MaxLength = 3;
            autoRefreshTextBox.Name = "autoRefreshTextBox";
            autoRefreshTextBox.Size = new Size(60, 23);
            autoRefreshTextBox.TabIndex = 5;
            autoRefreshTextBox.Text = "10";
            // 
            // loggingSwitch
            // 
            loggingSwitch.AutoSize = true;
            loggingSwitch.Location = new Point(25, 185);
            loggingSwitch.Name = "loggingSwitch";
            loggingSwitch.Size = new Size(105, 19);
            loggingSwitch.TabIndex = 4;
            loggingSwitch.Text = "Enable logging";
            loggingSwitch.UseVisualStyleBackColor = true;
            // 
            // popupsSwitch
            // 
            popupsSwitch.AutoSize = true;
            popupsSwitch.Location = new Point(25, 150);
            popupsSwitch.Name = "popupsSwitch";
            popupsSwitch.Size = new Size(173, 19);
            popupsSwitch.TabIndex = 3;
            popupsSwitch.Text = "Enable pop-up notifications";
            popupsSwitch.UseVisualStyleBackColor = true;
            popupsSwitch.CheckedChanged += PopupsSwitch_CheckedChanged;
            // 
            // darkModeSwitch
            // 
            darkModeSwitch.AutoSize = true;
            darkModeSwitch.Location = new Point(25, 115);
            darkModeSwitch.Name = "darkModeSwitch";
            darkModeSwitch.Size = new Size(84, 19);
            darkModeSwitch.TabIndex = 2;
            darkModeSwitch.Text = "Dark Mode";
            darkModeSwitch.UseVisualStyleBackColor = true;
            darkModeSwitch.CheckedChanged += DarkModeSwitch_CheckedChanged;
            // 
            // autoThemeSwitch
            // 
            autoThemeSwitch.AutoSize = true;
            autoThemeSwitch.Location = new Point(127, 115);
            autoThemeSwitch.Name = "autoThemeSwitch";
            autoThemeSwitch.Size = new Size(92, 19);
            autoThemeSwitch.TabIndex = 33;
            autoThemeSwitch.Text = "Auto Theme";
            autoThemeSwitch.UseVisualStyleBackColor = true;
            autoThemeSwitch.CheckedChanged += AutoThemeSwitch_CheckedChanged;
            // 
            // startOnStartupSwitch
            // 
            startOnStartupSwitch.AutoSize = true;
            startOnStartupSwitch.Location = new Point(25, 80);
            startOnStartupSwitch.Name = "startOnStartupSwitch";
            startOnStartupSwitch.Size = new Size(128, 19);
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
            closeToTraySwitch.Location = new Point(25, 45);
            closeToTraySwitch.Name = "closeToTraySwitch";
            closeToTraySwitch.Size = new Size(92, 19);
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
            mainSettingsPanel.Controls.Add(autoDisableOsBlockRulesCheck);
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
            mainSettingsPanel.Dock = DockStyle.Fill;
            mainSettingsPanel.Location = new Point(0, 0);
            mainSettingsPanel.Margin = new Padding(3, 2, 3, 2);
            mainSettingsPanel.Name = "mainSettingsPanel";
            mainSettingsPanel.Size = new Size(888, 694);
            mainSettingsPanel.TabIndex = 27;
            // 
            // exportDiagnosticButton
            // 
            exportDiagnosticButton.FlatAppearance.BorderSize = 0;
            exportDiagnosticButton.FlatStyle = FlatStyle.Flat;
            exportDiagnosticButton.Location = new Point(25, 560);
            exportDiagnosticButton.Name = "exportDiagnosticButton";
            exportDiagnosticButton.Size = new Size(210, 28);
            exportDiagnosticButton.TabIndex = 32;
            exportDiagnosticButton.Text = "Export Diagnostic Package...";
            exportDiagnosticButton.UseVisualStyleBackColor = true;
            exportDiagnosticButton.Click += exportDiagnosticButton_Click;
            // 
            // importReplaceButton
            // 
            importReplaceButton.FlatAppearance.BorderSize = 0;
            importReplaceButton.FlatStyle = FlatStyle.Flat;
            importReplaceButton.Location = new Point(465, 515);
            importReplaceButton.Name = "importReplaceButton";
            importReplaceButton.Size = new Size(210, 28);
            importReplaceButton.TabIndex = 30;
            importReplaceButton.Text = "Import && Replace Rules...";
            importReplaceButton.UseVisualStyleBackColor = true;
            importReplaceButton.Click += importReplaceButton_Click;
            // 
            // importMergeButton
            // 
            importMergeButton.FlatAppearance.BorderSize = 0;
            importMergeButton.FlatStyle = FlatStyle.Flat;
            importMergeButton.Location = new Point(245, 515);
            importMergeButton.Name = "importMergeButton";
            importMergeButton.Size = new Size(210, 28);
            importMergeButton.TabIndex = 29;
            importMergeButton.Text = "Import && Add Rules...";
            importMergeButton.UseVisualStyleBackColor = true;
            importMergeButton.Click += importMergeButton_Click;
            // 
            // exportRulesButton
            // 
            exportRulesButton.FlatAppearance.BorderSize = 0;
            exportRulesButton.FlatStyle = FlatStyle.Flat;
            exportRulesButton.Location = new Point(25, 515);
            exportRulesButton.Name = "exportRulesButton";
            exportRulesButton.Size = new Size(210, 28);
            exportRulesButton.TabIndex = 28;
            exportRulesButton.Text = "Export Rules...";
            exportRulesButton.UseVisualStyleBackColor = true;
            exportRulesButton.Click += exportRulesButton_Click;
            // 
            // cleanUpOrphanedRulesButton
            // 
            cleanUpOrphanedRulesButton.FlatAppearance.BorderSize = 0;
            cleanUpOrphanedRulesButton.FlatStyle = FlatStyle.Flat;
            cleanUpOrphanedRulesButton.Location = new Point(25, 470);
            cleanUpOrphanedRulesButton.Name = "cleanUpOrphanedRulesButton";
            cleanUpOrphanedRulesButton.Size = new Size(210, 28);
            cleanUpOrphanedRulesButton.TabIndex = 27;
            cleanUpOrphanedRulesButton.Text = "Clean Up Orphaned Rules";
            cleanUpOrphanedRulesButton.UseVisualStyleBackColor = true;
            cleanUpOrphanedRulesButton.Click += cleanUpOrphanedRulesButton_Click;
            // 
            // SettingsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainSettingsPanel);
            Margin = new Padding(3, 2, 3, 2);
            Name = "SettingsControl";
            Size = new Size(888, 694);
            coffeePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)coffeePictureBox).EndInit();
            mainSettingsPanel.ResumeLayout(false);
            mainSettingsPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button deleteAllRulesButton;
        private System.Windows.Forms.Button revertFirewallButton;
        private System.Windows.Forms.CheckBox auditAlertsSwitch;
        private System.Windows.Forms.Button managePublishersButton;
        private System.Windows.Forms.CheckBox autoAllowWhitelistedPublishersCheck;
        private System.Windows.Forms.CheckBox autoAllowSystemSignedAppsCheck;
        private System.Windows.Forms.CheckBox autoDisableOsBlockRulesCheck;
        private System.Windows.Forms.Button viewTrustedCertsButton;
        private System.Windows.Forms.Button excludedFoldersButton;
        private System.Windows.Forms.CheckBox showAppIconsSwitch;
        private System.Windows.Forms.CheckBox trafficMonitorSwitch;
        private System.Windows.Forms.Label autoRefreshLabel1;
        private System.Windows.Forms.Label autoRefreshLabel2;
        private System.Windows.Forms.Panel coffeePanel;
        private System.Windows.Forms.LinkLabel coffeeLinkLabel;
        private System.Windows.Forms.PictureBox coffeePictureBox;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Button checkForUpdatesButton;
        private System.Windows.Forms.Button openFirewallButton;
        private System.Windows.Forms.Button openAppDataButton;
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
        private System.Windows.Forms.Panel mainSettingsPanel;
        private System.Windows.Forms.Button cleanUpOrphanedRulesButton;
        private Button importReplaceButton;
        private Button importMergeButton;
        private Button exportRulesButton;
        private Button exportDiagnosticButton;
    }
}
