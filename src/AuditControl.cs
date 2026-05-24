using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DarkModeForms;
using MinimalFirewall.TypedObjects;
using static DarkModeForms.OSThemeColors;

namespace MinimalFirewall
{
    public partial class AuditControl : UserControl
    {
        // used by Designer.cs to dispose of tray icons/menus
        private IContainer? components = null;

        private MainViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private FirewallSentryService _firewallSentryService = null!;
        private DarkModeCS _dm = null!;
        private BindingSource _bindingSource = null!;

        // Cached GDI resources
        private Font _cachedBoldFont = null!;
        private readonly SolidBrush _cachedOverlayBrush;
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        public AuditControl()
        {
            InitializeComponent();
            SetupAdvancedGridProperties();

            _cachedOverlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
            _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            Disposed += OnDisposed;
        }

        private void SetupAdvancedGridProperties()
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, systemChangesDataGridView, [true]);
        }

        private void OnDisposed(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SystemChangesUpdated -= OnSystemChangesUpdated;
                _viewModel.StatusTextChanged -= OnStatusTextChanged;
            }

            _cachedBoldFont?.Dispose();
            _cachedOverlayBrush?.Dispose();
            _searchDebounceTimer?.Dispose();
        }

        public void Initialize(
            MainViewModel viewModel,
            FirewallSentryService firewallSentryService,
            AppSettings appSettings,
            DarkModeCS dm)
        {
            _viewModel = viewModel;
            _firewallSentryService = firewallSentryService;
            _appSettings = appSettings;
            _dm = dm;

            _bindingSource = [];
            systemChangesDataGridView.DataSource = _bindingSource;

            RegisterEventHandlers();

            auditSearchTextBox.Text = _appSettings.AuditSearchText;
            _cachedBoldFont = new Font(systemChangesDataGridView.DefaultCellStyle.Font ?? Control.DefaultFont, FontStyle.Bold);

            ApplySearchFilter();
        }

        private void RegisterEventHandlers()
        {
            _viewModel.SystemChangesUpdated += OnSystemChangesUpdated;
            _viewModel.StatusTextChanged += OnStatusTextChanged;
        }

        private void OnSystemChangesUpdated()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnSystemChangesUpdated));
                return;
            }
            ApplySearchFilter();
        }

        private void OnStatusTextChanged(string text)
        {
            if (InvokeRequired)
            {
                // Prevent shutdown deadlock 
                if (!IsDisposed && IsHandleCreated)
                {
                    try { BeginInvoke(() => OnStatusTextChanged(text)); }
                    catch (ObjectDisposedException) { }
                    catch (InvalidOperationException) { }
                }
                return;
            }

            statusLabel.Text = text;
        }

        public void ApplyThemeFixes()
        {
            if (Theme.IsSystemDarkMode())
            {
                statusStrip1.BackColor = Theme.Colors.Surface;
                statusLabel.ForeColor = Theme.Colors.TextActive;
            }
            else
            {
                statusStrip1.BackColor = SystemColors.Control;
                statusLabel.ForeColor = SystemColors.ControlText;
            }
        }

        public async void ApplySearchFilter()
        {
            if (systemChangesDataGridView is null || _viewModel?.SystemChanges is null)
            {
                return;
            }

            string searchText = auditSearchTextBox.Text;

            try
            {
                _bindingSource.RaiseListChangedEvents = false;

                List<FirewallRuleChange> changesCopy = [.. _viewModel.SystemChanges];

                var filteredChanges = await Task.Run(() =>
                {
                    return string.IsNullOrWhiteSpace(searchText) ? changesCopy : [.. changesCopy.Where(c =>
                          (c.Name != null && c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                          (c.Description != null && c.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                          (c.ApplicationName != null && c.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                          (c.Intervention != null && c.Intervention.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                          (c.Publisher != null && c.Publisher.Contains(searchText, StringComparison.OrdinalIgnoreCase)))];
                });

                if (IsDisposed)
                {
                    return;
                }

                SortableBindingList<FirewallRuleChange> bindableList = [.. filteredChanges];

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
                _bindingSource.RaiseListChangedEvents = true; 
                _bindingSource.ResetBindings(false); 
                systemChangesDataGridView.Refresh();

                foreach (DataGridViewColumn col in systemChangesDataGridView.Columns)
                {
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }

                sortColumn?.HeaderCell.SortGlyphDirection = sortDirection == ListSortDirection.Ascending ?
                        SortOrder.Ascending : SortOrder.Descending;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] ApplySearchFilter failed: {ex.Message}");
            }
        }

        private void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            if (_appSettings != null)
            {
                _appSettings.AuditSearchText = auditSearchTextBox.Text;
                _appSettings.Save();
                ApplySearchFilter();
            }
        }

        private void AuditSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void SystemChangesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }

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
            appPath = systemChangesDataGridView.SelectedRows.Count > 0 &&
                      systemChangesDataGridView.SelectedRows[0].DataBoundItem is FirewallRuleChange change
                      ? change.ApplicationName : null;

            return !string.IsNullOrEmpty(appPath);
        }

        private void AuditContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0)
            {
                openFileLocationToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
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
                    {
                        hasEnabledItems = true;
                    }
                    else
                    {
                        hasDisabledItems = true;
                    }

                    // Stop iterating if we already know we have both types
                    if (hasEnabledItems && hasDisabledItems)
                    {
                        break;
                    }
                }
            }

            deleteToolStripMenuItem.Enabled = true;

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

        private void ProcessSelectedChanges(Action<FirewallRuleChange, int> action)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            foreach (DataGridViewRow row in systemChangesDataGridView.SelectedRows)
            {
                if (row.DataBoundItem is FirewallRuleChange change)
                {
                    action(change, row.Index);
                }
            }
        }

        private void EnableSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessSelectedChanges((change, index) => _viewModel.EnableForeignRule(change));
        }

        private void DisableSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessSelectedChanges((change, index) => _viewModel.DisableForeignRule(change));
        }

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedAppContext(out string? appPath) || string.IsNullOrEmpty(appPath))
            {
                Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(appPath) && !Directory.Exists(appPath))
            {
                Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"/select, \"{appPath}\"");
            }
            catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
            {
                Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void CopyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
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

                        details.AppendLine($"Type: Logged Event ({change.Type})");
                        details.AppendLine($"Rule Name: {change.Name}");
                        details.AppendLine($"Application: {change.ApplicationName}");
                        details.AppendLine($"Publisher: {change.Publisher}");
                        details.AppendLine($"Intervention: {change.Intervention}");
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


        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemChangesDataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            var result = Messenger.MessageBox(
                $"Are you sure you want to permanently delete {systemChangesDataGridView.SelectedRows.Count} rule(s)?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                ProcessSelectedChanges((change, index) => _viewModel.DeleteForeignRule(change));
            }
        }


        private void SystemChangesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (sender is not DataGridView grid)
            {
                return;
            }

            if (grid.Rows[e.RowIndex].DataBoundItem is not FirewallRuleChange change)
            {
                return;
            }

            bool isDarkMode = Theme.IsSystemDarkMode();

            // Default fallback colors
            Color rowBackColor = Theme.Colors.Surface;
            Color foreColor = Theme.Colors.TextActive;

            string intervention = change.Intervention ?? string.Empty;
            bool isEnabled = change.Rule?.IsEnabled ?? false;

            // Route audit events to palette
            if (intervention.Contains("Auto-Disabled (OS)", StringComparison.OrdinalIgnoreCase))
            {
                rowBackColor = Theme.Colors.Control;
                foreColor = Theme.Colors.TextInactive;
            }
            else if (intervention.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || intervention.Contains("OS Block", StringComparison.OrdinalIgnoreCase) || !isEnabled)
            {
                rowBackColor = Theme.Colors.Danger;
                foreColor = Theme.Colors.DangerText;
            }
            else if (intervention.Contains("Allowed", StringComparison.OrdinalIgnoreCase) || isEnabled)
            {
                rowBackColor = Theme.Colors.ConnectionEstablished;
                foreColor = Theme.Colors.SuccessText;
            }

            if (grid.Columns[e.ColumnIndex].Name == "advInterventionColumn" ||
                grid.Columns[e.ColumnIndex].Name == "advStatusColumn")
            {
                if (_cachedBoldFont != null)
                {
                    e.CellStyle!.Font = _cachedBoldFont;
                }
            }

            // format timestamp
            var column = grid.Columns[e.ColumnIndex];
            if (column.Name == "advTimestampColumn" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("G");
                e.FormattingApplied = true;
            }

            e.CellStyle!.BackColor = rowBackColor;
            e.CellStyle.ForeColor = foreColor;

            // Preserve color on select
            if (grid.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.SelectionBackColor = SystemColors.Highlight;
                e.CellStyle.SelectionForeColor = SystemColors.HighlightText;
            }
            else
            {
                e.CellStyle.SelectionBackColor = rowBackColor;
                e.CellStyle.SelectionForeColor = foreColor;
            }
        }


        private void SystemChangesDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (sender is not DataGridView grid)
            {
                return;
            }

            var row = grid.Rows[e.RowIndex];

            if (row.Selected)
            {
                return;
            }

            var cursor = grid.PointToClient(MousePosition);
            var mouseOverRow = grid.HitTest(cursor.X, cursor.Y).RowIndex;

            if (e.RowIndex == mouseOverRow && _cachedOverlayBrush != null)
            {
                e.Graphics.FillRectangle(_cachedOverlayBrush, e.RowBounds);
            }
        }


        private void SystemChangesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (sender is not DataGridView grid)
                {
                    return;
                }

                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void SystemChangesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (sender is not DataGridView grid)
                {
                    return;
                }

                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void SystemChangesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                if (sender is not DataGridView grid)
                {
                    return;
                }

                var clickedRow = grid.Rows[e.RowIndex];

                if (!clickedRow.Selected)
                {
                    grid.ClearSelection();
                    clickedRow.Selected = true;
                }
            }
        }


        private void SystemChangesDataGridView_SelectionChanged(object sender, EventArgs e)
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

            bool isDark = Theme.IsSystemDarkMode();

            diffRichTextBox.BackColor = Theme.Colors.Surface;
            diffRichTextBox.ForeColor = Theme.Colors.TextActive;
            Font boldFont = new(diffRichTextBox.Font ?? Control.DefaultFont, FontStyle.Bold);
            Font regFont = diffRichTextBox.Font ?? Control.DefaultFont;

            Color oldColor = Theme.Colors.DangerText;
            Color newColor = Theme.Colors.SuccessText;
            Color labelColor = Theme.Colors.TextActive;

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
                var oldRule = change.OldRule;
                var newRule = change.Rule;

                if (oldRule == null || newRule == null)
                {
                    diffRichTextBox.AppendText("Rule modified, but previous state details are not available.");
                    return;
                }

                AppendText(diffRichTextBox, "Modifications Detected:", Color.Orange, boldFont);
                diffRichTextBox.AppendText(Environment.NewLine + Environment.NewLine);

                // Use a non-null placeholder so the compiler knows we are safe
                string oldStatus = oldRule.Status ?? "Unknown";
                string newStatus = newRule.Status ?? "Unknown";

                CompareAndPrint("Action", oldStatus, newStatus);
                CompareAndPrint("Direction", oldRule.Direction.ToString(), newRule.Direction.ToString());
                CompareAndPrint("Protocol", oldRule.ProtocolName ?? "", newRule.ProtocolName ?? "");
                CompareAndPrint("Local Ports", oldRule.LocalPorts ?? "", newRule.LocalPorts ?? "");
                CompareAndPrint("Remote Ports", oldRule.RemotePorts ?? "", newRule.RemotePorts ?? "");
                CompareAndPrint("Local Address", oldRule.LocalAddresses ?? "", newRule.LocalAddresses ?? "");
                CompareAndPrint("Remote Address", oldRule.RemoteAddresses ?? "", newRule.RemoteAddresses ?? "");
                CompareAndPrint("Program", oldRule.ApplicationName ?? "", newRule.ApplicationName ?? "");
                CompareAndPrint("Service", oldRule.ServiceName ?? "", newRule.ServiceName ?? "");
                CompareAndPrint("Status", oldRule.IsEnabled
                    ? "Enabled" : "Disabled", newRule.IsEnabled ? "Enabled" : "Disabled");
            }

            void CompareAndPrint(string label, string oldVal, string newVal)
            {
                if (!string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase))
                {
                    AppendText(diffRichTextBox, $"{label}: ", labelColor, boldFont);
                    AppendText(diffRichTextBox, oldVal, oldColor, new Font(regFont, FontStyle.Strikeout));
                    AppendText(diffRichTextBox, " -> ", labelColor, regFont);
                    AppendText(diffRichTextBox, newVal, newColor, boldFont);
                    diffRichTextBox.AppendText(Environment.NewLine);
                }
            }
        }

        private static string GetRuleSummary(AdvancedRuleViewModel rule)
        {
            if (rule == null)
            {
                return "Error: Rule is null";
            }

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

    }
}
