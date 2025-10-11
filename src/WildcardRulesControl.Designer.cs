// File: WildcardRulesControl.Designer.cs
namespace MinimalFirewall
{
    partial class WildcardRulesControl
    {
        private System.ComponentModel.IContainer components = null;

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
            this.topPanel = new System.Windows.Forms.Panel();
            this.deleteRuleButton = new System.Windows.Forms.Button();
            this.editRuleButton = new System.Windows.Forms.Button();
            this.addRuleButton = new System.Windows.Forms.Button();
            this.wildcardDataGridView = new System.Windows.Forms.DataGridView();
            this.colFolderPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProtocol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalPorts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemotePorts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemoteAddresses = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wildcardContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteDefinitionAndRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDefinitionOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteGeneratedRulesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wildcardDataGridView)).BeginInit();
            this.wildcardContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.deleteRuleButton);
            this.topPanel.Controls.Add(this.editRuleButton);
            this.topPanel.Controls.Add(this.addRuleButton);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(800, 50);
            this.topPanel.TabIndex = 0;
            // 
            // deleteRuleButton
            // 
            this.deleteRuleButton.Location = new System.Drawing.Point(225, 10);
            this.deleteRuleButton.Name = "deleteRuleButton";
            this.deleteRuleButton.Size = new System.Drawing.Size(100, 30);
            this.deleteRuleButton.TabIndex = 2;
            this.deleteRuleButton.Text = "Delete Rule(s)";
            this.deleteRuleButton.UseVisualStyleBackColor = true;
            this.deleteRuleButton.Click += new System.EventHandler(this.deleteRuleButton_Click);
            // 
            // editRuleButton
            // 
            this.editRuleButton.Location = new System.Drawing.Point(119, 10);
            this.editRuleButton.Name = "editRuleButton";
            this.editRuleButton.Size = new System.Drawing.Size(100, 30);
            this.editRuleButton.TabIndex = 1;
            this.editRuleButton.Text = "Edit Rule...";
            this.editRuleButton.UseVisualStyleBackColor = true;
            this.editRuleButton.Click += new System.EventHandler(this.editRuleButton_Click);
            // 
            // addRuleButton
            // 
            this.addRuleButton.Location = new System.Drawing.Point(13, 10);
            this.addRuleButton.Name = "addRuleButton";
            this.addRuleButton.Size = new System.Drawing.Size(100, 30);
            this.addRuleButton.TabIndex = 0;
            this.addRuleButton.Text = "Add Rule...";
            this.addRuleButton.UseVisualStyleBackColor = true;
            this.addRuleButton.Click += new System.EventHandler(this.addRuleButton_Click);
            // 
            // wildcardDataGridView
            // 
            this.wildcardDataGridView.AllowUserToAddRows = false;
            this.wildcardDataGridView.AllowUserToDeleteRows = false;
            this.wildcardDataGridView.AllowUserToResizeRows = false;
            this.wildcardDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.wildcardDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.wildcardDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.wildcardDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFolderPath,
            this.colExeName,
            this.colAction,
            this.colProtocol,
            this.colLocalPorts,
            this.colRemotePorts,
            this.colRemoteAddresses});
            this.wildcardDataGridView.ContextMenuStrip = this.wildcardContextMenu;
            this.wildcardDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wildcardDataGridView.Location = new System.Drawing.Point(0, 50);
            this.wildcardDataGridView.Name = "wildcardDataGridView";
            this.wildcardDataGridView.ReadOnly = true;
            this.wildcardDataGridView.RowHeadersVisible = false;
            this.wildcardDataGridView.RowTemplate.Height = 25;
            this.wildcardDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.wildcardDataGridView.Size = new System.Drawing.Size(800, 550);
            this.wildcardDataGridView.TabIndex = 1;
            // 
            // colFolderPath
            // 
            this.colFolderPath.DataPropertyName = "FolderPath";
            this.colFolderPath.FillWeight = 30F;
            this.colFolderPath.HeaderText = "Folder Path";
            this.colFolderPath.Name = "colFolderPath";
            this.colFolderPath.ReadOnly = true;
            // 
            // colExeName
            // 
            this.colExeName.DataPropertyName = "ExeName";
            this.colExeName.FillWeight = 15F;
            this.colExeName.HeaderText = "Executable Name";
            this.colExeName.Name = "colExeName";
            this.colExeName.ReadOnly = true;
            // 
            // colAction
            // 
            this.colAction.DataPropertyName = "Action";
            this.colAction.FillWeight = 10F;
            this.colAction.HeaderText = "Action";
            this.colAction.Name = "colAction";
            this.colAction.ReadOnly = true;
            // 
            // colProtocol
            // 
            this.colProtocol.DataPropertyName = "Protocol";
            this.colProtocol.FillWeight = 8F;
            this.colProtocol.HeaderText = "Protocol";
            this.colProtocol.Name = "colProtocol";
            this.colProtocol.ReadOnly = true;
            // 
            // colLocalPorts
            // 
            this.colLocalPorts.DataPropertyName = "LocalPorts";
            this.colLocalPorts.FillWeight = 10F;
            this.colLocalPorts.HeaderText = "Local Ports";
            this.colLocalPorts.Name = "colLocalPorts";
            this.colLocalPorts.ReadOnly = true;
            // 
            // colRemotePorts
            // 
            this.colRemotePorts.DataPropertyName = "RemotePorts";
            this.colRemotePorts.FillWeight = 10F;
            this.colRemotePorts.HeaderText = "Remote Ports";
            this.colRemotePorts.Name = "colRemotePorts";
            this.colRemotePorts.ReadOnly = true;
            // 
            // colRemoteAddresses
            // 
            this.colRemoteAddresses.DataPropertyName = "RemoteAddresses";
            this.colRemoteAddresses.FillWeight = 17F;
            this.colRemoteAddresses.HeaderText = "Remote Addresses";
            this.colRemoteAddresses.Name = "colRemoteAddresses";
            this.colRemoteAddresses.ReadOnly = true;
            // 
            // wildcardContextMenu
            // 
            this.wildcardContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.wildcardContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteDefinitionAndRulesToolStripMenuItem,
            this.deleteDefinitionOnlyToolStripMenuItem,
            this.deleteGeneratedRulesOnlyToolStripMenuItem});
            this.wildcardContextMenu.Name = "wildcardContextMenu";
            this.wildcardContextMenu.Size = new System.Drawing.Size(286, 106);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(285, 24);
            this.editToolStripMenuItem.Text = "Edit...";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editRuleButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(282, 6);
            // 
            // deleteDefinitionAndRulesToolStripMenuItem
            // 
            this.deleteDefinitionAndRulesToolStripMenuItem.Name = "deleteDefinitionAndRulesToolStripMenuItem";
            this.deleteDefinitionAndRulesToolStripMenuItem.Size = new System.Drawing.Size(285, 24);
            this.deleteDefinitionAndRulesToolStripMenuItem.Text = "Delete Definition && Rules";
            this.deleteDefinitionAndRulesToolStripMenuItem.Click += new System.EventHandler(this.deleteRuleButton_Click);
            // 
            // deleteDefinitionOnlyToolStripMenuItem
            // 
            this.deleteDefinitionOnlyToolStripMenuItem.Name = "deleteDefinitionOnlyToolStripMenuItem";
            this.deleteDefinitionOnlyToolStripMenuItem.Size = new System.Drawing.Size(285, 24);
            this.deleteDefinitionOnlyToolStripMenuItem.Text = "Delete Definition Only";
            this.deleteDefinitionOnlyToolStripMenuItem.Click += new System.EventHandler(this.deleteDefinitionOnlyToolStripMenuItem_Click);
            // 
            // deleteGeneratedRulesOnlyToolStripMenuItem
            // 
            this.deleteGeneratedRulesOnlyToolStripMenuItem.Name = "deleteGeneratedRulesOnlyToolStripMenuItem";
            this.deleteGeneratedRulesOnlyToolStripMenuItem.Size = new System.Drawing.Size(285, 24);
            this.deleteGeneratedRulesOnlyToolStripMenuItem.Text = "Delete Generated Rules Only";
            this.deleteGeneratedRulesOnlyToolStripMenuItem.Click += new System.EventHandler(this.deleteAllGeneratedRulesToolStripMenuItem_Click);
            // 
            // WildcardRulesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wildcardDataGridView);
            this.Controls.Add(this.topPanel);
            this.Name = "WildcardRulesControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.topPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.wildcardDataGridView)).EndInit();
            this.wildcardContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel topPanel;
        private Button deleteRuleButton;
        private Button editRuleButton;
        private Button addRuleButton;
        private DataGridView wildcardDataGridView;
        private ContextMenuStrip wildcardContextMenu;
        private ToolStripMenuItem editToolStripMenuItem;
        private DataGridViewTextBoxColumn colFolderPath;
        private DataGridViewTextBoxColumn colExeName;
        private DataGridViewTextBoxColumn colAction;
        private DataGridViewTextBoxColumn colProtocol;
        private DataGridViewTextBoxColumn colLocalPorts;
        private DataGridViewTextBoxColumn colRemotePorts;
        private DataGridViewTextBoxColumn colRemoteAddresses;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem deleteDefinitionAndRulesToolStripMenuItem;
        private ToolStripMenuItem deleteDefinitionOnlyToolStripMenuItem;
        private ToolStripMenuItem deleteGeneratedRulesOnlyToolStripMenuItem;
    }
}