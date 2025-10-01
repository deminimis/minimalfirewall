// File: AuditControl.Designer.cs
namespace MinimalFirewall
{
    partial class AuditControl
    {
        private System.Windows.Forms.TextBox auditSearchTextBox;
        private System.Windows.Forms.Button rebuildBaselineButton;
        private MinimalFirewall.ButtonListView systemChangesListView;
        private System.Windows.Forms.ColumnHeader changeActionColumn;
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
        private System.Windows.Forms.ContextMenuStrip auditContextMenu;
        private System.Windows.Forms.ToolStripMenuItem acceptAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.Panel topPanel;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.auditSearchTextBox = new System.Windows.Forms.TextBox();
            this.rebuildBaselineButton = new System.Windows.Forms.Button();
            this.systemChangesListView = new MinimalFirewall.ButtonListView();
            this.changeActionColumn = new System.Windows.Forms.ColumnHeader();
            this.advNameColumn = new System.Windows.Forms.ColumnHeader();
            this.advStatusColumn = new System.Windows.Forms.ColumnHeader();
            this.advProtocolColumn = new System.Windows.Forms.ColumnHeader();
            this.advLocalPortsColumn = new System.Windows.Forms.ColumnHeader();
            this.advRemotePortsColumn = new System.Windows.Forms.ColumnHeader();
            this.advLocalAddressColumn = new System.Windows.Forms.ColumnHeader();
            this.advRemoteAddressColumn = new System.Windows.Forms.ColumnHeader();
            this.advProgramColumn = new System.Windows.Forms.ColumnHeader();
            this.advServiceColumn = new System.Windows.Forms.ColumnHeader();
            this.advProfilesColumn = new System.Windows.Forms.ColumnHeader();
            this.advGroupingColumn = new System.Windows.Forms.ColumnHeader();
            this.advDescColumn = new System.Windows.Forms.ColumnHeader();
            this.auditContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.acceptAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPanel = new System.Windows.Forms.Panel();
            this.auditContextMenu.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // auditSearchTextBox
            // 
            this.auditSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.auditSearchTextBox.Location = new System.Drawing.Point(707, 27);
            this.auditSearchTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.auditSearchTextBox.Name = "auditSearchTextBox";
            this.auditSearchTextBox.PlaceholderText = "Search changes...";
            this.auditSearchTextBox.Size = new System.Drawing.Size(285, 27);
            this.auditSearchTextBox.TabIndex = 3;
            this.auditSearchTextBox.TextChanged += new System.EventHandler(this.auditSearchTextBox_TextChanged);
            // 
            // rebuildBaselineButton
            // 
            this.rebuildBaselineButton.Location = new System.Drawing.Point(3, 17);
            this.rebuildBaselineButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rebuildBaselineButton.Name = "rebuildBaselineButton";
            this.rebuildBaselineButton.Size = new System.Drawing.Size(173, 48);
            this.rebuildBaselineButton.TabIndex = 2;
            this.rebuildBaselineButton.Text = "Rebuild Baseline";
            this.rebuildBaselineButton.Click += new System.EventHandler(this.rebuildBaselineButton_Click);
            // 
            // systemChangesListView
            // 
            this.systemChangesListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.systemChangesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.changeActionColumn,
            this.advNameColumn,
            this.advStatusColumn,
            this.advProtocolColumn,
            this.advLocalPortsColumn,
            this.advRemotePortsColumn,
            this.advLocalAddressColumn,
            this.advRemoteAddressColumn,
            this.advProgramColumn,
            this.advServiceColumn,
            this.advProfilesColumn,
            this.advGroupingColumn,
            this.advDescColumn});
            this.systemChangesListView.ContextMenuStrip = this.auditContextMenu;
            this.systemChangesListView.DarkMode = null;
            this.systemChangesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemChangesListView.FullRowSelect = true;
            this.systemChangesListView.Location = new System.Drawing.Point(0, 77);
            this.systemChangesListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.systemChangesListView.Name = "systemChangesListView";
            this.systemChangesListView.Size = new System.Drawing.Size(1000, 843);
            this.systemChangesListView.TabIndex = 1;
            this.systemChangesListView.UseCompatibleStateImageBehavior = false;
            this.systemChangesListView.View = System.Windows.Forms.View.Details;
            this.systemChangesListView.ViewMode = MinimalFirewall.ButtonListView.Mode.Audit;
            this.systemChangesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.systemChangesListView_ColumnClick);
            // 
            // changeActionColumn
            // 
            this.changeActionColumn.Text = "Action";
            // 
            // advNameColumn
            // 
            this.advNameColumn.Text = "Name";
            // 
            // advStatusColumn
            // 
            this.advStatusColumn.Text = "Action";
            // 
            // advProtocolColumn
            // 
            this.advProtocolColumn.Text = "Protocol";
            // 
            // advLocalPortsColumn
            // 
            this.advLocalPortsColumn.Text = "Local Ports";
            // 
            // advRemotePortsColumn
            // 
            this.advRemotePortsColumn.Text = "Remote Ports";
            // 
            // advLocalAddressColumn
            // 
            this.advLocalAddressColumn.Text = "Local Address";
            // 
            // advRemoteAddressColumn
            // 
            this.advRemoteAddressColumn.Text = "Remote Address";
            // 
            // advProgramColumn
            // 
            this.advProgramColumn.Text = "Application";
            // 
            // advServiceColumn
            // 
            this.advServiceColumn.Text = "Service";
            // 
            // advProfilesColumn
            // 
            this.advProfilesColumn.Text = "Profiles";
            // 
            // advGroupingColumn
            // 
            this.advGroupingColumn.Text = "Group";
            // 
            // advDescColumn
            // 
            this.advDescColumn.Text = "Description";
            // 
            // auditContextMenu
            // 
            this.auditContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.auditContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.acceptAllToolStripMenuItem,
            this.copyDetailsToolStripMenuItem,
            this.toolStripSeparator1,
            this.openFileLocationToolStripMenuItem});
            this.auditContextMenu.Name = "auditContextMenu";
            this.auditContextMenu.Size = new System.Drawing.Size(207, 88);
            this.auditContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.auditContextMenu_Opening);
            // 
            // acceptAllToolStripMenuItem
            // 
            this.acceptAllToolStripMenuItem.Name = "acceptAllToolStripMenuItem";
            this.acceptAllToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.acceptAllToolStripMenuItem.Text = "Accept All Changes";
            this.acceptAllToolStripMenuItem.Click += new System.EventHandler(this.AcceptAllToolStripMenuItem_Click);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(203, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.openFileLocationToolStripMenuItem.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.rebuildBaselineButton);
            this.topPanel.Controls.Add(this.auditSearchTextBox);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1000, 77);
            this.topPanel.TabIndex = 4;
            // 
            // AuditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.systemChangesListView);
            this.Controls.Add(this.topPanel);
            this.Name = "AuditControl";
            this.Size = new System.Drawing.Size(1000, 920);
            this.auditContextMenu.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}