// File: LiveConnectionsControl.Designer.cs
namespace MinimalFirewall
{
    partial class LiveConnectionsControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label liveConnectionsDisabledLabel;
        private System.Windows.Forms.ListView liveConnectionsListView;
        private System.Windows.Forms.ColumnHeader liveIconColumn;
        private System.Windows.Forms.ColumnHeader processNameColumn;
        private System.Windows.Forms.ColumnHeader localAddressColumn;
        private System.Windows.Forms.ColumnHeader localPortColumn;
        private System.Windows.Forms.ColumnHeader remoteAddressColumn;
        private System.Windows.Forms.ColumnHeader remotePortColumn;
        private System.Windows.Forms.ColumnHeader stateColumn;
        private System.Windows.Forms.ContextMenuStrip liveConnectionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem killProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blockRemoteIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyDetailsToolStripMenuItem;

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
            this.liveConnectionsDisabledLabel = new System.Windows.Forms.Label();
            this.liveConnectionsListView = new System.Windows.Forms.ListView();
            this.liveIconColumn = new System.Windows.Forms.ColumnHeader();
            this.processNameColumn = new System.Windows.Forms.ColumnHeader();
            this.localAddressColumn = new System.Windows.Forms.ColumnHeader();
            this.localPortColumn = new System.Windows.Forms.ColumnHeader();
            this.remoteAddressColumn = new System.Windows.Forms.ColumnHeader();
            this.remotePortColumn = new System.Windows.Forms.ColumnHeader();
            this.stateColumn = new System.Windows.Forms.ColumnHeader();
            this.liveConnectionsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.killProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blockRemoteIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveConnectionsContextMenu.SuspendLayout();
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
            // liveConnectionsListView
            // 
            this.liveConnectionsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.liveConnectionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.liveIconColumn,
            this.processNameColumn,
            this.localAddressColumn,
            this.localPortColumn,
            this.remoteAddressColumn,
            this.remotePortColumn,
            this.stateColumn});
            this.liveConnectionsListView.ContextMenuStrip = this.liveConnectionsContextMenu;
            this.liveConnectionsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveConnectionsListView.FullRowSelect = true;
            this.liveConnectionsListView.Location = new System.Drawing.Point(0, 0);
            this.liveConnectionsListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.liveConnectionsListView.Name = "liveConnectionsListView";
            this.liveConnectionsListView.OwnerDraw = true;
            this.liveConnectionsListView.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsListView.TabIndex = 0;
            this.liveConnectionsListView.UseCompatibleStateImageBehavior = false;
            this.liveConnectionsListView.View = System.Windows.Forms.View.Details;
            this.liveConnectionsListView.VirtualMode = true;
            this.liveConnectionsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.liveConnectionsListView_ColumnClick);
            this.liveConnectionsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.liveConnectionsListView_DrawItem);
            this.liveConnectionsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.liveConnectionsListView_DrawSubItem);
            this.liveConnectionsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.liveConnectionsListView_RetrieveVirtualItem);
            this.liveConnectionsListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.liveConnectionsListView_MouseClick);
            this.liveConnectionsListView.MouseLeave += new System.EventHandler(this.liveConnectionsListView_MouseLeave);
            this.liveConnectionsListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.liveConnectionsListView_MouseMove);
            // 
            // liveIconColumn
            // 
            this.liveIconColumn.Text = "";
            this.liveIconColumn.Width = 32;
            // 
            // processNameColumn
            // 
            this.processNameColumn.Text = "Process Name";
            // 
            // localAddressColumn
            // 
            this.localAddressColumn.Text = "Local Address";
            // 
            // localPortColumn
            // 
            this.localPortColumn.Text = "Local Port";
            // 
            // remoteAddressColumn
            // 
            this.remoteAddressColumn.Text = "Remote Address";
            // 
            // remotePortColumn
            // 
            this.remotePortColumn.Text = "Remote Port";
            // 
            // stateColumn
            // 
            this.stateColumn.Text = "State";
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
            // LiveConnectionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.liveConnectionsDisabledLabel);
            this.Controls.Add(this.liveConnectionsListView);
            this.Name = "LiveConnectionsControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.liveConnectionsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}