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
using System.IO;
namespace MinimalFirewall
{
    public partial class AuditControl : UserControl
    {
        private IContainer? components = null;

        private MainViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private ForeignRuleTracker _foreignRuleTracker = null!;
        private FirewallSentryService _firewallSentryService = null!;
        private DarkModeCS _dm = null!;
        private BindingSource _bindingSource = null!;
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

            foreach (DataGridViewColumn col in systemChangesDataGridView.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
            }

            _bindingSource = new BindingSource();
            systemChangesDataGridView.DataSource = _bindingSource;
            _viewModel.SystemChangesUpdated += OnSystemChangesUpdated;
            _viewModel.StatusTextChanged += OnStatusTextChanged;
            auditSearchTextBox.Text = _appSettings.AuditSearchText;
            quarantineCheckBox.Checked = _appSettings.QuarantineMode;

            toolTip1.SetToolTip(rebuildBaselineButton, "Unarchives all hidden rules, making them visible again in this list.");
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

        private void OnStatusTextChanged(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnStatusTextChanged(text));
                return;
            }
            statusLabel.Text = text;
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;
            rebuildBaselineButton.FlatAppearance.BorderSize = 1;
            rebuildBaselineButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;

            if (_dm.IsDarkMode)
            {
                rebuildBaselineButton.ForeColor = Color.White;
                quarantineCheckBox.ForeColor = Color.White;
                statusStrip1.BackColor = _dm.OScolors.Surface;
                statusLabel.ForeColor = _dm.OScolors.TextActive;
            }
            else
            {
                rebuildBaselineButton.ForeColor = SystemColors.ControlText;
                quarantineCheckBox.ForeColor = SystemColors.ControlText;
                statusStrip1.BackColor = SystemColors.Control;
                statusLabel.ForeColor = SystemColors.ControlText;
            }
        }

        public void ApplySearchFilter()
        {
            if (systemChangesDataGridView is null || _viewModel?.SystemChanges is null) return;
            string searchText = auditSearchTextBox.Text;

            var filteredChanges = string.IsNullOrWhiteSpace(searchText) ?
                _viewModel.SystemChanges : _viewModel.SystemChanges.Where(c =>
                      (c.Name != null && c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                      (c.Description != null && c.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                      (c.ApplicationName != null && c.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                      (c.Publisher != null && c.Publisher.Contains(searchText, StringComparison.OrdinalIgnoreCase)));

            var bindableList = new SortableBindingList<FirewallRuleChange>([.. filteredChanges]);

            int sortColIndex = _appSettings.AuditSortColumn;
            ListSortDirection sortDirection = _appSettings.AuditSortOrder == 0 ?
                ListSortDirection.Ascending : ListSortDirection.Descending;

            DataGridViewColumn? sortColumn = null;
            if (sortColIndex >= 0 && sortColIndex < systemChangesDataGridView.Columns.Count)
            {
                sortColumn = systemChangesDataGridView.Columns[sortColIndex];
                if (!string.IsNullOrEmpty(sortColumn.DataPropertyName))
                {
                    bindableList.Sort(sortColumn.DataPropertyName, sortDirection);
                }
            }
            else
            {
                bindableList.Sort("Timestamp", ListSortDirection.Descending);
            }

            _bindingSource.DataSource = bindableList;
            _bindingSource.ResetBindings(false);
            systemChangesDataGridView.Refresh();

            foreach (DataGridViewColumn col in systemChangesDataGridView.Columns)
            {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            if (sortColumn != null)
            {
                sortColumn.HeaderCell.SortGlyphDirection = sortDirection == ListSortDirection.Ascending ?
                    SortOrder.Ascending : SortOrder.Descending;
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
            if (_appSettings == null) return;
            _appSettings.AuditSearchText = auditSearchTextBox.Text;
            _appSettings.Save();

            ApplySearchFilter();
        }

        private void systemChangesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            int currentDirection = _appSettings.AuditSortOrder;
            int newDirection = 0; 

            if (_appSettings.AuditSortColumn == e.ColumnIndex)
            {
                newDirection = (currentDirection == 0) ? 1 : 0;
            }

            _appSettings.AuditSortColumn = e.ColumnIndex;
            _appSettings.AuditSortOrder = newDirection;
            _appSettings.Save();

            ApplySearchFilter();
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
            if (systemChangesDataGridView.SelectedRows.Count == 0)
            {
                openFileLocationToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                archiveSelectedToolStripMenuItem.Enabled = false;
                enableSelectedToolStripMenuItem.Enabled = false;
                disableSelectedToolStripMenuItem.Enabled = false;
                return;
            }

            bool hasEnabledItems = false;
            bool hasDisabledItems = false;

            foreach (DataGridViewRow row in systemChangesDataGridView.SelectedRows)
            {
                if (row.DataBoundItem is FirewallRuleChange change && change.Rule != null)
                {
                    if (change.Rule.IsEnabled)
                        hasEnabledItems = true;
                    else
                        hasDisabledItems = true;
                }
            }

            deleteToolStripMenuItem.Enabled = true;
            archiveSelectedToolStripMenuItem.Enabled = true;

            enableSelectedToolStripMenuItem.Visible = hasDisabledItems;
            enableSelectedToolStripMenuItem.Enabled = hasDisabledItems;

            disableSelectedToolStripMenuItem.Visible = hasEnabledItems;
            disableSelectedToolStripMenuItem.Enabled = hasEnabledItems;

            if (!TryGetSelectedAppContext(out string? appPath))
            {
                openFileLocationToolStripMenuItem.Enabled = false;
                return;
            }

            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && (File.Exists(appPath) || Directory.Exists(appPath));
        }

        private void archiveSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0) return;
            var rows = systemChangesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as FirewallRuleChange)
                .Where(c => c != null)
                .ToList();
            foreach (var change in rows)
            {
                _viewModel.AcceptForeignRule(change!);
            }
        }

        private void enableSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0) return;
            var rows = systemChangesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Where(r => r.DataBoundItem is FirewallRuleChange)
                .ToList();

            foreach (var row in rows)
            {
                if (row.DataBoundItem is FirewallRuleChange change)
                {
                    _viewModel.EnableForeignRule(change);
                    systemChangesDataGridView.InvalidateRow(row.Index); 
                }
            }
        }

        private void disableSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0) return;

            var rows = systemChangesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Where(r => r.DataBoundItem is FirewallRuleChange)
                .ToList();

            foreach (var row in rows)
            {
                if (row.DataBoundItem is FirewallRuleChange change)
                {
                    _viewModel.DisableForeignRule(change);
                    systemChangesDataGridView.InvalidateRow(row.Index); 
                }
            }
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedAppContext(out string? appPath) || string.IsNullOrEmpty(appPath))
            {
                DarkModeForms.Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(appPath) && !Directory.Exists(appPath))
            {
                DarkModeForms.Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        details.AppendLine($"Publisher: {change.Publisher}");
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count > 0)
            {
                var result = DarkModeForms.Messenger.MessageBox($"Are you sure you want to permanently delete {systemChangesDataGridView.SelectedRows.Count} rule(s)?", "Confirm Delete", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;

                var rows = systemChangesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                   .Select(r => r.DataBoundItem as FirewallRuleChange)
                   .Where(c => c != null)
                   .ToList();
                foreach (var change in rows)
                {
                    _viewModel.DeleteForeignRule(change!);
                }
            }
        }

        private void systemChangesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;
            if (grid.Rows[e.RowIndex].DataBoundItem is not FirewallRuleChange change) return;

            Color rowBackColor;
            Color foreColor = Color.Black;
            string pub = change.Publisher ?? string.Empty;
            bool isMicrosoft = pub.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(pub))
                rowBackColor = Color.FromArgb(255, 220, 220);
            else if (isMicrosoft)
                rowBackColor = Color.FromArgb(220, 255, 220);
            else
                rowBackColor = Color.FromArgb(255, 255, 220);
            if (change.Rule is { IsEnabled: false })
            {
                foreColor = Color.DimGray;
                if (grid.Columns[e.ColumnIndex].Name == "advStatusColumn")
                {
                    e.CellStyle!.Font = new(e.CellStyle.Font, FontStyle.Bold);
                }
            }

            var column = grid.Columns[e.ColumnIndex];
            if (column.Name == "advTimestampColumn" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("G");
                e.FormattingApplied = true;
            }
            else
            {
                e.CellStyle!.BackColor = rowBackColor;
                e.CellStyle.ForeColor = foreColor;
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

        private void systemChangesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count != 1)
            {
                diffRichTextBox.Clear();
                return;
            }

            if (systemChangesDataGridView.SelectedRows[0].DataBoundItem is FirewallRuleChange change)
            {
                ShowDiff(change);
            }
        }

        private void ShowDiff(FirewallRuleChange change)
        {
            diffRichTextBox.Clear();
            if (_dm != null && _dm.IsDarkMode)
            {
                diffRichTextBox.BackColor = _dm.OScolors.Surface;
                diffRichTextBox.ForeColor = _dm.OScolors.TextActive;
            }
            else
            {
                diffRichTextBox.BackColor = Color.White;
                diffRichTextBox.ForeColor = Color.Black;
            }

            Font boldFont = new(diffRichTextBox.Font, FontStyle.Bold);
            Font regFont = diffRichTextBox.Font;
            Color oldColor = Color.Red;
            Color newColor = Color.Green;
            if (_dm != null && _dm.IsDarkMode)
            {
                oldColor = Color.FromArgb(255, 100, 100);
                newColor = Color.LightGreen;
            }

            if (change.Type == ChangeType.New && change.Rule != null)
            {
                AppendText(diffRichTextBox, "New Rule Created:", newColor, boldFont);
                diffRichTextBox.AppendText(Environment.NewLine);
                diffRichTextBox.AppendText(GetRuleSummary(change.Rule));
            }
            else if (change.Type == ChangeType.Deleted && change.Rule != null)
            {
                AppendText(diffRichTextBox, "Rule Deleted:", oldColor, boldFont);
                diffRichTextBox.AppendText(Environment.NewLine);
                diffRichTextBox.AppendText(GetRuleSummary(change.Rule));
            }
            else if (change.Type == ChangeType.Modified)
            {
                if (change.OldRule == null || change.Rule == null)
                {
                    diffRichTextBox.AppendText("Rule modified, but previous state details are not available.");
                    return;
                }

                AppendText(diffRichTextBox, "Modifications Detected:", Color.Orange, boldFont);
                diffRichTextBox.AppendText(Environment.NewLine + Environment.NewLine);

                CompareAndPrint("Action", change.OldRule.Status, change.Rule.Status);
                CompareAndPrint("Direction", change.OldRule.Direction.ToString(), change.Rule.Direction.ToString());
                CompareAndPrint("Protocol", change.OldRule.ProtocolName, change.Rule.ProtocolName);
                CompareAndPrint("Local Ports", change.OldRule.LocalPorts, change.Rule.LocalPorts);
                CompareAndPrint("Remote Ports", change.OldRule.RemotePorts, change.Rule.RemotePorts);
                CompareAndPrint("Local Address", change.OldRule.LocalAddresses, change.Rule.LocalAddresses);
                CompareAndPrint("Remote Address", change.OldRule.RemoteAddresses, change.Rule.RemoteAddresses);
                CompareAndPrint("Program", change.OldRule.ApplicationName, change.Rule.ApplicationName);
                CompareAndPrint("Service", change.OldRule.ServiceName, change.Rule.ServiceName);
                CompareAndPrint("Status", change.OldRule.IsEnabled
                    ? "Enabled" : "Disabled", change.Rule.IsEnabled ? "Enabled" : "Disabled");
            }

            void CompareAndPrint(string label, string oldVal, string newVal)
            {
                if (!string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase))
                {
                    AppendText(diffRichTextBox, $"{label}: ", _dm?.IsDarkMode == true ? Color.White : Color.Black, boldFont);
                    AppendText(diffRichTextBox, oldVal, oldColor, new Font(regFont, FontStyle.Strikeout));
                    AppendText(diffRichTextBox, " -> ", _dm?.IsDarkMode == true ? Color.White : Color.Black, regFont);
                    AppendText(diffRichTextBox, newVal, newColor, boldFont);
                    diffRichTextBox.AppendText(Environment.NewLine);
                }
            }
        }

        private static string GetRuleSummary(AdvancedRuleViewModel rule)
        {
            if (rule == null) return "Error: Rule is null";
            return $"Name: {rule.Name}\nProgram: {rule.ApplicationName}\nAction: {rule.Status}\nDirection: {rule.Direction}\nRemote Ports: {rule.RemotePorts}\nStatus: {(rule.IsEnabled ? "Enabled" : "Disabled")}";
        }

        private static void AppendText(RichTextBox box, string text, Color color, Font font)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.SelectionFont = font;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        private void quarantineCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings == null) return;
            _appSettings.QuarantineMode = quarantineCheckBox.Checked;
            _appSettings.Save();
        }
    }
}