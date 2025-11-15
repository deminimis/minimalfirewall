namespace MinimalFirewall
{
    partial class LiveConnectionsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel disabledPanel;
        private System.Windows.Forms.Label disabledLabel;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.liveConnectionsDataGridView = new System.Windows.Forms.DataGridView();
            this.connIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.connNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connStateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connLocalAddrColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connLocalPortColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connRemoteAddrColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connRemotePortColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.connPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liveConnectionsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.killProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockRemoteIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.createAdvancedRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disabledPanel = new System.Windows.Forms.Panel();
            this.disabledLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.liveConnectionsDataGridView)).BeginInit();
            this.liveConnectionsContextMenu.SuspendLayout();
            this.disabledPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // liveConnectionsDataGridView
            // 
            this.liveConnectionsDataGridView.AllowUserToAddRows = false;
            this.liveConnectionsDataGridView.AllowUserToDeleteRows = false;
            this.liveConnectionsDataGridView.AllowUserToResizeRows = false;
            this.liveConnectionsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.liveConnectionsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.liveConnectionsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.liveConnectionsDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.liveConnectionsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.liveConnectionsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.liveConnectionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.liveConnectionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.connIconColumn,
            this.connNameColumn,
            this.connStateColumn,
            this.connLocalAddrColumn,
            this.connLocalPortColumn,
            this.connRemoteAddrColumn,
            this.connRemotePortColumn,
            this.connPathColumn});
            this.liveConnectionsDataGridView.ContextMenuStrip = this.liveConnectionsContextMenu;
            this.liveConnectionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveConnectionsDataGridView.EnableHeadersVisualStyles = false;
            this.liveConnectionsDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.liveConnectionsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.liveConnectionsDataGridView.MultiSelect = false;
            this.liveConnectionsDataGridView.Name = "liveConnectionsDataGridView";
            this.liveConnectionsDataGridView.ReadOnly = true;
            this.liveConnectionsDataGridView.RowHeadersVisible = false;
            this.liveConnectionsDataGridView.RowTemplate.Height = 28;
            this.liveConnectionsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.liveConnectionsDataGridView.ShowCellToolTips = true;
            this.liveConnectionsDataGridView.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsDataGridView.TabIndex = 0;
            this.liveConnectionsDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.liveConnectionsDataGridView_CellFormatting);
            this.liveConnectionsDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.liveConnectionsDataGridView_CellMouseDown);
            this.liveConnectionsDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.liveConnectionsDataGridView_CellMouseEnter);
            this.liveConnectionsDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.liveConnectionsDataGridView_CellMouseLeave);
            this.liveConnectionsDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.liveConnectionsDataGridView_ColumnHeaderMouseClick);
            this.liveConnectionsDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.liveConnectionsDataGridView_RowPostPaint);
            // 
            // connIconColumn
            // 
            this.connIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.connIconColumn.DataPropertyName = "ProcessPath";
            this.connIconColumn.FillWeight = 10F;
            this.connIconColumn.HeaderText = "";
            this.connIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.connIconColumn.MinimumWidth = 32;
            this.connIconColumn.Name = "connIconColumn";
            this.connIconColumn.ReadOnly = true;
            this.connIconColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.connIconColumn.Width = 32;
            // 
            // connNameColumn
            // 
            this.connNameColumn.DataPropertyName = "DisplayName";
            this.connNameColumn.FillWeight = 25F;
            this.connNameColumn.HeaderText = "Application";
            this.connNameColumn.Name = "connNameColumn";
            this.connNameColumn.ReadOnly = true;
            // 
            // connStateColumn
            // 
            this.connStateColumn.DataPropertyName = "State";
            this.connStateColumn.FillWeight = 15F;
            this.connStateColumn.HeaderText = "State";
            this.connStateColumn.Name = "connStateColumn";
            this.connStateColumn.ReadOnly = true;
            // 
            // connLocalAddrColumn
            // 
            this.connLocalAddrColumn.DataPropertyName = "LocalAddress";
            this.connLocalAddrColumn.FillWeight = 20F;
            this.connLocalAddrColumn.HeaderText = "Local Address";
            this.connLocalAddrColumn.Name = "connLocalAddrColumn";
            this.connLocalAddrColumn.ReadOnly = true;
            // 
            // connLocalPortColumn
            // 
            this.connLocalPortColumn.DataPropertyName = "LocalPort";
            this.connLocalPortColumn.FillWeight = 10F;
            this.connLocalPortColumn.HeaderText = "Port";
            this.connLocalPortColumn.Name = "connLocalPortColumn";
            this.connLocalPortColumn.ReadOnly = true;
            // 
            // connRemoteAddrColumn
            // 
            this.connRemoteAddrColumn.DataPropertyName = "RemoteAddress";
            this.connRemoteAddrColumn.FillWeight = 20F;
            this.connRemoteAddrColumn.HeaderText = "Remote Address";
            this.connRemoteAddrColumn.Name = "connRemoteAddrColumn";
            this.connRemoteAddrColumn.ReadOnly = true;
            // 
            // connRemotePortColumn
            // 
            this.connRemotePortColumn.DataPropertyName = "RemotePort";
            this.connRemotePortColumn.FillWeight = 10F;
            this.connRemotePortColumn.HeaderText = "Port";
            this.connRemotePortColumn.Name = "connRemotePortColumn";
            this.connRemotePortColumn.ReadOnly = true;
            // 
            // connPathColumn
            // 
            this.connPathColumn.DataPropertyName = "ProcessPath";
            this.connPathColumn.FillWeight = 30F;
            this.connPathColumn.HeaderText = "Path";
            this.connPathColumn.Name = "connPathColumn";
            this.connPathColumn.ReadOnly = true;
            // 
            // liveConnectionsContextMenu
            // 
            this.liveConnectionsContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.liveConnectionsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.killProcessToolStripMenuItem,
            this.blockRemoteIPToolStripMenuItem,
            this.toolStripSeparator1,
            this.createAdvancedRuleToolStripMenuItem,
            this.toolStripSeparator2,
            this.openFileLocationToolStripMenuItem,
            this.copyDetailsToolStripMenuItem});
            this.liveConnectionsContextMenu.Name = "liveConnectionsContextMenu";
            this.liveConnectionsContextMenu.Size = new System.Drawing.Size(228, 142);
            this.liveConnectionsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.liveConnectionsContextMenu_Opening);
            // 
            // killProcessToolStripMenuItem
            // 
            this.killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
            this.killProcessToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.killProcessToolStripMenuItem.Text = "Kill Process";
            this.killProcessToolStripMenuItem.Click += new System.EventHandler(this.killProcessToolStripMenuItem_Click);
            // 
            // blockRemoteIPToolStripMenuItem
            // 
            this.blockRemoteIPToolStripMenuItem.Name = "blockRemoteIPToolStripMenuItem";
            this.blockRemoteIPToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.blockRemoteIPToolStripMenuItem.Text = "Block Remote IP";
            this.blockRemoteIPToolStripMenuItem.Click += new System.EventHandler(this.blockRemoteIPToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(224, 6);
            // 
            // createAdvancedRuleToolStripMenuItem
            // 
            this.createAdvancedRuleToolStripMenuItem.Name = "createAdvancedRuleToolStripMenuItem";
            this.createAdvancedRuleToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.createAdvancedRuleToolStripMenuItem.Text = "Create Advanced Rule...";
            this.createAdvancedRuleToolStripMenuItem.Click += new System.EventHandler(this.createAdvancedRuleToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(224, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.openFileLocationToolStripMenuItem.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // disabledPanel
            // 
            this.disabledPanel.Controls.Add(this.disabledLabel);
            this.disabledPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disabledPanel.Location = new System.Drawing.Point(0, 0);
            this.disabledPanel.Name = "disabledPanel";
            this.disabledPanel.Size = new System.Drawing.Size(800, 600);
            this.disabledPanel.TabIndex = 1;
            this.disabledPanel.Visible = false;
            // 
            // disabledLabel
            // 
            this.disabledLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disabledLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.disabledLabel.Location = new System.Drawing.Point(0, 0);
            this.disabledLabel.Name = "disabledLabel";
            this.disabledLabel.Size = new System.Drawing.Size(800, 600);
            this.disabledLabel.TabIndex = 0;
            this.disabledLabel.Text = "Live connection monitoring is disabled.\r\n\r\nYou can enable it in the Settings tab.";
            this.disabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LiveConnectionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.disabledPanel);
            this.Controls.Add(this.liveConnectionsDataGridView);
            this.Name = "LiveConnectionsControl";
            this.Size = new System.Drawing.Size(800, 600);
            ((System.ComponentModel.ISupportInitialize)(this.liveConnectionsDataGridView)).EndInit();
            this.liveConnectionsContextMenu.ResumeLayout(false);
            this.disabledPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView liveConnectionsDataGridView;
        private System.Windows.Forms.ContextMenuStrip liveConnectionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem killProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockRemoteIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem createAdvancedRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.DataGridViewImageColumn connIconColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connStateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connLocalAddrColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connLocalPortColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connRemoteAddrColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connRemotePortColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn connPathColumn;
    }
}