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
        private System.Windows.Forms.TabPage wildcardRulesTabPage;
        private WildcardRulesControl wildcardRulesControl1;

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
            logoPictureBox = new PictureBox();
            arrowPictureBox = new PictureBox();
            instructionLabel = new Label();
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
            lockdownButton = new Button();
            rescanButton = new Button();
            mainToolTip = new ToolTip(components);
            appIconList = new ImageList(components);
            mainTabControl.SuspendLayout();
            dashboardTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).BeginInit();
            logoPictureBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(arrowPictureBox)).BeginInit();
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
            mainTabControl.BorderColor = SystemColors.ControlDark;
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
            mainTabControl.ItemSize = new Size(70, 120);
            mainTabControl.LineColor = SystemColors.Highlight;
            mainTabControl.Location = new Point(0, 0);
            mainTabControl.Multiline = true;
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedForeColor = SystemColors.HighlightText;
            mainTabControl.SelectedIndex = 0;
            mainTabControl.SelectTabColor = SystemColors.ControlLight;
            mainTabControl.Size = new Size(1000, 700);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.TabColor = SystemColors.ControlLight;
            mainTabControl.TabIndex = 0;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            mainTabControl.Deselecting += MainTabControl_Deselecting;
            // 
            // dashboardTabPage
            // 
            dashboardTabPage.Controls.Add(logoPictureBox);
            dashboardTabPage.Controls.Add(dashboardControl1);
            dashboardTabPage.ImageIndex = 3;
            dashboardTabPage.Location = new Point(124, 4);
            dashboardTabPage.Name = "dashboardTabPage";
            dashboardTabPage.Padding = new Padding(3, 3, 3, 3);
            dashboardTabPage.Size = new Size(872, 692);
            dashboardTabPage.TabIndex = 0;
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
            logoPictureBox.Size = new Size(866, 686);
            logoPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            logoPictureBox.TabIndex = 1;
            logoPictureBox.TabStop = false;
            // 
            // arrowPictureBox
            // 
            arrowPictureBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            arrowPictureBox.BackColor = Color.Transparent;
            arrowPictureBox.Location = new Point(20, 620);
            arrowPictureBox.Name = "arrowPictureBox";
            arrowPictureBox.Size = new Size(60, 40);
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
            instructionLabel.Location = new Point(20, 590);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(241, 15);
            instructionLabel.TabIndex = 2;
            instructionLabel.Text = "Press the lock key to initiate firewall defense.";
            // 
            // dashboardControl1
            // 
            dashboardControl1.Dock = DockStyle.Fill;
            dashboardControl1.Location = new Point(3, 3);
            dashboardControl1.Margin = new Padding(3, 2, 3, 2);
            dashboardControl1.Name = "dashboardControl1";
            dashboardControl1.Size = new Size(866, 686);
            dashboardControl1.TabIndex = 2;
            // 
            // rulesTabPage
            // 
            rulesTabPage.Controls.Add(rulesControl1);
            rulesTabPage.ImageIndex = 8;
            rulesTabPage.Location = new Point(124, 4);
            rulesTabPage.Name = "rulesTabPage";
            rulesTabPage.Padding = new Padding(3, 3, 3, 3);
            rulesTabPage.Size = new Size(872, 692);
            rulesTabPage.TabIndex = 1;
            rulesTabPage.Text = "Rules";
            rulesTabPage.UseVisualStyleBackColor = true;
            // 
            // rulesControl1
            // 
            rulesControl1.Dock = DockStyle.Fill;
            rulesControl1.Location = new Point(3, 3);
            rulesControl1.Margin = new Padding(3, 2, 3, 2);
            rulesControl1.Name = "rulesControl1";
            rulesControl1.Size = new Size(866, 686);
            rulesControl1.TabIndex = 0;
            // 
            // wildcardRulesTabPage
            // 
            wildcardRulesTabPage.Controls.Add(wildcardRulesControl1);
            wildcardRulesTabPage.ImageIndex = 11;
            wildcardRulesTabPage.Location = new Point(124, 4);
            wildcardRulesTabPage.Margin = new Padding(3, 2, 3, 2);
            wildcardRulesTabPage.Name = "wildcardRulesTabPage";
            wildcardRulesTabPage.Padding = new Padding(3, 2, 3, 2);
            wildcardRulesTabPage.Size = new Size(872, 692);
            wildcardRulesTabPage.TabIndex = 7;
            wildcardRulesTabPage.Text = "Wildcard Rules";
            wildcardRulesTabPage.UseVisualStyleBackColor = true;
            // 
            // wildcardRulesControl1
            // 
            wildcardRulesControl1.Dock = DockStyle.Fill;
            wildcardRulesControl1.Location = new Point(3, 2);
            wildcardRulesControl1.Margin = new Padding(3, 2, 3, 2);
            wildcardRulesControl1.Name = "wildcardRulesControl1";
            wildcardRulesControl1.Size = new Size(866, 688);
            wildcardRulesControl1.TabIndex = 0;
            // 
            // systemChangesTabPage
            // 
            systemChangesTabPage.Controls.Add(auditControl1);
            systemChangesTabPage.ImageIndex = 1;
            systemChangesTabPage.Location = new Point(124, 4);
            systemChangesTabPage.Name = "systemChangesTabPage";
            systemChangesTabPage.Size = new Size(872, 692);
            systemChangesTabPage.TabIndex = 2;
            systemChangesTabPage.Text = "Audit";
            systemChangesTabPage.UseVisualStyleBackColor = true;
            // 
            // auditControl1
            // 
            auditControl1.Dock = DockStyle.Fill;
            auditControl1.Location = new Point(0, 0);
            auditControl1.Margin = new Padding(3, 2, 3, 2);
            auditControl1.Name = "auditControl1";
            auditControl1.Size = new Size(872, 692);
            auditControl1.TabIndex = 0;
            // 
            // groupsTabPage
            // 
            groupsTabPage.Controls.Add(groupsControl1);
            groupsTabPage.ImageIndex = 4;
            groupsTabPage.Location = new Point(124, 4);
            groupsTabPage.Name = "groupsTabPage";
            groupsTabPage.Padding = new Padding(3, 3, 3, 3);
            groupsTabPage.Size = new Size(872, 692);
            groupsTabPage.TabIndex = 5;
            groupsTabPage.Text = "Groups";
            groupsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupsControl1
            // 
            groupsControl1.Dock = DockStyle.Fill;
            groupsControl1.Location = new Point(3, 3);
            groupsControl1.Margin = new Padding(3, 2, 3, 2);
            groupsControl1.Name = "groupsControl1";
            groupsControl1.Size = new Size(866, 686);
            groupsControl1.TabIndex = 0;
            // 
            // liveConnectionsTabPage
            // 
            liveConnectionsTabPage.Controls.Add(liveConnectionsControl1);
            liveConnectionsTabPage.ImageIndex = 0;
            liveConnectionsTabPage.Location = new Point(124, 4);
            liveConnectionsTabPage.Name = "liveConnectionsTabPage";
            liveConnectionsTabPage.Padding = new Padding(3, 3, 3, 3);
            liveConnectionsTabPage.Size = new Size(872, 692);
            liveConnectionsTabPage.TabIndex = 6;
            liveConnectionsTabPage.Text = "Live Connections";
            liveConnectionsTabPage.UseVisualStyleBackColor = true;
            // 
            // liveConnectionsControl1
            // 
            liveConnectionsControl1.Dock = DockStyle.Fill;
            liveConnectionsControl1.Location = new Point(3, 3);
            liveConnectionsControl1.Margin = new Padding(3, 2, 3, 2);
            liveConnectionsControl1.Name = "liveConnectionsControl1";
            liveConnectionsControl1.Size = new Size(866, 686);
            liveConnectionsControl1.TabIndex = 0;
            // 
            // settingsTabPage
            // 
            settingsTabPage.Controls.Add(settingsControl1);
            settingsTabPage.ImageIndex = 9;
            settingsTabPage.Location = new Point(124, 4);
            settingsTabPage.Name = "settingsTabPage";
            settingsTabPage.Size = new Size(872, 692);
            settingsTabPage.TabIndex = 4;
            settingsTabPage.Text = "Settings";
            settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // settingsControl1
            // 
            settingsControl1.Dock = DockStyle.Fill;
            settingsControl1.Location = new Point(0, 0);
            settingsControl1.Margin = new Padding(3, 2, 3, 2);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Size = new Size(872, 692);
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
            lockdownButton.FlatAppearance.BorderColor = SystemColors.Control;
            lockdownButton.FlatAppearance.BorderSize = 2;
            lockdownButton.FlatStyle = FlatStyle.Flat;
            lockdownButton.Location = new Point(65, 652);
            lockdownButton.Name = "lockdownButton";
            lockdownButton.Size = new Size(40, 36);
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
            rescanButton.Location = new Point(15, 652);
            rescanButton.Name = "rescanButton";
            rescanButton.Size = new Size(40, 36);
            rescanButton.TabIndex = 1;
            rescanButton.UseVisualStyleBackColor = false;
            rescanButton.Click += RescanButton_Click;
            rescanButton.MouseEnter += RescanButton_MouseEnter;
            rescanButton.MouseLeave += RescanButton_MouseLeave;
            // 
            // appIconList
            // 
            appIconList.ColorDepth = ColorDepth.Depth32Bit;
            appIconList.ImageSize = new Size(32, 32);
            appIconList.TransparentColor = Color.Transparent;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 700);
            Controls.Add(rescanButton);
            Controls.Add(lockdownButton);
            Controls.Add(mainTabControl);
            Name = "MainForm";
            FormClosing += MainForm_FormClosing;
            mainTabControl.ResumeLayout(false);
            dashboardTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).EndInit();
            logoPictureBox.ResumeLayout(false);
            logoPictureBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(arrowPictureBox)).EndInit();
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