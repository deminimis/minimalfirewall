// File: DashboardControl.Designer.cs
namespace MinimalFirewall
{
    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null;
        private MinimalFirewall.ButtonListView dashboardListView;
        private System.Windows.Forms.ColumnHeader dashIconColumn;
        private System.Windows.Forms.ColumnHeader dashActionColumn;
        private System.Windows.Forms.ColumnHeader dashAppColumn;
        private System.Windows.Forms.ColumnHeader dashServiceColumn;
        private System.Windows.Forms.ColumnHeader dashDirectionColumn;
        private System.Windows.Forms.ColumnHeader dashPathColumn;
        private System.Windows.Forms.ContextMenuStrip dashboardContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tempAllowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow2MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow5MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow15MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow1HourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow3HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow8HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem permanentAllowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowAndTrustPublisherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem permanentBlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ignoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem createWildcardRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem createAdvancedRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
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
            this.components = new System.ComponentModel.Container();
            this.dashboardListView = new MinimalFirewall.ButtonListView();
            this.dashIconColumn = new System.Windows.Forms.ColumnHeader();
            this.dashActionColumn = new System.Windows.Forms.ColumnHeader();
            this.dashAppColumn = new System.Windows.Forms.ColumnHeader();
            this.dashServiceColumn = new System.Windows.Forms.ColumnHeader();
            this.dashDirectionColumn = new System.Windows.Forms.ColumnHeader();
            this.dashPathColumn = new System.Windows.Forms.ColumnHeader();
            this.dashboardContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tempAllowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow2MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow5MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow15MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow1HourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow3HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow8HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.permanentAllowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowAndTrustPublisherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.permanentBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.createWildcardRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.createAdvancedRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dashboardListView
            // 
            this.dashboardListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dashboardListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.dashIconColumn,
            this.dashActionColumn,
            this.dashAppColumn,
            this.dashServiceColumn,
            this.dashDirectionColumn,
            this.dashPathColumn});
            this.dashboardListView.ContextMenuStrip = this.dashboardContextMenu;
            this.dashboardListView.DarkMode = null;
            this.dashboardListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardListView.FullRowSelect = true;
            this.dashboardListView.Location = new System.Drawing.Point(0, 0);
            this.dashboardListView.Name = "dashboardListView";
            this.dashboardListView.Size = new System.Drawing.Size(800, 600);
            this.dashboardListView.TabIndex = 1;
            this.dashboardListView.UseCompatibleStateImageBehavior = false;
            this.dashboardListView.View = System.Windows.Forms.View.Details;
            this.dashboardListView.ViewMode = MinimalFirewall.ButtonListView.Mode.Dashboard;
            // 
            // dashIconColumn
            // 
            this.dashIconColumn.Text = "";
            this.dashIconColumn.Width = 32;
            // 
            // dashActionColumn
            // 
            this.dashActionColumn.Text = "Action";
            // 
            // dashAppColumn
            // 
            this.dashAppColumn.Text = "Application";
            // 
            // dashServiceColumn
            // 
            this.dashServiceColumn.Text = "Service";
            // 
            // dashDirectionColumn
            // 
            this.dashDirectionColumn.Text = "Direction";
            // 
            // dashPathColumn
            // 
            this.dashPathColumn.Text = "Path";
            // 
            // dashboardContextMenu
            // 
            this.dashboardContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.dashboardContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tempAllowToolStripMenuItem,
            this.toolStripSeparator3,
            this.permanentAllowToolStripMenuItem,
            this.allowAndTrustPublisherToolStripMenuItem,
            this.permanentBlockToolStripMenuItem,
            this.ignoreToolStripMenuItem,
            this.toolStripSeparator4,
            this.createWildcardRuleToolStripMenuItem,

            this.toolStripSeparator5,
            this.createAdvancedRuleToolStripMenuItem,
            this.toolStripSeparator7,
            this.openFileLocationToolStripMenuItem1,
            this.toolStripSeparator6,
            this.copyDetailsToolStripMenuItem});
            this.dashboardContextMenu.Name = "dashboardContextMenu";
            this.dashboardContextMenu.Size = new System.Drawing.Size(228, 290);
            this.dashboardContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tempAllowToolStripMenuItem
            // 
            this.tempAllowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allow2MinutesToolStripMenuItem,
            this.allow5MinutesToolStripMenuItem,
            this.allow15MinutesToolStripMenuItem,
            this.allow1HourToolStripMenuItem,

             this.allow3HoursToolStripMenuItem,
            this.allow8HoursToolStripMenuItem});
            this.tempAllowToolStripMenuItem.Name = "tempAllowToolStripMenuItem";
            this.tempAllowToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.tempAllowToolStripMenuItem.Text = "Allow Temporarily";
            // 
            // allow2MinutesToolStripMenuItem
            // 
            this.allow2MinutesToolStripMenuItem.Name = "allow2MinutesToolStripMenuItem";
            this.allow2MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow2MinutesToolStripMenuItem.Tag = "2";
            this.allow2MinutesToolStripMenuItem.Text = "2 minutes";
            this.allow2MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow5MinutesToolStripMenuItem
            // 
            this.allow5MinutesToolStripMenuItem.Name = "allow5MinutesToolStripMenuItem";
            this.allow5MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow5MinutesToolStripMenuItem.Tag = "5";
            this.allow5MinutesToolStripMenuItem.Text = "5 minutes";
            this.allow5MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow15MinutesToolStripMenuItem
            // 
            this.allow15MinutesToolStripMenuItem.Name = "allow15MinutesToolStripMenuItem";
            this.allow15MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow15MinutesToolStripMenuItem.Tag = "15";
            this.allow15MinutesToolStripMenuItem.Text = "15 minutes";
            this.allow15MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow1HourToolStripMenuItem
            // 
            this.allow1HourToolStripMenuItem.Name = "allow1HourToolStripMenuItem";
            this.allow1HourToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow1HourToolStripMenuItem.Tag = "60";
            this.allow1HourToolStripMenuItem.Text = "1 hour";
            this.allow1HourToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow3HoursToolStripMenuItem
            // 
            this.allow3HoursToolStripMenuItem.Name = "allow3HoursToolStripMenuItem";
            this.allow3HoursToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow3HoursToolStripMenuItem.Tag = "180";
            this.allow3HoursToolStripMenuItem.Text = "3 hours";
            this.allow3HoursToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow8HoursToolStripMenuItem
            // 
            this.allow8HoursToolStripMenuItem.Name = "allow8HoursToolStripMenuItem";
            this.allow8HoursToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow8HoursToolStripMenuItem.Tag = "480";
            this.allow8HoursToolStripMenuItem.Text = "8 hours";
            this.allow8HoursToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(224, 6);
            // 
            // permanentAllowToolStripMenuItem
            // 
            this.permanentAllowToolStripMenuItem.Name = "permanentAllowToolStripMenuItem";
            this.permanentAllowToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.permanentAllowToolStripMenuItem.Text = "Allow";
            this.permanentAllowToolStripMenuItem.Click += new System.EventHandler(this.PermanentAllowToolStripMenuItem_Click);
            // 
            // allowAndTrustPublisherToolStripMenuItem
            // 
            this.allowAndTrustPublisherToolStripMenuItem.Name = "allowAndTrustPublisherToolStripMenuItem";
            this.allowAndTrustPublisherToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.allowAndTrustPublisherToolStripMenuItem.Text = "Allow and Trust Publisher";
            this.allowAndTrustPublisherToolStripMenuItem.Click += new System.EventHandler(this.AllowAndTrustPublisherToolStripMenuItem_Click);
            // 
            // permanentBlockToolStripMenuItem
            // 
            this.permanentBlockToolStripMenuItem.Name = "permanentBlockToolStripMenuItem";
            this.permanentBlockToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.permanentBlockToolStripMenuItem.Text = "Block";
            this.permanentBlockToolStripMenuItem.Click += new System.EventHandler(this.PermanentBlockToolStripMenuItem_Click);
            // 
            // ignoreToolStripMenuItem
            // 
            this.ignoreToolStripMenuItem.Name = "ignoreToolStripMenuItem";
            this.ignoreToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.ignoreToolStripMenuItem.Text = "Ignore";
            this.ignoreToolStripMenuItem.Click += new System.EventHandler(this.IgnoreToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(224, 6);
            // 
            // createWildcardRuleToolStripMenuItem
            // 
            this.createWildcardRuleToolStripMenuItem.Name = "createWildcardRuleToolStripMenuItem";
            this.createWildcardRuleToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.createWildcardRuleToolStripMenuItem.Text = "Create Wildcard Rule...";
            this.createWildcardRuleToolStripMenuItem.Click += new System.EventHandler(this.createWildcardRuleToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(224, 6);
            // 
            // createAdvancedRuleToolStripMenuItem
            // 
            this.createAdvancedRuleToolStripMenuItem.Name = "createAdvancedRuleToolStripMenuItem";
            this.createAdvancedRuleToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.createAdvancedRuleToolStripMenuItem.Text = "Create Advanced Rule...";
            this.createAdvancedRuleToolStripMenuItem.Click += new System.EventHandler(this.createAdvancedRuleToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(224, 6);
            // 
            // openFileLocationToolStripMenuItem1
            // 
            this.openFileLocationToolStripMenuItem1.Name = "openFileLocationToolStripMenuItem1";
            this.openFileLocationToolStripMenuItem1.Size = new System.Drawing.Size(227, 24);
            this.openFileLocationToolStripMenuItem1.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem1.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem1_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(224, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // DashboardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dashboardListView);
            this.Name = "DashboardControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.dashboardContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}