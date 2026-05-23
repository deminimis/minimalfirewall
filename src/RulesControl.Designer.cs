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
            if (disposing)
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.RulesListUpdated -= OnRulesListUpdated;
                }

                _searchDebounceTimer?.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var dataGridViewCellStyle1 = new DataGridViewCellStyle();
            rulesSearchTextBox = new TextBox();
            createRuleButton = new Button();
            rulesContextMenu = new ContextMenuStrip(components);
            allowToolStripMenuItem = new ToolStripMenuItem();
            allowOutboundToolStripMenuItem = new ToolStripMenuItem();
            allowInboundToolStripMenuItem = new ToolStripMenuItem();
            allowAllToolStripMenuItem = new ToolStripMenuItem();
            blockToolStripMenuItem = new ToolStripMenuItem();
            blockOutboundToolStripMenuItem = new ToolStripMenuItem();
            blockInboundToolStripMenuItem = new ToolStripMenuItem();
            blockAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            editRuleToolStripMenuItem = new ToolStripMenuItem();
            deleteRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            copyDetailsToolStripMenuItem = new ToolStripMenuItem();
            rulesDataGridView = new DataGridView();
            filterPanel = new FlowLayoutPanel();
            programFilterCheckBox = new CheckBox();
            serviceFilterCheckBox = new CheckBox();
            uwpFilterCheckBox = new CheckBox();
            wildcardFilterCheckBox = new CheckBox();
            systemFilterCheckBox = new CheckBox();
            topPanel = new TableLayoutPanel();
            advIconColumn = new DataGridViewImageColumn();
            advNameColumn = new DataGridViewTextBoxColumn();
            inboundStatusColumn = new DataGridViewTextBoxColumn();
            outboundStatusColumn = new DataGridViewTextBoxColumn();
            advProtocolColumn = new DataGridViewTextBoxColumn();
            advLocalPortsColumn = new DataGridViewTextBoxColumn();
            advRemotePortsColumn = new DataGridViewTextBoxColumn();
            advLocalAddressColumn = new DataGridViewTextBoxColumn();
            advRemoteAddressColumn = new DataGridViewTextBoxColumn();
            advProgramColumn = new DataGridViewTextBoxColumn();
            advServiceColumn = new DataGridViewTextBoxColumn();
            advProfilesColumn = new DataGridViewTextBoxColumn();
            advGroupingColumn = new DataGridViewTextBoxColumn();
            advDescColumn = new DataGridViewTextBoxColumn();
            dateAddedColumn = new DataGridViewTextBoxColumn();
            autoAllowedColumn = new DataGridViewTextBoxColumn();
            rulesContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rulesDataGridView).BeginInit();
            filterPanel.SuspendLayout();
            topPanel.SuspendLayout();
            SuspendLayout();
            // 
            // rulesSearchTextBox
            // 
            rulesSearchTextBox.Anchor = AnchorStyles.Right;
            rulesSearchTextBox.Location = new Point(705, 18);
            rulesSearchTextBox.Margin = new Padding(3, 4, 12, 4);
            rulesSearchTextBox.Name = "rulesSearchTextBox";
            rulesSearchTextBox.PlaceholderText = "Search rules...";
            rulesSearchTextBox.Size = new Size(285, 23);
            rulesSearchTextBox.TabIndex = 16;
            rulesSearchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // createRuleButton
            // 
            createRuleButton.Anchor = AnchorStyles.Left;
            createRuleButton.Location = new Point(12, 8);
            createRuleButton.Margin = new Padding(12, 8, 8, 8);
            createRuleButton.Name = "createRuleButton";
            createRuleButton.Size = new Size(160, 44);
            createRuleButton.TabIndex = 9;
            createRuleButton.Text = "Create New Rule...";
            createRuleButton.UseVisualStyleBackColor = true;
            createRuleButton.Click += CreateRuleButton_Click;
            // 
            // rulesContextMenu
            // 
            rulesContextMenu.ImageScalingSize = new Size(20, 20);
            rulesContextMenu.Items.AddRange(new ToolStripItem[] { allowToolStripMenuItem, blockToolStripMenuItem, toolStripSeparator1, editRuleToolStripMenuItem, deleteRuleToolStripMenuItem, toolStripSeparator2, openFileLocationToolStripMenuItem, toolStripSeparator3, copyDetailsToolStripMenuItem });
            rulesContextMenu.Name = "rulesContextMenu";
            rulesContextMenu.Size = new Size(174, 154);
            rulesContextMenu.Opening += rulesContextMenu_Opening;
            // 
            // allowToolStripMenuItem
            // 
            allowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allowOutboundToolStripMenuItem, allowInboundToolStripMenuItem, allowAllToolStripMenuItem });
            allowToolStripMenuItem.Name = "allowToolStripMenuItem";
            allowToolStripMenuItem.Size = new Size(173, 22);
            allowToolStripMenuItem.Text = "Allow";
            // 
            // allowOutboundToolStripMenuItem
            // 
            allowOutboundToolStripMenuItem.Name = "allowOutboundToolStripMenuItem";
            allowOutboundToolStripMenuItem.Size = new Size(129, 22);
            allowOutboundToolStripMenuItem.Tag = "Allow (Outbound)";
            allowOutboundToolStripMenuItem.Text = "Outbound";
            allowOutboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // allowInboundToolStripMenuItem
            // 
            allowInboundToolStripMenuItem.Name = "allowInboundToolStripMenuItem";
            allowInboundToolStripMenuItem.Size = new Size(129, 22);
            allowInboundToolStripMenuItem.Tag = "Allow (Inbound)";
            allowInboundToolStripMenuItem.Text = "Inbound";
            allowInboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // allowAllToolStripMenuItem
            // 
            allowAllToolStripMenuItem.Name = "allowAllToolStripMenuItem";
            allowAllToolStripMenuItem.Size = new Size(129, 22);
            allowAllToolStripMenuItem.Tag = "Allow (All)";
            allowAllToolStripMenuItem.Text = "All";
            allowAllToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockToolStripMenuItem
            // 
            blockToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { blockOutboundToolStripMenuItem, blockInboundToolStripMenuItem, blockAllToolStripMenuItem });
            blockToolStripMenuItem.Name = "blockToolStripMenuItem";
            blockToolStripMenuItem.Size = new Size(173, 22);
            blockToolStripMenuItem.Text = "Block";
            // 
            // blockOutboundToolStripMenuItem
            // 
            blockOutboundToolStripMenuItem.Name = "blockOutboundToolStripMenuItem";
            blockOutboundToolStripMenuItem.Size = new Size(129, 22);
            blockOutboundToolStripMenuItem.Tag = "Block (Outbound)";
            blockOutboundToolStripMenuItem.Text = "Outbound";
            blockOutboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockInboundToolStripMenuItem
            // 
            blockInboundToolStripMenuItem.Name = "blockInboundToolStripMenuItem";
            blockInboundToolStripMenuItem.Size = new Size(129, 22);
            blockInboundToolStripMenuItem.Tag = "Block (Inbound)";
            blockInboundToolStripMenuItem.Text = "Inbound";
            blockInboundToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // blockAllToolStripMenuItem
            // 
            blockAllToolStripMenuItem.Name = "blockAllToolStripMenuItem";
            blockAllToolStripMenuItem.Size = new Size(129, 22);
            blockAllToolStripMenuItem.Tag = "Block (All)";
            blockAllToolStripMenuItem.Text = "All";
            blockAllToolStripMenuItem.Click += ApplyRuleMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(170, 6);
            // 
            // editRuleToolStripMenuItem
            // 
            editRuleToolStripMenuItem.Name = "editRuleToolStripMenuItem";
            editRuleToolStripMenuItem.Size = new Size(173, 22);
            editRuleToolStripMenuItem.Text = "Edit Rule...";
            editRuleToolStripMenuItem.Click += editRuleToolStripMenuItem_Click;
            // 
            // deleteRuleToolStripMenuItem
            // 
            deleteRuleToolStripMenuItem.Name = "deleteRuleToolStripMenuItem";
            deleteRuleToolStripMenuItem.Size = new Size(173, 22);
            deleteRuleToolStripMenuItem.Text = "Delete Rule(s)";
            deleteRuleToolStripMenuItem.Click += DeleteRuleMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(170, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            openFileLocationToolStripMenuItem.Size = new Size(173, 22);
            openFileLocationToolStripMenuItem.Text = "Open File Location";
            openFileLocationToolStripMenuItem.Click += openFileLocationToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(170, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            copyDetailsToolStripMenuItem.Size = new Size(173, 22);
            copyDetailsToolStripMenuItem.Text = "Copy Details";
            copyDetailsToolStripMenuItem.Click += copyDetailsToolStripMenuItem_Click;
            // 
            // rulesDataGridView
            // 
            rulesDataGridView.AllowUserToAddRows = false;
            rulesDataGridView.AllowUserToDeleteRows = false;
            rulesDataGridView.AllowUserToResizeRows = false;
            rulesDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rulesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            rulesDataGridView.BackgroundColor = SystemColors.Control;
            rulesDataGridView.BorderStyle = BorderStyle.None;
            rulesDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            rulesDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            rulesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            rulesDataGridView.Columns.AddRange(new DataGridViewColumn[] { advIconColumn, advNameColumn, inboundStatusColumn, outboundStatusColumn, advProtocolColumn, advLocalPortsColumn, advRemotePortsColumn, advLocalAddressColumn, advRemoteAddressColumn, advProgramColumn, advServiceColumn, advProfilesColumn, advGroupingColumn, advDescColumn, dateAddedColumn, autoAllowedColumn });
            rulesDataGridView.ContextMenuStrip = rulesContextMenu;
            rulesDataGridView.EnableHeadersVisualStyles = false;
            rulesDataGridView.GridColor = SystemColors.Control;
            rulesDataGridView.Location = new Point(3, 66);
            rulesDataGridView.Margin = new Padding(3, 6, 3, 3);
            rulesDataGridView.Name = "rulesDataGridView";
            rulesDataGridView.ReadOnly = true;
            rulesDataGridView.RowHeadersVisible = false;
            rulesDataGridView.RowTemplate.Height = 28;
            rulesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            rulesDataGridView.Size = new Size(996, 839);
            rulesDataGridView.TabIndex = 18;
            rulesDataGridView.VirtualMode = true;
            rulesDataGridView.CellFormatting += rulesDataGridView_CellFormatting;
            rulesDataGridView.CellMouseDown += rulesDataGridView_CellMouseDown;
            rulesDataGridView.CellMouseEnter += rulesDataGridView_CellMouseEnter;
            rulesDataGridView.CellMouseLeave += rulesDataGridView_CellMouseLeave;
            rulesDataGridView.ColumnHeaderMouseClick += rulesDataGridView_ColumnHeaderMouseClick;
            rulesDataGridView.RowPostPaint += rulesDataGridView_RowPostPaint;
            // 
            // filterPanel
            // 
            filterPanel.Anchor = AnchorStyles.Left;
            filterPanel.AutoSize = true;
            filterPanel.Controls.Add(programFilterCheckBox);
            filterPanel.Controls.Add(serviceFilterCheckBox);
            filterPanel.Controls.Add(uwpFilterCheckBox);
            filterPanel.Controls.Add(wildcardFilterCheckBox);
            filterPanel.Controls.Add(systemFilterCheckBox);
            filterPanel.Location = new Point(188, 17);
            filterPanel.Margin = new Padding(8, 4, 8, 4);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(377, 26);
            filterPanel.TabIndex = 19;
            filterPanel.WrapContents = false;
            // 
            // programFilterCheckBox
            // 
            programFilterCheckBox.AutoSize = true;
            programFilterCheckBox.Checked = true;
            programFilterCheckBox.CheckState = CheckState.Checked;
            programFilterCheckBox.Location = new Point(3, 3);
            programFilterCheckBox.Name = "programFilterCheckBox";
            programFilterCheckBox.Size = new Size(75, 20);
            programFilterCheckBox.TabIndex = 0;
            programFilterCheckBox.Text = "Program";
            programFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // serviceFilterCheckBox
            // 
            serviceFilterCheckBox.AutoSize = true;
            serviceFilterCheckBox.Checked = true;
            serviceFilterCheckBox.CheckState = CheckState.Checked;
            serviceFilterCheckBox.Location = new Point(84, 3);
            serviceFilterCheckBox.Name = "serviceFilterCheckBox";
            serviceFilterCheckBox.Size = new Size(75, 20);
            serviceFilterCheckBox.TabIndex = 1;
            serviceFilterCheckBox.Text = "Service";
            serviceFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // uwpFilterCheckBox
            // 
            uwpFilterCheckBox.AutoSize = true;
            uwpFilterCheckBox.Checked = true;
            uwpFilterCheckBox.CheckState = CheckState.Checked;
            uwpFilterCheckBox.Location = new Point(165, 3);
            uwpFilterCheckBox.Name = "uwpFilterCheckBox";
            uwpFilterCheckBox.Size = new Size(47, 20);
            uwpFilterCheckBox.TabIndex = 2;
            uwpFilterCheckBox.Text = "UWP";
            uwpFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // wildcardFilterCheckBox
            // 
            wildcardFilterCheckBox.AutoSize = true;
            wildcardFilterCheckBox.Checked = true;
            wildcardFilterCheckBox.CheckState = CheckState.Checked;
            wildcardFilterCheckBox.Location = new Point(218, 3);
            wildcardFilterCheckBox.Name = "wildcardFilterCheckBox";
            wildcardFilterCheckBox.Size = new Size(82, 20);
            wildcardFilterCheckBox.TabIndex = 3;
            wildcardFilterCheckBox.Text = "Wildcard";
            wildcardFilterCheckBox.UseVisualStyleBackColor = true;
            // 
            // systemFilterCheckBox
            // 
            systemFilterCheckBox.AutoSize = true;
            systemFilterCheckBox.Location = new Point(306, 3);
            systemFilterCheckBox.Name = "systemFilterCheckBox";
            systemFilterCheckBox.Size = new Size(68, 20);
            systemFilterCheckBox.TabIndex = 5;
            systemFilterCheckBox.Text = "System";
            systemFilterCheckBox.UseVisualStyleBackColor = true;
            systemFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            // 
            // topPanel
            // 
            topPanel.ColumnCount = 3;
            topPanel.ColumnStyles.Add(new ColumnStyle());
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topPanel.ColumnStyles.Add(new ColumnStyle());
            topPanel.Controls.Add(createRuleButton, 0, 0);
            topPanel.Controls.Add(rulesSearchTextBox, 2, 0);
            topPanel.Controls.Add(filterPanel, 1, 0);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Margin = new Padding(0);
            topPanel.Name = "topPanel";
            topPanel.RowCount = 1;
            topPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            topPanel.Size = new Size(1002, 60);
            topPanel.TabIndex = 20;
            // 
            // advIconColumn
            // 
            advIconColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            advIconColumn.FillWeight = 3F;
            advIconColumn.HeaderText = "";
            advIconColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            advIconColumn.MinimumWidth = 32;
            advIconColumn.Name = "advIconColumn";
            advIconColumn.ReadOnly = true;
            advIconColumn.Resizable = DataGridViewTriState.False;
            advIconColumn.Width = 32;
            // 
            // advNameColumn
            // 
            advNameColumn.DataPropertyName = "Name";
            advNameColumn.FillWeight = 20F;
            advNameColumn.HeaderText = "Name";
            advNameColumn.Name = "advNameColumn";
            advNameColumn.ReadOnly = true;
            advNameColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // inboundStatusColumn
            // 
            inboundStatusColumn.DataPropertyName = "InboundStatus";
            inboundStatusColumn.FillWeight = 10F;
            inboundStatusColumn.HeaderText = "Inbound";
            inboundStatusColumn.Name = "inboundStatusColumn";
            inboundStatusColumn.ReadOnly = true;
            inboundStatusColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // outboundStatusColumn
            // 
            outboundStatusColumn.DataPropertyName = "OutboundStatus";
            outboundStatusColumn.FillWeight = 10F;
            outboundStatusColumn.HeaderText = "Outbound";
            outboundStatusColumn.Name = "outboundStatusColumn";
            outboundStatusColumn.ReadOnly = true;
            outboundStatusColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advProtocolColumn
            // 
            advProtocolColumn.DataPropertyName = "ProtocolName";
            advProtocolColumn.FillWeight = 8F;
            advProtocolColumn.HeaderText = "Protocol";
            advProtocolColumn.Name = "advProtocolColumn";
            advProtocolColumn.ReadOnly = true;
            advProtocolColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advLocalPortsColumn
            // 
            advLocalPortsColumn.DataPropertyName = "LocalPorts";
            advLocalPortsColumn.FillWeight = 12F;
            advLocalPortsColumn.HeaderText = "Local Ports";
            advLocalPortsColumn.Name = "advLocalPortsColumn";
            advLocalPortsColumn.ReadOnly = true;
            advLocalPortsColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advRemotePortsColumn
            // 
            advRemotePortsColumn.DataPropertyName = "RemotePorts";
            advRemotePortsColumn.FillWeight = 12F;
            advRemotePortsColumn.HeaderText = "Remote Ports";
            advRemotePortsColumn.Name = "advRemotePortsColumn";
            advRemotePortsColumn.ReadOnly = true;
            advRemotePortsColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advLocalAddressColumn
            // 
            advLocalAddressColumn.DataPropertyName = "LocalAddresses";
            advLocalAddressColumn.FillWeight = 15F;
            advLocalAddressColumn.HeaderText = "Local Address";
            advLocalAddressColumn.Name = "advLocalAddressColumn";
            advLocalAddressColumn.ReadOnly = true;
            advLocalAddressColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advRemoteAddressColumn
            // 
            advRemoteAddressColumn.DataPropertyName = "RemoteAddresses";
            advRemoteAddressColumn.FillWeight = 15F;
            advRemoteAddressColumn.HeaderText = "Remote Address";
            advRemoteAddressColumn.Name = "advRemoteAddressColumn";
            advRemoteAddressColumn.ReadOnly = true;
            advRemoteAddressColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advProgramColumn
            // 
            advProgramColumn.DataPropertyName = "ApplicationName";
            advProgramColumn.FillWeight = 25F;
            advProgramColumn.HeaderText = "Program";
            advProgramColumn.Name = "advProgramColumn";
            advProgramColumn.ReadOnly = true;
            advProgramColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advServiceColumn
            // 
            advServiceColumn.DataPropertyName = "ServiceName";
            advServiceColumn.FillWeight = 15F;
            advServiceColumn.HeaderText = "Service";
            advServiceColumn.Name = "advServiceColumn";
            advServiceColumn.ReadOnly = true;
            advServiceColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advProfilesColumn
            // 
            advProfilesColumn.DataPropertyName = "Profiles";
            advProfilesColumn.FillWeight = 10F;
            advProfilesColumn.HeaderText = "Profiles";
            advProfilesColumn.Name = "advProfilesColumn";
            advProfilesColumn.ReadOnly = true;
            advProfilesColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advGroupingColumn
            // 
            advGroupingColumn.DataPropertyName = "Grouping";
            advGroupingColumn.FillWeight = 15F;
            advGroupingColumn.HeaderText = "Group";
            advGroupingColumn.Name = "advGroupingColumn";
            advGroupingColumn.ReadOnly = true;
            advGroupingColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advDescColumn
            // 
            advDescColumn.DataPropertyName = "Description";
            advDescColumn.FillWeight = 30F;
            advDescColumn.HeaderText = "Description";
            advDescColumn.Name = "advDescColumn";
            advDescColumn.ReadOnly = true;
            advDescColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // dateAddedColumn
            // 
            dateAddedColumn.DataPropertyName = "DateAdded";
            dataGridViewCellStyle1.Format = "yyyy-MM-dd HH:mm";
            dateAddedColumn.DefaultCellStyle = dataGridViewCellStyle1;
            dateAddedColumn.FillWeight = 12F;
            dateAddedColumn.HeaderText = "Date Added";
            dateAddedColumn.Name = "dateAddedColumn";
            dateAddedColumn.ReadOnly = true;
            dateAddedColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // autoAllowedColumn
            // 
            autoAllowedColumn.DataPropertyName = "AutoAllowedPublisher";
            autoAllowedColumn.FillWeight = 8F;
            autoAllowedColumn.HeaderText = "Origin";
            autoAllowedColumn.Name = "autoAllowedColumn";
            autoAllowedColumn.ReadOnly = true;
            autoAllowedColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // RulesControl
            // 
            Controls.Add(topPanel);
            Controls.Add(rulesDataGridView);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "RulesControl";
            Size = new Size(1002, 911);
            rulesContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rulesDataGridView).EndInit();
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ResumeLayout(false);
        }

        private DataGridViewImageColumn advIconColumn;
        private DataGridViewTextBoxColumn advNameColumn;
        private DataGridViewTextBoxColumn inboundStatusColumn;
        private DataGridViewTextBoxColumn outboundStatusColumn;
        private DataGridViewTextBoxColumn advProtocolColumn;
        private DataGridViewTextBoxColumn advLocalPortsColumn;
        private DataGridViewTextBoxColumn advRemotePortsColumn;
        private DataGridViewTextBoxColumn advLocalAddressColumn;
        private DataGridViewTextBoxColumn advRemoteAddressColumn;
        private DataGridViewTextBoxColumn advProgramColumn;
        private DataGridViewTextBoxColumn advServiceColumn;
        private DataGridViewTextBoxColumn advProfilesColumn;
        private DataGridViewTextBoxColumn advGroupingColumn;
        private DataGridViewTextBoxColumn advDescColumn;
        private DataGridViewTextBoxColumn dateAddedColumn;
        private DataGridViewTextBoxColumn autoAllowedColumn;
    }
}
