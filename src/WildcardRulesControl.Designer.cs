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
            components = new System.ComponentModel.Container();
            topPanel = new DarkModeForms.ThemedPanel();
            deleteRuleButton = new DarkModeForms.ThemedButton();
            editRuleButton = new DarkModeForms.ThemedButton();
            addRuleButton = new DarkModeForms.ThemedButton();
            wildcardDataGridView = new DarkModeForms.ThemedDataGridView();
            colFolderPath = new DataGridViewTextBoxColumn();
            colExeName = new DataGridViewTextBoxColumn();
            colAction = new DataGridViewTextBoxColumn();
            colProtocol = new DataGridViewTextBoxColumn();
            colLocalPorts = new DataGridViewTextBoxColumn();
            colRemotePorts = new DataGridViewTextBoxColumn();
            colRemoteAddresses = new DataGridViewTextBoxColumn();
            wildcardContextMenu = new ContextMenuStrip(components);
            editToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            deleteDefinitionAndRulesToolStripMenuItem = new ToolStripMenuItem();
            deleteDefinitionOnlyToolStripMenuItem = new ToolStripMenuItem();
            deleteGeneratedRulesOnlyToolStripMenuItem = new ToolStripMenuItem();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)wildcardDataGridView).BeginInit();
            wildcardContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(deleteRuleButton);
            topPanel.Controls.Add(editRuleButton);
            topPanel.Controls.Add(addRuleButton);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Margin = new Padding(3, 2, 3, 2);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(700, 41);
            topPanel.TabIndex = 0;
            // 
            // deleteRuleButton
            // 
            deleteRuleButton.Location = new Point(197, 9);
            deleteRuleButton.Margin = new Padding(3, 2, 3, 2);
            deleteRuleButton.Name = "deleteRuleButton";
            deleteRuleButton.Size = new Size(88, 23);
            deleteRuleButton.TabIndex = 2;
            deleteRuleButton.Text = "Delete Rule(s)";
            deleteRuleButton.UseVisualStyleBackColor = true;
            deleteRuleButton.Click += deleteRuleButton_Click;
            // 
            // editRuleButton
            // 
            editRuleButton.Location = new Point(104, 9);
            editRuleButton.Margin = new Padding(3, 2, 3, 2);
            editRuleButton.Name = "editRuleButton";
            editRuleButton.Size = new Size(88, 23);
            editRuleButton.TabIndex = 1;
            editRuleButton.Text = "Edit Rule...";
            editRuleButton.UseVisualStyleBackColor = true;
            editRuleButton.Click += editRuleButton_Click;
            // 
            // addRuleButton
            // 
            addRuleButton.Location = new Point(11, 9);
            addRuleButton.Margin = new Padding(3, 2, 3, 2);
            addRuleButton.Name = "addRuleButton";
            addRuleButton.Size = new Size(88, 23);
            addRuleButton.TabIndex = 0;
            addRuleButton.Text = "Add Rule...";
            addRuleButton.UseVisualStyleBackColor = true;
            addRuleButton.Click += addRuleButton_Click;
            // 
            // wildcardDataGridView
            // 
            wildcardDataGridView.AllowUserToAddRows = false;
            wildcardDataGridView.AllowUserToDeleteRows = false;
            wildcardDataGridView.AllowUserToResizeRows = false;
            wildcardDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            wildcardDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            wildcardDataGridView.Columns.AddRange(new DataGridViewColumn[] { colFolderPath, colExeName, colAction, colProtocol, colLocalPorts, colRemotePorts, colRemoteAddresses });
            wildcardDataGridView.ContextMenuStrip = wildcardContextMenu;
            wildcardDataGridView.Dock = DockStyle.Fill;
            wildcardDataGridView.Location = new Point(0, 41);
            wildcardDataGridView.Margin = new Padding(3, 2, 3, 2);
            wildcardDataGridView.Name = "wildcardDataGridView";
            wildcardDataGridView.ReadOnly = true;
            wildcardDataGridView.RowHeadersVisible = false;
            wildcardDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            wildcardDataGridView.Size = new Size(700, 439);
            wildcardDataGridView.TabIndex = 1;
            // 
            // colFolderPath
            // 
            colFolderPath.DataPropertyName = "FolderPath";
            colFolderPath.FillWeight = 30F;
            colFolderPath.HeaderText = "Folder Path";
            colFolderPath.Name = "colFolderPath";
            colFolderPath.ReadOnly = true;
            // 
            // colExeName
            // 
            colExeName.DataPropertyName = "ExeName";
            colExeName.FillWeight = 15F;
            colExeName.HeaderText = "Executable Name";
            colExeName.Name = "colExeName";
            colExeName.ReadOnly = true;
            // 
            // colAction
            // 
            colAction.DataPropertyName = "Action";
            colAction.FillWeight = 10F;
            colAction.HeaderText = "Action";
            colAction.Name = "colAction";
            colAction.ReadOnly = true;
            // 
            // colProtocol
            // 
            colProtocol.DataPropertyName = "Protocol";
            colProtocol.FillWeight = 8F;
            colProtocol.HeaderText = "Protocol";
            colProtocol.Name = "colProtocol";
            colProtocol.ReadOnly = true;
            // 
            // colLocalPorts
            // 
            colLocalPorts.DataPropertyName = "LocalPorts";
            colLocalPorts.FillWeight = 10F;
            colLocalPorts.HeaderText = "Local Ports";
            colLocalPorts.Name = "colLocalPorts";
            colLocalPorts.ReadOnly = true;
            // 
            // colRemotePorts
            // 
            colRemotePorts.DataPropertyName = "RemotePorts";
            colRemotePorts.FillWeight = 10F;
            colRemotePorts.HeaderText = "Remote Ports";
            colRemotePorts.Name = "colRemotePorts";
            colRemotePorts.ReadOnly = true;
            // 
            // colRemoteAddresses
            // 
            colRemoteAddresses.DataPropertyName = "RemoteAddresses";
            colRemoteAddresses.FillWeight = 17F;
            colRemoteAddresses.HeaderText = "Remote Addresses";
            colRemoteAddresses.Name = "colRemoteAddresses";
            colRemoteAddresses.ReadOnly = true;
            // 
            // wildcardContextMenu
            // 
            wildcardContextMenu.ImageScalingSize = new Size(20, 20);
            wildcardContextMenu.Items.AddRange(new ToolStripItem[] { editToolStripMenuItem, toolStripSeparator1, deleteDefinitionAndRulesToolStripMenuItem, deleteDefinitionOnlyToolStripMenuItem, deleteGeneratedRulesOnlyToolStripMenuItem });
            wildcardContextMenu.Name = "wildcardContextMenu";
            wildcardContextMenu.Size = new Size(224, 98);
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(223, 22);
            editToolStripMenuItem.Text = "Edit...";
            editToolStripMenuItem.Click += editRuleButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(220, 6);
            // 
            // deleteDefinitionAndRulesToolStripMenuItem
            // 
            deleteDefinitionAndRulesToolStripMenuItem.Name = "deleteDefinitionAndRulesToolStripMenuItem";
            deleteDefinitionAndRulesToolStripMenuItem.Size = new Size(223, 22);
            deleteDefinitionAndRulesToolStripMenuItem.Text = "Delete Definition && Rules";
            deleteDefinitionAndRulesToolStripMenuItem.Click += deleteRuleButton_Click;
            // 
            // deleteDefinitionOnlyToolStripMenuItem
            // 
            deleteDefinitionOnlyToolStripMenuItem.Name = "deleteDefinitionOnlyToolStripMenuItem";
            deleteDefinitionOnlyToolStripMenuItem.Size = new Size(223, 22);
            deleteDefinitionOnlyToolStripMenuItem.Text = "Delete Definition Only";
            deleteDefinitionOnlyToolStripMenuItem.Click += deleteDefinitionOnlyToolStripMenuItem_Click;
            // 
            // deleteGeneratedRulesOnlyToolStripMenuItem
            // 
            deleteGeneratedRulesOnlyToolStripMenuItem.Name = "deleteGeneratedRulesOnlyToolStripMenuItem";
            deleteGeneratedRulesOnlyToolStripMenuItem.Size = new Size(223, 22);
            deleteGeneratedRulesOnlyToolStripMenuItem.Text = "Delete Generated Rules Only";
            deleteGeneratedRulesOnlyToolStripMenuItem.Click += deleteAllGeneratedRulesToolStripMenuItem_Click;
            // 
            // WildcardRulesControl
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(wildcardDataGridView);
            Controls.Add(topPanel);
            Font = new Font("Roboto Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 2, 3, 2);
            Name = "WildcardRulesControl";
            Size = new Size(700, 480);
            topPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)wildcardDataGridView).EndInit();
            wildcardContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private DarkModeForms.ThemedPanel topPanel;
        private DarkModeForms.ThemedButton deleteRuleButton;
        private DarkModeForms.ThemedButton editRuleButton;
        private DarkModeForms.ThemedButton addRuleButton;
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
