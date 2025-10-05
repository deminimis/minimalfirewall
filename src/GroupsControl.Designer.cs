// File: GroupsControl.Designer.cs
namespace MinimalFirewall
{
    partial class GroupsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ContextMenuStrip groupsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteGroupToolStripMenuItem;
        private System.Windows.Forms.DataGridView groupsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn groupNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn groupEnabledColumn;
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
            this.groupsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupsDataGridView = new System.Windows.Forms.DataGridView();
            this.groupNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupEnabledColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupsContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupsContextMenu
            // 
            this.groupsContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.groupsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteGroupToolStripMenuItem});
            this.groupsContextMenu.Name = "groupsContextMenu";
            this.groupsContextMenu.Size = new System.Drawing.Size(177, 28);
            // 
            // deleteGroupToolStripMenuItem
            // 
            this.deleteGroupToolStripMenuItem.Name = "deleteGroupToolStripMenuItem";
            this.deleteGroupToolStripMenuItem.Size = new System.Drawing.Size(176, 24);
            this.deleteGroupToolStripMenuItem.Text = "Delete Group...";
            this.deleteGroupToolStripMenuItem.Click += new System.EventHandler(this.deleteGroupToolStripMenuItem_Click);
            // 
            // groupsDataGridView
            // 
            this.groupsDataGridView.AllowUserToAddRows = false;
            this.groupsDataGridView.AllowUserToDeleteRows = false;
            this.groupsDataGridView.AllowUserToResizeRows = false;
            this.groupsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.groupsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.groupsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.groupsDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.groupsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.groupsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.groupsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.groupsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.groupNameColumn,
            this.groupEnabledColumn});
            this.groupsDataGridView.ContextMenuStrip = this.groupsContextMenu;
            this.groupsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupsDataGridView.EnableHeadersVisualStyles = false;
            this.groupsDataGridView.GridColor = System.Drawing.SystemColors.Control;
            this.groupsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.groupsDataGridView.Name = "groupsDataGridView";
            this.groupsDataGridView.ReadOnly = true;
            this.groupsDataGridView.RowHeadersVisible = false;
            this.groupsDataGridView.RowTemplate.Height = 35;
            this.groupsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.groupsDataGridView.ShowCellToolTips = true;
            this.groupsDataGridView.Size = new System.Drawing.Size(800, 600);
            this.groupsDataGridView.TabIndex = 1;
            this.groupsDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.groupsDataGridView_CellContentClick);
            this.groupsDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.groupsDataGridView_CellMouseDown);
            this.groupsDataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.groupsDataGridView_CellPainting);
            this.groupsDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.groupsDataGridView_ColumnHeaderMouseClick);
            // 
            // groupNameColumn
            // 
            this.groupNameColumn.DataPropertyName = "Name";
            this.groupNameColumn.FillWeight = 80F;
            this.groupNameColumn.HeaderText = "Group Name";
            this.groupNameColumn.Name = "groupNameColumn";
            this.groupNameColumn.ReadOnly = true;
            // 
            // groupEnabledColumn
            // 
            this.groupEnabledColumn.DataPropertyName = "IsEnabled";
            this.groupEnabledColumn.FillWeight = 20F;
            this.groupEnabledColumn.HeaderText = "Enabled";
            this.groupEnabledColumn.Name = "groupEnabledColumn";
            this.groupEnabledColumn.ReadOnly = true;
            // 
            // GroupsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupsDataGridView);
            this.Name = "GroupsControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.groupsContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupsDataGridView)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
    }
}