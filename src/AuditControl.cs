// File: AuditControl.cs
using MinimalFirewall.TypedObjects;
using System.ComponentModel;
using DarkModeForms;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace MinimalFirewall
{
    public partial class AuditControl : UserControl
    {
        private IContainer components = null;
        private MainViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private ForeignRuleTracker _foreignRuleTracker;
        private FirewallSentryService _firewallSentryService;
        private DarkModeCS _dm;
        private BindingSource _bindingSource;
        public AuditControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(
            MainViewModel viewModel,
            ForeignRuleTracker foreignRuleTracker,
            FirewallSentryService firewallSentryService,
            AppSettings appSettings,
            DarkModeCS dm)
        {
            _viewModel = viewModel;
            _foreignRuleTracker = foreignRuleTracker;
            _firewallSentryService = firewallSentryService;
            _appSettings = appSettings;
            _dm = dm;

            systemChangesDataGridView.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            systemChangesDataGridView.DataSource = _bindingSource;
            _viewModel.SystemChangesUpdated += OnSystemChangesUpdated;

            auditSearchTextBox.Text = _appSettings.AuditSearchText;
        }

        private void OnSystemChangesUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(OnSystemChangesUpdated);
                return;
            }
            ApplySearchFilter();
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;
            rebuildBaselineButton.FlatAppearance.BorderSize = 1;
            rebuildBaselineButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            if (_dm.IsDarkMode)
            {
                rebuildBaselineButton.ForeColor = Color.White;
            }
            else
            {
                rebuildBaselineButton.ForeColor = SystemColors.ControlText;
            }
        }

        public void ApplySearchFilter()
        {
            if (systemChangesDataGridView is null || _viewModel?.SystemChanges is null) return;
            string searchText = auditSearchTextBox.Text;

            var filteredChanges = string.IsNullOrWhiteSpace(searchText) ?
                _viewModel.SystemChanges : _viewModel.SystemChanges.Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                                    c.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                                   c.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            _bindingSource.DataSource = new SortableBindingList<FirewallRuleChange>(filteredChanges.ToList());

            int sortCol = _appSettings.AuditSortColumn;
            var sortOrder = (SortOrder)_appSettings.AuditSortOrder;

            if (sortCol > -1 && sortOrder != SortOrder.None)
            {
                var column = systemChangesDataGridView.Columns[sortCol];
                var direction = sortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                systemChangesDataGridView.Sort(column, direction);
            }

            _bindingSource.ResetBindings(false);
            systemChangesDataGridView.Refresh();
        }

        private void AcceptAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_viewModel.SystemChanges.Count == 0) return;
            var result = DarkModeForms.Messenger.MessageBox($"Are you sure you want to accept all {_viewModel.SystemChanges.Count} detected changes? They will be hidden from this list.", "Confirm Accept All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _viewModel.AcceptAllForeignRules();
            }
        }

        private async void rebuildBaselineButton_Click(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var result = DarkModeForms.Messenger.MessageBox("This will clear all accepted (hidden) rules from the Audit list, causing them to be displayed again. Are you sure?", "Clear Accepted Rules", MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;

                await _viewModel.RebuildBaselineAsync();
            }
        }

        private void auditSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _appSettings.AuditSearchText = auditSearchTextBox.Text;
            ApplySearchFilter();
        }

        private void systemChangesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            var newColumn = systemChangesDataGridView.Columns[e.ColumnIndex];
            var sortOrder = systemChangesDataGridView.SortOrder;

            _appSettings.AuditSortColumn = e.ColumnIndex;
            _appSettings.AuditSortOrder = (int)sortOrder;
        }

        private bool TryGetSelectedAppContext(out string? appPath)
        {
            appPath = null;
            if (systemChangesDataGridView.SelectedRows.Count == 0)
            {
                return false;
            }

            if (systemChangesDataGridView.SelectedRows[0].DataBoundItem is FirewallRuleChange change)
            {
                appPath = change.ApplicationName;
            }

            return !string.IsNullOrEmpty(appPath);
        }

        private void auditContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (!TryGetSelectedAppContext(out string? appPath))
            {
                openFileLocationToolStripMenuItem.Enabled = false;
                return;
            }

            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && (File.Exists(appPath) || Directory.Exists(appPath));
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedAppContext(out string? appPath))
            {
                DarkModeForms.Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool pathExists = File.Exists(appPath) || Directory.Exists(appPath);
            if (!pathExists)
            {
                DarkModeForms.Messenger.MessageBox("The file or folder path for this item could not be found.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start("explorer.exe", $"/select, \"{appPath}\"");
            }
            catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
            {
                DarkModeForms.Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count > 0)
            {
                var details = new System.Text.StringBuilder();

                foreach (DataGridViewRow row in systemChangesDataGridView.SelectedRows)
                {
                    if (row.DataBoundItem is FirewallRuleChange change)
                    {
                        if (details.Length > 0)
                        {
                            details.AppendLine();
                            details.AppendLine();
                        }

                        details.AppendLine($"Type: Audited Change ({change.Type})");
                        details.AppendLine($"Rule Name: {change.Name}");
                        details.AppendLine($"Application: {change.ApplicationName}");
                        details.AppendLine($"Action: {change.Status}");
                        details.AppendLine($"Direction: {change.Rule.Direction}");
                        details.AppendLine($"Protocol: {change.ProtocolName}");
                        details.AppendLine($"Local Ports: {change.LocalPorts}");
                        details.AppendLine($"Remote Ports: {change.RemotePorts}");
                        details.AppendLine($"Local Addresses: {change.LocalAddresses}");
                        details.AppendLine($"Remote Addresses: {change.RemoteAddresses}");
                    }
                }

                if (details.Length > 0)
                {
                    Clipboard.SetText(details.ToString());
                }
            }
        }

        private void systemChangesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;
            var column = grid.Columns[e.ColumnIndex];

            if (grid.Rows[e.RowIndex].DataBoundItem is FirewallRuleChange change)
            {
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "acceptButtonColumn")
                    {
                        _viewModel.AcceptForeignRule(change);
                    }
                    else if (column.Name == "deleteButtonColumn")
                    {
                        _viewModel.DeleteForeignRule(change);
                    }
                }
            }
        }

        private void systemChangesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;
            if (grid.Rows[e.RowIndex].DataBoundItem is not FirewallRuleChange change) return;

            Color rowBackColor;
            switch (change.Type)
            {
                case ChangeType.New:
                    rowBackColor = Color.FromArgb(204, 255, 204);
                    break;
                case ChangeType.Modified:
                    rowBackColor = change.Status.Contains("Allow", StringComparison.OrdinalIgnoreCase)
                        ? Color.FromArgb(204, 255, 204)
                        : Color.FromArgb(255, 204, 204);
                    break;
                case ChangeType.Deleted:
                    rowBackColor = Color.FromArgb(255, 204, 204);
                    break;
                default:
                    rowBackColor = e.CellStyle.BackColor;
                    break;
            }

            var column = grid.Columns[e.ColumnIndex];
            if (column.Name == "acceptButtonColumn")
            {
                e.CellStyle.BackColor = Color.FromArgb(108, 117, 125);
                e.CellStyle.ForeColor = Color.White;
            }
            else if (column.Name == "deleteButtonColumn")
            {
                e.CellStyle.BackColor = Color.FromArgb(52, 58, 64);
                e.CellStyle.ForeColor = Color.White;
            }
            else
            {
                e.CellStyle.BackColor = rowBackColor;
                e.CellStyle.ForeColor = Color.Black;
            }

            if (grid.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.SelectionBackColor = SystemColors.Highlight;
                e.CellStyle.SelectionForeColor = SystemColors.HighlightText;
            }
            else
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
        }

        private void systemChangesDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];

            if (row.Selected) return;
            var mouseOverRow = grid.HitTest(grid.PointToClient(MousePosition).X, grid.PointToClient(MousePosition).Y).RowIndex;
            if (e.RowIndex == mouseOverRow)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void systemChangesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void systemChangesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void systemChangesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                var clickedRow = grid.Rows[e.RowIndex];

                if (!clickedRow.Selected)
                {
                    grid.ClearSelection();
                    clickedRow.Selected = true;
                }
            }
        }
    }
}