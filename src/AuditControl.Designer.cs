namespace MinimalFirewall
{
    partial class AuditControl
    {
        private System.Windows.Forms.TextBox auditSearchTextBox;
        private System.Windows.Forms.ContextMenuStrip auditContextMenu;
        private System.Windows.Forms.ToolStripMenuItem enableSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.DataGridView systemChangesDataGridView;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox diffRichTextBox;
        private System.Windows.Forms.Label diffLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
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
            components = new System.ComponentModel.Container();
            var dataGridViewCellStyle1 = new DataGridViewCellStyle();
            auditSearchTextBox = new TextBox();
            auditContextMenu = new ContextMenuStrip(components);
            enableSelectedToolStripMenuItem = new ToolStripMenuItem();
            disableSelectedToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            copyDetailsToolStripMenuItem = new ToolStripMenuItem();
            openFileLocationToolStripMenuItem = new ToolStripMenuItem();
            topPanel = new Panel();
            splitContainer = new SplitContainer();
            systemChangesDataGridView = new DataGridView();
            diffRichTextBox = new RichTextBox();
            diffLabel = new Label();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolTip1 = new ToolTip(components);
            advTimestampColumn = new DataGridViewTextBoxColumn();
            advNameColumn = new DataGridViewTextBoxColumn();
            advInterventionColumn = new DataGridViewTextBoxColumn();
            advStatusColumn = new DataGridViewTextBoxColumn();
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
            advPublisherColumn = new DataGridViewTextBoxColumn();
            auditContextMenu.SuspendLayout();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)systemChangesDataGridView).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // auditSearchTextBox
            // 
            auditSearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            auditSearchTextBox.Location = new Point(619, 20);
            auditSearchTextBox.Name = "auditSearchTextBox";
            auditSearchTextBox.PlaceholderText = "Search history...";
            auditSearchTextBox.Size = new Size(250, 23);
            auditSearchTextBox.TabIndex = 3;
            auditSearchTextBox.TextChanged += AuditSearchTextBox_TextChanged;
            // 
            // auditContextMenu
            // 
            auditContextMenu.ImageScalingSize = new Size(20, 20);
            auditContextMenu.Items.AddRange(new ToolStripItem[] { enableSelectedToolStripMenuItem, disableSelectedToolStripMenuItem, toolStripSeparator1, deleteToolStripMenuItem, toolStripSeparator2, copyDetailsToolStripMenuItem, openFileLocationToolStripMenuItem });
            auditContextMenu.Name = "auditContextMenu";
            auditContextMenu.Size = new Size(174, 126);
            auditContextMenu.Opening += AuditContextMenu_Opening;
            // 
            // enableSelectedToolStripMenuItem
            // 
            enableSelectedToolStripMenuItem.Name = "enableSelectedToolStripMenuItem";
            enableSelectedToolStripMenuItem.Size = new Size(173, 22);
            enableSelectedToolStripMenuItem.Text = "Enable Selected";
            enableSelectedToolStripMenuItem.Click += EnableSelectedToolStripMenuItem_Click;
            // 
            // disableSelectedToolStripMenuItem
            // 
            disableSelectedToolStripMenuItem.Name = "disableSelectedToolStripMenuItem";
            disableSelectedToolStripMenuItem.Size = new Size(173, 22);
            disableSelectedToolStripMenuItem.Text = "Disable Selected";
            disableSelectedToolStripMenuItem.Click += DisableSelectedToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(170, 6);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(173, 22);
            deleteToolStripMenuItem.Text = "Delete Rule";
            deleteToolStripMenuItem.Click += DeleteToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(170, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            copyDetailsToolStripMenuItem.Size = new Size(173, 22);
            copyDetailsToolStripMenuItem.Text = "Copy Details";
            copyDetailsToolStripMenuItem.Click += CopyDetailsToolStripMenuItem_Click;
            // 
            // openFileLocationToolStripMenuItem
            // 
            openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            openFileLocationToolStripMenuItem.Size = new Size(173, 22);
            openFileLocationToolStripMenuItem.Text = "Open File Location";
            openFileLocationToolStripMenuItem.Click += OpenFileLocationToolStripMenuItem_Click;
            // 
            // topPanel
            // 
            topPanel.Controls.Add(auditSearchTextBox);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Margin = new Padding(3, 2, 3, 2);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(875, 58);
            topPanel.TabIndex = 4;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel2;
            splitContainer.Location = new Point(0, 58);
            splitContainer.Margin = new Padding(3, 2, 3, 2);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(systemChangesDataGridView);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(diffRichTextBox);
            splitContainer.Panel2.Controls.Add(diffLabel);
            splitContainer.Panel2MinSize = 100;
            splitContainer.Size = new Size(875, 632);
            splitContainer.SplitterDistance = 483;
            splitContainer.SplitterWidth = 3;
            splitContainer.TabIndex = 5;
            // 
            // systemChangesDataGridView
            // 
            systemChangesDataGridView.AllowUserToAddRows = false;
            systemChangesDataGridView.AllowUserToDeleteRows = false;
            systemChangesDataGridView.AllowUserToResizeRows = false;
            systemChangesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            systemChangesDataGridView.BackgroundColor = SystemColors.Control;
            systemChangesDataGridView.BorderStyle = BorderStyle.None;
            systemChangesDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            systemChangesDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            systemChangesDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            systemChangesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            systemChangesDataGridView.Columns.AddRange(new DataGridViewColumn[] { advTimestampColumn, advNameColumn, advInterventionColumn, advStatusColumn, advProtocolColumn, advLocalPortsColumn, advRemotePortsColumn, advLocalAddressColumn, advRemoteAddressColumn, advProgramColumn, advServiceColumn, advProfilesColumn, advGroupingColumn, advDescColumn, advPublisherColumn });
            systemChangesDataGridView.ContextMenuStrip = auditContextMenu;
            systemChangesDataGridView.Dock = DockStyle.Fill;
            systemChangesDataGridView.EnableHeadersVisualStyles = false;
            systemChangesDataGridView.GridColor = SystemColors.Control;
            systemChangesDataGridView.Location = new Point(0, 0);
            systemChangesDataGridView.Margin = new Padding(3, 2, 3, 2);
            systemChangesDataGridView.Name = "systemChangesDataGridView";
            systemChangesDataGridView.ReadOnly = true;
            systemChangesDataGridView.RowHeadersVisible = false;
            systemChangesDataGridView.RowTemplate.Height = 28;
            systemChangesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            systemChangesDataGridView.Size = new Size(875, 483);
            systemChangesDataGridView.TabIndex = 0;
            systemChangesDataGridView.CellFormatting += SystemChangesDataGridView_CellFormatting;
            systemChangesDataGridView.CellMouseDown += SystemChangesDataGridView_CellMouseDown;
            systemChangesDataGridView.CellMouseEnter += SystemChangesDataGridView_CellMouseEnter;
            systemChangesDataGridView.CellMouseLeave += SystemChangesDataGridView_CellMouseLeave;
            systemChangesDataGridView.ColumnHeaderMouseClick += SystemChangesDataGridView_ColumnHeaderMouseClick;
            systemChangesDataGridView.RowPostPaint += SystemChangesDataGridView_RowPostPaint;
            systemChangesDataGridView.SelectionChanged += SystemChangesDataGridView_SelectionChanged;
            // 
            // diffRichTextBox
            // 
            diffRichTextBox.BackColor = Color.White;
            diffRichTextBox.BorderStyle = BorderStyle.None;
            diffRichTextBox.Dock = DockStyle.Fill;
            diffRichTextBox.Font = new Font("Consolas", 9.75F);
            diffRichTextBox.Location = new Point(0, 19);
            diffRichTextBox.Margin = new Padding(3, 2, 3, 2);
            diffRichTextBox.Name = "diffRichTextBox";
            diffRichTextBox.ReadOnly = true;
            diffRichTextBox.Size = new Size(875, 127);
            diffRichTextBox.TabIndex = 2;
            diffRichTextBox.Text = "";
            // 
            // diffLabel
            // 
            diffLabel.Dock = DockStyle.Top;
            diffLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            diffLabel.Location = new Point(0, 0);
            diffLabel.Name = "diffLabel";
            diffLabel.Padding = new Padding(4, 4, 0, 0);
            diffLabel.Size = new Size(875, 19);
            diffLabel.TabIndex = 1;
            diffLabel.Text = "Change Details:";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 668);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 12, 0);
            statusStrip1.Size = new Size(875, 22);
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 17);
            statusLabel.Text = "Ready";
            // 
            // advTimestampColumn
            // 
            advTimestampColumn.DataPropertyName = "Timestamp";
            advTimestampColumn.FillWeight = 15F;
            advTimestampColumn.HeaderText = "Detected";
            advTimestampColumn.Name = "advTimestampColumn";
            advTimestampColumn.ReadOnly = true;
            advTimestampColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
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
            // advInterventionColumn
            // 
            advInterventionColumn.DataPropertyName = "Intervention";
            advInterventionColumn.FillWeight = 25F;
            advInterventionColumn.HeaderText = "Intervention";
            advInterventionColumn.Name = "advInterventionColumn";
            advInterventionColumn.ReadOnly = true;
            advInterventionColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // advStatusColumn
            // 
            advStatusColumn.DataPropertyName = "Status";
            advStatusColumn.FillWeight = 15F;
            advStatusColumn.HeaderText = "Action";
            advStatusColumn.Name = "advStatusColumn";
            advStatusColumn.ReadOnly = true;
            advStatusColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
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
            advProgramColumn.HeaderText = "Application";
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
            // advPublisherColumn
            // 
            advPublisherColumn.DataPropertyName = "Publisher";
            advPublisherColumn.FillWeight = 25F;
            advPublisherColumn.HeaderText = "Verified Signer";
            advPublisherColumn.Name = "advPublisherColumn";
            advPublisherColumn.ReadOnly = true;
            advPublisherColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // AuditControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(statusStrip1);
            Controls.Add(splitContainer);
            Controls.Add(topPanel);
            DoubleBuffered = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "AuditControl";
            Size = new Size(875, 690);
            auditContextMenu.ResumeLayout(false);
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)systemChangesDataGridView).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private DataGridViewTextBoxColumn advTimestampColumn;
        private DataGridViewTextBoxColumn advNameColumn;
        private DataGridViewTextBoxColumn advInterventionColumn;
        private DataGridViewTextBoxColumn advStatusColumn;
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
        private DataGridViewTextBoxColumn advPublisherColumn;
    }
}
