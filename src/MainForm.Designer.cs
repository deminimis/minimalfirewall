// File: MainForm.Designer.cs
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
        private System.Windows.Forms.ToolTip mainToolTip;
        private System.Windows.Forms.ImageList appImageList;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.PictureBox arrowPictureBox;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.ImageList appIconList;
        private DashboardControl dashboardControl1;
        private RulesControl rulesControl1;
        private AuditControl auditControl1;
        private GroupsControl groupsControl1;
        private LiveConnectionsControl liveConnectionsControl1;
        private SettingsControl settingsControl1;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainTabControl = new DarkModeForms.FlatTabControl();
            this.dashboardTabPage = new System.Windows.Forms.TabPage();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.arrowPictureBox = new System.Windows.Forms.PictureBox();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.dashboardControl1 = new MinimalFirewall.DashboardControl();
            this.rulesTabPage = new System.Windows.Forms.TabPage();
            this.rulesControl1 = new MinimalFirewall.RulesControl();
            this.systemChangesTabPage = new System.Windows.Forms.TabPage();
            this.auditControl1 = new MinimalFirewall.AuditControl();
            this.groupsTabPage = new System.Windows.Forms.TabPage();
            this.groupsControl1 = new MinimalFirewall.GroupsControl();
            this.liveConnectionsTabPage = new System.Windows.Forms.TabPage();
            this.liveConnectionsControl1 = new MinimalFirewall.LiveConnectionsControl();
            this.settingsTabPage = new System.Windows.Forms.TabPage();
            this.settingsControl1 = new MinimalFirewall.SettingsControl();
            this.appImageList = new System.Windows.Forms.ImageList(this.components);
            this.lockdownButton = new System.Windows.Forms.Button();
            this.rescanButton = new System.Windows.Forms.Button();
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.appIconList = new System.Windows.Forms.ImageList(this.components);
            this.mainTabControl.SuspendLayout();
            this.dashboardTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.logoPictureBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrowPictureBox)).BeginInit();
            this.rulesTabPage.SuspendLayout();
            this.systemChangesTabPage.SuspendLayout();
            this.groupsTabPage.SuspendLayout();
            this.liveConnectionsTabPage.SuspendLayout();
            this.settingsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.mainTabControl.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.mainTabControl.Controls.Add(this.dashboardTabPage);
            this.mainTabControl.Controls.Add(this.rulesTabPage);
            this.mainTabControl.Controls.Add(this.systemChangesTabPage);
            this.mainTabControl.Controls.Add(this.groupsTabPage);
            this.mainTabControl.Controls.Add(this.liveConnectionsTabPage);
            this.mainTabControl.Controls.Add(this.settingsTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.mainTabControl.ImageList = this.appImageList;
            this.mainTabControl.ItemSize = new System.Drawing.Size(70, 120);
            this.mainTabControl.LineColor = System.Drawing.SystemColors.Highlight;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.mainTabControl.Multiline = true;
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedForeColor = System.Drawing.SystemColors.HighlightText;
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.SelectTabColor = System.Drawing.SystemColors.ControlLight;
            this.mainTabControl.Size = new System.Drawing.Size(1143, 933);
            this.mainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.mainTabControl.TabColor = System.Drawing.SystemColors.ControlLight;
            this.mainTabControl.TabIndex = 0;
            this.mainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
            this.mainTabControl.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Deselecting);
            // 
            // dashboardTabPage
            // 
            this.dashboardTabPage.Controls.Add(this.logoPictureBox);
            this.dashboardTabPage.Controls.Add(this.dashboardControl1);
            this.dashboardTabPage.ImageIndex = 7;
            this.dashboardTabPage.Location = new System.Drawing.Point(124, 4);
            this.dashboardTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dashboardTabPage.Name = "dashboardTabPage";
            this.dashboardTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dashboardTabPage.Size = new System.Drawing.Size(1015, 925);
            this.dashboardTabPage.TabIndex = 0;
            this.dashboardTabPage.Text = "Dashboard";
            this.dashboardTabPage.UseVisualStyleBackColor = true;
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Controls.Add(this.arrowPictureBox);
            this.logoPictureBox.Controls.Add(this.instructionLabel);
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoPictureBox.Location = new System.Drawing.Point(3, 4);
            this.logoPictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Size = new System.Drawing.Size(1009, 917);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.logoPictureBox.TabIndex = 1;
            this.logoPictureBox.TabStop = false;
            // 
            // arrowPictureBox
            // 
            this.arrowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.arrowPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.arrowPictureBox.Location = new System.Drawing.Point(23, 829);
            this.arrowPictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.arrowPictureBox.Name = "arrowPictureBox";
            this.arrowPictureBox.Size = new System.Drawing.Size(69, 53);
            this.arrowPictureBox.TabIndex = 3;
            this.arrowPictureBox.TabStop = false;
            this.arrowPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ArrowPictureBox_Paint);
            // 
            // instructionLabel
            // 
            this.instructionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.BackColor = System.Drawing.Color.Transparent;
            this.instructionLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.instructionLabel.Location = new System.Drawing.Point(23, 789);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(304, 20);
            this.instructionLabel.TabIndex = 2;
            this.instructionLabel.Text = "Press the lock key to initiate firewall defense.";
            // 
            // dashboardControl1
            // 
            this.dashboardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardControl1.Location = new System.Drawing.Point(3, 4);
            this.dashboardControl1.Name = "dashboardControl1";
            this.dashboardControl1.Size = new System.Drawing.Size(1009, 917);
            this.dashboardControl1.TabIndex = 2;
            // 
            // rulesTabPage
            // 
            this.rulesTabPage.Controls.Add(this.rulesControl1);
            this.rulesTabPage.ImageIndex = 5;
            this.rulesTabPage.Location = new System.Drawing.Point(124, 4);
            this.rulesTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rulesTabPage.Name = "rulesTabPage";
            this.rulesTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rulesTabPage.Size = new System.Drawing.Size(1015, 925);
            this.rulesTabPage.TabIndex = 1;
            this.rulesTabPage.Text = "Rules";
            this.rulesTabPage.UseVisualStyleBackColor = true;
            // 
            // rulesControl1
            // 
            this.rulesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rulesControl1.Location = new System.Drawing.Point(3, 4);
            this.rulesControl1.Name = "rulesControl1";
            this.rulesControl1.Size = new System.Drawing.Size(1009, 917);
            this.rulesControl1.TabIndex = 0;
            // 
            // systemChangesTabPage
            // 
            this.systemChangesTabPage.Controls.Add(this.auditControl1);
            this.systemChangesTabPage.ImageIndex = 3;
            this.systemChangesTabPage.Location = new System.Drawing.Point(124, 4);
            this.systemChangesTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.systemChangesTabPage.Name = "systemChangesTabPage";
            this.systemChangesTabPage.Size = new System.Drawing.Size(1015, 925);
            this.systemChangesTabPage.TabIndex = 2;
            this.systemChangesTabPage.Text = "Audit";
            this.systemChangesTabPage.UseVisualStyleBackColor = true;
            // 
            // auditControl1
            // 
            this.auditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.auditControl1.Location = new System.Drawing.Point(0, 0);
            this.auditControl1.Name = "auditControl1";
            this.auditControl1.Size = new System.Drawing.Size(1015, 925);
            this.auditControl1.TabIndex = 0;
            // 
            // groupsTabPage
            // 
            this.groupsTabPage.Controls.Add(this.groupsControl1);
            this.groupsTabPage.ImageIndex = 5;
            this.groupsTabPage.Location = new System.Drawing.Point(124, 4);
            this.groupsTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupsTabPage.Name = "groupsTabPage";
            this.groupsTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupsTabPage.Size = new System.Drawing.Size(1015, 925);
            this.groupsTabPage.TabIndex = 5;
            this.groupsTabPage.Text = "Groups";
            this.groupsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupsControl1
            // 
            this.groupsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupsControl1.Location = new System.Drawing.Point(3, 4);
            this.groupsControl1.Name = "groupsControl1";
            this.groupsControl1.Size = new System.Drawing.Size(1009, 917);
            this.groupsControl1.TabIndex = 0;
            // 
            // liveConnectionsTabPage
            // 
            this.liveConnectionsTabPage.Controls.Add(this.liveConnectionsControl1);
            this.liveConnectionsTabPage.ImageIndex = 10;
            this.liveConnectionsTabPage.Location = new System.Drawing.Point(124, 4);
            this.liveConnectionsTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.liveConnectionsTabPage.Name = "liveConnectionsTabPage";
            this.liveConnectionsTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.liveConnectionsTabPage.Size = new System.Drawing.Size(1015, 925);
            this.liveConnectionsTabPage.TabIndex = 6;
            this.liveConnectionsTabPage.Text = "Live Connections";
            this.liveConnectionsTabPage.UseVisualStyleBackColor = true;
            // 
            // liveConnectionsControl1
            // 
            this.liveConnectionsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveConnectionsControl1.Location = new System.Drawing.Point(3, 4);
            this.liveConnectionsControl1.Name = "liveConnectionsControl1";
            this.liveConnectionsControl1.Size = new System.Drawing.Size(1009, 917);
            this.liveConnectionsControl1.TabIndex = 0;
            // 
            // settingsTabPage
            // 
            this.settingsTabPage.Controls.Add(this.settingsControl1);
            this.settingsTabPage.ImageIndex = 6;
            this.settingsTabPage.Location = new System.Drawing.Point(124, 4);
            this.settingsTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.settingsTabPage.Name = "settingsTabPage";
            this.settingsTabPage.Size = new System.Drawing.Size(1015, 925);
            this.settingsTabPage.TabIndex = 4;
            this.settingsTabPage.Text = "Settings";
            this.settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // settingsControl1
            // 
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsControl1.Location = new System.Drawing.Point(0, 0);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Size = new System.Drawing.Size(1015, 925);
            this.settingsControl1.TabIndex = 0;
            // 
            // appImageList
            // 
            this.appImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.appImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("appImageList.ImageStream")));
            this.appImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.appImageList.Images.SetKeyName(0, "coffee.png");
            this.appImageList.Images.SetKeyName(1, "refresh.png");
            this.appImageList.Images.SetKeyName(2, "rules.png");
            this.appImageList.Images.SetKeyName(3, "system_changes.png");
            this.appImageList.Images.SetKeyName(4, "locked.png");
            this.appImageList.Images.SetKeyName(5, "advanced.png");
            this.appImageList.Images.SetKeyName(6, "settings.png");
            this.appImageList.Images.SetKeyName(7, "dashboard.png");
            this.appImageList.Images.SetKeyName(8, "unlocked.png");
            this.appImageList.Images.SetKeyName(9, "logo.png");
            this.appImageList.Images.SetKeyName(10, "antenna.png");
            // 
            // lockdownButton
            // 
            this.lockdownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lockdownButton.BackColor = System.Drawing.Color.Transparent;
            this.lockdownButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.lockdownButton.FlatAppearance.BorderSize = 2;
            this.lockdownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lockdownButton.Location = new System.Drawing.Point(74, 869);
            this.lockdownButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lockdownButton.Name = "lockdownButton";
            this.lockdownButton.Size = new System.Drawing.Size(46, 48);
            this.lockdownButton.TabIndex = 3;
            this.lockdownButton.UseVisualStyleBackColor = false;
            this.lockdownButton.Click += new System.EventHandler(this.ToggleLockdownButton_Click);
            this.lockdownButton.MouseEnter += new System.EventHandler(this.LockdownButton_MouseEnter);
            this.lockdownButton.MouseLeave += new System.EventHandler(this.LockdownButton_MouseLeave);
            // 
            // rescanButton
            // 
            this.rescanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rescanButton.BackColor = System.Drawing.Color.Transparent;
            this.rescanButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.rescanButton.FlatAppearance.BorderSize = 2;
            this.rescanButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rescanButton.Location = new System.Drawing.Point(17, 869);
            this.rescanButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rescanButton.Name = "rescanButton";
            this.rescanButton.Size = new System.Drawing.Size(46, 48);
            this.rescanButton.TabIndex = 1;
            this.rescanButton.UseVisualStyleBackColor = false;
            this.rescanButton.Click += new System.EventHandler(this.RescanButton_Click);
            this.rescanButton.MouseEnter += new System.EventHandler(this.RescanButton_MouseEnter);
            this.rescanButton.MouseLeave += new System.EventHandler(this.RescanButton_MouseLeave);
            // 
            // appIconList
            // 
            this.appIconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.appIconList.ImageSize = new System.Drawing.Size(16, 16);
            this.appIconList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1143, 933);
            this.Controls.Add(this.rescanButton);
            this.Controls.Add(this.lockdownButton);
            this.Controls.Add(this.mainTabControl);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.mainTabControl.ResumeLayout(false);
            this.dashboardTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.logoPictureBox.ResumeLayout(false);
            this.logoPictureBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrowPictureBox)).EndInit();
            this.rulesTabPage.ResumeLayout(false);
            this.systemChangesTabPage.ResumeLayout(false);
            this.groupsTabPage.ResumeLayout(false);
            this.liveConnectionsTabPage.ResumeLayout(false);
            this.settingsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}