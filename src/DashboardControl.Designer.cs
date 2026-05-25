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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem copyHashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkVirusTotalToolStripMenuItem;
        private DarkModeForms.ThemedDataGridView dashboardDataGridView;
        private System.Windows.Forms.DataGridViewImageColumn dashIconColumn;
        private System.Windows.Forms.DataGridViewButtonColumn allowButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn blockButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn ignoreButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashAppColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashServiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashDirectionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashProtocolColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dashPathColumn;
        private System.Windows.Forms.ToolStripMenuItem showBlockingRuleInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox detailsRichTextBox;
        private DarkModeForms.ThemedLabel detailsLabel;


        protected override void Dispose(bool disposing)
        {
            // Unsubscribe to prevent memory leaks 
            if (disposing && _viewModel != null)
            {
                _viewModel.PendingConnections.CollectionChanged -= PendingConnections_CollectionChanged;
            }

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
            var dataGridViewCellStyle2 = new DataGridViewCellStyle();
            var dataGridViewCellStyle3 = new DataGridViewCellStyle();
            var dataGridViewCellStyle4 = new DataGridViewCellStyle();
            var dataGridViewCellStyle5 = new DataGridViewCellStyle();
            dashboardContextMenu = new ContextMenuStrip(components);
            tempAllowToolStripMenuItem = new ToolStripMenuItem();
            allow2MinutesToolStripMenuItem = new ToolStripMenuItem();
            allow5MinutesToolStripMenuItem = new ToolStripMenuItem();
            allow15MinutesToolStripMenuItem = new ToolStripMenuItem();
            allow1HourToolStripMenuItem = new ToolStripMenuItem();
            allow3HoursToolStripMenuItem = new ToolStripMenuItem();
            allow8HoursToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            permanentAllowToolStripMenuItem = new ToolStripMenuItem();
            allowAndTrustPublisherToolStripMenuItem = new ToolStripMenuItem();
            permanentBlockToolStripMenuItem = new ToolStripMenuItem();
            ignoreToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            createWildcardRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            createAdvancedRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            openFileLocationToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            showBlockingRuleInfoToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            copyDetailsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            copyHashToolStripMenuItem = new ToolStripMenuItem();
            checkVirusTotalToolStripMenuItem = new ToolStripMenuItem();
            dashboardDataGridView = new DarkModeForms.ThemedDataGridView();
            dashIconColumn = new DataGridViewImageColumn();
            dashAppColumn = new DataGridViewTextBoxColumn();
            dashServiceColumn = new DataGridViewTextBoxColumn();
            dashDirectionColumn = new DataGridViewTextBoxColumn();
            dashProtocolColumn = new DataGridViewTextBoxColumn();
            dashPathColumn = new DataGridViewTextBoxColumn();
            allowButtonColumn = new DataGridViewButtonColumn();
            blockButtonColumn = new DataGridViewButtonColumn();
            ignoreButtonColumn = new DataGridViewButtonColumn();
            splitContainer = new SplitContainer();
            detailsRichTextBox = new RichTextBox();
            detailsLabel = new DarkModeForms.ThemedLabel();
            dashboardContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dashboardDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // dashboardContextMenu
            // 
            dashboardContextMenu.ImageScalingSize = new Size(20, 20);
            dashboardContextMenu.Items.AddRange(new ToolStripItem[] { tempAllowToolStripMenuItem, toolStripSeparator3, permanentAllowToolStripMenuItem, allowAndTrustPublisherToolStripMenuItem, permanentBlockToolStripMenuItem, ignoreToolStripMenuItem, toolStripSeparator4, createWildcardRuleToolStripMenuItem, toolStripSeparator5, createAdvancedRuleToolStripMenuItem, toolStripSeparator7, openFileLocationToolStripMenuItem1, toolStripSeparator8, showBlockingRuleInfoToolStripMenuItem, toolStripSeparator6, copyDetailsToolStripMenuItem, toolStripSeparator9, copyHashToolStripMenuItem, checkVirusTotalToolStripMenuItem });
            dashboardContextMenu.Name = "dashboardContextMenu";
            dashboardContextMenu.Size = new Size(211, 310);
            dashboardContextMenu.Opening += ContextMenu_Opening;
            // 
            // tempAllowToolStripMenuItem
            // 
            tempAllowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allow2MinutesToolStripMenuItem, allow5MinutesToolStripMenuItem, allow15MinutesToolStripMenuItem, allow1HourToolStripMenuItem, allow3HoursToolStripMenuItem, allow8HoursToolStripMenuItem });
            tempAllowToolStripMenuItem.Name = "tempAllowToolStripMenuItem";
            tempAllowToolStripMenuItem.Size = new Size(210, 22);
            tempAllowToolStripMenuItem.Text = "Allow Temporarily";
            // 
            // allow2MinutesToolStripMenuItem
            // 
            allow2MinutesToolStripMenuItem.Name = "allow2MinutesToolStripMenuItem";
            allow2MinutesToolStripMenuItem.Size = new Size(180, 22);
            allow2MinutesToolStripMenuItem.Tag = "2";
            allow2MinutesToolStripMenuItem.Text = "2 minutes";
            allow2MinutesToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // allow5MinutesToolStripMenuItem
            // 
            allow5MinutesToolStripMenuItem.Name = "allow5MinutesToolStripMenuItem";
            allow5MinutesToolStripMenuItem.Size = new Size(180, 22);
            allow5MinutesToolStripMenuItem.Tag = "5";
            allow5MinutesToolStripMenuItem.Text = "5 minutes";
            allow5MinutesToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // allow15MinutesToolStripMenuItem
            // 
            allow15MinutesToolStripMenuItem.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            allow15MinutesToolStripMenuItem.Name = "allow15MinutesToolStripMenuItem";
            allow15MinutesToolStripMenuItem.Size = new Size(180, 22);
            allow15MinutesToolStripMenuItem.Tag = "15";
            allow15MinutesToolStripMenuItem.Text = "15 minutes";
            allow15MinutesToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // allow1HourToolStripMenuItem
            // 
            allow1HourToolStripMenuItem.Name = "allow1HourToolStripMenuItem";
            allow1HourToolStripMenuItem.Size = new Size(180, 22);
            allow1HourToolStripMenuItem.Tag = "60";
            allow1HourToolStripMenuItem.Text = "1 hour";
            allow1HourToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // allow3HoursToolStripMenuItem
            // 
            allow3HoursToolStripMenuItem.Name = "allow3HoursToolStripMenuItem";
            allow3HoursToolStripMenuItem.Size = new Size(180, 22);
            allow3HoursToolStripMenuItem.Tag = "180";
            allow3HoursToolStripMenuItem.Text = "3 hours";
            allow3HoursToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // allow8HoursToolStripMenuItem
            // 
            allow8HoursToolStripMenuItem.Name = "allow8HoursToolStripMenuItem";
            allow8HoursToolStripMenuItem.Size = new Size(180, 22);
            allow8HoursToolStripMenuItem.Tag = "480";
            allow8HoursToolStripMenuItem.Text = "8 hours";
            allow8HoursToolStripMenuItem.Click += TempAllowMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(207, 6);
            // 
            // permanentAllowToolStripMenuItem
            // 
            permanentAllowToolStripMenuItem.Name = "permanentAllowToolStripMenuItem";
            permanentAllowToolStripMenuItem.Size = new Size(210, 22);
            permanentAllowToolStripMenuItem.Text = "Allow";
            permanentAllowToolStripMenuItem.Click += PermanentAllowToolStripMenuItem_Click;
            // 
            // allowAndTrustPublisherToolStripMenuItem
            // 
            allowAndTrustPublisherToolStripMenuItem.Name = "allowAndTrustPublisherToolStripMenuItem";
            allowAndTrustPublisherToolStripMenuItem.Size = new Size(210, 22);
            allowAndTrustPublisherToolStripMenuItem.Text = "Allow and Trust Publisher";
            allowAndTrustPublisherToolStripMenuItem.Click += AllowAndTrustPublisherToolStripMenuItem_Click;
            // 
            // permanentBlockToolStripMenuItem
            // 
            permanentBlockToolStripMenuItem.Name = "permanentBlockToolStripMenuItem";
            permanentBlockToolStripMenuItem.Size = new Size(210, 22);
            permanentBlockToolStripMenuItem.Text = "Block";
            permanentBlockToolStripMenuItem.Click += PermanentBlockToolStripMenuItem_Click;
            // 
            // ignoreToolStripMenuItem
            // 
            ignoreToolStripMenuItem.Name = "ignoreToolStripMenuItem";
            ignoreToolStripMenuItem.Size = new Size(210, 22);
            ignoreToolStripMenuItem.Text = "Ignore";
            ignoreToolStripMenuItem.Click += IgnoreToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(207, 6);
            // 
            // createWildcardRuleToolStripMenuItem
            // 
            createWildcardRuleToolStripMenuItem.Name = "createWildcardRuleToolStripMenuItem";
            createWildcardRuleToolStripMenuItem.Size = new Size(210, 22);
            createWildcardRuleToolStripMenuItem.Text = "Create Wildcard Rule...";
            createWildcardRuleToolStripMenuItem.Click += CreateWildcardRuleToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(207, 6);
            // 
            // createAdvancedRuleToolStripMenuItem
            // 
            createAdvancedRuleToolStripMenuItem.Name = "createAdvancedRuleToolStripMenuItem";
            createAdvancedRuleToolStripMenuItem.Size = new Size(210, 22);
            createAdvancedRuleToolStripMenuItem.Text = "Create Advanced Rule...";
            createAdvancedRuleToolStripMenuItem.Click += CreateAdvancedRuleToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(207, 6);
            // 
            // openFileLocationToolStripMenuItem1
            // 
            openFileLocationToolStripMenuItem1.Name = "openFileLocationToolStripMenuItem1";
            openFileLocationToolStripMenuItem1.Size = new Size(210, 22);
            openFileLocationToolStripMenuItem1.Text = "Open File Location";
            openFileLocationToolStripMenuItem1.Click += OpenFileLocationToolStripMenuItem1_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(207, 6);
            // 
            // showBlockingRuleInfoToolStripMenuItem
            // 
            showBlockingRuleInfoToolStripMenuItem.Name = "showBlockingRuleInfoToolStripMenuItem";
            showBlockingRuleInfoToolStripMenuItem.Size = new Size(210, 22);
            showBlockingRuleInfoToolStripMenuItem.Text = "Show Blocking Rule Info";
            showBlockingRuleInfoToolStripMenuItem.Click += ShowBlockingRuleInfoToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(207, 6);
            // 
            // copyDetailsToolStripMenuItem
            // 
            copyDetailsToolStripMenuItem.Name = "copyDetailsToolStripMenuItem";
            copyDetailsToolStripMenuItem.Size = new Size(210, 22);
            copyDetailsToolStripMenuItem.Text = "Copy Details";
            copyDetailsToolStripMenuItem.Click += CopyDetailsToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(207, 6);
            // 
            // copyHashToolStripMenuItem
            // 
            copyHashToolStripMenuItem.Name = "copyHashToolStripMenuItem";
            copyHashToolStripMenuItem.Size = new Size(210, 22);
            copyHashToolStripMenuItem.Text = "Copy File Hash (SHA-256)";
            copyHashToolStripMenuItem.Click += CopyHashToolStripMenuItem_Click;
            // 
            // checkVirusTotalToolStripMenuItem
            // 
            checkVirusTotalToolStripMenuItem.Name = "checkVirusTotalToolStripMenuItem";
            checkVirusTotalToolStripMenuItem.Size = new Size(210, 22);
            checkVirusTotalToolStripMenuItem.Text = "Check on VirusTotal";
            checkVirusTotalToolStripMenuItem.Click += CheckVirusTotalToolStripMenuItem_Click;
            // 
            // dashboardDataGridView
            // 
            dashboardDataGridView.AllowUserToAddRows = false;
            dashboardDataGridView.AllowUserToDeleteRows = false;
            dashboardDataGridView.AllowUserToResizeRows = false;
            dashboardDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dashboardDataGridView.BackgroundColor = SystemColors.Control;
            dashboardDataGridView.BorderStyle = BorderStyle.None;
            dashboardDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dashboardDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dashboardDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dashboardDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dashboardDataGridView.Columns.AddRange(new DataGridViewColumn[] { dashIconColumn, dashAppColumn, dashServiceColumn, dashDirectionColumn, dashProtocolColumn, dashPathColumn, allowButtonColumn, blockButtonColumn, ignoreButtonColumn });
            dashboardDataGridView.ContextMenuStrip = dashboardContextMenu;
            dashboardDataGridView.Dock = DockStyle.Fill;
            dashboardDataGridView.GridColor = SystemColors.Control;
            dashboardDataGridView.Location = new Point(0, 0);
            dashboardDataGridView.Margin = new Padding(3, 2, 3, 2);
            dashboardDataGridView.MultiSelect = false;
            dashboardDataGridView.Name = "dashboardDataGridView";
            dashboardDataGridView.ReadOnly = true;
            dashboardDataGridView.RowHeadersVisible = false;
            dashboardDataGridView.RowTemplate.Height = 40;
            dashboardDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dashboardDataGridView.Size = new Size(700, 361);
            dashboardDataGridView.TabIndex = 2;
            dashboardDataGridView.CellContentClick += DashboardDataGridView_CellContentClick;
            dashboardDataGridView.CellFormatting += DashboardDataGridView_CellFormatting;
            dashboardDataGridView.CellMouseEnter += DashboardDataGridView_CellMouseEnter;
            dashboardDataGridView.CellMouseLeave += DashboardDataGridView_CellMouseLeave;
            dashboardDataGridView.RowPostPaint += DashboardDataGridView_RowPostPaint;
            // 
            // dashIconColumn
            // 
            dashIconColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dashIconColumn.DataPropertyName = "AppPath";
            dashIconColumn.FillWeight = 10F;
            dashIconColumn.HeaderText = "";
            dashIconColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dashIconColumn.MinimumWidth = 32;
            dashIconColumn.Name = "dashIconColumn";
            dashIconColumn.ReadOnly = true;
            dashIconColumn.Resizable = DataGridViewTriState.False;
            dashIconColumn.Width = 32;
            // 
            // dashAppColumn
            // 
            dashAppColumn.DataPropertyName = "FileName";
            dashAppColumn.FillWeight = 30F;
            dashAppColumn.HeaderText = "Application";
            dashAppColumn.Name = "dashAppColumn";
            dashAppColumn.ReadOnly = true;
            // 
            // dashServiceColumn
            // 
            dashServiceColumn.DataPropertyName = "ServiceName";
            dashServiceColumn.FillWeight = 30F;
            dashServiceColumn.HeaderText = "Service";
            dashServiceColumn.Name = "dashServiceColumn";
            dashServiceColumn.ReadOnly = true;
            // 
            // dashDirectionColumn
            // 
            dashDirectionColumn.DataPropertyName = "Direction";
            dashDirectionColumn.FillWeight = 10F;
            dashDirectionColumn.HeaderText = "Direction";
            dashDirectionColumn.Name = "dashDirectionColumn";
            dashDirectionColumn.ReadOnly = true;
            // 
            // dashProtocolColumn
            // 
            dashProtocolColumn.DataPropertyName = "ProtocolDisplay";
            dashProtocolColumn.FillWeight = 8F;
            dashProtocolColumn.HeaderText = "Protocol";
            dashProtocolColumn.Name = "dashProtocolColumn";
            dashProtocolColumn.ReadOnly = true;
            // 
            // dashPathColumn
            // 
            dashPathColumn.DataPropertyName = "AppPath";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dashPathColumn.DefaultCellStyle = dataGridViewCellStyle2;
            dashPathColumn.FillWeight = 42F;
            dashPathColumn.HeaderText = "Path";
            dashPathColumn.Name = "dashPathColumn";
            dashPathColumn.ReadOnly = true;
            // 
            // allowButtonColumn
            // 
            allowButtonColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            allowButtonColumn.DefaultCellStyle = dataGridViewCellStyle3;
            allowButtonColumn.FillWeight = 12F;
            allowButtonColumn.FlatStyle = FlatStyle.Flat;
            allowButtonColumn.HeaderText = "Actions";
            allowButtonColumn.MinimumWidth = 90;
            allowButtonColumn.Name = "allowButtonColumn";
            allowButtonColumn.ReadOnly = true;
            allowButtonColumn.Text = "Allow";
            allowButtonColumn.UseColumnTextForButtonValue = true;
            allowButtonColumn.Width = 90;
            // 
            // blockButtonColumn
            // 
            blockButtonColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            blockButtonColumn.DefaultCellStyle = dataGridViewCellStyle4;
            blockButtonColumn.FillWeight = 12F;
            blockButtonColumn.FlatStyle = FlatStyle.Flat;
            blockButtonColumn.HeaderText = "";
            blockButtonColumn.MinimumWidth = 90;
            blockButtonColumn.Name = "blockButtonColumn";
            blockButtonColumn.ReadOnly = true;
            blockButtonColumn.Text = "Block";
            blockButtonColumn.UseColumnTextForButtonValue = true;
            blockButtonColumn.Width = 90;
            // 
            // ignoreButtonColumn
            // 
            ignoreButtonColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
            ignoreButtonColumn.DefaultCellStyle = dataGridViewCellStyle5;
            ignoreButtonColumn.FillWeight = 12F;
            ignoreButtonColumn.FlatStyle = FlatStyle.Flat;
            ignoreButtonColumn.HeaderText = "";
            ignoreButtonColumn.MinimumWidth = 90;
            ignoreButtonColumn.Name = "ignoreButtonColumn";
            ignoreButtonColumn.ReadOnly = true;
            ignoreButtonColumn.Text = "Ignore";
            ignoreButtonColumn.UseColumnTextForButtonValue = true;
            ignoreButtonColumn.Width = 90;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel2;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Margin = new Padding(3, 2, 3, 2);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(dashboardDataGridView);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(detailsRichTextBox);
            splitContainer.Panel2.Controls.Add(detailsLabel);
            splitContainer.Panel2MinSize = 100;
            splitContainer.Size = new Size(700, 480);
            splitContainer.SplitterDistance = 361;
            splitContainer.SplitterWidth = 3;
            splitContainer.TabIndex = 3;
            // 
            // detailsRichTextBox
            // 
            detailsRichTextBox.BackColor = Color.White;
            detailsRichTextBox.BorderStyle = BorderStyle.None;
            detailsRichTextBox.Dock = DockStyle.Fill;
            detailsRichTextBox.Font = new Font("Consolas", 9.75F);
            detailsRichTextBox.Location = new Point(0, 20);
            detailsRichTextBox.Margin = new Padding(3, 2, 3, 2);
            detailsRichTextBox.Name = "detailsRichTextBox";
            detailsRichTextBox.ReadOnly = true;
            detailsRichTextBox.Size = new Size(700, 96);
            detailsRichTextBox.TabIndex = 2;
            detailsRichTextBox.Text = "";
            // 
            // detailsLabel
            // 
            detailsLabel.Dock = DockStyle.Top;
            detailsLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            detailsLabel.Location = new Point(0, 0);
            detailsLabel.Name = "detailsLabel";
            detailsLabel.Padding = new Padding(4, 4, 0, 0);
            detailsLabel.Size = new Size(700, 20);
            detailsLabel.TabIndex = 1;
            detailsLabel.Text = "Connection Details:";
            // 
            // DashboardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            Name = "DashboardControl";
            Size = new Size(700, 480);
            dashboardContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dashboardDataGridView).EndInit();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
    }
}
