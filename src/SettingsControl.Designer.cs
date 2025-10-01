// File: C:/Users/anon/PROGRAMMING/C#/SimpleFirewall/VS Minimal Firewall/MinimalFirewall-NET8/MinimalFirewall-WindowsStore/SettingsControl.Designer.cs
namespace MinimalFirewall
{
    partial class SettingsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.deleteAllRulesButton = new System.Windows.Forms.Button();
            this.revertFirewallButton = new System.Windows.Forms.Button();
            this.auditAlertsSwitch = new System.Windows.Forms.CheckBox();
            this.managePublishersButton = new System.Windows.Forms.Button();
            this.autoAllowSystemTrustedCheck = new System.Windows.Forms.CheckBox();
            this.showAppIconsSwitch = new System.Windows.Forms.CheckBox();
            this.trafficMonitorSwitch = new System.Windows.Forms.CheckBox();
            this.autoRefreshLabel1 = new System.Windows.Forms.Label();
            this.autoRefreshLabel2 = new System.Windows.Forms.Label();
            this.coffeePanel = new System.Windows.Forms.Panel();
            this.coffeeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.coffeePictureBox = new System.Windows.Forms.PictureBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.checkForUpdatesButton = new System.Windows.Forms.Button();
            this.openFirewallButton = new System.Windows.Forms.Button();
            this.forumLink = new System.Windows.Forms.LinkLabel();
            this.reportProblemLink = new System.Windows.Forms.LinkLabel();
            this.helpLink = new System.Windows.Forms.LinkLabel();
            this.autoRefreshTextBox = new System.Windows.Forms.TextBox();
            this.loggingSwitch = new System.Windows.Forms.CheckBox();
            this.popupsSwitch = new System.Windows.Forms.CheckBox();
            this.darkModeSwitch = new System.Windows.Forms.CheckBox();
            this.startOnStartupSwitch = new System.Windows.Forms.CheckBox();
            this.closeToTraySwitch = new System.Windows.Forms.CheckBox();
            this.mainSettingsPanel = new System.Windows.Forms.Panel();
            this.coffeePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coffeePictureBox)).BeginInit();
            this.mainSettingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // deleteAllRulesButton
            // 
            this.deleteAllRulesButton.FlatAppearance.BorderSize = 0;
            this.deleteAllRulesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteAllRulesButton.Location = new System.Drawing.Point(29, 700);
            this.deleteAllRulesButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.deleteAllRulesButton.Name = "deleteAllRulesButton";
            this.deleteAllRulesButton.Size = new System.Drawing.Size(240, 37);
            this.deleteAllRulesButton.TabIndex = 25;
            this.deleteAllRulesButton.Text = "Delete all Minimal Firewall rules";
            this.deleteAllRulesButton.UseVisualStyleBackColor = true;
            this.deleteAllRulesButton.Click += new System.EventHandler(this.deleteAllRulesButton_Click);
            // 
            // revertFirewallButton
            // 
            this.revertFirewallButton.FlatAppearance.BorderSize = 0;
            this.revertFirewallButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.revertFirewallButton.Location = new System.Drawing.Point(280, 700);
            this.revertFirewallButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.revertFirewallButton.Name = "revertFirewallButton";
            this.revertFirewallButton.Size = new System.Drawing.Size(240, 37);
            this.revertFirewallButton.TabIndex = 26;
            this.revertFirewallButton.Text = "Revert Windows Firewall";
            this.revertFirewallButton.UseVisualStyleBackColor = true;
            this.revertFirewallButton.Click += new System.EventHandler(this.revertFirewallButton_Click);
            // 
            // auditAlertsSwitch
            // 
            this.auditAlertsSwitch.AutoSize = true;
            this.auditAlertsSwitch.Location = new System.Drawing.Point(350, 153);
            this.auditAlertsSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.auditAlertsSwitch.Name = "auditAlertsSwitch";
            this.auditAlertsSwitch.Size = new System.Drawing.Size(211, 24);
            this.auditAlertsSwitch.TabIndex = 24;
            this.auditAlertsSwitch.Text = "Alert on new system rules";
            this.auditAlertsSwitch.UseVisualStyleBackColor = true;
            // 
            // managePublishersButton
            // 
            this.managePublishersButton.FlatAppearance.BorderSize = 0;
            this.managePublishersButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.managePublishersButton.Location = new System.Drawing.Point(29, 350);
            this.managePublishersButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.managePublishersButton.Name = "managePublishersButton";
            this.managePublishersButton.Size = new System.Drawing.Size(200, 37);
            this.managePublishersButton.TabIndex = 23;
            this.managePublishersButton.Text = "Manage Trusted Publishers";
            this.managePublishersButton.UseVisualStyleBackColor = true;
            this.managePublishersButton.Click += new System.EventHandler(this.managePublishersButton_Click);
            // 
            // autoAllowSystemTrustedCheck
            // 
            this.autoAllowSystemTrustedCheck.AutoSize = true;
            this.autoAllowSystemTrustedCheck.Location = new System.Drawing.Point(350, 200);
            this.autoAllowSystemTrustedCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.autoAllowSystemTrustedCheck.Name = "autoAllowSystemTrustedCheck";
            this.autoAllowSystemTrustedCheck.Size = new System.Drawing.Size(276, 24);
            this.autoAllowSystemTrustedCheck.TabIndex = 22;
            this.autoAllowSystemTrustedCheck.Text = "Auto-allow apps trusted by Windows";
            this.autoAllowSystemTrustedCheck.UseVisualStyleBackColor = true;
            // 
            // showAppIconsSwitch
            // 
            this.showAppIconsSwitch.AutoSize = true;
            this.showAppIconsSwitch.Location = new System.Drawing.Point(350, 107);
            this.showAppIconsSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.showAppIconsSwitch.Name = "showAppIconsSwitch";
            this.showAppIconsSwitch.Size = new System.Drawing.Size(177, 24);
            this.showAppIconsSwitch.TabIndex = 21;
            this.showAppIconsSwitch.Text = "Show application icons";
            this.showAppIconsSwitch.UseVisualStyleBackColor = true;
            this.showAppIconsSwitch.CheckedChanged += new System.EventHandler(this.ShowAppIconsSwitch_CheckedChanged);
            // 
            // trafficMonitorSwitch
            // 
            this.trafficMonitorSwitch.AutoSize = true;
            this.trafficMonitorSwitch.Location = new System.Drawing.Point(350, 60);
            this.trafficMonitorSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.trafficMonitorSwitch.Name = "trafficMonitorSwitch";
            this.trafficMonitorSwitch.Size = new System.Drawing.Size(191, 24);
            this.trafficMonitorSwitch.TabIndex = 20;
            this.trafficMonitorSwitch.Text = "Enable Live Connections";
            this.trafficMonitorSwitch.UseVisualStyleBackColor = true;
            this.trafficMonitorSwitch.CheckedChanged += new System.EventHandler(this.TrafficMonitorSwitch_CheckedChanged);
            // 
            // autoRefreshLabel1
            // 
            this.autoRefreshLabel1.AutoSize = true;
            this.autoRefreshLabel1.Location = new System.Drawing.Point(29, 299);
            this.autoRefreshLabel1.Name = "autoRefreshLabel1";
            this.autoRefreshLabel1.Size = new System.Drawing.Size(117, 20);
            this.autoRefreshLabel1.TabIndex = 18;
            this.autoRefreshLabel1.Text = "List refresh time:";
            // 
            // autoRefreshLabel2
            // 
            this.autoRefreshLabel2.AutoSize = true;
            this.autoRefreshLabel2.Location = new System.Drawing.Point(251, 299);
            this.autoRefreshLabel2.Name = "autoRefreshLabel2";
            this.autoRefreshLabel2.Size = new System.Drawing.Size(61, 20);
            this.autoRefreshLabel2.TabIndex = 19;
            this.autoRefreshLabel2.Text = "minutes";
            // 
            // coffeePanel
            // 
            this.coffeePanel.BackColor = System.Drawing.Color.Transparent;
            this.coffeePanel.Controls.Add(this.coffeeLinkLabel);
            this.coffeePanel.Controls.Add(this.coffeePictureBox);
            this.coffeePanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.coffeePanel.Location = new System.Drawing.Point(21, 570);
            this.coffeePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.coffeePanel.Name = "coffeePanel";
            this.coffeePanel.Size = new System.Drawing.Size(434, 107);
            this.coffeePanel.TabIndex = 17;
            this.coffeePanel.Click += new System.EventHandler(this.CoffeeLink_Click);
            // 
            // coffeeLinkLabel
            // 
            this.coffeeLinkLabel.ActiveLinkColor = System.Drawing.Color.DodgerBlue;
            this.coffeeLinkLabel.AutoSize = true;
            this.coffeeLinkLabel.Location = new System.Drawing.Point(69, 24);
            this.coffeeLinkLabel.MaximumSize = new System.Drawing.Size(366, 0);
            this.coffeeLinkLabel.Name = "coffeeLinkLabel";
            this.coffeeLinkLabel.Size = new System.Drawing.Size(335, 20);
            this.coffeeLinkLabel.TabIndex = 15;
            this.coffeeLinkLabel.TabStop = true;
            this.coffeeLinkLabel.Tag = "https://www.buymeacoffee.com/deminimis";
            this.coffeeLinkLabel.Text = "Support my caffeine addiction if you like this app";
            this.coffeeLinkLabel.Click += new System.EventHandler(this.CoffeeLink_Click);
            // 
            // coffeePictureBox
            // 
            this.coffeePictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.coffeePictureBox.Location = new System.Drawing.Point(0, 0);
            this.coffeePictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.coffeePictureBox.Name = "coffeePictureBox";
            this.coffeePictureBox.Size = new System.Drawing.Size(62, 72);
            this.coffeePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.coffeePictureBox.TabIndex = 13;
            this.coffeePictureBox.TabStop = false;
            this.coffeePictureBox.Click += new System.EventHandler(this.CoffeeLink_Click);
            this.coffeePictureBox.MouseEnter += new System.EventHandler(this.CoffeePictureBox_MouseEnter);
            this.coffeePictureBox.MouseLeave += new System.EventHandler(this.CoffeePictureBox_MouseLeave);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.versionLabel.Location = new System.Drawing.Point(223, 470);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(57, 20);
            this.versionLabel.TabIndex = 12;
            this.versionLabel.Text = "Version";
            // 
            // checkForUpdatesButton
            // 
            this.checkForUpdatesButton.FlatAppearance.BorderSize = 0;
            this.checkForUpdatesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkForUpdatesButton.Location = new System.Drawing.Point(29, 460);
            this.checkForUpdatesButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.checkForUpdatesButton.Name = "checkForUpdatesButton";
            this.checkForUpdatesButton.Size = new System.Drawing.Size(183, 37);
            this.checkForUpdatesButton.TabIndex = 11;
            this.checkForUpdatesButton.Text = "Check for Updates";
            this.checkForUpdatesButton.Click += new System.EventHandler(this.CheckForUpdatesButton_Click);
            // 
            // openFirewallButton
            // 
            this.openFirewallButton.FlatAppearance.BorderSize = 0;
            this.openFirewallButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openFirewallButton.Location = new System.Drawing.Point(29, 420);
            this.openFirewallButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.openFirewallButton.Name = "openFirewallButton";
            this.openFirewallButton.Size = new System.Drawing.Size(183, 37);
            this.openFirewallButton.TabIndex = 10;
            this.openFirewallButton.Text = "Open Windows Firewall";
            this.openFirewallButton.Click += new System.EventHandler(this.OpenFirewallButton_Click);
            // 
            // forumLink
            // 
            this.forumLink.AutoSize = true;
            this.forumLink.Location = new System.Drawing.Point(29, 510);
            this.forumLink.Name = "forumLink";
            this.forumLink.Size = new System.Drawing.Size(140, 20);
            this.forumLink.TabIndex = 9;
            this.forumLink.TabStop = true;
            this.forumLink.Tag = "https://github.com/deminimis/minimalfirewall/discussions";
            this.forumLink.Text = "Forum / Discussions";
            this.forumLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // reportProblemLink
            // 
            this.reportProblemLink.AutoSize = true;
            this.reportProblemLink.Location = new System.Drawing.Point(29, 530);
            this.reportProblemLink.Name = "reportProblemLink";
            this.reportProblemLink.Size = new System.Drawing.Size(126, 20);
            this.reportProblemLink.TabIndex = 8;
            this.reportProblemLink.TabStop = true;
            this.reportProblemLink.Tag = "https://github.com/deminimis/minimalfirewall/issues";
            this.reportProblemLink.Text = "Report a Problem";
            this.reportProblemLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // helpLink
            // 
            this.helpLink.AutoSize = true;
            this.helpLink.Location = new System.Drawing.Point(29, 550);
            this.helpLink.Name = "helpLink";
            this.helpLink.Size = new System.Drawing.Size(158, 20);
            this.helpLink.TabIndex = 7;
            this.helpLink.TabStop = true;
            this.helpLink.Tag = "https://github.com/deminimis/minimalfirewall";
            this.helpLink.Text = "Help / Documentation";
            this.helpLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // autoRefreshTextBox
            // 
            this.autoRefreshTextBox.Location = new System.Drawing.Point(171, 293);
            this.autoRefreshTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.autoRefreshTextBox.MaxLength = 3;
            this.autoRefreshTextBox.Name = "autoRefreshTextBox";
            this.autoRefreshTextBox.Size = new System.Drawing.Size(68, 27);
            this.autoRefreshTextBox.TabIndex = 5;
            this.autoRefreshTextBox.Text = "10";
            // 
            // loggingSwitch
            // 
            this.loggingSwitch.AutoSize = true;
            this.loggingSwitch.Location = new System.Drawing.Point(29, 247);
            this.loggingSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.loggingSwitch.Name = "loggingSwitch";
            this.loggingSwitch.Size = new System.Drawing.Size(132, 24);
            this.loggingSwitch.TabIndex = 4;
            this.loggingSwitch.Text = "Enable logging";
            this.loggingSwitch.UseVisualStyleBackColor = true;
            // 
            // popupsSwitch
            // 
            this.popupsSwitch.AutoSize = true;
            this.popupsSwitch.Location = new System.Drawing.Point(29, 200);
            this.popupsSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.popupsSwitch.Name = "popupsSwitch";
            this.popupsSwitch.Size = new System.Drawing.Size(216, 24);
            this.popupsSwitch.TabIndex = 3;
            this.popupsSwitch.Text = "Enable pop-up notifications";
            this.popupsSwitch.UseVisualStyleBackColor = true;
            this.popupsSwitch.CheckedChanged += new System.EventHandler(this.PopupsSwitch_CheckedChanged);
            // 
            // darkModeSwitch
            // 
            this.darkModeSwitch.AutoSize = true;
            this.darkModeSwitch.Location = new System.Drawing.Point(29, 153);
            this.darkModeSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.darkModeSwitch.Name = "darkModeSwitch";
            this.darkModeSwitch.Size = new System.Drawing.Size(105, 24);
            this.darkModeSwitch.TabIndex = 2;
            this.darkModeSwitch.Text = "Dark Mode";
            this.darkModeSwitch.UseVisualStyleBackColor = true;
            this.darkModeSwitch.CheckedChanged += new System.EventHandler(this.DarkModeSwitch_CheckedChanged);
            // 
            // startOnStartupSwitch
            // 
            this.startOnStartupSwitch.AutoSize = true;
            this.startOnStartupSwitch.Location = new System.Drawing.Point(29, 107);
            this.startOnStartupSwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.startOnStartupSwitch.Name = "startOnStartupSwitch";
            this.startOnStartupSwitch.Size = new System.Drawing.Size(159, 24);
            this.startOnStartupSwitch.TabIndex = 1;
            this.startOnStartupSwitch.Text = "Start with Windows";
            this.startOnStartupSwitch.UseVisualStyleBackColor = true;
            this.startOnStartupSwitch.CheckedChanged += new System.EventHandler(this.startOnStartupSwitch_CheckedChanged);
            // 
            // closeToTraySwitch
            // 
            this.closeToTraySwitch.AutoSize = true;
            this.closeToTraySwitch.Checked = true;
            this.closeToTraySwitch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.closeToTraySwitch.Location = new System.Drawing.Point(29, 60);
            this.closeToTraySwitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.closeToTraySwitch.Name = "closeToTraySwitch";
            this.closeToTraySwitch.Size = new System.Drawing.Size(114, 24);
            this.closeToTraySwitch.TabIndex = 0;
            this.closeToTraySwitch.Text = "Close to tray";
            this.closeToTraySwitch.UseVisualStyleBackColor = true;
            // 
            // mainSettingsPanel
            // 
            this.mainSettingsPanel.AutoScroll = true;
            this.mainSettingsPanel.Controls.Add(this.deleteAllRulesButton);
            this.mainSettingsPanel.Controls.Add(this.revertFirewallButton);
            this.mainSettingsPanel.Controls.Add(this.auditAlertsSwitch);
            this.mainSettingsPanel.Controls.Add(this.managePublishersButton);
            this.mainSettingsPanel.Controls.Add(this.autoAllowSystemTrustedCheck);
            this.mainSettingsPanel.Controls.Add(this.showAppIconsSwitch);
            this.mainSettingsPanel.Controls.Add(this.trafficMonitorSwitch);
            this.mainSettingsPanel.Controls.Add(this.autoRefreshLabel1);
            this.mainSettingsPanel.Controls.Add(this.autoRefreshLabel2);
            this.mainSettingsPanel.Controls.Add(this.coffeePanel);
            this.mainSettingsPanel.Controls.Add(this.versionLabel);
            this.mainSettingsPanel.Controls.Add(this.checkForUpdatesButton);
            this.mainSettingsPanel.Controls.Add(this.openFirewallButton);
            this.mainSettingsPanel.Controls.Add(this.forumLink);
            this.mainSettingsPanel.Controls.Add(this.reportProblemLink);
            this.mainSettingsPanel.Controls.Add(this.helpLink);
            this.mainSettingsPanel.Controls.Add(this.autoRefreshTextBox);
            this.mainSettingsPanel.Controls.Add(this.loggingSwitch);
            this.mainSettingsPanel.Controls.Add(this.popupsSwitch);
            this.mainSettingsPanel.Controls.Add(this.darkModeSwitch);
            this.mainSettingsPanel.Controls.Add(this.startOnStartupSwitch);
            this.mainSettingsPanel.Controls.Add(this.closeToTraySwitch);
            this.mainSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.mainSettingsPanel.Name = "mainSettingsPanel";
            this.mainSettingsPanel.Size = new System.Drawing.Size(1015, 925);
            this.mainSettingsPanel.TabIndex = 27;
            // 
            // SettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainSettingsPanel);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(1015, 925);
            this.coffeePanel.ResumeLayout(false);
            this.coffeePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coffeePictureBox)).EndInit();
            this.mainSettingsPanel.ResumeLayout(false);
            this.mainSettingsPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button deleteAllRulesButton;
        private System.Windows.Forms.Button revertFirewallButton;
        private System.Windows.Forms.CheckBox auditAlertsSwitch;
        private System.Windows.Forms.Button managePublishersButton;
        private System.Windows.Forms.CheckBox autoAllowSystemTrustedCheck;
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
        private System.Windows.Forms.LinkLabel forumLink;
        private System.Windows.Forms.LinkLabel reportProblemLink;
        private System.Windows.Forms.LinkLabel helpLink;
        private System.Windows.Forms.TextBox autoRefreshTextBox;
        private System.Windows.Forms.CheckBox loggingSwitch;
        private System.Windows.Forms.CheckBox popupsSwitch;
        private System.Windows.Forms.CheckBox darkModeSwitch;
        private System.Windows.Forms.CheckBox startOnStartupSwitch;
        private System.Windows.Forms.CheckBox closeToTraySwitch;
        private Panel mainSettingsPanel;
    }
}