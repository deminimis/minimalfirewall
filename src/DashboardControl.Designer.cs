// File: DashboardControl.Designer.cs
namespace MinimalFirewall
{
    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ContextMenuStrip dashboardContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tempAllowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow2MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow5MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow15MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow1HourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow3HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allow8HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem permanentAllowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowAndTrustPublisherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem permanentBlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ignoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem createWildcardRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem createAdvancedRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;
        private System.Windows.Forms.DataGridView dashboardDataGridView;
        private System.Windows.Forms.DataGridViewImageColumn dashIconColumn;
        private System.Windows.Forms.DataGridViewButtonColumn allowButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn blockButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn ignoreButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashAppColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashServiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashDirectionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashPathColumn;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dashboardContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tempAllowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow2MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow5MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow15MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow1HourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow3HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allow8HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.permanentAllowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowAndTrustPublisherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.permanentBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.createWildcardRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.createAdvancedRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardDataGridView = new System.Windows.Forms.DataGridView();
            this.dashIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.allowButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.blockButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ignoreButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dashAppColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dashServiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dashDirectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dashPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dashboardContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dashboardContextMenu
            // 
            this.dashboardContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.dashboardContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tempAllowToolStripMenuItem,
            this.toolStripSeparator3,
            this.permanentAllowToolStripMenuItem,
            this.allowAndTrustPublisherToolStripMenuItem,
            this.permanentBlockToolStripMenuItem,
            this.ignoreToolStripMenuItem,
            this.toolStripSeparator4,
            this.createWildcardRuleToolStripMenuItem,
            this.toolStripSeparator5,
            this.createAdvancedRuleToolStripMenuItem,
            this.toolStripSeparator7,
            this.openFileLocationToolStripMenuItem1,
            this.toolStripSeparator6,
            this.copyDetailsToolStripMenuItem});
            this.dashboardContextMenu.Name = "dashboardContextMenu";
            this.dashboardContextMenu.Size = new System.Drawing.Size(228, 290);
            this.dashboardContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // tempAllowToolStripMenuItem
            // 
            this.tempAllowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allow2MinutesToolStripMenuItem,
            this.allow5MinutesToolStripMenuItem,
            this.allow15MinutesToolStripMenuItem,
            this.allow1HourToolStripMenuItem,
            this.allow3HoursToolStripMenuItem,
            this.allow8HoursToolStripMenuItem});
            this.tempAllowToolStripMenuItem.Name = "tempAllowToolStripMenuItem";
            this.tempAllowToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.tempAllowToolStripMenuItem.Text = "Allow Temporarily";
            // 
            // allow2MinutesToolStripMenuItem
            // 
            this.allow2MinutesToolStripMenuItem.Name = "allow2MinutesToolStripMenuItem";
            this.allow2MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow2MinutesToolStripMenuItem.Tag = "2";
            this.allow2MinutesToolStripMenuItem.Text = "2 minutes";
            this.allow2MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow5MinutesToolStripMenuItem
            // 
            this.allow5MinutesToolStripMenuItem.Name = "allow5MinutesToolStripMenuItem";
            this.allow5MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow5MinutesToolStripMenuItem.Tag = "5";
            this.allow5MinutesToolStripMenuItem.Text = "5 minutes";
            this.allow5MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow15MinutesToolStripMenuItem
            // 
            this.allow15MinutesToolStripMenuItem.Name = "allow15MinutesToolStripMenuItem";
            this.allow15MinutesToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow15MinutesToolStripMenuItem.Tag = "15";
            this.allow15MinutesToolStripMenuItem.Text = "15 minutes";
            this.allow15MinutesToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow1HourToolStripMenuItem
            // 
            this.allow1HourToolStripMenuItem.Name = "allow1HourToolStripMenuItem";
            this.allow1HourToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow1HourToolStripMenuItem.Tag = "60";
            this.allow1HourToolStripMenuItem.Text = "1 hour";
            this.allow1HourToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow3HoursToolStripMenuItem
            // 
            this.allow3HoursToolStripMenuItem.Name = "allow3HoursToolStripMenuItem";
            this.allow3HoursToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow3HoursToolStripMenuItem.Tag = "180";
            this.allow3HoursToolStripMenuItem.Text = "3 hours";
            this.allow3HoursToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // allow8HoursToolStripMenuItem
            // 
            this.allow8HoursToolStripMenuItem.Name = "allow8HoursToolStripMenuItem";
            this.allow8HoursToolStripMenuItem.Size = new System.Drawing.Size(162, 26);
            this.allow8HoursToolStripMenuItem.Tag = "480";
            this.allow8HoursToolStripMenuItem.Text = "8 hours";
            this.allow8HoursToolStripMenuItem.Click += new System.EventHandler(this.TempAllowMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(224, 6);
            // 
            // permanentAllowToolStripMenuItem
            // 
            this.permanentAllowToolStripMenuItem.Name = "permanentAllowToolStripMenuItem";
            this.permanentAllowToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.permanentAllowToolStripMenuItem.Text = "Allow";
            this.permanentAllowToolStripMenuItem.Click += new System.EventHandler(this.PermanentAllowToolStripMenuItem_Click);
            // 
            // allowAndTrustPublisherToolStripMenuItem
            // 
            this.allowAndTrustPublisherToolStripMenuItem.Name = "allowAndTrustPublisherToolStripMenuItem";
            this.allowAndTrustPublisherToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.allowAndTrustPublisherToolStripMenuItem.Text = "Allow and Trust Publisher";
            this.allowAndTrustPublisherToolStripMenuItem.Click += new System.EventHandler(this.AllowAndTrustPublisherToolStripMenuItem_Click);
            // 
            // permanentBlockToolStripMenuItem
            // 
            this.permanentBlockToolStripMenuItem.Name = "permanentBlockToolStripMenuItem";
            this.permanentBlockToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.permanentBlockToolStripMenuItem.Text = "Block";
            this.permanentBlockToolStripMenuItem.Click += new System.EventHandler(this.PermanentBlockToolStripMenuItem_Click);
            // 
            // ignoreToolStripMenuItem
            // 
            this.ignoreToolStripMenuItem.Name = "ignoreToolStripMenuItem";
            this.ignoreToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.ignoreToolStripMenuItem.Text = "Ignore";
            this.ignoreToolStripMenuItem.Click += new System.EventHandler(this.IgnoreToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(224, 6);
            // 
            // createWildcardRuleToolStripMenuItem
            // 
            this.createWildcardRuleToolStripMenuItem.Name = "createWildcardRuleToolStripMenuItem";
            this.createWildcardRuleToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.createWildcardRuleToolStripMenuItem.Text = "Create Wildcard Rule...";
            this.createWildcardRuleToolStripMenuItem.Click += new System.EventHandler(this.createWildcardRuleToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(224, 6);
            // 
            // createAdvancedRuleToolStripMenuItem
            // 
            this.createAdvancedRuleToolStripMenuItem.Name = "createAdvancedRuleToolStripMenuItem";
            this.createAdvancedRuleToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.createAdvancedRuleToolStripMenuItem.Text = "Create Advanced Rule...";
            this.createAdvancedRuleToolStripMenuItem.Click += new System.EventHandler(this.createAdvancedRuleToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(224, 6);
            // 
            // openFileLocationToolStripMenuItem1
            // 
            this.openFileLocationToolStripMenuItem1.Name = "openFileLocationToolStripMenuItem1";
            this.openFileLocationToolStripMenuItem1.Size = new System.Drawing.Size(227, 24);
            this.openFileLocationToolStripMenuItem1.Text = "Open File Location";
            this.openFileLocationToolStripMenuItem1.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem1_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(224, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            this.copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            this.copyDetailsToolStripMenuItem.Size = new System.Drawing.Size(227, 24);
            this.copyDetailsToolStripMenuItem.Text = "Copy Details";
            this.copyDetailsToolStripMenuItem.Click += new System.EventHandler(this.copyDetailsToolStripMenuItem_Click);
            // 
            // dashboardDataGridView
            // 
            this.dashboardDataGridView.AllowUserToAddRows = false;
            this.dashboardDataGridView.AllowUserToDeleteRows = false;
            this.dashboardDataGridView.AllowUserToResizeRows = false;
            this.dashboardDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dashboardDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dashboardDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dashboardDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dashboardDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dashboardDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dashboardDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dashboardDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dashIconColumn,
            this.allowButtonColumn,
            this.blockButtonColumn,
            this.ignoreButtonColumn,
            this.dashAppColumn,
            this.dashServiceColumn,
            this.dashDirectionColumn,
            this.dashPathColumn});
            this.dashboardDataGridView.ContextMenuStrip = this.dashboardContextMenu;
            this.dashboardDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardDataGridView.EnableHeadersVisualStyles = false;
            this.dashboardDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.dashboardDataGridView.Location = new System.Drawing.Point(0, 0);
            this.dashboardDataGridView.MultiSelect = false;
            this.dashboardDataGridView.Name = "dashboardDataGridView";
            this.dashboardDataGridView.ReadOnly = true;
            this.dashboardDataGridView.RowHeadersVisible = false;
            this.dashboardDataGridView.RowTemplate.Height = 32;
            this.dashboardDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dashboardDataGridView.Size = new System.Drawing.Size(800, 600);
            this.dashboardDataGridView.TabIndex = 2;
            this.dashboardDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dashboardDataGridView_CellContentClick);
            this.dashboardDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dashboardDataGridView_CellFormatting);
            this.dashboardDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dashboardDataGridView_CellMouseEnter);
            this.dashboardDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dashboardDataGridView_CellMouseLeave);
            this.dashboardDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dashboardDataGridView_RowPostPaint);
            // 
            // dashIconColumn
            // 
            this.dashIconColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dashIconColumn.DataPropertyName = "AppPath";
            this.dashIconColumn.FillWeight = 10F;
            this.dashIconColumn.HeaderText = "";
            this.dashIconColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.dashIconColumn.MinimumWidth = 32;
            this.dashIconColumn.Name = "dashIconColumn";
            this.dashIconColumn.ReadOnly = true;
            this.dashIconColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dashIconColumn.Width = 32;
            // 
            // allowButtonColumn
            // 
            this.allowButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.allowButtonColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.allowButtonColumn.FillWeight = 15F;
            this.allowButtonColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.allowButtonColumn.HeaderText = "Action";
            this.allowButtonColumn.MinimumWidth = 70;
            this.allowButtonColumn.Name = "allowButtonColumn";
            this.allowButtonColumn.ReadOnly = true;
            this.allowButtonColumn.Text = "Allow";
            this.allowButtonColumn.UseColumnTextForButtonValue = true;
            this.allowButtonColumn.Width = 70;
            // 
            // blockButtonColumn
            // 
            this.blockButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.blockButtonColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.blockButtonColumn.FillWeight = 15F;
            this.blockButtonColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.blockButtonColumn.HeaderText = "";
            this.blockButtonColumn.MinimumWidth = 70;
            this.blockButtonColumn.Name = "blockButtonColumn";
            this.blockButtonColumn.ReadOnly = true;
            this.blockButtonColumn.Text = "Block";
            this.blockButtonColumn.UseColumnTextForButtonValue = true;
            this.blockButtonColumn.Width = 70;
            // 
            // ignoreButtonColumn
            // 
            this.ignoreButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ignoreButtonColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.ignoreButtonColumn.FillWeight = 15F;
            this.ignoreButtonColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ignoreButtonColumn.HeaderText = "";
            this.ignoreButtonColumn.MinimumWidth = 70;
            this.ignoreButtonColumn.Name = "ignoreButtonColumn";
            this.ignoreButtonColumn.ReadOnly = true;
            this.ignoreButtonColumn.Text = "Ignore";
            this.ignoreButtonColumn.UseColumnTextForButtonValue = true;
            this.ignoreButtonColumn.Width = 70;
            // 
            // dashAppColumn
            // 
            this.dashAppColumn.DataPropertyName = "FileName";
            this.dashAppColumn.FillWeight = 30F;
            this.dashAppColumn.HeaderText = "Application";
            this.dashAppColumn.Name = "dashAppColumn";
            this.dashAppColumn.ReadOnly = true;
            // 
            // dashServiceColumn
            // 
            this.dashServiceColumn.DataPropertyName = "ServiceName";
            this.dashServiceColumn.FillWeight = 30F;
            this.dashServiceColumn.HeaderText = "Service";
            this.dashServiceColumn.Name = "dashServiceColumn";
            this.dashServiceColumn.ReadOnly = true;
            // 
            // dashDirectionColumn
            // 
            this.dashDirectionColumn.DataPropertyName = "Direction";
            this.dashDirectionColumn.FillWeight = 20F;
            this.dashDirectionColumn.HeaderText = "Direction";
            this.dashDirectionColumn.Name = "dashDirectionColumn";
            this.dashDirectionColumn.ReadOnly = true;
            // 
            // dashPathColumn
            // 
            this.dashPathColumn.DataPropertyName = "AppPath";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dashPathColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.dashPathColumn.FillWeight = 50F;
            this.dashPathColumn.HeaderText = "Path";
            this.dashPathColumn.Name = "dashPathColumn";
            this.dashPathColumn.ReadOnly = true;
            // 
            // DashboardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dashboardDataGridView);
            this.Name = "DashboardControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.dashboardContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dashboardDataGridView)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
