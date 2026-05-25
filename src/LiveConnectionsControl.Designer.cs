namespace MinimalFirewall
{
    partial class LiveConnectionsControl
    {
        private System.ComponentModel.IContainer components = null;
        private DarkModeForms.ThemedPanel disabledPanel;
        private DarkModeForms.ThemedLabel disabledLabel;

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
            components = new System.ComponentModel.Container();
            var dataGridViewCellStyle1 = new DataGridViewCellStyle();
            liveConnectionsDataGridView = new DarkModeForms.ThemedDataGridView();
            connIconColumn = new DataGridViewImageColumn();
            connNameColumn = new DataGridViewTextBoxColumn();
            connStateColumn = new DataGridViewTextBoxColumn();
            connLocalAddrColumn = new DataGridViewTextBoxColumn();
            connLocalPortColumn = new DataGridViewTextBoxColumn();
            connRemoteAddrColumn = new DataGridViewTextBoxColumn();
            connRemotePortColumn = new DataGridViewTextBoxColumn();
            connPathColumn = new DataGridViewTextBoxColumn();
            liveConnectionsContextMenu = new ContextMenuStrip(components);
            killProcessToolStripMenuItem = new ToolStripMenuItem();
            blockRemoteIPToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            createAdvancedRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem = new ToolStripMenuItem();
            copyDetailsToolStripMenuItem = new ToolStripMenuItem();
            disabledPanel = new DarkModeForms.ThemedPanel();
            disabledLabel = new DarkModeForms.ThemedLabel();
            ((System.ComponentModel.ISupportInitialize)liveConnectionsDataGridView).BeginInit();
            liveConnectionsContextMenu.SuspendLayout();
            disabledPanel.SuspendLayout();
            SuspendLayout();
            // 
            // liveConnectionsDataGridView
            // 
            liveConnectionsDataGridView.AllowUserToAddRows = false;
            liveConnectionsDataGridView.AllowUserToDeleteRows = false;
            liveConnectionsDataGridView.AllowUserToResizeRows = false;
            liveConnectionsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            liveConnectionsDataGridView.BackgroundColor = SystemColors.Control;
            liveConnectionsDataGridView.BorderStyle = BorderStyle.None;
            liveConnectionsDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            liveConnectionsDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            liveConnectionsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            liveConnectionsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            liveConnectionsDataGridView.Columns.AddRange(new DataGridViewColumn[] { connIconColumn, connNameColumn, connStateColumn, connLocalAddrColumn, connLocalPortColumn, connRemoteAddrColumn, connRemotePortColumn, connPathColumn });
            liveConnectionsDataGridView.ContextMenuStrip = liveConnectionsContextMenu;
            liveConnectionsDataGridView.Dock = DockStyle.Fill;
            liveConnectionsDataGridView.GridColor = SystemColors.Control;
            liveConnectionsDataGridView.Location = new Point(0, 0);
            liveConnectionsDataGridView.Margin = new Padding(3, 2, 3, 2);
            liveConnectionsDataGridView.MultiSelect = false;
            liveConnectionsDataGridView.Name = "liveConnectionsDataGridView";
            liveConnectionsDataGridView.ReadOnly = true;
            liveConnectionsDataGridView.RowHeadersVisible = false;
            liveConnectionsDataGridView.RowTemplate.Height = 28;
            liveConnectionsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            liveConnectionsDataGridView.Size = new Size(700, 480);
            liveConnectionsDataGridView.TabIndex = 0;
            liveConnectionsDataGridView.CellFormatting += LiveConnectionsDataGridView_CellFormatting;
            liveConnectionsDataGridView.CellMouseDown += LiveConnectionsDataGridView_CellMouseDown;
            liveConnectionsDataGridView.CellMouseEnter += LiveConnectionsDataGridView_CellMouseEnter;
            liveConnectionsDataGridView.CellMouseLeave += LiveConnectionsDataGridView_CellMouseLeave;
            liveConnectionsDataGridView.RowPostPaint += LiveConnectionsDataGridView_RowPostPaint;
            // 
            // connIconColumn
            // 
            connIconColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            connIconColumn.DataPropertyName = "ProcessPath";
            connIconColumn.FillWeight = 10F;
            connIconColumn.HeaderText = "";
            connIconColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            connIconColumn.MinimumWidth = 32;
            connIconColumn.Name = "connIconColumn";
            connIconColumn.ReadOnly = true;
            connIconColumn.Resizable = DataGridViewTriState.False;
            connIconColumn.Width = 32;
            // 
            // connNameColumn
            // 
            connNameColumn.DataPropertyName = "DisplayName";
            connNameColumn.FillWeight = 25F;
            connNameColumn.HeaderText = "Application";
            connNameColumn.Name = "connNameColumn";
            connNameColumn.ReadOnly = true;
            // 
            // connStateColumn
            // 
            connStateColumn.DataPropertyName = "State";
            connStateColumn.FillWeight = 15F;
            connStateColumn.HeaderText = "State";
            connStateColumn.Name = "connStateColumn";
            connStateColumn.ReadOnly = true;
            // 
            // connLocalAddrColumn
            // 
            connLocalAddrColumn.DataPropertyName = "LocalAddress";
            connLocalAddrColumn.FillWeight = 20F;
            connLocalAddrColumn.HeaderText = "Local Address";
            connLocalAddrColumn.Name = "connLocalAddrColumn";
            connLocalAddrColumn.ReadOnly = true;
            // 
            // connLocalPortColumn
            // 
            connLocalPortColumn.DataPropertyName = "LocalPort";
            connLocalPortColumn.FillWeight = 10F;
            connLocalPortColumn.HeaderText = "Port";
            connLocalPortColumn.Name = "connLocalPortColumn";
            connLocalPortColumn.ReadOnly = true;
            // 
            // connRemoteAddrColumn
            // 
            connRemoteAddrColumn.DataPropertyName = "RemoteAddress";
            connRemoteAddrColumn.FillWeight = 20F;
            connRemoteAddrColumn.HeaderText = "Remote Address";
            connRemoteAddrColumn.Name = "connRemoteAddrColumn";
            connRemoteAddrColumn.ReadOnly = true;
            // 
            // connRemotePortColumn
            // 
            connRemotePortColumn.DataPropertyName = "RemotePort";
            connRemotePortColumn.FillWeight = 10F;
            connRemotePortColumn.HeaderText = "Port";
            connRemotePortColumn.Name = "connRemotePortColumn";
            connRemotePortColumn.ReadOnly = true;
            // 
            // connPathColumn
            // 
            connPathColumn.DataPropertyName = "ProcessPath";
            connPathColumn.FillWeight = 30F;
            connPathColumn.HeaderText = "Path";
            connPathColumn.Name = "connPathColumn";
            connPathColumn.ReadOnly = true;
            // 
            // liveConnectionsContextMenu
            // 
            liveConnectionsContextMenu.ImageScalingSize = new Size(20, 20);
            liveConnectionsContextMenu.Items.AddRange(new ToolStripItem[] { killProcessToolStripMenuItem, blockRemoteIPToolStripMenuItem, toolStripSeparator1, createAdvancedRuleToolStripMenuItem, toolStripSeparator2, openFileLocationToolStripMenuItem, copyDetailsToolStripMenuItem });
            liveConnectionsContextMenu.Name = "liveConnectionsContextMenu";
            liveConnectionsContextMenu.Size = new Size(200, 126);
            liveConnectionsContextMenu.Opening += LiveConnectionsContextMenu_Opening;
            // 
            // killProcessToolStripMenuItem
            // 
            killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
            killProcessToolStripMenuItem.Size = new Size(199, 22);
            killProcessToolStripMenuItem.Text = "Kill Process";
            killProcessToolStripMenuItem.Click += KillProcessToolStripMenuItem_Click;
            // 
            // blockRemoteIPToolStripMenuItem
            // 
            blockRemoteIPToolStripMenuItem.Name = "blockRemoteIPToolStripMenuItem";
            blockRemoteIPToolStripMenuItem.Size = new Size(199, 22);
            blockRemoteIPToolStripMenuItem.Text = "Block Remote IP";
            blockRemoteIPToolStripMenuItem.Click += BlockRemoteIPToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(196, 6);
            // 
            // createAdvancedRuleToolStripMenuItem
            // 
            createAdvancedRuleToolStripMenuItem.Name = "createAdvancedRuleToolStripMenuItem";
            createAdvancedRuleToolStripMenuItem.Size = new Size(199, 22);
            createAdvancedRuleToolStripMenuItem.Text = "Create Advanced Rule...";
            createAdvancedRuleToolStripMenuItem.Click += CreateAdvancedRuleToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(196, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            openFileLocationToolStripMenuItem.Size = new Size(199, 22);
            openFileLocationToolStripMenuItem.Text = "Open File Location";
            openFileLocationToolStripMenuItem.Click += OpenFileLocationToolStripMenuItem_Click;
            // 
            // copyDetailsToolStripMenuItem
            // 
            copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            copyDetailsToolStripMenuItem.Size = new Size(199, 22);
            copyDetailsToolStripMenuItem.Text = "Copy Details";
            copyDetailsToolStripMenuItem.Click += CopyDetailsToolStripMenuItem_Click;
            // 
            // disabledPanel
            // 
            disabledPanel.Controls.Add(disabledLabel);
            disabledPanel.Dock = DockStyle.Fill;
            disabledPanel.Location = new Point(0, 0);
            disabledPanel.Margin = new Padding(3, 2, 3, 2);
            disabledPanel.Name = "disabledPanel";
            disabledPanel.Size = new Size(700, 480);
            disabledPanel.TabIndex = 1;
            disabledPanel.Visible = false;
            // 
            // disabledLabel
            // 
            disabledLabel.Dock = DockStyle.Fill;
            disabledLabel.Font = new Font("Segoe UI", 12F);
            disabledLabel.Location = new Point(0, 0);
            disabledLabel.Name = "disabledLabel";
            disabledLabel.Size = new Size(700, 480);
            disabledLabel.TabIndex = 0;
            disabledLabel.Text = "Live connection monitoring is disabled.\r\n\r\nYou can enable it in the Settings tab.";
            disabledLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LiveConnectionsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(disabledPanel);
            Controls.Add(liveConnectionsDataGridView);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            Name = "LiveConnectionsControl";
            Size = new Size(700, 480);
            ((System.ComponentModel.ISupportInitialize)liveConnectionsDataGridView).EndInit();
            liveConnectionsContextMenu.ResumeLayout(false);
            disabledPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DarkModeForms.ThemedDataGridView liveConnectionsDataGridView;
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
