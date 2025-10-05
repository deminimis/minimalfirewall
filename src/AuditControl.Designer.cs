namespace MinimalFirewall
{
    partial class AuditControl
    {
        private System.Windows.Forms.TextBox auditSearchTextBox;
        private System.Windows.Forms.Button rebuildBaselineButton;
        private System.Windows.Forms.ContextMenuStrip auditContextMenu;
        private System.Windows.Forms.ToolStripMenuItem acceptAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.DataGridView systemChangesDataGridView;
        private System.Windows.Forms.DataGridViewButtonColumn acceptButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn deleteButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn advStatusColumn;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.auditSearchTextBox = new System.Windows.Forms.TextBox();
            this.rebuildBaselineButton = new System.Windows.Forms.Button();
            this.auditContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.acceptAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPanel = new System.Windows.Forms.Panel();
            this.systemChangesDataGridView = new System.Windows.Forms.DataGridView();
            this.acceptButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.deleteButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.advNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.advStatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.auditContextMenu.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.systemChangesDataGridView)).BeginInit();
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
            // systemChangesDataGridView
            // 
            this.systemChangesDataGridView.AllowUserToAddRows = false;
            this.systemChangesDataGridView.AllowUserToDeleteRows = false;
            this.systemChangesDataGridView.AllowUserToResizeRows = false;
            this.systemChangesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.systemChangesDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.systemChangesDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.systemChangesDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.systemChangesDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.systemChangesDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.systemChangesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.systemChangesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.acceptButtonColumn,
            this.deleteButtonColumn,
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
            this.systemChangesDataGridView.ContextMenuStrip = this.auditContextMenu;
            this.systemChangesDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemChangesDataGridView.EnableHeadersVisualStyles = false;
            this.systemChangesDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.systemChangesDataGridView.Location = new System.Drawing.Point(0, 77);
            this.systemChangesDataGridView.MultiSelect = true;
            this.systemChangesDataGridView.Name = "systemChangesDataGridView";
            this.systemChangesDataGridView.ReadOnly = true;
            this.systemChangesDataGridView.RowHeadersVisible = false;
            this.systemChangesDataGridView.RowTemplate.Height = 28;
            this.systemChangesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.systemChangesDataGridView.ShowCellToolTips = true;
            this.systemChangesDataGridView.Size = new System.Drawing.Size(1000, 843);
            this.systemChangesDataGridView.TabIndex = 5;
            this.systemChangesDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.systemChangesDataGridView_CellContentClick);
            this.systemChangesDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.systemChangesDataGridView_CellFormatting);
            this.systemChangesDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.systemChangesDataGridView_CellMouseDown);
            this.systemChangesDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.systemChangesDataGridView_CellMouseEnter);
            this.systemChangesDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.systemChangesDataGridView_CellMouseLeave);
            this.systemChangesDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.systemChangesDataGridView_ColumnHeaderMouseClick);
            this.systemChangesDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.systemChangesDataGridView_RowPostPaint);
            // 
            // acceptButtonColumn
            // 
            this.acceptButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.acceptButtonColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.acceptButtonColumn.FillWeight = 15F;
            this.acceptButtonColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.acceptButtonColumn.HeaderText = "Action";
            this.acceptButtonColumn.MinimumWidth = 70;
            this.acceptButtonColumn.Name = "acceptButtonColumn";
            this.acceptButtonColumn.ReadOnly = true;
            this.acceptButtonColumn.Text = "Accept";
            this.acceptButtonColumn.UseColumnTextForButtonValue = true;
            this.acceptButtonColumn.Width = 70;
            // 
            // deleteButtonColumn
            // 
            this.deleteButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.deleteButtonColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.deleteButtonColumn.FillWeight = 15F;
            this.deleteButtonColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteButtonColumn.HeaderText = "";
            this.deleteButtonColumn.MinimumWidth = 70;
            this.deleteButtonColumn.Name = "deleteButtonColumn";
            this.deleteButtonColumn.ReadOnly = true;
            this.deleteButtonColumn.Text = "Delete";
            this.deleteButtonColumn.UseColumnTextForButtonValue = true;
            this.deleteButtonColumn.Width = 70;
            // 
            // advNameColumn
            // 
            this.advNameColumn.DataPropertyName = "Rule.Name";
            this.advNameColumn.FillWeight = 20F;
            this.advNameColumn.HeaderText = "Name";
            this.advNameColumn.Name = "advNameColumn";
            this.advNameColumn.ReadOnly = true;
            // 
            // advStatusColumn
            // 
            this.advStatusColumn.DataPropertyName = "Rule.Status";
            this.advStatusColumn.FillWeight = 15F;
            this.advStatusColumn.HeaderText = "Action";
            this.advStatusColumn.Name = "advStatusColumn";
            this.advStatusColumn.ReadOnly = true;
            // 
            // advProtocolColumn
            // 
            this.advProtocolColumn.DataPropertyName = "Rule.ProtocolName";
            this.advProtocolColumn.FillWeight = 8F;
            this.advProtocolColumn.HeaderText = "Protocol";
            this.advProtocolColumn.Name = "advProtocolColumn";
            this.advProtocolColumn.ReadOnly = true;
            // 
            // advLocalPortsColumn
            // 
            this.advLocalPortsColumn.DataPropertyName = "Rule.LocalPorts";
            this.advLocalPortsColumn.FillWeight = 12F;
            this.advLocalPortsColumn.HeaderText = "Local Ports";
            this.advLocalPortsColumn.Name = "advLocalPortsColumn";
            this.advLocalPortsColumn.ReadOnly = true;
            // 
            // advRemotePortsColumn
            // 
            this.advRemotePortsColumn.DataPropertyName = "Rule.RemotePorts";
            this.advRemotePortsColumn.FillWeight = 12F;
            this.advRemotePortsColumn.HeaderText = "Remote Ports";
            this.advRemotePortsColumn.Name = "advRemotePortsColumn";
            this.advRemotePortsColumn.ReadOnly = true;
            // 
            // advLocalAddressColumn
            // 
            this.advLocalAddressColumn.DataPropertyName = "Rule.LocalAddresses";
            this.advLocalAddressColumn.FillWeight = 15F;
            this.advLocalAddressColumn.HeaderText = "Local Address";
            this.advLocalAddressColumn.Name = "advLocalAddressColumn";
            this.advLocalAddressColumn.ReadOnly = true;
            // 
            // advRemoteAddressColumn
            // 
            this.advRemoteAddressColumn.DataPropertyName = "Rule.RemoteAddresses";
            this.advRemoteAddressColumn.FillWeight = 15F;
            this.advRemoteAddressColumn.HeaderText = "Remote Address";
            this.advRemoteAddressColumn.Name = "advRemoteAddressColumn";
            this.advRemoteAddressColumn.ReadOnly = true;
            // 
            // advProgramColumn
            // 
            this.advProgramColumn.DataPropertyName = "Rule.ApplicationName";
            this.advProgramColumn.FillWeight = 25F;
            this.advProgramColumn.HeaderText = "Application";
            this.advProgramColumn.Name = "advProgramColumn";
            this.advProgramColumn.ReadOnly = true;
            // 
            // advServiceColumn
            // 
            this.advServiceColumn.DataPropertyName = "Rule.ServiceName";
            this.advServiceColumn.FillWeight = 15F;
            this.advServiceColumn.HeaderText = "Service";
            this.advServiceColumn.Name = "advServiceColumn";
            this.advServiceColumn.ReadOnly = true;
            // 
            // advProfilesColumn
            // 
            this.advProfilesColumn.DataPropertyName = "Rule.Profiles";
            this.advProfilesColumn.FillWeight = 10F;
            this.advProfilesColumn.HeaderText = "Profiles";
            this.advProfilesColumn.Name = "advProfilesColumn";
            this.advProfilesColumn.ReadOnly = true;
            // 
            // advGroupingColumn
            // 
            this.advGroupingColumn.DataPropertyName = "Rule.Grouping";
            this.advGroupingColumn.FillWeight = 15F;
            this.advGroupingColumn.HeaderText = "Group";
            this.advGroupingColumn.Name = "advGroupingColumn";
            this.advGroupingColumn.ReadOnly = true;
            // 
            // advDescColumn
            // 
            this.advDescColumn.DataPropertyName = "Rule.Description";
            this.advDescColumn.FillWeight = 30F;
            this.advDescColumn.HeaderText = "Description";
            this.advDescColumn.Name = "advDescColumn";
            this.advDescColumn.ReadOnly = true;
            // 
            // AuditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.systemChangesDataGridView);
            this.Controls.Add(this.topPanel);
            this.Name = "AuditControl";
            this.Size = new System.Drawing.Size(1000, 920);
            this.auditContextMenu.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.systemChangesDataGridView)).EndInit();
            this.ResumeLayout(false);
        }
    }
}