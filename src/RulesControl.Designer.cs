// File: RulesControl.Designer.cs
namespace MinimalFirewall
{
    partial class RulesControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button createRuleButton;
        private System.Windows.Forms.TextBox rulesSearchTextBox;
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
        private System.Windows.Forms.DataGridView rulesDataGridView;
        private System.Windows.Forms.DataGridViewImageColumn advIconColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn inboundStatusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn outboundStatusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advProtocolColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advLocalPortsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advRemotePortsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advLocalAddressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advRemoteAddressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advProgramColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advServiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advProfilesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advGroupingColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advDescColumn;
        private System.Windows.Forms.FlowLayoutPanel filterPanel;
        private System.Windows.Forms.CheckBox programFilterCheckBox;
        private System.Windows.Forms.CheckBox serviceFilterCheckBox;
        private System.Windows.Forms.CheckBox uwpFilterCheckBox;
        private System.Windows.Forms.CheckBox wildcardFilterCheckBox;
        private System.Windows.Forms.TableLayoutPanel topPanel;
        private System.Windows.Forms.ToolStripMenuItem editRuleToolStripMenuItem;
        private System.Windows.Forms.CheckBox systemFilterCheckBox;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.rulesSearchTextBox = new System.Windows.Forms.TextBox();
            this.createRuleButton = new System.Windows.Forms.Button();
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
            this.editRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rulesDataGridView = new System.Windows.Forms.DataGridView();
            this.advIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.advNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.inboundStatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.outboundStatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advProtocolColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advLocalPortsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advRemotePortsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advLocalAddressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advRemoteAddressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advProgramColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advServiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advProfilesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advGroupingColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advDescColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filterPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.programFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.serviceFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.uwpFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.wildcardFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.systemFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.topPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rulesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rulesDataGridView)).BeginInit();
            this.filterPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // rulesSearchTextBox
            // 
            this.rulesSearchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.rulesSearchTextBox.Location = new System.Drawing.Point(714, 18);
            this.rulesSearchTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rulesSearchTextBox.Name = "rulesSearchTextBox";
            this.rulesSearchTextBox.PlaceholderText = "Search rules...";
            this.rulesSearchTextBox.Size = new System.Drawing.Size(285, 27);
            this.rulesSearchTextBox.TabIndex = 16;
            this.rulesSearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            // 
            // createRuleButton
            // 
            this.createRuleButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createRuleButton.Location = new System.Drawing.Point(3, 7);
            this.createRuleButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.createRuleButton.Name = "createRuleButton";
            this.createRuleButton.Size = new System.Drawing.Size(180, 48);
            this.createRuleButton.TabIndex = 9;
            this.createRuleButton.Text = "Create New Rule...";
            this.createRuleButton.UseVisualStyleBackColor = true;
            this.createRuleButton.Click += new System.EventHandler(this.CreateRuleButton_Click);
            // 
            // rulesContextMenu
            // 
            this.rulesContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.rulesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowToolStripMenuItem,
            this.blockToolStripMenuItem,
            this.toolStripSeparator1,
            this.editRuleToolStripMenuItem,
            this.deleteRuleToolStripMenuItem,
            this.toolStripSeparator2,
            this.openFileLocationToolStripMenuItem,
            this.toolStripSeparator3,
            this.copyDetailsToolStripMenuItem});
            this.rulesContextMenu.Name = "rulesContextMenu";
            this.rulesContextMenu.Size = new System.Drawing.Size(207, 194);
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
            // editRuleToolStripMenuItem
            // 
            this.editRuleToolStripMenuItem.Name = "editRuleToolStripMenuItem";
            this.editRuleToolStripMenuItem.Size = new System.Drawing.Size(206, 24);
            this.editRuleToolStripMenuItem.Text = "Edit Rule...";
            this.editRuleToolStripMenuItem.Click += new System.EventHandler(this.editRuleToolStripMenuItem_Click);
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
            // rulesDataGridView
            // 
            this.rulesDataGridView.AllowUserToAddRows = false;
            this.rulesDataGridView.AllowUserToDeleteRows = false;
            this.rulesDataGridView.AllowUserToResizeRows = false;
            this.rulesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rulesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.rulesDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.rulesDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rulesDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.rulesDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.rulesDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.rulesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.rulesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.advIconColumn,
            this.advNameColumn,
            this.inboundStatusColumn,
            this.outboundStatusColumn,
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
            this.rulesDataGridView.ContextMenuStrip = this.rulesContextMenu;
            this.rulesDataGridView.EnableHeadersVisualStyles = false;
            this.rulesDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.rulesDataGridView.Location = new System.Drawing.Point(3, 69);
            this.rulesDataGridView.Name = "rulesDataGridView";
            this.rulesDataGridView.ReadOnly = true;
            this.rulesDataGridView.RowHeadersVisible = false;
            this.rulesDataGridView.RowTemplate.Height = 28;
            this.rulesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.rulesDataGridView.ShowCellToolTips = true;
            this.rulesDataGridView.Size = new System.Drawing.Size(996, 839);
            this.rulesDataGridView.TabIndex = 18;
            this.rulesDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.rulesDataGridView_CellFormatting);
            this.rulesDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.rulesDataGridView_CellMouseDown);
            this.rulesDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.rulesDataGridView_CellMouseEnter);
            this.rulesDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.rulesDataGridView_CellMouseLeave);
            this.rulesDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.rulesDataGridView_ColumnHeaderMouseClick);
            this.rulesDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.rulesDataGridView_RowPostPaint);
            // 
            // advIconColumn
            // 
            this.advIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.advIconColumn.FillWeight = 3F;
            this.advIconColumn.HeaderText = "";
            this.advIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.advIconColumn.MinimumWidth = 32;
            this.advIconColumn.Name = "advIconColumn";
            this.advIconColumn.ReadOnly = true;
            this.advIconColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.advIconColumn.Width = 32;
            // 
            // advNameColumn
            // 
            this.advNameColumn.DataPropertyName = "Name";
            this.advNameColumn.FillWeight = 20F;
            this.advNameColumn.HeaderText = "Name";
            this.advNameColumn.Name = "advNameColumn";
            this.advNameColumn.ReadOnly = true;
            // 
            // inboundStatusColumn
            // 
            this.inboundStatusColumn.DataPropertyName = "InboundStatus";
            this.inboundStatusColumn.FillWeight = 10F;
            this.inboundStatusColumn.HeaderText = "Inbound";
            this.inboundStatusColumn.Name = "inboundStatusColumn";
            this.inboundStatusColumn.ReadOnly = true;
            // 
            // outboundStatusColumn
            // 
            this.outboundStatusColumn.DataPropertyName = "OutboundStatus";
            this.outboundStatusColumn.FillWeight = 10F;
            this.outboundStatusColumn.HeaderText = "Outbound";
            this.outboundStatusColumn.Name = "outboundStatusColumn";
            this.outboundStatusColumn.ReadOnly = true;
            // 
            // advProtocolColumn
            // 
            this.advProtocolColumn.DataPropertyName = "ProtocolName";
            this.advProtocolColumn.FillWeight = 8F;
            this.advProtocolColumn.HeaderText = "Protocol";
            this.advProtocolColumn.Name = "advProtocolColumn";
            this.advProtocolColumn.ReadOnly = true;
            // 
            // advLocalPortsColumn
            // 
            this.advLocalPortsColumn.DataPropertyName = "LocalPorts";
            this.advLocalPortsColumn.FillWeight = 12F;
            this.advLocalPortsColumn.HeaderText = "Local Ports";
            this.advLocalPortsColumn.Name = "advLocalPortsColumn";
            this.advLocalPortsColumn.ReadOnly = true;
            // 
            // advRemotePortsColumn
            // 
            this.advRemotePortsColumn.DataPropertyName = "RemotePorts";
            this.advRemotePortsColumn.FillWeight = 12F;
            this.advRemotePortsColumn.HeaderText = "Remote Ports";
            this.advRemotePortsColumn.Name = "advRemotePortsColumn";
            this.advRemotePortsColumn.ReadOnly = true;
            // 
            // advLocalAddressColumn
            // 
            this.advLocalAddressColumn.DataPropertyName = "LocalAddresses";
            this.advLocalAddressColumn.FillWeight = 15F;
            this.advLocalAddressColumn.HeaderText = "Local Address";
            this.advLocalAddressColumn.Name = "advLocalAddressColumn";
            this.advLocalAddressColumn.ReadOnly = true;
            // 
            // advRemoteAddressColumn
            // 
            this.advRemoteAddressColumn.DataPropertyName = "RemoteAddresses";
            this.advRemoteAddressColumn.FillWeight = 15F;
            this.advRemoteAddressColumn.HeaderText = "Remote Address";
            this.advRemoteAddressColumn.Name = "advRemoteAddressColumn";
            this.advRemoteAddressColumn.ReadOnly = true;
            // 
            // advProgramColumn
            // 
            this.advProgramColumn.DataPropertyName = "ApplicationName";
            this.advProgramColumn.FillWeight = 25F;
            this.advProgramColumn.HeaderText = "Program";
            this.advProgramColumn.Name = "advProgramColumn";
            this.advProgramColumn.ReadOnly = true;
            // 
            // advServiceColumn
            // 
            this.advServiceColumn.DataPropertyName = "ServiceName";
            this.advServiceColumn.FillWeight = 15F;
            this.advServiceColumn.HeaderText = "Service";
            this.advServiceColumn.Name = "advServiceColumn";
            this.advServiceColumn.ReadOnly = true;
            // 
            // advProfilesColumn
            // 
            this.advProfilesColumn.DataPropertyName = "Profiles";
            this.advProfilesColumn.FillWeight = 10F;
            this.advProfilesColumn.HeaderText = "Profiles";
            this.advProfilesColumn.Name = "advProfilesColumn";
            this.advProfilesColumn.ReadOnly = true;
            // 
            // advGroupingColumn
            // 
            this.advGroupingColumn.DataPropertyName = "Grouping";
            this.advGroupingColumn.FillWeight = 15F;
            this.advGroupingColumn.HeaderText = "Group";
            this.advGroupingColumn.Name = "advGroupingColumn";
            this.advGroupingColumn.ReadOnly = true;
            // 
            // advDescColumn
            // 
            this.advDescColumn.DataPropertyName = "Description";
            this.advDescColumn.FillWeight = 30F;
            this.advDescColumn.HeaderText = "Description";
            this.advDescColumn.Name = "advDescColumn";
            this.advDescColumn.ReadOnly = true;
            // 
            // filterPanel
            // 
            this.filterPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.filterPanel.AutoSize = true;
            this.filterPanel.Controls.Add(this.programFilterCheckBox);
            this.filterPanel.Controls.Add(this.serviceFilterCheckBox);
            this.filterPanel.Controls.Add(this.uwpFilterCheckBox);
            this.filterPanel.Controls.Add(this.wildcardFilterCheckBox);
            this.filterPanel.Controls.Add(this.systemFilterCheckBox);
            this.filterPanel.Location = new System.Drawing.Point(189, 15);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(421, 32);
            this.filterPanel.TabIndex = 19;
            this.filterPanel.WrapContents = false;
            // 
            // programFilterCheckBox
            // 
            this.programFilterCheckBox.AutoSize = true;
            this.programFilterCheckBox.Checked = true;
            this.programFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.programFilterCheckBox.Location = new System.Drawing.Point(3, 3);
            this.programFilterCheckBox.Name = "programFilterCheckBox";
            this.programFilterCheckBox.Size = new System.Drawing.Size(91, 24);
            this.programFilterCheckBox.TabIndex = 0;
            this.programFilterCheckBox.Text = "Program";
            this.programFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // serviceFilterCheckBox
            // 
            this.serviceFilterCheckBox.AutoSize = true;
            this.serviceFilterCheckBox.Checked = true;
            this.serviceFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.serviceFilterCheckBox.Location = new System.Drawing.Point(100, 3);
            this.serviceFilterCheckBox.Name = "serviceFilterCheckBox";
            this.serviceFilterCheckBox.Size = new System.Drawing.Size(78, 24);
            this.serviceFilterCheckBox.TabIndex = 1;
            this.serviceFilterCheckBox.Text = "Service";
            this.serviceFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // uwpFilterCheckBox
            // 
            this.uwpFilterCheckBox.AutoSize = true;
            this.uwpFilterCheckBox.Checked = true;
            this.uwpFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.uwpFilterCheckBox.Location = new System.Drawing.Point(184, 3);
            this.uwpFilterCheckBox.Name = "uwpFilterCheckBox";
            this.uwpFilterCheckBox.Size = new System.Drawing.Size(64, 24);
            this.uwpFilterCheckBox.TabIndex = 2;
            this.uwpFilterCheckBox.Text = "UWP";
            this.uwpFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // wildcardFilterCheckBox
            // 
            this.wildcardFilterCheckBox.AutoSize = true;
            this.wildcardFilterCheckBox.Checked = true;
            this.wildcardFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wildcardFilterCheckBox.Location = new System.Drawing.Point(254, 3);
            this.wildcardFilterCheckBox.Name = "wildcardFilterCheckBox";
            this.wildcardFilterCheckBox.Size = new System.Drawing.Size(88, 24);
            this.wildcardFilterCheckBox.TabIndex = 3;
            this.wildcardFilterCheckBox.Text = "Wildcard";
            this.wildcardFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // systemFilterCheckBox
            // 
            this.systemFilterCheckBox.AutoSize = true;
            this.systemFilterCheckBox.Location = new System.Drawing.Point(348, 3);
            this.systemFilterCheckBox.Name = "systemFilterCheckBox";
            this.systemFilterCheckBox.Size = new System.Drawing.Size(79, 24);
            this.systemFilterCheckBox.TabIndex = 5;
            this.systemFilterCheckBox.Text = "System";
            this.systemFilterCheckBox.UseVisualStyleBackColor = true;
            this.systemFilterCheckBox.CheckedChanged += new System.EventHandler(this.filterCheckBox_CheckedChanged);
            // 
            // topPanel
            // 
            this.topPanel.ColumnCount = 3;
            this.topPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.topPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.topPanel.Controls.Add(this.createRuleButton, 0, 0);
            this.topPanel.Controls.Add(this.rulesSearchTextBox, 2, 0);
            this.topPanel.Controls.Add(this.filterPanel, 1, 0);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.RowCount = 1;
            this.topPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topPanel.Size = new System.Drawing.Size(1002, 63);
            this.topPanel.TabIndex = 20;
            // 
            // RulesControl
            // 
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.rulesDataGridView);
            this.Name = "RulesControl";
            this.Size = new System.Drawing.Size(1002, 911);
            this.rulesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rulesDataGridView)).EndInit();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
