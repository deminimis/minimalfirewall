namespace MinimalFirewall
{
    partial class AuditControl
    {
        private System.Windows.Forms.TextBox auditSearchTextBox;
        private System.Windows.Forms.Button rebuildBaselineButton;
        private System.Windows.Forms.ContextMenuStrip auditContextMenu;
        private System.Windows.Forms.ToolStripMenuItem archiveSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.CheckBox quarantineCheckBox;
        private System.Windows.Forms.DataGridView systemChangesDataGridView;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox diffRichTextBox;
        private System.Windows.Forms.Label diffLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn advPublisherColumn;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn advTimestampColumn;
        private System.Windows.Forms.ToolTip toolTip1;

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
            this.auditSearchTextBox = new System.Windows.Forms.TextBox();
            this.rebuildBaselineButton = new System.Windows.Forms.Button();
            this.auditContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.archiveSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPanel = new System.Windows.Forms.Panel();
            this.quarantineCheckBox = new System.Windows.Forms.CheckBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.systemChangesDataGridView = new System.Windows.Forms.DataGridView();
            this.diffLabel = new System.Windows.Forms.Label();
            this.diffRichTextBox = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.advPublisherColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.advTimestampColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.auditContextMenu.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.systemChangesDataGridView)).BeginInit();
            this.statusStrip1.SuspendLayout();
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
            this.archiveSelectedToolStripMenuItem,
            this.enableSelectedToolStripMenuItem,
            this.disableSelectedToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator2,
            this.copyDetailsToolStripMenuItem,
            this.openFileLocationToolStripMenuItem});
            this.auditContextMenu.Name = "auditContextMenu";
            this.auditContextMenu.Size = new System.Drawing.Size(211, 208);
            this.auditContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.auditContextMenu_Opening);
            // 
            // archiveSelectedToolStripMenuItem
            // 
            this.archiveSelectedToolStripMenuItem.Name = "archiveSelectedToolStripMenuItem";
            this.archiveSelectedToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.archiveSelectedToolStripMenuItem.Text = "Archive (Hide)";
            this.archiveSelectedToolStripMenuItem.Click += new System.EventHandler(this.archiveSelectedToolStripMenuItem_Click);
            // 
            // enableSelectedToolStripMenuItem
            // 
            this.enableSelectedToolStripMenuItem.Name = "enableSelectedToolStripMenuItem";
            this.enableSelectedToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.enableSelectedToolStripMenuItem.Text = "Enable Selected";
            this.enableSelectedToolStripMenuItem.Click += new System.EventHandler(this.enableSelectedToolStripMenuItem_Click);
            // 
            // disableSelectedToolStripMenuItem
            // 
            this.disableSelectedToolStripMenuItem.Name = "disableSelectedToolStripMenuItem";
            this.disableSelectedToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.disableSelectedToolStripMenuItem.Text = "Disable Selected";
            this.disableSelectedToolStripMenuItem.Click += new System.EventHandler(this.disableSelectedToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.deleteToolStripMenuItem.Text = "Delete Rule";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(207, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.openFileLocationToolStripMenuItem.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.quarantineCheckBox);
            this.topPanel.Controls.Add(this.rebuildBaselineButton);
            this.topPanel.Controls.Add(this.auditSearchTextBox);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1000, 77);
            this.topPanel.TabIndex = 4;
            // 
            // quarantineCheckBox
            // 
            this.quarantineCheckBox.AutoSize = true;
            this.quarantineCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.quarantineCheckBox.Location = new System.Drawing.Point(195, 29);
            this.quarantineCheckBox.Name = "quarantineCheckBox";
            this.quarantineCheckBox.Size = new System.Drawing.Size(196, 24);
            this.quarantineCheckBox.TabIndex = 4;
            this.quarantineCheckBox.Text = "Quarantine New Rules";
            this.quarantineCheckBox.UseVisualStyleBackColor = true;
            this.quarantineCheckBox.CheckedChanged += new System.EventHandler(this.quarantineCheckBox_CheckedChanged);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer.Location = new System.Drawing.Point(0, 77);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.systemChangesDataGridView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.diffRichTextBox);
            this.splitContainer.Panel2.Controls.Add(this.diffLabel);
            this.splitContainer.Panel2MinSize = 100;
            this.splitContainer.Size = new System.Drawing.Size(1000, 843);
            this.splitContainer.SplitterDistance = 643;
            this.splitContainer.TabIndex = 5;
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
            this.advTimestampColumn,
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
            this.advDescColumn,
            this.advPublisherColumn});
            this.systemChangesDataGridView.ContextMenuStrip = this.auditContextMenu;
            this.systemChangesDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemChangesDataGridView.EnableHeadersVisualStyles = false;
            this.systemChangesDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.systemChangesDataGridView.Location = new System.Drawing.Point(0, 0);
            this.systemChangesDataGridView.MultiSelect = true;
            this.systemChangesDataGridView.Name = "systemChangesDataGridView";
            this.systemChangesDataGridView.ReadOnly = true;
            this.systemChangesDataGridView.RowHeadersVisible = false;
            this.systemChangesDataGridView.RowTemplate.Height = 28;
            this.systemChangesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.systemChangesDataGridView.ShowCellToolTips = true;
            this.systemChangesDataGridView.Size = new System.Drawing.Size(1000, 643);
            this.systemChangesDataGridView.TabIndex = 0;
            this.systemChangesDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.systemChangesDataGridView_CellFormatting);
            this.systemChangesDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.systemChangesDataGridView_CellMouseDown);
            this.systemChangesDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.systemChangesDataGridView_CellMouseEnter);
            this.systemChangesDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.systemChangesDataGridView_CellMouseLeave);
            this.systemChangesDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.systemChangesDataGridView_ColumnHeaderMouseClick);
            this.systemChangesDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.systemChangesDataGridView_RowPostPaint);
            this.systemChangesDataGridView.SelectionChanged += new System.EventHandler(this.systemChangesDataGridView_SelectionChanged);
            // 
            // diffLabel
            // 
            this.diffLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.diffLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.diffLabel.Location = new System.Drawing.Point(0, 0);
            this.diffLabel.Name = "diffLabel";
            this.diffLabel.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
            this.diffLabel.Size = new System.Drawing.Size(1000, 25);
            this.diffLabel.TabIndex = 1;
            this.diffLabel.Text = "Change Details:";
            // 
            // diffRichTextBox
            // 
            this.diffRichTextBox.BackColor = System.Drawing.Color.White;
            this.diffRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.diffRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffRichTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.diffRichTextBox.Location = new System.Drawing.Point(0, 25);
            this.diffRichTextBox.Name = "diffRichTextBox";
            this.diffRichTextBox.ReadOnly = true;
            this.diffRichTextBox.Size = new System.Drawing.Size(1000, 171);
            this.diffRichTextBox.TabIndex = 2;
            this.diffRichTextBox.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 898);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // advPublisherColumn
            // 
            this.advPublisherColumn.DataPropertyName = "Publisher";
            this.advPublisherColumn.FillWeight = 25F;
            this.advPublisherColumn.HeaderText = "Verified Signer";
            this.advPublisherColumn.Name = "advPublisherColumn";
            this.advPublisherColumn.ReadOnly = true;
            // 
            // advTimestampColumn
            // 
            this.advTimestampColumn.DataPropertyName = "Timestamp";
            this.advTimestampColumn.HeaderText = "Detected";
            this.advTimestampColumn.Name = "advTimestampColumn";
            this.advTimestampColumn.ReadOnly = true;
            this.advTimestampColumn.FillWeight = 15F;
            // 
            // advNameColumn
            // 
            this.advNameColumn.DataPropertyName = "Name";
            this.advNameColumn.FillWeight = 20F;
            this.advNameColumn.HeaderText = "Name";
            this.advNameColumn.Name = "advNameColumn";
            this.advNameColumn.ReadOnly = true;
            // 
            // advStatusColumn
            // 
            this.advStatusColumn.DataPropertyName = "Status";
            this.advStatusColumn.FillWeight = 15F;
            this.advStatusColumn.HeaderText = "Action";
            this.advStatusColumn.Name = "advStatusColumn";
            this.advStatusColumn.ReadOnly = true;
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
            this.advProgramColumn.HeaderText = "Application";
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
            // AuditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.topPanel);
            this.Name = "AuditControl";
            this.Size = new System.Drawing.Size(1000, 920);
            this.auditContextMenu.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.systemChangesDataGridView)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}