// File: GroupsControl.Designer.cs
namespace MinimalFirewall
{
    partial class GroupsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView groupsListView;
        private System.Windows.Forms.ColumnHeader groupNameColumn;
        private System.Windows.Forms.ColumnHeader groupEnabledColumn;
        private System.Windows.Forms.ContextMenuStrip groupsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteGroupToolStripMenuItem;
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
            this.groupsListView = new System.Windows.Forms.ListView();
            this.groupNameColumn = new System.Windows.Forms.ColumnHeader();
            this.groupEnabledColumn = new System.Windows.Forms.ColumnHeader();
            this.groupsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupsContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupsListView
            // 
            this.groupsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.groupsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.groupNameColumn,
            this.groupEnabledColumn});
            this.groupsListView.ContextMenuStrip = this.groupsContextMenu;
            this.groupsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupsListView.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupsListView.FullRowSelect = true;
            this.groupsListView.Location = new System.Drawing.Point(0, 0);
            this.groupsListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupsListView.Name = "groupsListView";
            this.groupsListView.OwnerDraw = true;
            this.groupsListView.Size = new System.Drawing.Size(800, 600);
            this.groupsListView.TabIndex = 0;
            this.groupsListView.UseCompatibleStateImageBehavior = false;
            this.groupsListView.View = System.Windows.Forms.View.Details;
            this.groupsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.groupsListView_ColumnClick);
            this.groupsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.groupsListView_DrawItem);
            this.groupsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.groupsListView_DrawSubItem);
            this.groupsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupsListView_MouseClick);
            this.groupsListView.MouseLeave += new System.EventHandler(this.groupsListView_MouseLeave);
            this.groupsListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.groupsListView_MouseMove);
            // 
            // groupNameColumn
            // 
            this.groupNameColumn.Text = "Group Name";
            // 
            // groupEnabledColumn
            // 
            this.groupEnabledColumn.Text = "Enabled";
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
            // GroupsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupsListView);
            this.Name = "GroupsControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.groupsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}