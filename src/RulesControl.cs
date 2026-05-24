using NetFwTypeLib;
using MinimalFirewall.TypedObjects;
using DarkModeForms;
using System.Data;
using System.ComponentModel;

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
        #region Fields & Dependencies
        private MainViewModel _mainViewModel = null!;
        private FirewallActionsService _actionsService = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private IconService _iconService = null!;
        private AppSettings _appSettings = null!;


        // Sorting and Data State
        private int _rulesSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private SortableBindingList<AggregatedRuleViewModel> _currentRuleList = [];
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        public event Func<Task>? DataRefreshRequested;
        #endregion

        #region Initialization
        public RulesControl()
        {
            InitializeComponent();
            DoubleBuffered = true;

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
            ImageList appIconList)
        {
            _mainViewModel = mainViewModel;
            _actionsService = actionsService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _iconService = iconService;
            _appSettings = appSettings;

            // Load initial filter states
            programFilterCheckBox.Checked = _appSettings.FilterPrograms;
            serviceFilterCheckBox.Checked = _appSettings.FilterServices;
            uwpFilterCheckBox.Checked = _appSettings.FilterUwp;
            wildcardFilterCheckBox.Checked = _appSettings.FilterWildcards;
            systemFilterCheckBox.Checked = _appSettings.FilterSystem;
            rulesSearchTextBox.Text = _appSettings.RulesSearchText;

            _rulesSortColumn = _appSettings.RulesSortColumn;
            _rulesSortOrder = (SortOrder)_appSettings.RulesSortOrder;

            // Keep here, not in designer properties
            rulesDataGridView.AutoGenerateColumns = false;
            rulesDataGridView.DataSource = null;

            // Advanced double buffering 
            typeof(DataGridView).InvokeMember(
               "DoubleBuffered",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
               null,
               rulesDataGridView,
               new object[] { true });

            rulesDataGridView.CellValueNeeded += RulesDataGridView_CellValueNeeded;
            rulesDataGridView.MouseDown += RulesDataGridView_MouseDown;

            _mainViewModel.RulesListUpdated += OnRulesListUpdated;

            programFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            serviceFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            uwpFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
            wildcardFilterCheckBox.CheckedChanged += filterCheckBox_CheckedChanged;
        }
        #endregion

        #region Data & Logic
        public async Task RefreshDataAsync(bool forceUwpScan = false, IProgress<int>? progress = null, CancellationToken token = default)
        {
            await _mainViewModel.RefreshRulesDataAsync(token, progress);
            await DisplayRulesAsync();
        }

        public async Task OnTabSelectedAsync()
        {
            await DisplayRulesAsync();
        }

        private async Task DisplayRulesAsync()
        {
            ApplyRulesFilters();
            await Task.CompletedTask;
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

        private void OnRulesListUpdated()
        {
            if (InvokeRequired)
            {
                Invoke(OnRulesListUpdated);
                return;
            }

            _currentRuleList = _mainViewModel.VirtualRulesData;
            if (_appSettings != null && _rulesSortColumn > -1 && _rulesSortColumn < rulesDataGridView.Columns.Count)
            {
                var col = rulesDataGridView.Columns[_rulesSortColumn];
                if (!string.IsNullOrEmpty(col.DataPropertyName))
                {
                    ListSortDirection dir = _rulesSortOrder == SortOrder.Ascending ?
                        ListSortDirection.Ascending : ListSortDirection.Descending;
                    _currentRuleList.Sort(col.DataPropertyName, dir);
                    col.HeaderCell.SortGlyphDirection = _rulesSortOrder;
                }
            }

            rulesDataGridView.RowCount = _currentRuleList.Count;
            rulesDataGridView.Invalidate();
        }

        private List<AggregatedRuleViewModel> GetSelectedRules()
        {
            return rulesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Where(r => r.Index >= 0 && r.Index < _currentRuleList.Count)
                .Select(r => _currentRuleList[r.Index])
                .ToList();
        }

        private AggregatedRuleViewModel? GetFirstSelectedRule()
        {
            if (rulesDataGridView.SelectedRows.Count > 0)
            {
                int index = rulesDataGridView.SelectedRows[0].Index;
                if (index >= 0 && index < _currentRuleList.Count)
                {
                    return _currentRuleList[index];
                }
            }
            return null;
        }
        #endregion

        #region UI Behavior (Events & Interaction)
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

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private async void CreateRuleButton_Click(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new RuleWizardForm(_actionsService, _wildcardRuleService, _backgroundTaskService, _appSettings);
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    await _backgroundTaskService.WhenIdleAsync();
                    if (DataRefreshRequested != null)
                    {
                        await DataRefreshRequested.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Error creating rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void editRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (rulesDataGridView.SelectedRows.Count == 1)
                {
                    var aggRule = GetFirstSelectedRule();
                    if (aggRule == null) return;

                    var originalRule = aggRule.UnderlyingRules?.FirstOrDefault();
                    if (originalRule == null)
                    {
                        Messenger.MessageBox("Cannot edit this rule as it has no underlying rule definition.", "Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using var dialog = new CreateAdvancedRuleForm(_actionsService, originalRule, _appSettings);
                    if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                    {
                        if (dialog.RuleVm != null)
                        {
                            if (originalRule.HasSameSettings(dialog.RuleVm)) return;

                            var deletePayload = new DeleteRulesPayload { RuleIdentifiers = aggRule.UnderlyingRules?.Select(r => r.Name).ToList() ?? [] };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, deletePayload));

                            var createPayload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, createPayload));

                            await _backgroundTaskService.WhenIdleAsync();
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
                Messenger.MessageBox($"An error occurred while editing the rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            var items = GetSelectedRules();
            if (items.Count > 0)
            {
                var result = Messenger.MessageBox($"Are you sure you want to delete the {items.Count} selected rule(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                _mainViewModel.DeleteRules(items);
                ApplyRulesFilters();
            }
        }

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action) return;

            foreach (var item in GetSelectedRules())
            {
                _mainViewModel.ApplyRuleChange(item, action);
            }
        }

        private void rulesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var rule = GetFirstSelectedRule();
            if (rule == null)
            {
                e.Cancel = true;
                return;
            }

            string? appPath = rule.ApplicationName;
            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && File.Exists(appPath);

            var firstUnderlyingRule = rule.UnderlyingRules.FirstOrDefault();
            bool isEditableType = rule.Type == RuleType.Program || rule.Type == RuleType.Service || rule.Type == RuleType.Advanced;

            bool hasTarget = firstUnderlyingRule != null &&
                             ((!string.IsNullOrEmpty(firstUnderlyingRule.ApplicationName) && firstUnderlyingRule.ApplicationName != "*") ||
                              !string.IsNullOrEmpty(firstUnderlyingRule.ServiceName));
            editRuleToolStripMenuItem.Enabled = rulesDataGridView.SelectedRows.Count == 1 && isEditableType && hasTarget;
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var rule = GetFirstSelectedRule();
                if (rule != null)
                {
                    string? appPath = rule.ApplicationName;

                    if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
                    {
                        string safePath = appPath.Replace("\"", "");
                        Process.Start("explorer.exe", $"/select, \"{safePath}\"");
                    }
                    else
                    {
                        Messenger.MessageBox("The path for this item is not available or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRules = GetSelectedRules();
            if (selectedRules.Count > 0)
            {
                var details = new System.Text.StringBuilder();
                foreach (var rule in selectedRules)
                {
                    if (details.Length > 0) details.AppendLine().AppendLine();

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
                    if (!string.IsNullOrEmpty(rule.AutoAllowedPublisher))
                    {
                        details.AppendLine($"Auto-allowed publisher: {rule.AutoAllowedPublisher}");
                    }
                }

                if (details.Length > 0)
                {
                    Clipboard.SetText(details.ToString());
                }
            }
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

        private void RulesDataGridView_MouseDown(object? sender, MouseEventArgs e)
        {
            var hit = rulesDataGridView.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.None)
            {
                rulesDataGridView.ClearSelection();
            }
        }

        private void rulesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || _appSettings == null) return;

            if (_rulesSortColumn == e.ColumnIndex)
            {
                _rulesSortOrder = _rulesSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _rulesSortColumn = e.ColumnIndex;
                _rulesSortOrder = SortOrder.Ascending;
            }

            foreach (DataGridViewColumn col in rulesDataGridView.Columns)
            {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            rulesDataGridView.Columns[_rulesSortColumn].HeaderCell.SortGlyphDirection = _rulesSortOrder;

            _appSettings.RulesSortColumn = _rulesSortColumn;
            _appSettings.RulesSortOrder = (int)_rulesSortOrder;
            _appSettings.Save();

            OnRulesListUpdated();
        }
        #endregion

        #region Theme & Styling
        public void ApplyThemeFixes()
        {
            if (Disposing || IsDisposed) return;

            createRuleButton.FlatAppearance.BorderColor = Theme.Colors.ControlDark;
            createRuleButton.ForeColor = Theme.Colors.TextActive;
        }

        public void UpdateIconColumnVisibility()
        {
            if (advIconColumn != null && _appSettings != null)
            {
                advIconColumn.Visible = _appSettings.ShowAppIcons;
                rulesDataGridView.InvalidateColumn(advIconColumn.Index);
            }
        }

        private Image? GetIconForRule(AggregatedRuleViewModel rule)
        {
            if (!_appSettings.ShowAppIcons || string.IsNullOrEmpty(rule.ApplicationName)) return null;

            if (rule.Type == RuleType.UWP ||
                rule.ApplicationName.StartsWith("@", StringComparison.Ordinal) ||
                rule.ApplicationName == "*" ||
                rule.ApplicationName.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            int iconIndex = _iconService.GetIconIndex(rule.ApplicationName);
            return (iconIndex != -1 && _iconService.ImageList != null)
                   ? _iconService.ImageList.Images[iconIndex]
                   : null;
        }

        private void RulesDataGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= _currentRuleList.Count || e.RowIndex < 0) return;

            var rule = _currentRuleList[e.RowIndex];
            var col = rulesDataGridView.Columns[e.ColumnIndex];
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
                _ when col == dateAddedColumn => rule.DateAdded.HasValue ? rule.DateAdded.Value.ToLocalTime() : null,
                _ when col == autoAllowedColumn => string.IsNullOrEmpty(rule.AutoAllowedPublisher) ? "" : "Auto",
                _ => e.Value
            };
        }

        private void rulesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentRuleList.Count) return;

            var rule = _currentRuleList[e.RowIndex];
            ApplyRuleCellTheme(e, rule);
        }

        private void ApplyRuleCellTheme(DataGridViewCellFormattingEventArgs e, AggregatedRuleViewModel rule)
        {
            bool hasAllow = (rule.InboundStatus != null && rule.InboundStatus.Contains("Allow")) ||
                            (rule.OutboundStatus != null && rule.OutboundStatus.Contains("Allow"));
            bool hasBlock = (rule.InboundStatus != null && rule.InboundStatus.Contains("Block")) ||
                            (rule.OutboundStatus != null && rule.OutboundStatus.Contains("Block"));

            if (hasAllow && hasBlock) e.CellStyle.BackColor = Theme.Colors.Warning;
            else if (hasAllow) e.CellStyle.BackColor = Theme.Colors.Success;
            else if (hasBlock) e.CellStyle.BackColor = Theme.Colors.Danger;

            // black text on highlight
            if (hasAllow || hasBlock) e.CellStyle.ForeColor = Color.Black;

            if (rulesDataGridView.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.SelectionBackColor = Theme.Colors.SelectionInfo;
                // black text on blue
                e.CellStyle.SelectionForeColor = Color.Black;
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
                using var overlayBrush = new SolidBrush(Theme.Colors.HighlightOverlay);
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void rulesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) rulesDataGridView.InvalidateRow(e.RowIndex);
        }

        private void rulesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) rulesDataGridView.InvalidateRow(e.RowIndex);
        }
        #endregion
    }
}
