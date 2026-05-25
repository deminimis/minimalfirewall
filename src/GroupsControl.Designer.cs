// File: GroupsControl.Designer.cs
namespace MinimalFirewall
{
    partial class GroupsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ContextMenuStrip groupsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteGroupToolStripMenuItem;
        private DarkModeForms.ThemedDataGridView groupsDataGridView;
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
            components = new System.ComponentModel.Container();
            var dataGridViewCellStyle1 = new DataGridViewCellStyle();
            groupsContextMenu = new ContextMenuStrip(components);
            deleteGroupToolStripMenuItem = new ToolStripMenuItem();
            groupsDataGridView = new DarkModeForms.ThemedDataGridView();
            groupNameColumn = new DataGridViewTextBoxColumn();
            groupEnabledColumn = new DataGridViewTextBoxColumn();
            groupsContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // groupsContextMenu
            // 
            groupsContextMenu.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupsContextMenu.ImageScalingSize = new Size(20, 20);
            groupsContextMenu.Items.AddRange(new ToolStripItem[] { deleteGroupToolStripMenuItem });
            groupsContextMenu.Name = "groupsContextMenu";
            groupsContextMenu.Size = new Size(180, 26);
            // 
            // deleteGroupToolStripMenuItem
            // 
            deleteGroupToolStripMenuItem.Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            deleteGroupToolStripMenuItem.Name = "deleteGroupToolStripMenuItem";
            deleteGroupToolStripMenuItem.Size = new Size(179, 22);
            deleteGroupToolStripMenuItem.Text = "Delete Group...";
            deleteGroupToolStripMenuItem.Click += DeleteGroupToolStripMenuItem_Click;
            // 
            // groupsDataGridView
            // 
            groupsDataGridView.AllowUserToAddRows = false;
            groupsDataGridView.AllowUserToDeleteRows = false;
            groupsDataGridView.AllowUserToResizeRows = false;
            groupsDataGridView.BackgroundColor = SystemColors.Control;
            groupsDataGridView.BorderStyle = BorderStyle.None;
            groupsDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            groupsDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 12F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            groupsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            groupsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            groupsDataGridView.Columns.AddRange(new DataGridViewColumn[] { groupNameColumn, groupEnabledColumn });
            groupsDataGridView.ContextMenuStrip = groupsContextMenu;
            groupsDataGridView.Dock = DockStyle.Fill;
            groupsDataGridView.GridColor = SystemColors.Control;
            groupsDataGridView.Location = new Point(0, 0);
            groupsDataGridView.Margin = new Padding(3, 2, 3, 2);
            groupsDataGridView.Name = "groupsDataGridView";
            groupsDataGridView.ReadOnly = true;
            groupsDataGridView.RowHeadersVisible = false;
            groupsDataGridView.RowTemplate.Height = 35;
            groupsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            groupsDataGridView.Size = new Size(700, 480);
            groupsDataGridView.TabIndex = 1;
            groupsDataGridView.CellClick += GroupsDataGridView_CellClick;
            groupsDataGridView.CellMouseDown += GroupsDataGridView_CellMouseDown;
            groupsDataGridView.CellPainting += GroupsDataGridView_CellPainting;
            groupsDataGridView.ColumnHeaderMouseClick += GroupsDataGridView_ColumnHeaderMouseClick;
            // 
            // groupNameColumn
            // 
            groupNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            groupNameColumn.DataPropertyName = "Name";
            groupNameColumn.HeaderText = "Group Name";
            groupNameColumn.Name = "groupNameColumn";
            groupNameColumn.ReadOnly = true;
            // 
            // groupEnabledColumn
            // 
            groupEnabledColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            groupEnabledColumn.DataPropertyName = "IsEnabled";
            groupEnabledColumn.HeaderText = "Enabled";
            groupEnabledColumn.Name = "groupEnabledColumn";
            groupEnabledColumn.ReadOnly = true;
            // 
            // GroupsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupsDataGridView);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            Name = "GroupsControl";
            Size = new Size(700, 480);
            groupsContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupsDataGridView).EndInit();
            ResumeLayout(false);
        }
        #endregion
    }
}
