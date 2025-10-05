namespace MinimalFirewall
{
    partial class LiveConnectionsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label liveConnectionsDisabledLabel;
        private System.Windows.Forms.ContextMenuStrip liveConnectionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem killProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockRemoteIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.DataGridView liveConnectionsDataGridView;
        private System.Windows.Forms.DataGridViewImageColumn liveIconColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn processNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn localAddressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn localPortColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remoteAddressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remotePortColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn stateColumn;

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
            this.liveConnectionsDisabledLabel = new System.Windows.Forms.Label();
            this.liveConnectionsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.killProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockRemoteIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveConnectionsDataGridView = new System.Windows.Forms.DataGridView();
            this.liveIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.processNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localAddressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localPortColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remoteAddressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remotePortColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liveConnectionsContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.liveConnectionsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // liveConnectionsDisabledLabel
            // 
            this.liveConnectionsDisabledLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveConnectionsDisabledLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.liveConnectionsDisabledLabel.Location = new System.Drawing.Point(0, 0);
            this.liveConnectionsDisabledLabel.Name = "liveConnectionsDisabledLabel";
            this.liveConnectionsDisabledLabel.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsDisabledLabel.TabIndex = 1;
            this.liveConnectionsDisabledLabel.Text = "Please turn on Live Connections in the Settings tab.";
            this.liveConnectionsDisabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.liveConnectionsDisabledLabel.Visible = false;
            // 
            // liveConnectionsContextMenu
            // 
            this.liveConnectionsContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.liveConnectionsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.killProcessToolStripMenuItem,
            this.blockRemoteIPToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyDetailsToolStripMenuItem});
            this.liveConnectionsContextMenu.Name = "liveConnectionsContextMenu";
            this.liveConnectionsContextMenu.Size = new System.Drawing.Size(186, 82);
            // 
            // killProcessToolStripMenuItem
            // 
            this.killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
            this.killProcessToolStripMenuItem.Size = new System.Drawing.Size(185, 24);
            this.killProcessToolStripMenuItem.Text = "Kill Process";
            this.killProcessToolStripMenuItem.Click += new System.EventHandler(this.killProcessToolStripMenuItem_Click);
            // 
            // blockRemoteIPToolStripMenuItem
            // 
            this.blockRemoteIPToolStripMenuItem.Name = "blockRemoteIPToolStripMenuItem";
            this.blockRemoteIPToolStripMenuItem.Size = new System.Drawing.Size(185, 24);
            this.blockRemoteIPToolStripMenuItem.Text = "Block Remote IP";
            this.blockRemoteIPToolStripMenuItem.Click += new System.EventHandler(this.blockRemoteIPToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(182, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(185, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
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
            this.liveIconColumn,
            this.processNameColumn,
            this.localAddressColumn,
            this.localPortColumn,
            this.remoteAddressColumn,
            this.remotePortColumn,
            this.stateColumn});
            this.liveConnectionsDataGridView.ContextMenuStrip = this.liveConnectionsContextMenu;
            this.liveConnectionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveConnectionsDataGridView.EnableHeadersVisualStyles = false;
            this.liveConnectionsDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.liveConnectionsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.liveConnectionsDataGridView.MultiSelect = true;
            this.liveConnectionsDataGridView.Name = "liveConnectionsDataGridView";
            this.liveConnectionsDataGridView.ReadOnly = true;
            this.liveConnectionsDataGridView.RowHeadersVisible = false;
            this.liveConnectionsDataGridView.RowTemplate.Height = 28;
            this.liveConnectionsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.liveConnectionsDataGridView.ShowCellToolTips = true;
            this.liveConnectionsDataGridView.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsDataGridView.TabIndex = 2;
            this.liveConnectionsDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.liveConnectionsDataGridView_CellFormatting);
            this.liveConnectionsDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.liveConnectionsDataGridView_CellMouseDown);
            this.liveConnectionsDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.liveConnectionsDataGridView_CellMouseEnter);
            this.liveConnectionsDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.liveConnectionsDataGridView_CellMouseLeave);
            this.liveConnectionsDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.liveConnectionsDataGridView_ColumnHeaderMouseClick);
            this.liveConnectionsDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.liveConnectionsDataGridView_RowPostPaint);
            // 
            // liveIconColumn
            // 
            this.liveIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.liveIconColumn.FillWeight = 5F;
            this.liveIconColumn.HeaderText = "";
            this.liveIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.liveIconColumn.MinimumWidth = 32;
            this.liveIconColumn.Name = "liveIconColumn";
            this.liveIconColumn.ReadOnly = true;
            this.liveIconColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.liveIconColumn.Width = 32;
            // 
            // processNameColumn
            // 
            this.processNameColumn.DataPropertyName = "DisplayName";
            this.processNameColumn.FillWeight = 30F;
            this.processNameColumn.HeaderText = "Process Name";
            this.processNameColumn.Name = "processNameColumn";
            this.processNameColumn.ReadOnly = true;
            // 
            // localAddressColumn
            // 
            this.localAddressColumn.DataPropertyName = "LocalAddress";
            this.localAddressColumn.FillWeight = 25F;
            this.localAddressColumn.HeaderText = "Local Address";
            this.localAddressColumn.Name = "localAddressColumn";
            this.localAddressColumn.ReadOnly = true;
            // 
            // localPortColumn
            // 
            this.localPortColumn.DataPropertyName = "LocalPort";
            this.localPortColumn.FillWeight = 10F;
            this.localPortColumn.HeaderText = "Local Port";
            this.localPortColumn.Name = "localPortColumn";
            this.localPortColumn.ReadOnly = true;
            // 
            // remoteAddressColumn
            // 
            this.remoteAddressColumn.DataPropertyName = "RemoteAddress";
            this.remoteAddressColumn.FillWeight = 25F;
            this.remoteAddressColumn.HeaderText = "Remote Address";
            this.remoteAddressColumn.Name = "remoteAddressColumn";
            this.remoteAddressColumn.ReadOnly = true;
            // 
            // remotePortColumn
            // 
            this.remotePortColumn.DataPropertyName = "RemotePort";
            this.remotePortColumn.FillWeight = 10F;
            this.remotePortColumn.HeaderText = "Remote Port";
            this.remotePortColumn.Name = "remotePortColumn";
            this.remotePortColumn.ReadOnly = true;
            // 
            // stateColumn
            // 
            this.stateColumn.DataPropertyName = "State";
            this.stateColumn.FillWeight = 15F;
            this.stateColumn.HeaderText = "State";
            this.stateColumn.Name = "stateColumn";
            this.stateColumn.ReadOnly = true;
            // 
            // LiveConnectionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.liveConnectionsDataGridView);
            this.Controls.Add(this.liveConnectionsDisabledLabel);
            this.Name = "LiveConnectionsControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.liveConnectionsDataGridView)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
    }
}