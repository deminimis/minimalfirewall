using NetFwTypeLib;
using MinimalFirewall.TypedObjects;
using System.Data;
using System.ComponentModel;
using DarkModeForms;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System;
using System.Drawing;

namespace MinimalFirewall
{
    public partial class RulesControl : UserControl
    {
        private MainViewModel _mainViewModel = null!;
        private FirewallActionsService _actionsService = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private IconService _iconService = null!;
        private AppSettings _appSettings = null!;
        private DarkModeCS _dm = null!;

        // Sorting and Data State
        private int _rulesSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private SortableBindingList<AggregatedRuleViewModel> _currentRuleList = new();

        // UI Helpers
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        public event Func<Task>? DataRefreshRequested;

        public RulesControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                if (_appSettings != null)
                {
                    _appSettings.RulesSearchText = rulesSearchTextBox.Text;
                    await DisplayRulesAsync();
                }
            };
        }

        public void Initialize(
            MainViewModel mainViewModel,
            FirewallActionsService actionsService,
            WildcardRuleService wildcardRuleService,
            BackgroundFirewallTaskService backgroundTaskService,
            IconService iconService,
            AppSettings appSettings,
            ImageList appIconList,
            DarkModeCS dm)
        {
            _mainViewModel = mainViewModel;
            _actionsService = actionsService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _iconService = iconService;
            _appSettings = appSettings;
            _dm = dm;

            // Load initial filter states
            programFilterCheckBox.Checked = _appSettings.FilterPrograms;
            serviceFilterCheckBox.Checked = _appSettings.FilterServices;
            uwpFilterCheckBox.Checked = _appSettings.FilterUwp;
            wildcardFilterCheckBox.Checked = _appSettings.FilterWildcards;
            systemFilterCheckBox.Checked = _appSettings.FilterSystem;
            rulesSearchTextBox.Text = _appSettings.RulesSearchText;

            _rulesSortColumn = _appSettings.RulesSortColumn;
            _rulesSortOrder = (SortOrder)_appSettings.RulesSortOrder;

            // Reflection hack to enable double buffering on the grid for smoother scrolling
            typeof(DataGridView).InvokeMember(
               "DoubleBuffered",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
               null,
               rulesDataGridView,
               new object[] { true });

            rulesDataGridView.VirtualMode = true;
            rulesDataGridView.AutoGenerateColumns = false;
            rulesDataGridView.DataSource = null;
            rulesDataGridView.CellValueNeeded += RulesDataGridView_CellValueNeeded;

            _mainViewModel.RulesListUpdated += OnRulesListUpdated;

            programFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            serviceFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            uwpFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            wildcardFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
        }

        private void OnRulesListUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(OnRulesListUpdated);
                return;
            }

            _currentRuleList = _mainViewModel.VirtualRulesData;

            // Re-apply sorting if applicable
            if (_appSettings != null && _rulesSortColumn > -1 && _rulesSortColumn < rulesDataGridView.Columns.Count)
            {
                var col = rulesDataGridView.Columns[_rulesSortColumn];
                if (!string.IsNullOrEmpty(col.DataPropertyName))
                {
                    ListSortDirection dir = _rulesSortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                    _currentRuleList.Sort(col.DataPropertyName, dir);
                    col.HeaderCell.SortGlyphDirection = _rulesSortOrder;
                }
            }

            rulesDataGridView.RowCount = _currentRuleList.Count;
            rulesDataGridView.Invalidate();
        }

        private void RulesDataGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            // Safety check for bounds
            if (e.RowIndex >= _currentRuleList.Count || e.RowIndex < 0) return;

            var rule = _currentRuleList[e.RowIndex];
            var col = rulesDataGridView.Columns[e.ColumnIndex];

            // Modern switch expression for cleaner column mapping
            e.Value = col switch
            {
                _ when col == advIconColumn => GetIconForRule(rule),
                _ when col == advNameColumn => rule.Name,
                _ when col == inboundStatusColumn => rule.InboundStatus,
                _ when col == outboundStatusColumn => rule.OutboundStatus,
                _ when col == advProtocolColumn => rule.ProtocolName,
                _ when col == advLocalPortsColumn => rule.LocalPorts,
                _ when col == advRemotePortsColumn => rule.RemotePorts,
                _ when col == advLocalAddressColumn => rule.LocalAddresses,
                _ when col == advRemoteAddressColumn => rule.RemoteAddresses,
                _ when col == advProgramColumn => rule.ApplicationName,
                _ when col == advServiceColumn => rule.ServiceName,
                _ when col == advProfilesColumn => rule.Profiles,
                _ when col == advGroupingColumn => rule.Grouping,
                _ when col == advDescColumn => rule.Description,
                _ => e.Value
            };
        }

        private Image? GetIconForRule(AggregatedRuleViewModel rule)
        {
            if (!_appSettings.ShowAppIcons || string.IsNullOrEmpty(rule.ApplicationName)) return null;

            int iconIndex = _iconService.GetIconIndex(rule.ApplicationName);
            return (iconIndex != -1 && _iconService.ImageList != null)
                   ? _iconService.ImageList.Images[iconIndex]
                   : null;
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null || this.Disposing || this.IsDisposed) return;

            createRuleButton.FlatAppearance.BorderSize = 1;
            createRuleButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;

            if (_dm.IsDarkMode)
            {
                createRuleButton.ForeColor = Color.White;
            }
            else
            {
                createRuleButton.ForeColor = SystemColors.ControlText;
            }
        }

        public async Task RefreshDataAsync(bool forceUwpScan = false, IProgress<int>? progress = null, CancellationToken token = default)
        {
            await _mainViewModel.RefreshRulesDataAsync(token, progress);
            await DisplayRulesAsync();
        }

        public async Task OnTabSelectedAsync()
        {
            await DisplayRulesAsync();
        }

        public void UpdateIconColumnVisibility()
        {
            if (advIconColumn != null && _appSettings != null)
            {
                advIconColumn.Visible = _appSettings.ShowAppIcons;
                rulesDataGridView.InvalidateColumn(advIconColumn.Index);
            }
        }

        private async Task DisplayRulesAsync()
        {
            ApplyRulesFilters();
            await Task.CompletedTask;
        }

        private void filterCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_appSettings == null) return;

            _appSettings.FilterPrograms = programFilterCheckBox.Checked;
            _appSettings.FilterServices = serviceFilterCheckBox.Checked;
            _appSettings.FilterUwp = uwpFilterCheckBox.Checked;
            _appSettings.FilterWildcards = wildcardFilterCheckBox.Checked;
            _appSettings.FilterSystem = systemFilterCheckBox.Checked;

            ApplyRulesFilters();
        }

        private void ApplyRulesFilters()
        {
            var enabledTypes = new HashSet<RuleType>();
            if (programFilterCheckBox.Checked) enabledTypes.Add(RuleType.Program);
            if (serviceFilterCheckBox.Checked) enabledTypes.Add(RuleType.Service);
            if (uwpFilterCheckBox.Checked) enabledTypes.Add(RuleType.UWP);
            if (wildcardFilterCheckBox.Checked) enabledTypes.Add(RuleType.Wildcard);
            enabledTypes.Add(RuleType.Advanced);

            bool showSystemRules = systemFilterCheckBox.Checked;
            _mainViewModel.ApplyRulesFilters(rulesSearchTextBox.Text, enabledTypes, showSystemRules);
        }

        private void rulesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                if (!grid.Rows[e.RowIndex].Selected)
                {
                    grid.ClearSelection();
                    grid.Rows[e.RowIndex].Selected = true;
                }
            }
        }

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action || rulesDataGridView.SelectedRows.Count == 0) return;

            var items = new List<AggregatedRuleViewModel>();
            foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
            {
                if (row.Index < _currentRuleList.Count)
                {
                    items.Add(_currentRuleList[row.Index]);
                }
            }

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    _mainViewModel.ApplyRuleChange(item, action);
                }
            }
        }

        private async void editRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (rulesDataGridView.SelectedRows.Count == 1)
                {
                    int index = rulesDataGridView.SelectedRows[0].Index;
                    if (index >= _currentRuleList.Count) return;

                    var aggRule = _currentRuleList[index];
                    var originalRule = aggRule.UnderlyingRules?.FirstOrDefault();

                    if (originalRule == null)
                    {
                        DarkModeForms.Messenger.MessageBox("Cannot edit this rule as it has no underlying rule definition.", "Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using var dialog = new CreateAdvancedRuleForm(_actionsService, originalRule, _appSettings);
                    if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        if (dialog.RuleVm != null)
                        {
                            if (originalRule.HasSameSettings(dialog.RuleVm)) return;

                            var deletePayload = new DeleteRulesPayload { RuleIdentifiers = aggRule.UnderlyingRules.Select(r => r.Name).ToList() };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, deletePayload));

                            var createPayload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, createPayload));

                            await Task.Delay(500);
                            if (DataRefreshRequested != null)
                            {
                                await DataRefreshRequested.Invoke();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DarkModeForms.Messenger.MessageBox($"An error occurred while editing the rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>();

            foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
            {
                if (row.Index < _currentRuleList.Count)
                {
                    items.Add(_currentRuleList[row.Index]);
                }
            }

            if (items.Count > 0)
            {
                var result = DarkModeForms.Messenger.MessageBox($"Are you sure you want to delete the {items.Count} selected rule(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                _mainViewModel.DeleteRules(items);
                ApplyRulesFilters();
            }
        }

        private async void CreateRuleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new RuleWizardForm(_actionsService, _wildcardRuleService, _backgroundTaskService, _appSettings);
                if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    await Task.Delay(2000); // Wait for background service to process
                    if (DataRefreshRequested != null)
                    {
                        await DataRefreshRequested.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                DarkModeForms.Messenger.MessageBox($"Error creating rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            // Debounce the search input to improve performance
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void rulesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            int index = rulesDataGridView.SelectedRows[0].Index;
            if (index >= _currentRuleList.Count) return;

            var rule = _currentRuleList[index];

            string? appPath = rule.ApplicationName;
            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && File.Exists(appPath);

            var firstUnderlyingRule = rule.UnderlyingRules.FirstOrDefault();
            bool isEditableType = rule.Type == RuleType.Program || rule.Type == RuleType.Service || rule.Type == RuleType.Advanced;

            // Only allow editing if there is a concrete target (not a wildcard catch-all)
            bool hasTarget = firstUnderlyingRule != null &&
                             ((!string.IsNullOrEmpty(firstUnderlyingRule.ApplicationName) && firstUnderlyingRule.ApplicationName != "*") ||
                              !string.IsNullOrEmpty(firstUnderlyingRule.ServiceName));

            editRuleToolStripMenuItem.Enabled = rulesDataGridView.SelectedRows.Count == 1 && isEditableType && hasTarget;
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (rulesDataGridView.SelectedRows.Count > 0)
                {
                    int index = rulesDataGridView.SelectedRows[0].Index;
                    if (index < _currentRuleList.Count)
                    {
                        var rule = _currentRuleList[index];
                        string? appPath = rule.ApplicationName;

                        if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
                        {
                            // "explorer.exe /select, path" opens folder and highlights file
                            Process.Start("explorer.exe", $"/select, \"{appPath}\"");
                        }
                        else
                        {
                            DarkModeForms.Messenger.MessageBox("The path for this item is not available or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DarkModeForms.Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count > 0)
            {
                var details = new System.Text.StringBuilder();

                foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
                {
                    if (row.Index < _currentRuleList.Count)
                    {
                        var rule = _currentRuleList[row.Index];
                        if (details.Length > 0)
                        {
                            details.AppendLine().AppendLine();
                        }

                        details.AppendLine($"Rule Name: {rule.Name}");
                        details.AppendLine($"Type: {rule.Type}");
                        details.AppendLine($"Inbound: {rule.InboundStatus}");
                        details.AppendLine($"Outbound: {rule.OutboundStatus}");
                        details.AppendLine($"Application: {rule.ApplicationName}");
                        details.AppendLine($"Service: {rule.ServiceName}");
                        details.AppendLine($"Protocol: {rule.ProtocolName}");
                        details.AppendLine($"Local Ports: {rule.LocalPorts}");
                        details.AppendLine($"Remote Ports: {rule.RemotePorts}");
                        details.AppendLine($"Local Addresses: {rule.LocalAddresses}");
                        details.AppendLine($"Remote Addresses: {rule.RemoteAddresses}");
                        details.AppendLine($"Profiles: {rule.Profiles}");
                        details.AppendLine($"Group: {rule.Grouping}");
                        details.AppendLine($"Description: {rule.Description}");
                    }
                }

                if (details.Length > 0)
                {
                    Clipboard.SetText(details.ToString());
                }
            }
        }

        private void rulesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentRuleList.Count) return;

            var rule = _currentRuleList[e.RowIndex];

            // Color coding logic for Allow/Block
            bool hasAllow = (rule.InboundStatus != null && rule.InboundStatus.Contains("Allow")) || (rule.OutboundStatus != null && rule.OutboundStatus.Contains("Allow"));
            bool hasBlock = (rule.InboundStatus != null && rule.InboundStatus.Contains("Block")) || (rule.OutboundStatus != null && rule.OutboundStatus.Contains("Block"));

            if (hasAllow && hasBlock)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 255, 204); // Mixed
            }
            else if (hasAllow)
            {
                e.CellStyle.BackColor = Color.FromArgb(204, 255, 204); // Allow (Greenish)
            }
            else if (hasBlock)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 204, 204); // Block (Reddish)
            }

            if (hasAllow || hasBlock)
            {
                e.CellStyle.ForeColor = Color.Black;
            }

            // Maintain highlight contrast on selection
            if (rulesDataGridView.Rows[e.RowIndex].Selected)
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

        private void rulesDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (rulesDataGridView.Rows[e.RowIndex].Selected) return;

            var mouseOverRow = rulesDataGridView.HitTest(rulesDataGridView.PointToClient(MousePosition).X, rulesDataGridView.PointToClient(MousePosition).Y).RowIndex;
            if (e.RowIndex == mouseOverRow)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void rulesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                rulesDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void rulesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                rulesDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void rulesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || _appSettings == null) return;

            _rulesSortColumn = e.ColumnIndex;
            _rulesSortOrder = _rulesSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            foreach (DataGridViewColumn col in rulesDataGridView.Columns)
            {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            rulesDataGridView.Columns[_rulesSortColumn].HeaderCell.SortGlyphDirection = _rulesSortOrder;

            // Persist settings
            _appSettings.RulesSortColumn = _rulesSortColumn;
            _appSettings.RulesSortOrder = (int)_rulesSortOrder;
            _appSettings.Save();

            OnRulesListUpdated();
        }
    }
}