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

            InitializeColumnSettings(appSettings);
        }

        internal void InitializeColumnSettings(AppSettings appSettings)
        {
            _appSettings = appSettings;
            rulesDataGridView.AllowUserToOrderColumns = true;

            ConfigureCompactRuleColumns();

            DataGridViewHelper.RestoreColumnSettings(
                rulesDataGridView,
                _appSettings.RulesColumns,
                _appSettings);

            rulesDataGridView.ColumnDisplayIndexChanged += (_, _) =>
                DataGridViewHelper.SaveColumnSettings(
                    rulesDataGridView,
                    _appSettings.RulesColumns,
                    _appSettings);

            rulesDataGridView.ColumnWidthChanged +=
                RulesDataGridView_ColumnWidthChanged;
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

        public void ApplyRulesFilters()
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
                    
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Error creating rule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    private async void EditRuleToolStripMenuItem_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                if (rulesDataGridView.SelectedRows.Count != 1)
                {
                    return;
                }

                var aggregatedRule = GetFirstSelectedRule();
                if (aggregatedRule == null)
                {
                    return;
                }

                if (aggregatedRule.UnderlyingRules?.Count != 1)
                {
                    Messenger.MessageBox(
                        "This row represents multiple Windows Firewall rules. " +
                        "Minimal Firewall cannot safely determine which individual " +
                        "rule should be edited. Refine the selection or edit an " +
                        "individual rule after refreshing.",
                        "Rule Edit Stopped",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                AdvancedRuleViewModel originalRule =
                    aggregatedRule.UnderlyingRules[0];

                using var dialog = new CreateAdvancedRuleForm(
                    _actionsService,
                    originalRule,
                    _appSettings);

                if (dialog.ShowDialog(FindForm()) != DialogResult.OK ||
                    dialog.RuleVm == null)
                {
                    return;
                }

                if (originalRule.HasSameSettings(dialog.RuleVm))
                {
                    return;
                }

                var payload = new EditAdvancedRulePayload
                {
                    OriginalRule = originalRule,
                    UpdatedRule = dialog.RuleVm
                };

                _backgroundTaskService.EnqueueTask(
                    new FirewallTask(
                        FirewallTaskType.EditAdvancedRule,
                        payload,
                        $"Editing firewall rule '{originalRule.Name}'..."));

                await _backgroundTaskService.WhenIdleAsync();

                
            }
            catch (Exception ex)
            {
                Messenger.MessageBox(
                    $"An error occurred while editing the rule: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void DeleteSelectedRuleMenuItem_Click(
     object sender,
     EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count != 1)
            {
                Messenger.MessageBox(
                    "Select exactly one rule row. This command deletes one " +
                    "individual Windows Firewall rule.",
                    "Delete selected rule",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            AggregatedRuleViewModel? selected = GetFirstSelectedRule();
            if (selected == null)
            {
                return;
            }

            if (selected.IsWsl)
            {
                if (Messenger.MessageBox($"Delete the WSL rule '{selected.Name}' from OpenSnitch?", "Delete selected rule",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                _mainViewModel.DeleteWslRule(selected);
                ApplyRulesFilters();
                return;
            }

            if (selected.UnderlyingRules?.Count != 1)
            {
                Messenger.MessageBox(
                    "This row represents multiple Windows Firewall rules. " +
                    "Minimal Firewall cannot safely treat it as one selected " +
                    "rule. Refine the selection or refresh until one individual " +
                    "underlying rule is shown.\r\n\r\n" +
                    "No rules were deleted.",
                    "Delete selected rule stopped",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            AdvancedRuleViewModel rule =
                selected.UnderlyingRules[0];

            DialogResult confirmation = Messenger.MessageBox(
                "Delete one selected rule\r\n\r\n" +
                $"Rule name:\r\n{rule.Name}\r\n\r\n" +
                $"Application:\r\n" +
                $"{DisplayDeletionValue(rule.ApplicationName)}\r\n\r\n" +
                $"Direction: {rule.Direction}\r\n" +
                $"Action: {rule.Status}\r\n" +
                $"Protocol: {DisplayDeletionValue(rule.ProtocolName)}\r\n\r\n" +
                "Only this individual rule will be requested for deletion. " +
                "If another Windows Firewall rule has the same name, deletion " +
                "will be refused before INetFwRules.Remove is called.\r\n\r\n" +
                "Continue?",
                "Delete selected rule",
                MsgIcon.Warning,
                MessageBoxButtons.YesNo,
                pIsDarkMode: Theme.Colors.Surface.R < 128,
                defaultButton: MessageBoxDefaultButton.Button2,
                owner: FindForm());

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _backgroundTaskService.TakeLastOperationFailure();

                _backgroundTaskService.EnqueueTask(
                    new FirewallTask(
                        FirewallTaskType.DeleteSelectedRule,
                        new DeleteSelectedRulePayload
                        {
                            Rule = rule
                        },
                        $"Deleting selected rule '{rule.Name}'..."));
                await _backgroundTaskService.WhenIdleAsync();

                Exception? failure =
                    _backgroundTaskService.TakeLastOperationFailure();

                if (failure != null)
                {
                    throw failure;
                }

                if (DataRefreshRequested != null)
                {
                    await DataRefreshRequested.Invoke();
                }
                else
                {
                    await RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox(
                    "The selected rule was not deleted.\r\n\r\n" +
                    ex.Message,
                    "Delete selected rule stopped",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async void DeleteAllApplicationRulesMenuItem_Click(
            object sender,
            EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count != 1)
            {
                Messenger.MessageBox(
                    "Select exactly one application rule row before using " +
                    "application-wide deletion.",
                    "Delete all rules for this application",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            AggregatedRuleViewModel? selected = GetFirstSelectedRule();
            string appPath = selected?.ApplicationName?.Trim() ?? string.Empty;

            if (!IsDeletableExecutablePath(appPath))
            {
                Messenger.MessageBox(
                    "The selected row does not contain a fully qualified " +
                    "executable path. Application-wide deletion is unavailable " +
                    "for global, package, or pathless rules.",
                    "Delete all rules for this application",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ApplicationRuleDeletionPreview preview;

            try
            {
                preview =
                    FirewallRuleService.PreviewApplicationRuleDeletion(
                        appPath);
            }
            catch (Exception ex)
            {
                Messenger.MessageBox(
                    "The application-wide deletion preview could not be " +
                    "created.\r\n\r\n" + ex.Message,
                    "Delete all rules for this application",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (preview.MatchingRuleCount == 0)
            {
                Messenger.MessageBox(
                    "No firewall rules currently target this executable.",
                    "Delete all rules for this application",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmation = Messenger.MessageBox(
                BuildApplicationDeletionWarning(preview),
                "Delete all rules for this application",
                MsgIcon.Warning,
                MessageBoxButtons.YesNo,
                pIsDarkMode: Theme.Colors.Surface.R < 128,
                defaultButton: MessageBoxDefaultButton.Button2,
                owner: FindForm());

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _backgroundTaskService.TakeLastOperationFailure();

                _backgroundTaskService.EnqueueTask(
                    new FirewallTask(
                        FirewallTaskType.DeleteApplicationRules,
                        new DeleteApplicationRulesPayload
                        {
                            AppPaths = [preview.AppPath]
                        },
                        "Deleting all firewall rules for " +
                        $"{Path.GetFileName(preview.AppPath)}..."));

                await _backgroundTaskService.WhenIdleAsync();

                Exception? failure =
                    _backgroundTaskService.TakeLastOperationFailure();

                if (failure != null)
                {
                    throw failure;
                }

                if (DataRefreshRequested != null)
                {
                    await DataRefreshRequested.Invoke();
                }
                else
                {
                    await RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox(
                    "Application-wide deletion did not complete.\r\n\r\n" +
                    ex.Message,
                    "Delete all rules for this application stopped",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private static bool IsDeletableExecutablePath(string? appPath)
        {
            if (string.IsNullOrWhiteSpace(appPath))
            {
                return false;
            }

            string value = appPath.Trim().Trim('"');

            return value != "*" &&
                   !value.StartsWith('@') &&
                   Path.IsPathFullyQualified(value);
        }

        private static string DisplayDeletionValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "(none)"
                : value;
        }

        private static string BuildApplicationDeletionWarning(
            ApplicationRuleDeletionPreview preview)
        {
            const int nameLimit = 10;

            string displayedNames = string.Join(
                "\r\n",
                preview.RuleNames
                    .Take(nameLimit)
                    .Select(name => "• " + name));

            if (preview.RuleNames.Count > nameLimit)
            {
                displayedNames +=
                    $"\r\n• ...and " +
                    $"{preview.RuleNames.Count - nameLimit} more";
            }

            string types = preview.RuleTypes.Count == 0
                ? "(none)"
                : string.Join(
                    "\r\n",
                    preview.RuleTypes.Select(type => "• " + type));

            return
                "Delete all rules for this application\r\n\r\n" +
                $"Executable:\r\n{preview.AppPath}\r\n\r\n" +
                $"Total rules: {preview.MatchingRuleCount}\r\n" +
                $"Inbound: {preview.InboundRuleCount}\r\n" +
                $"Outbound: {preview.OutboundRuleCount}\r\n" +
                $"Allow: {preview.AllowRuleCount}\r\n" +
                $"Block: {preview.BlockRuleCount}\r\n" +
                $"Minimal Firewall rules: " +
                $"{preview.MinimalFirewallRuleCount}\r\n" +
                $"Windows, manual, or third-party rules: " +
                $"{preview.OtherRuleCount}\r\n\r\n" +
                "Affected rule types:\r\n" +
                $"{types}\r\n\r\n" +
                "Affected rule names:\r\n" +
                $"{displayedNames}\r\n\r\n" +
                "WARNING: This deletes every Windows Firewall rule whose " +
                "application path matches this executable. Windows-created, " +
                "manually created, and third-party rules may be included. " +
                "This is not equivalent to deleting one selected rule.\r\n\r\n" +
                "Continue?";
        }

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action) return;

            var selected = GetSelectedRules();
            foreach (var item in selected)
            {
                _mainViewModel.ApplyRuleChange(item, action);
            }
            if (action.Contains("(Inbound)", StringComparison.Ordinal) && selected.Any(r => r.IsWsl))
            {
                Messenger.MessageBox("OpenSnitch filters outbound connections only, so the selected WSL rules were not changed.", "WSL rules", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            bool hasSingleSelectedRow =
    rulesDataGridView.SelectedRows.Count == 1;

            bool hasOneUnderlyingRule =
                rule.UnderlyingRules?.Count == 1;

            editRuleToolStripMenuItem.Enabled =
                hasSingleSelectedRow &&
                isEditableType &&
                hasTarget &&
                hasOneUnderlyingRule;

            deleteSelectedRuleToolStripMenuItem.Enabled =
                hasSingleSelectedRow &&
                (hasOneUnderlyingRule || rule.IsWsl);

            deleteAllApplicationRulesToolStripMenuItem.Enabled =
                hasSingleSelectedRow &&
                IsDeletableExecutablePath(rule.ApplicationName);

            manageDomainsToolStripMenuItem.Enabled =
                hasSingleSelectedRow &&
                isEditableType &&
                !rule.IsWsl;
        }

        private async void ManageDomainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count != 1) return;
            var aggRule = GetFirstSelectedRule();
            if (aggRule == null)
            {
                return;
            }

            if (aggRule.UnderlyingRules?.Count != 1)
            {
                Messenger.MessageBox(
                    "This row represents multiple Windows Firewall rules. " +
                    "Domains cannot be changed safely until one individual rule " +
                    "can be selected.",
                    "Domain Edit Stopped",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var originalRule = aggRule.UnderlyingRules[0];
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

                var editPayload = new EditAdvancedRulePayload
                {
                    OriginalRule = originalRule,
                    UpdatedRule = updatedRule
                };

                _backgroundTaskService.EnqueueTask(
                    new FirewallTask(
                        FirewallTaskType.EditAdvancedRule,
                        editPayload,
                        $"Updating domains for firewall rule '{originalRule.Name}'..."));

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

        private void ConfigureCompactRuleColumns()
        {
            foreach (var column in new[] { inboundStatusColumn, outboundStatusColumn, advProtocolColumn })
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                int maximumWidth = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, false);
                if (column == inboundStatusColumn || column == outboundStatusColumn)
                {
                    var headerStyle = column.HeaderCell.InheritedStyle;
                    var font = headerStyle.Font ?? rulesDataGridView.Font;
                    int statusWidth = TextRenderer.MeasureText("Allow, Block", font).Width + headerStyle.Padding.Horizontal + 2;
                    maximumWidth = Math.Max(maximumWidth, statusWidth);
                }

                if (column.Width > maximumWidth)
                {
                    column.Width = maximumWidth;
                }
            }
        }

        private void RulesDataGridView_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
        {
            DataGridViewHelper.SaveColumnSettings(rulesDataGridView, _appSettings.RulesColumns, _appSettings);
        }

        private void RulesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.ColumnIndex < 0 || _appSettings == null) return;

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
                advIconColumn.Visible = DataGridViewHelper.IsIconColumnVisible(_appSettings, _appSettings.RulesColumns, advIconColumn.Name);
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
                _ when col == attributesColumn => rule.Attributes,
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
