// File: MainForm.Designer.cs
namespace MinimalFirewall
{
    public partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private DarkModeForms.ThemedTabControl mainTabControl;
        private System.Windows.Forms.TabPage dashboardTabPage;
        private System.Windows.Forms.TabPage rulesTabPage;
        private System.Windows.Forms.TabPage systemChangesTabPage;
        private System.Windows.Forms.TabPage settingsTabPage;
        private System.Windows.Forms.TabPage groupsTabPage;
        private System.Windows.Forms.TabPage liveConnectionsTabPage;
        private DarkModeForms.ThemedButton lockdownButton;
        private DarkModeForms.ThemedButton rescanButton;
        private System.Windows.Forms.ToolTip mainToolTip;
        private System.Windows.Forms.ImageList appImageList;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.PictureBox arrowPictureBox;
        private DarkModeForms.ThemedLabel instructionLabel;
        private System.Windows.Forms.ImageList appIconList;
        private DashboardControl dashboardControl1;
        private RulesControl rulesControl1;
        private AuditControl auditControl1;
        private GroupsControl groupsControl1;
        private LiveConnectionsControl liveConnectionsControl1;
        private SettingsControl settingsControl1;
        private System.Windows.Forms.TabPage wildcardRulesTabPage;
        private WildcardRulesControl wildcardRulesControl1;

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                _trayBlinkTimer?.Dispose();
                _autoRefreshTimer?.Dispose();
                // Order matters: view model -> producers -> consumer queue.
                // In-flight watcher callbacks racing past this are guarded in EnqueueTask.
                _mainViewModel?.Dispose();
                _firewallSentryService?.Dispose();
                _eventListenerService?.Dispose();
                _backgroundTaskService?.Dispose();
                _lockedGreenIcon?.Dispose();
                _unlockedWhiteIcon?.Dispose();
                _refreshWhiteIcon?.Dispose();
                _defaultTrayIcon?.Dispose();
                _unlockedTrayIcon?.Dispose();
                _alertTrayIcon?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainTabControl = new DarkModeForms.ThemedTabControl();
            dashboardTabPage = new TabPage();
            logoPictureBox = new PictureBox();
            arrowPictureBox = new PictureBox();
            instructionLabel = new DarkModeForms.ThemedLabel();
            dashboardControl1 = new DashboardControl();
            rulesTabPage = new TabPage();
            rulesControl1 = new RulesControl();
            wildcardRulesTabPage = new TabPage();
            wildcardRulesControl1 = new WildcardRulesControl();
            systemChangesTabPage = new TabPage();
            auditControl1 = new AuditControl();
            groupsTabPage = new TabPage();
            groupsControl1 = new GroupsControl();
            liveConnectionsTabPage = new TabPage();
            liveConnectionsControl1 = new LiveConnectionsControl();
            settingsTabPage = new TabPage();
            settingsControl1 = new SettingsControl();
            appImageList = new ImageList(components);
            lockdownButton = new DarkModeForms.ThemedButton();
            rescanButton = new DarkModeForms.ThemedButton();
            mainToolTip = new ToolTip(components);
            appIconList = new ImageList(components);
            mainTabControl.SuspendLayout();
            dashboardTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            logoPictureBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)arrowPictureBox).BeginInit();
            rulesTabPage.SuspendLayout();
            wildcardRulesTabPage.SuspendLayout();
            systemChangesTabPage.SuspendLayout();
            groupsTabPage.SuspendLayout();
            liveConnectionsTabPage.SuspendLayout();
            settingsTabPage.SuspendLayout();
            SuspendLayout();
            // 
            // mainTabControl
            // 
            mainTabControl.Alignment = TabAlignment.Left;
            mainTabControl.Controls.Add(dashboardTabPage);
            mainTabControl.Controls.Add(rulesTabPage);
            mainTabControl.Controls.Add(wildcardRulesTabPage);
            mainTabControl.Controls.Add(systemChangesTabPage);
            mainTabControl.Controls.Add(groupsTabPage);
            mainTabControl.Controls.Add(liveConnectionsTabPage);
            mainTabControl.Controls.Add(settingsTabPage);
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabControl.ImageList = appImageList;
            mainTabControl.ItemSize = new Size(85, 120);
            mainTabControl.Location = new Point(0, 0);
            mainTabControl.Multiline = true;
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(1000, 747);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.TabIndex = 0;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            mainTabControl.Deselecting += MainTabControl_Deselecting;
            // 
            // dashboardTabPage
            // 
            dashboardTabPage.BorderStyle = BorderStyle.FixedSingle;
            dashboardTabPage.Controls.Add(logoPictureBox);
            dashboardTabPage.Controls.Add(dashboardControl1);
            dashboardTabPage.ImageIndex = 3;
            dashboardTabPage.Location = new Point(124, 4);
            dashboardTabPage.Name = "dashboardTabPage";
            dashboardTabPage.Padding = new Padding(3);
            dashboardTabPage.Size = new Size(872, 739);
            dashboardTabPage.TabIndex = 0;
            dashboardTabPage.Tag = "themed";
            dashboardTabPage.Text = "Dashboard";
            dashboardTabPage.UseVisualStyleBackColor = true;
            // 
            // logoPictureBox
            // 
            logoPictureBox.Controls.Add(arrowPictureBox);
            logoPictureBox.Controls.Add(instructionLabel);
            logoPictureBox.Dock = DockStyle.Fill;
            logoPictureBox.Location = new Point(3, 3);
            logoPictureBox.Name = "logoPictureBox";
            logoPictureBox.Size = new Size(864, 731);
            logoPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            logoPictureBox.TabIndex = 1;
            logoPictureBox.TabStop = false;
            // 
            // arrowPictureBox
            // 
            arrowPictureBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            arrowPictureBox.BackColor = Color.Transparent;
            arrowPictureBox.Location = new Point(20, 660);
            arrowPictureBox.Name = "arrowPictureBox";
            arrowPictureBox.Size = new Size(60, 43);
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
            instructionLabel.ForeColor = Color.White;
            instructionLabel.Location = new Point(20, 628);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(241, 15);
            instructionLabel.TabIndex = 2;
            instructionLabel.Text = "Press the lock key to initiate firewall defense.";
            // 
            // dashboardControl1
            // 
            dashboardControl1.Dock = DockStyle.Fill;
            dashboardControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dashboardControl1.Location = new Point(3, 3);
            dashboardControl1.Margin = new Padding(3, 2, 3, 2);
            dashboardControl1.Name = "dashboardControl1";
            dashboardControl1.Size = new Size(864, 731);
            dashboardControl1.TabIndex = 2;
            // 
            // rulesTabPage
            // 
            rulesTabPage.BorderStyle = BorderStyle.FixedSingle;
            rulesTabPage.Controls.Add(rulesControl1);
            rulesTabPage.ImageIndex = 8;
            rulesTabPage.Location = new Point(124, 4);
            rulesTabPage.Name = "rulesTabPage";
            rulesTabPage.Padding = new Padding(3);
            rulesTabPage.Size = new Size(872, 739);
            rulesTabPage.TabIndex = 1;
            rulesTabPage.Tag = "themed";
            rulesTabPage.Text = "Rules";
            rulesTabPage.UseVisualStyleBackColor = true;
            // 
            // rulesControl1
            // 
            rulesControl1.Dock = DockStyle.Fill;
            rulesControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rulesControl1.Location = new Point(3, 3);
            rulesControl1.Margin = new Padding(3, 2, 3, 2);
            rulesControl1.Name = "rulesControl1";
            rulesControl1.Size = new Size(864, 731);
            rulesControl1.TabIndex = 0;
            // 
            // wildcardRulesTabPage
            // 
            wildcardRulesTabPage.BorderStyle = BorderStyle.FixedSingle;
            wildcardRulesTabPage.Controls.Add(wildcardRulesControl1);
            wildcardRulesTabPage.ImageIndex = 11;
            wildcardRulesTabPage.Location = new Point(124, 4);
            wildcardRulesTabPage.Margin = new Padding(3, 2, 3, 2);
            wildcardRulesTabPage.Name = "wildcardRulesTabPage";
            wildcardRulesTabPage.Padding = new Padding(3, 2, 3, 2);
            wildcardRulesTabPage.Size = new Size(872, 739);
            wildcardRulesTabPage.TabIndex = 7;
            wildcardRulesTabPage.Tag = "themed";
            wildcardRulesTabPage.Text = "Wildcard Rules";
            wildcardRulesTabPage.UseVisualStyleBackColor = true;
            // 
            // wildcardRulesControl1
            // 
            wildcardRulesControl1.Dock = DockStyle.Fill;
            wildcardRulesControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            wildcardRulesControl1.Location = new Point(3, 2);
            wildcardRulesControl1.Margin = new Padding(3, 2, 3, 2);
            wildcardRulesControl1.Name = "wildcardRulesControl1";
            wildcardRulesControl1.Size = new Size(864, 733);
            wildcardRulesControl1.TabIndex = 0;
            // 
            // systemChangesTabPage
            // 
            systemChangesTabPage.BorderStyle = BorderStyle.FixedSingle;
            systemChangesTabPage.Controls.Add(auditControl1);
            systemChangesTabPage.ImageIndex = 1;
            systemChangesTabPage.Location = new Point(124, 4);
            systemChangesTabPage.Name = "systemChangesTabPage";
            systemChangesTabPage.Size = new Size(872, 739);
            systemChangesTabPage.TabIndex = 2;
            systemChangesTabPage.Tag = "themed";
            systemChangesTabPage.Text = "Audit";
            systemChangesTabPage.UseVisualStyleBackColor = true;
            // 
            // auditControl1
            // 
            auditControl1.Dock = DockStyle.Fill;
            auditControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            auditControl1.Location = new Point(0, 0);
            auditControl1.Margin = new Padding(3, 2, 3, 2);
            auditControl1.Name = "auditControl1";
            auditControl1.Size = new Size(870, 737);
            auditControl1.TabIndex = 0;
            // 
            // groupsTabPage
            // 
            groupsTabPage.BorderStyle = BorderStyle.FixedSingle;
            groupsTabPage.Controls.Add(groupsControl1);
            groupsTabPage.ImageIndex = 4;
            groupsTabPage.Location = new Point(124, 4);
            groupsTabPage.Name = "groupsTabPage";
            groupsTabPage.Padding = new Padding(3);
            groupsTabPage.Size = new Size(872, 739);
            groupsTabPage.TabIndex = 5;
            groupsTabPage.Tag = "themed";
            groupsTabPage.Text = "Groups";
            groupsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupsControl1
            // 
            groupsControl1.Dock = DockStyle.Fill;
            groupsControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupsControl1.Location = new Point(3, 3);
            groupsControl1.Margin = new Padding(3, 2, 3, 2);
            groupsControl1.Name = "groupsControl1";
            groupsControl1.Size = new Size(864, 731);
            groupsControl1.TabIndex = 0;
            // 
            // liveConnectionsTabPage
            // 
            liveConnectionsTabPage.BorderStyle = BorderStyle.FixedSingle;
            liveConnectionsTabPage.Controls.Add(liveConnectionsControl1);
            liveConnectionsTabPage.ImageIndex = 0;
            liveConnectionsTabPage.Location = new Point(124, 4);
            liveConnectionsTabPage.Name = "liveConnectionsTabPage";
            liveConnectionsTabPage.Padding = new Padding(3);
            liveConnectionsTabPage.Size = new Size(872, 739);
            liveConnectionsTabPage.TabIndex = 6;
            liveConnectionsTabPage.Tag = "themed";
            liveConnectionsTabPage.Text = "Live Connections";
            liveConnectionsTabPage.UseVisualStyleBackColor = true;
            // 
            // liveConnectionsControl1
            // 
            liveConnectionsControl1.Dock = DockStyle.Fill;
            liveConnectionsControl1.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            liveConnectionsControl1.Location = new Point(3, 3);
            liveConnectionsControl1.Margin = new Padding(3, 2, 3, 2);
            liveConnectionsControl1.Name = "liveConnectionsControl1";
            liveConnectionsControl1.Size = new Size(864, 731);
            liveConnectionsControl1.TabIndex = 0;
            // 
            // settingsTabPage
            // 
            settingsTabPage.Controls.Add(settingsControl1);
            settingsTabPage.ImageIndex = 9;
            settingsTabPage.Location = new Point(124, 4);
            settingsTabPage.Name = "settingsTabPage";
            settingsTabPage.Size = new Size(872, 739);
            settingsTabPage.TabIndex = 4;
            settingsTabPage.Text = "Settings";
            settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // settingsControl1
            // 
            settingsControl1.Dock = DockStyle.Fill;
            settingsControl1.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            settingsControl1.Location = new Point(0, 0);
            settingsControl1.Margin = new Padding(3, 2, 3, 2);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Size = new Size(872, 739);
            settingsControl1.TabIndex = 0;
            // 
            // appImageList
            // 
            appImageList.ColorDepth = ColorDepth.Depth32Bit;
            appImageList.ImageStream = (ImageListStreamer)resources.GetObject("appImageList.ImageStream");
            appImageList.TransparentColor = Color.Transparent;
            appImageList.Images.SetKeyName(0, "antenna.png");
            appImageList.Images.SetKeyName(1, "audit.png");
            appImageList.Images.SetKeyName(2, "coffee.png");
            appImageList.Images.SetKeyName(3, "dashboard.png");
            appImageList.Images.SetKeyName(4, "groups.png");
            appImageList.Images.SetKeyName(5, "locked.png");
            appImageList.Images.SetKeyName(6, "logo.png");
            appImageList.Images.SetKeyName(7, "refresh.png");
            appImageList.Images.SetKeyName(8, "rules.png");
            appImageList.Images.SetKeyName(9, "settings.png");
            appImageList.Images.SetKeyName(10, "unlocked.png");
            appImageList.Images.SetKeyName(11, "wildcard.png");
            // 
            // lockdownButton
            // 
            lockdownButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lockdownButton.BackColor = Color.Transparent;
            lockdownButton.FlatAppearance.BorderColor = Color.FromArgb(18, 18, 18);
            lockdownButton.FlatAppearance.BorderSize = 2;
            lockdownButton.FlatAppearance.CheckedBackColor = Color.FromArgb(196, 34, 118, 250);
            lockdownButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(67, 67, 67);
            lockdownButton.FlatStyle = FlatStyle.Flat;
            lockdownButton.ForeColor = Color.White;
            lockdownButton.Location = new Point(68, 691);
            lockdownButton.Name = "lockdownButton";
            lockdownButton.Size = new Size(44, 47);
            lockdownButton.TabIndex = 3;
            lockdownButton.UseVisualStyleBackColor = false;
            lockdownButton.Click += ToggleLockdownButton_Click;
            lockdownButton.MouseEnter += OwnerDrawnButton_MouseEnterLeave;
            lockdownButton.MouseLeave += OwnerDrawnButton_MouseEnterLeave;
            // 
            // rescanButton
            // 
            rescanButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            rescanButton.BackColor = Color.Transparent;
            rescanButton.FlatAppearance.BorderColor = Color.FromArgb(18, 18, 18);
            rescanButton.FlatAppearance.BorderSize = 2;
            rescanButton.FlatAppearance.CheckedBackColor = Color.FromArgb(196, 34, 118, 250);
            rescanButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(67, 67, 67);
            rescanButton.FlatStyle = FlatStyle.Flat;
            rescanButton.ForeColor = Color.White;
            rescanButton.Location = new Point(16, 691);
            rescanButton.Name = "rescanButton";
            rescanButton.Size = new Size(44, 47);
            rescanButton.TabIndex = 1;
            rescanButton.UseVisualStyleBackColor = false;
            rescanButton.Click += RescanButton_Click;
            rescanButton.MouseEnter += OwnerDrawnButton_MouseEnterLeave;
            rescanButton.MouseLeave += OwnerDrawnButton_MouseEnterLeave;
            // 
            // appIconList
            // 
            appIconList.ColorDepth = ColorDepth.Depth32Bit;
            appIconList.ImageSize = new Size(32, 32);
            appIconList.TransparentColor = Color.Transparent;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 747);
            Controls.Add(rescanButton);
            Controls.Add(lockdownButton);
            Controls.Add(mainTabControl);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "MainForm";
            FormClosing += MainForm_FormClosing;
            mainTabControl.ResumeLayout(false);
            dashboardTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            logoPictureBox.ResumeLayout(false);
            logoPictureBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)arrowPictureBox).EndInit();
            rulesTabPage.ResumeLayout(false);
            wildcardRulesTabPage.ResumeLayout(false);
            systemChangesTabPage.ResumeLayout(false);
            groupsTabPage.ResumeLayout(false);
            liveConnectionsTabPage.ResumeLayout(false);
            settingsTabPage.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion
    }
}
