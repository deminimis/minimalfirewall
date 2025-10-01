// File: AuditControl.cs
using MinimalFirewall.TypedObjects;
using System.ComponentModel;
using DarkModeForms;
using System.Diagnostics;

namespace MinimalFirewall
{
    public partial class AuditControl : UserControl
    {
        private IContainer components = null;
        private MainViewModel _viewModel = null!;
        private ForeignRuleTracker _foreignRuleTracker;
        private FirewallSentryService _firewallSentryService;
        private DarkModeCS _dm;

        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;

        public AuditControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(systemChangesListView, true);
        }

        public void Initialize(
            MainViewModel viewModel,
            ForeignRuleTracker foreignRuleTracker,
            FirewallSentryService firewallSentryService,
            DarkModeCS dm)
        {
            _viewModel = viewModel;
            _foreignRuleTracker = foreignRuleTracker;
            _firewallSentryService = firewallSentryService;
            _dm = dm;
            systemChangesListView.DarkMode = dm;
            systemChangesListView.AcceptClicked += SystemChanges_AcceptClicked;
            systemChangesListView.DeleteClicked += SystemChanges_DeleteClicked;
            _viewModel.SystemChangesUpdated += OnSystemChangesUpdated;
            SetDefaultColumnWidths();
            this.systemChangesListView.Resize += new System.EventHandler(this.SystemChangesListView_Resize);
        }

        private void SetDefaultColumnWidths()
        {
            changeActionColumn.Width = 220;
            advNameColumn.Width = 200;
            advStatusColumn.Width = 150;
            advProtocolColumn.Width = 80;
            advLocalPortsColumn.Width = 120;
            advRemotePortsColumn.Width = 120;
            advLocalAddressColumn.Width = 150;
            advRemoteAddressColumn.Width = 150;
            advProgramColumn.Width = 250;
            advServiceColumn.Width = 150;
            advProfilesColumn.Width = 100;
            advGroupingColumn.Width = 150;
            advDescColumn.Width = 300;
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
            if (systemChangesListView is null || _viewModel?.SystemChanges is null) return;
            systemChangesListView.BeginUpdate();
            systemChangesListView.Items.Clear();
            string searchText = auditSearchTextBox.Text;

            var filteredChanges = string.IsNullOrWhiteSpace(searchText) ?
                _viewModel.SystemChanges : _viewModel.SystemChanges.Where(c => c.Rule?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
            foreach (var change in filteredChanges)
            {
                var item = new ListViewItem(
                    new[]
                    {
                        "",
                        change.Rule?.Name ?? "N/A",
                        change.Rule?.Status ?? "N/A",
                        change.Rule?.ProtocolName ?? "Any",
                        change.Rule?.LocalPorts.Any() == true ? string.Join(",", change.Rule.LocalPorts) : "Any",
                        change.Rule?.RemotePorts.Any() == true ? string.Join(",", change.Rule.RemotePorts) : "Any",
                        change.Rule?.LocalAddresses.Any() == true ? string.Join(",", change.Rule.LocalAddresses) : "Any",
                        change.Rule?.RemoteAddresses.Any() == true ? string.Join(",", change.Rule.RemoteAddresses) : "Any",
                        change.Rule?.ApplicationName,
                        change.Rule?.ServiceName,
                        change.Rule?.Profiles,
                        change.Rule?.Grouping,
                        change.Rule?.Description
                    })
                {
                    Tag = change
                };
                systemChangesListView.Items.Add(item);
            }
            systemChangesListView.EndUpdate();
        }

        private void SystemChanges_AcceptClicked(object? sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is FirewallRuleChange change)
            {
                _viewModel.AcceptForeignRule(change);
            }
        }

        private void SystemChanges_DeleteClicked(object? sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is FirewallRuleChange change)
            {
                _viewModel.DeleteForeignRule(change);
            }
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
            ApplySearchFilter();
        }

        private void systemChangesListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                _sortOrder = (_sortOrder == SortOrder.Ascending) ?
                    SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }
            _sortColumn = e.Column;
            systemChangesListView.ListViewItemSorter = new ListViewItemComparer(e.Column > 0 ? e.Column : 1, _sortOrder);
            systemChangesListView.Sort();
        }

        private bool TryGetSelectedAppContext(out string? appPath)
        {
            appPath = null;
            if (systemChangesListView.SelectedItems.Count == 0)
            {
                return false;
            }

            var selectedItem = systemChangesListView.SelectedItems[0];
            if (selectedItem.Tag is FirewallRuleChange change)
            {
                appPath = change.Rule?.ApplicationName;
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
            if (systemChangesListView.SelectedItems.Count == 0)
            {
                return;
            }

            var selectedItem = systemChangesListView.SelectedItems[0];
            if (selectedItem.Tag is not FirewallRuleChange change) return;

            var details = new System.Text.StringBuilder();
            details.AppendLine($"Type: Audited Change ({change.Type})");
            if (change.Rule != null)
            {
                details.AppendLine($"Rule Name: {change.Rule.Name}");
                details.AppendLine($"Application: {change.Rule.ApplicationName}");
                details.AppendLine($"Action: {change.Rule.Status}");
                details.AppendLine($"Direction: {change.Rule.Direction}");
                details.AppendLine($"Protocol: {change.Rule.ProtocolName}");
                details.AppendLine($"Remote Addresses: {(change.Rule.RemoteAddresses.Any() ? string.Join(", ", change.Rule.RemoteAddresses) : "Any")}");
            }

            if (details.Length > 0)
            {
                Clipboard.SetText(details.ToString());
            }
        }

        private void SystemChangesListView_Resize(object? sender, EventArgs e)
        {
            if (systemChangesListView.Columns.Count < 2) return;
            int totalColumnWidths = 0;
            for (int i = 0; i < systemChangesListView.Columns.Count - 1; i++)
            {
                totalColumnWidths += systemChangesListView.Columns[i].Width;
            }

            int lastColumnIndex = systemChangesListView.Columns.Count - 1;
            int lastColumnWidth = systemChangesListView.ClientSize.Width - totalColumnWidths;

            int minWidth = TextRenderer.MeasureText(systemChangesListView.Columns[lastColumnIndex].Text, systemChangesListView.Font).Width + 10;
            if (lastColumnWidth > minWidth)
            {
                systemChangesListView.Columns[lastColumnIndex].Width = lastColumnWidth;
            }
            else
            {
                systemChangesListView.Columns[lastColumnIndex].Width = minWidth;
            }
        }
    }
}