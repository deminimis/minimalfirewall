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

            if (advIconColumn != null) advIconColumn.DefaultCellStyle.NullValue = new Bitmap(1, 1);

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
            rulesDataGridView.VirtualMode = true;

            rulesDataGridView.CellValueNeeded += RulesDataGridView_CellValueNeeded;
            rulesDataGridView.MouseDown += RulesDataGridView_MouseDown;

            _mainViewModel.RulesListUpdated += OnRulesListUpdated;

            programFilterCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            serviceFilterCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            uwpFilterCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            wildcardFilterCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;

            rulesDataGridView.AllowUserToOrderColumns = true;
            DataGridViewHelper.RestoreColumnSettings(rulesDataGridView, _appSettings.RulesColumns);

            rulesDataGridView.ColumnDisplayIndexChanged += (s, e) => DataGridViewHelper.SaveColumnSettings(rulesDataGridView, _appSettings.RulesColumns, _appSettings);
            rulesDataGridView.ColumnWidthChanged += (s, e) => DataGridViewHelper.SaveColumnSettings(rulesDataGridView, _appSettings.RulesColumns, _appSettings);
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
            return [.. rulesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Where(r => r.Index >= 0 && r.Index < _currentRuleList.Count)
                .Select(r => _currentRuleList[r.Index])];
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
        private void FilterCheckBox_CheckedChanged(object? _1, EventArgs _2)
        {
            if (_appSettings == null) return;

            _appSettings.FilterPrograms = programFilterCheckBox.Checked;
            _appSettings.FilterServices = serviceFilterCheckBox.Checked;
            _appSettings.FilterUwp = uwpFilterCheckBox.Checked;
            _appSettings.FilterWildcards = wildcardFilterCheckBox.Checked;
            _appSettings.FilterSystem = systemFilterCheckBox.Checked;

            ApplyRulesFilters();
        }

        private void SearchTextBox_TextChanged(object? _1, EventArgs _2)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private async void CreateRuleButton_Click(object _, EventArgs _1)
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

        private async void EditRuleToolStripMenuItem_Click(object sender, EventArgs e)
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

                            var deletePayload = new DeleteRulesPayload { RuleIdentifiers = [.. aggRule.UnderlyingRules?.Select(r => r.Name) ?? []] };
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

        private void DeleteRuleMenuItem_Click(object _, EventArgs _1)
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

        private void RulesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var rule = GetFirstSelectedRule();
            if (rule == null)
            {
                e.Cancel = true;
                return;
            }

            string? appPath = rule.ApplicationName;
            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && (File.Exists(appPath) || appPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) || appPath.Contains("SystemApps", StringComparison.OrdinalIgnoreCase));

            var firstUnderlyingRule = rule.UnderlyingRules.FirstOrDefault();
            bool isEditableType = rule.Type == RuleType.Program || rule.Type == RuleType.Service || rule.Type == RuleType.Advanced;

            bool hasTarget = firstUnderlyingRule != null &&
                             ((!string.IsNullOrEmpty(firstUnderlyingRule.ApplicationName) && firstUnderlyingRule.ApplicationName != "*") ||
                              !string.IsNullOrEmpty(firstUnderlyingRule.ServiceName));
            editRuleToolStripMenuItem.Enabled = rulesDataGridView.SelectedRows.Count == 1 && isEditableType && hasTarget;
            manageDomainsToolStripMenuItem.Enabled = rulesDataGridView.SelectedRows.Count == 1 && isEditableType;
        }

        private async void ManageDomainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count != 1) return;
            var aggRule = GetFirstSelectedRule();
            if (aggRule == null) return;
            var originalRule = aggRule.UnderlyingRules?.FirstOrDefault();
            if (originalRule == null) return;

            string currentDescription = originalRule.Description ?? "";
            string currentDomains = "";

            // Extract existing domains from the tag
            int start = currentDescription.IndexOf("[MFW-Domain:");
            if (start >= 0)
            {
                int end = currentDescription.IndexOf("]", start);
                if (end > start)
                {
                    currentDomains = currentDescription.Substring(start + 12, end - start - 12).Trim();
                }
            }

            using var dialog = new ManageDomainsForm(currentDomains, originalRule.Status);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                string newDomains = dialog.Domains;
                // Strip out old tag so we can append a fresh one
                string cleanDescription = MyRegex().Replace(currentDescription, "").Trim();
                string newDescription = cleanDescription;

                if (!string.IsNullOrWhiteSpace(newDomains))
                {
                    newDescription += $" [MFW-Domain: {newDomains}]";
                }

                string newRemoteAddresses = originalRule.RemoteAddresses ?? "*";

                // Resolve entered domains immediately for the initial rule state
                if (!string.IsNullOrWhiteSpace(newDomains))
                {
                    var resolvedIps = new List<string>();
                    var parts = newDomains.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var part in parts)
                    {
                        try
                        {
                            var ips = await System.Net.Dns.GetHostAddressesAsync(part);
                            resolvedIps.AddRange(ips.Select(ip => ip.ToString()));
                        }
                        catch
                        {
                            Messenger.MessageBox($"Could not resolve domain '{part}'. Check spelling or internet connection.", "DNS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Halt if a domain fails to prevent creating a broken rule
                        }
                    }
                    newRemoteAddresses = resolvedIps.Count > 0 ? string.Join(",", resolvedIps.Distinct()) : "*";
                }
                else if (currentDomains != "")
                {
                    // If domains were removed entirely, revert remote addresses to Any (*)
                    newRemoteAddresses = "*";
                }

                // Don't recreate if no change
                if (originalRule.Description == newDescription && originalRule.RemoteAddresses == newRemoteAddresses)
                    return;

                // Clone the rule properties 
                var updatedRule = new AdvancedRuleViewModel
                {
                    Name = originalRule.Name,
                    Description = newDescription,
                    IsEnabled = originalRule.IsEnabled,
                    Status = originalRule.Status,
                    Direction = originalRule.Direction,
                    Protocol = originalRule.Protocol,
                    ProtocolName = originalRule.ProtocolName,
                    ApplicationName = originalRule.ApplicationName,
                    ServiceName = originalRule.ServiceName,
                    LocalPorts = originalRule.LocalPorts,
                    RemotePorts = originalRule.RemotePorts,
                    LocalAddresses = originalRule.LocalAddresses,
                    RemoteAddresses = newRemoteAddresses,
                    Profiles = originalRule.Profiles,
                    Grouping = originalRule.Grouping,
                    Type = originalRule.Type,
                    InterfaceTypes = originalRule.InterfaceTypes,
                    IcmpTypesAndCodes = originalRule.IcmpTypesAndCodes
                };

                // Queue the replacement via the background service
                var deletePayload = new DeleteRulesPayload { RuleIdentifiers = [.. aggRule.UnderlyingRules?.Select(r => r.Name) ?? []] };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, deletePayload));

                var createPayload = new CreateAdvancedRulePayload { ViewModel = updatedRule, InterfaceTypes = updatedRule.InterfaceTypes, IcmpTypesAndCodes = updatedRule.IcmpTypesAndCodes };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, createPayload));

                await _backgroundTaskService.WhenIdleAsync();
                if (DataRefreshRequested != null)
                {
                    await DataRefreshRequested.Invoke();
                }
            }
        }

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var rule = GetFirstSelectedRule();
                if (rule != null)
                {
                    string? appPath = rule.ApplicationName;

                    if (!string.IsNullOrEmpty(appPath) && (File.Exists(appPath) || appPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) || appPath.Contains("SystemApps", StringComparison.OrdinalIgnoreCase)))
                    {
                        string safePath = appPath.Trim('\"');
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

        private void CopyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void RulesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
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

        private void RulesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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

            // Allow '@' strings through so UWP icons can be processed
            if (rule.ApplicationName == "*" ||
                rule.ApplicationName.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            int iconIndex = (rule.Type == RuleType.UWP)
                ? _iconService.GetUwpIconIndex(rule.ApplicationName)
                : _iconService.GetIconIndex(rule.ApplicationName);

            return (iconIndex != -1 && _iconService.ImageList != null)
                   ? _iconService.ImageList.Images[iconIndex]
                   : null;
        }

        private static string GetDomainsFromDescription(string? description)
        {
            if (string.IsNullOrEmpty(description)) return "";
            int start = description.IndexOf("[MFW-Domain:");
            if (start >= 0)
            {
                int end = description.IndexOf("]", start);
                if (end > start) return description.Substring(start + 12, end - start - 12).Trim();
            }
            return "";
        }

        private static string CleanDescription(string? description)
        {
            if (string.IsNullOrEmpty(description)) return "";
            return MyRegex().Replace(description, "").Trim();
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
                _ when col == advDomainsColumn => GetDomainsFromDescription(rule.Description),
                _ when col == advDescColumn => CleanDescription(rule.Description),
                _ when col == dateAddedColumn => rule.DateAdded.HasValue ? rule.DateAdded.Value.ToLocalTime() : null,
                _ when col == autoAllowedColumn => string.IsNullOrEmpty(rule.AutoAllowedPublisher) ? "" : "Auto",
                _ => e.Value
            };
        }

        private void RulesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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

        private void RulesDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (rulesDataGridView.Rows[e.RowIndex].Selected) return;

            var mouseOverRow = rulesDataGridView.HitTest(rulesDataGridView.PointToClient(MousePosition).X, rulesDataGridView.PointToClient(MousePosition).Y).RowIndex;
            if (e.RowIndex == mouseOverRow)
            {
                using var overlayBrush = new SolidBrush(Theme.Colors.HighlightOverlay);
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void RulesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) rulesDataGridView.InvalidateRow(e.RowIndex);
        }

        private void RulesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) rulesDataGridView.InvalidateRow(e.RowIndex);
        }
        #endregion

        private void RulesControl_Load(object? _1, EventArgs _2)
        {

        }

        [System.Text.RegularExpressions.GeneratedRegex(@"\s*\[MFW-Domain:.*?\]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}
