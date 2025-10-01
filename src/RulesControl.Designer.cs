// File: RulesControl.Designer.cs
namespace MinimalFirewall
{
    partial class RulesControl
    {
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
        private System.Windows.Forms.CheckBox advFilterProgramCheck;
        private System.Windows.Forms.CheckBox advFilterServiceCheck;
        private System.Windows.Forms.CheckBox advFilterUwpCheck;
        private System.Windows.Forms.CheckBox advFilterWildcardCheck;
        private System.Windows.Forms.Button advancedRuleButton;
        private System.Windows.Forms.CheckBox advFilterAdvancedCheck;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;

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
            this.advFilterAdvancedCheck = new System.Windows.Forms.CheckBox();
            this.advancedRuleButton = new System.Windows.Forms.Button();
            this.advFilterWildcardCheck = new System.Windows.Forms.CheckBox();
            this.advFilterUwpCheck = new System.Windows.Forms.CheckBox();
            this.advFilterServiceCheck = new System.Windows.Forms.CheckBox();
            this.advFilterProgramCheck = new System.Windows.Forms.CheckBox();
            this.rulesSearchTextBox = new System.Windows.Forms.TextBox();
            this.createRuleButton = new System.Windows.Forms.Button();
            this.rulesListView = new System.Windows.Forms.ListView();
            this.advIconColumn = new System.Windows.Forms.ColumnHeader();
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
            this.rulesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.allowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowOutboundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowInboundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockOutboundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockInboundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rulesContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // advFilterAdvancedCheck
            // 
            this.advFilterAdvancedCheck.AutoSize = true;
            this.advFilterAdvancedCheck.Checked = true;
            this.advFilterAdvancedCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.advFilterAdvancedCheck.Location = new System.Drawing.Point(639, 20);
            this.advFilterAdvancedCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advFilterAdvancedCheck.Name = "advFilterAdvancedCheck";
            this.advFilterAdvancedCheck.Size = new System.Drawing.Size(97, 24);
            this.advFilterAdvancedCheck.TabIndex = 15;
            this.advFilterAdvancedCheck.Text = "Advanced";
            this.advFilterAdvancedCheck.UseVisualStyleBackColor = true;
            this.advFilterAdvancedCheck.CheckedChanged += new System.EventHandler(this.AdvFilter_CheckedChanged);
            // 
            // advancedRuleButton
            // 
            this.advancedRuleButton.Location = new System.Drawing.Point(157, 8);
            this.advancedRuleButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advancedRuleButton.Name = "advancedRuleButton";
            this.advancedRuleButton.Size = new System.Drawing.Size(143, 48);
            this.advancedRuleButton.TabIndex = 10;
            this.advancedRuleButton.Text = "Advanced Rule";
            this.advancedRuleButton.UseVisualStyleBackColor = true;
            this.advancedRuleButton.Click += new System.EventHandler(this.AdvancedRuleButton_Click);
            // 
            // advFilterWildcardCheck
            // 
            this.advFilterWildcardCheck.AutoSize = true;
            this.advFilterWildcardCheck.Checked = true;
            this.advFilterWildcardCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.advFilterWildcardCheck.Location = new System.Drawing.Point(544, 20);
            this.advFilterWildcardCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advFilterWildcardCheck.Name = "advFilterWildcardCheck";
            this.advFilterWildcardCheck.Size = new System.Drawing.Size(91, 24);
            this.advFilterWildcardCheck.TabIndex = 14;
            this.advFilterWildcardCheck.Text = "Wildcard";
            this.advFilterWildcardCheck.UseVisualStyleBackColor = true;
            this.advFilterWildcardCheck.CheckedChanged += new System.EventHandler(this.AdvFilter_CheckedChanged);
            // 
            // advFilterUwpCheck
            // 
            this.advFilterUwpCheck.AutoSize = true;
            this.advFilterUwpCheck.Location = new System.Drawing.Point(477, 20);
            this.advFilterUwpCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advFilterUwpCheck.Name = "advFilterUwpCheck";
            this.advFilterUwpCheck.Size = new System.Drawing.Size(63, 24);
            this.advFilterUwpCheck.TabIndex = 13;
            this.advFilterUwpCheck.Text = "UWP";
            this.advFilterUwpCheck.UseVisualStyleBackColor = true;
            this.advFilterUwpCheck.CheckedChanged += new System.EventHandler(this.AdvFilter_CheckedChanged);
            // 
            // advFilterServiceCheck
            // 
            this.advFilterServiceCheck.AutoSize = true;
            this.advFilterServiceCheck.Checked = true;
            this.advFilterServiceCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.advFilterServiceCheck.Location = new System.Drawing.Point(395, 20);
            this.advFilterServiceCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advFilterServiceCheck.Name = "advFilterServiceCheck";
            this.advFilterServiceCheck.Size = new System.Drawing.Size(78, 24);
            this.advFilterServiceCheck.TabIndex = 12;
            this.advFilterServiceCheck.Text = "Service";
            this.advFilterServiceCheck.UseVisualStyleBackColor = true;
            this.advFilterServiceCheck.CheckedChanged += new System.EventHandler(this.AdvFilter_CheckedChanged);
            // 
            // advFilterProgramCheck
            // 
            this.advFilterProgramCheck.AutoSize = true;
            this.advFilterProgramCheck.Checked = true;
            this.advFilterProgramCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.advFilterProgramCheck.Location = new System.Drawing.Point(306, 20);
            this.advFilterProgramCheck.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.advFilterProgramCheck.Name = "advFilterProgramCheck";
            this.advFilterProgramCheck.Size = new System.Drawing.Size(85, 24);
            this.advFilterProgramCheck.TabIndex = 11;
            this.advFilterProgramCheck.Text = "Program";
            this.advFilterProgramCheck.UseVisualStyleBackColor = true;
            this.advFilterProgramCheck.CheckedChanged += new System.EventHandler(this.AdvFilter_CheckedChanged);
            // 
            // rulesSearchTextBox
            // 
            this.rulesSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rulesSearchTextBox.Location = new System.Drawing.Point(714, 17);
            this.rulesSearchTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rulesSearchTextBox.Name = "rulesSearchTextBox";
            this.rulesSearchTextBox.PlaceholderText = "Search rules...";
            this.rulesSearchTextBox.Size = new System.Drawing.Size(285, 27);
            this.rulesSearchTextBox.TabIndex = 16;
            this.rulesSearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            // 
            // createRuleButton
            // 
            this.createRuleButton.Location = new System.Drawing.Point(7, 8);
            this.createRuleButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.createRuleButton.Name = "createRuleButton";
            this.createRuleButton.Size = new System.Drawing.Size(143, 48);
            this.createRuleButton.TabIndex = 9;
            this.createRuleButton.Text = "Create Rule";
            this.createRuleButton.UseVisualStyleBackColor = true;
            this.createRuleButton.Click += new System.EventHandler(this.CreateRuleButton_Click);
            // 
            // rulesListView
            // 
            this.rulesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rulesListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rulesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.advIconColumn,
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
            this.rulesListView.ContextMenuStrip = this.rulesContextMenu;
            this.rulesListView.FullRowSelect = true;
            this.rulesListView.Location = new System.Drawing.Point(7, 60);
            this.rulesListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.OwnerDraw = true;
            this.rulesListView.Size = new System.Drawing.Size(992, 843);
            this.rulesListView.TabIndex = 17;
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = System.Windows.Forms.View.Details;
            this.rulesListView.VirtualMode = true;
            this.rulesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            this.rulesListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.RulesListView_DrawItem);
            this.rulesListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.RulesListView_DrawSubItem);
            this.rulesListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ListView_RetrieveVirtualItem);
            this.rulesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RulesListView_MouseClick);
            this.rulesListView.MouseLeave += new System.EventHandler(this.ListView_MouseLeave);
            this.rulesListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseMove);
            // 
            // advIconColumn
            // 
            this.advIconColumn.Text = "";
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
            this.advProgramColumn.Text = "Program";
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
            // rulesContextMenu
            // 
            this.rulesContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.rulesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowToolStripMenuItem,
            this.blockToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteRuleToolStripMenuItem,
            this.toolStripSeparator2,
            this.openFileLocationToolStripMenuItem,
            this.toolStripSeparator3,
            this.copyDetailsToolStripMenuItem});
            this.rulesContextMenu.Name = "rulesContextMenu";
            this.rulesContextMenu.Size = new System.Drawing.Size(207, 166);
            this.rulesContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.rulesContextMenu_Opening);
            // 
            // allowToolStripMenuItem
            // 
            this.allowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowOutboundToolStripMenuItem,
            this.allowInboundToolStripMenuItem,
            this.allowAllToolStripMenuItem});
            this.allowToolStripMenuItem.Name = "allowToolStripMenuItem";
            this.allowToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.allowToolStripMenuItem.Text = "Allow";
            // 
            // allowOutboundToolStripMenuItem
            // 
            this.allowOutboundToolStripMenuItem.Name = "allowOutboundToolStripMenuItem";
            this.allowOutboundToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.allowOutboundToolStripMenuItem.Tag = "Allow (Outbound)";
            this.allowOutboundToolStripMenuItem.Text = "Outbound";
            this.allowOutboundToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // allowInboundToolStripMenuItem
            // 
            this.allowInboundToolStripMenuItem.Name = "allowInboundToolStripMenuItem";
            this.allowInboundToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.allowInboundToolStripMenuItem.Tag = "Allow (Inbound)";
            this.allowInboundToolStripMenuItem.Text = "Inbound";
            this.allowInboundToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // allowAllToolStripMenuItem
            // 
            this.allowAllToolStripMenuItem.Name = "allowAllToolStripMenuItem";
            this.allowAllToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.allowAllToolStripMenuItem.Tag = "Allow (All)";
            this.allowAllToolStripMenuItem.Text = "All";
            this.allowAllToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // blockToolStripMenuItem
            // 
            this.blockToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blockOutboundToolStripMenuItem,
            this.blockInboundToolStripMenuItem,
            this.blockAllToolStripMenuItem});
            this.blockToolStripMenuItem.Name = "blockToolStripMenuItem";
            this.blockToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.blockToolStripMenuItem.Text = "Block";
            // 
            // blockOutboundToolStripMenuItem
            // 
            this.blockOutboundToolStripMenuItem.Name = "blockOutboundToolStripMenuItem";
            this.blockOutboundToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.blockOutboundToolStripMenuItem.Tag = "Block (Outbound)";
            this.blockOutboundToolStripMenuItem.Text = "Outbound";
            this.blockOutboundToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // blockInboundToolStripMenuItem
            // 
            this.blockInboundToolStripMenuItem.Name = "blockInboundToolStripMenuItem";
            this.blockInboundToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.blockInboundToolStripMenuItem.Tag = "Block (Inbound)";
            this.blockInboundToolStripMenuItem.Text = "Inbound";
            this.blockInboundToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // blockAllToolStripMenuItem
            // 
            this.blockAllToolStripMenuItem.Name = "blockAllToolStripMenuItem";
            this.blockAllToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.blockAllToolStripMenuItem.Tag = "Block (All)";
            this.blockAllToolStripMenuItem.Text = "All";
            this.blockAllToolStripMenuItem.Click += new System.EventHandler(this.ApplyRuleMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(203, 6);
            // 
            // deleteRuleToolStripMenuItem
            // 
            this.deleteRuleToolStripMenuItem.Name = "deleteRuleToolStripMenuItem";
            this.deleteRuleToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.deleteRuleToolStripMenuItem.Text = "Delete Rule(s)";
            this.deleteRuleToolStripMenuItem.Click += new System.EventHandler(this.DeleteRuleMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(203, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.openFileLocationToolStripMenuItem.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(203, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // RulesControl
            // 
            this.Controls.Add(this.advFilterAdvancedCheck);
            this.Controls.Add(this.advancedRuleButton);
            this.Controls.Add(this.advFilterWildcardCheck);
            this.Controls.Add(this.advFilterUwpCheck);
            this.Controls.Add(this.advFilterServiceCheck);
            this.Controls.Add(this.advFilterProgramCheck);
            this.Controls.Add(this.rulesSearchTextBox);
            this.Controls.Add(this.createRuleButton);
            this.Controls.Add(this.rulesListView);
            this.Name = "RulesControl";
            this.Size = new System.Drawing.Size(1002, 911);
            this.rulesContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}