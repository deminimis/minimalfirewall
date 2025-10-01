// File: RulesControl.cs
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

namespace MinimalFirewall
{
    public partial class RulesControl : UserControl
    {
        private IContainer? components = null;
        private MainViewModel _mainViewModel = null!;
        private FirewallActionsService _actionsService = null!;
        private INetFwPolicy2 _firewallPolicy = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private IconService _iconService = null!;
        private AppSettings _appSettings = null!;
        private DarkModeCS _dm = null!;

        private int _rulesSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private bool _uwpAppsScanned = false;
        private ListViewItem? _hoveredItem = null;
        private readonly ToolTip _cellToolTip = new ToolTip();
        private ListViewItem.ListViewSubItem? _lastHoveredSubItem = null;

        public RulesControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(rulesListView, true);
        }

        public void Initialize(
            MainViewModel mainViewModel,
            FirewallActionsService actionsService,
            INetFwPolicy2 firewallPolicy,
            WildcardRuleService wildcardRuleService,
            BackgroundFirewallTaskService backgroundTaskService,
            IconService iconService,
            AppSettings appSettings,
            ImageList appIconList,
            DarkModeCS dm)
        {
            _mainViewModel = mainViewModel;
            _actionsService = actionsService;
            _firewallPolicy = firewallPolicy;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _iconService = iconService;
            _appSettings = appSettings;
            _dm = dm;

            rulesListView.SmallImageList = appIconList;
            _mainViewModel.RulesListUpdated += OnRulesListUpdated;
            SetDefaultColumnWidths();
            this.rulesListView.Resize += new System.EventHandler(this.RulesListView_Resize);
        }

        private void SetDefaultColumnWidths()
        {
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

        private void OnRulesListUpdated()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(OnRulesListUpdated);
                return;
            }
            rulesListView.VirtualListSize = _mainViewModel.VirtualRulesData.Count;
            rulesListView.Invalidate();
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;
            createRuleButton.FlatAppearance.BorderSize = 1;
            createRuleButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            advancedRuleButton.FlatAppearance.BorderSize = 1;
            advancedRuleButton.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
            if (_dm.IsDarkMode)
            {
                createRuleButton.ForeColor = Color.White;
                advancedRuleButton.ForeColor = Color.White;
            }
            else
            {
                createRuleButton.ForeColor = SystemColors.ControlText;
                advancedRuleButton.ForeColor = SystemColors.ControlText;
            }
        }

        public async Task RefreshDataAsync(bool forceUwpScan = false, IProgress<int>? progress = null, CancellationToken token = default)
        {
            if (forceUwpScan)
            {
                _uwpAppsScanned = true;
            }
            await _mainViewModel.RefreshRulesDataAsync(token, progress);
            await DisplayRulesAsync();
        }

        public async Task OnTabSelectedAsync()
        {
            await DisplayRulesAsync();
        }

        public void UpdateIconColumnVisibility()
        {
            if (advIconColumn != null)
            {
                advIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
            }
        }

        private async Task DisplayRulesAsync()
        {
            ApplyRulesFilters();
            await Task.CompletedTask;
        }

        private void ApplyRulesFilters()
        {
            var enabledTypes = new HashSet<RuleType>();
            if (advFilterProgramCheck.Checked) enabledTypes.Add(RuleType.Program);
            if (advFilterServiceCheck.Checked) enabledTypes.Add(RuleType.Service);
            if (advFilterUwpCheck.Checked) enabledTypes.Add(RuleType.UWP);
            if (advFilterWildcardCheck.Checked) enabledTypes.Add(RuleType.Wildcard);
            if (advFilterAdvancedCheck.Checked) enabledTypes.Add(RuleType.Advanced);

            _mainViewModel.ApplyRulesFilters(rulesSearchTextBox.Text, enabledTypes, _rulesSortColumn, _rulesSortOrder);
        }

        private void ListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < _mainViewModel.VirtualRulesData.Count)
            {
                var rule = _mainViewModel.VirtualRulesData[e.ItemIndex];
                int iconIndex = _appSettings.ShowAppIcons && !string.IsNullOrEmpty(rule.ApplicationName) ?
                    _iconService.GetIconIndex(rule.ApplicationName) : -1;
                var item = new ListViewItem("", iconIndex) { Tag = rule };
                item.SubItems.AddRange(new[]
                {
                    rule.Name,
                    rule.Status,
                    rule.ProtocolName,
                    !string.IsNullOrEmpty(rule.LocalPorts) && rule.LocalPorts != "*" ?
                        rule.LocalPorts : "Any",
                    !string.IsNullOrEmpty(rule.RemotePorts) && rule.RemotePorts != "*" ?
                        rule.RemotePorts : "Any",
                    !string.IsNullOrEmpty(rule.LocalAddresses) && rule.LocalAddresses != "*" ?
                        rule.LocalAddresses : "Any",
                    !string.IsNullOrEmpty(rule.RemoteAddresses) && rule.RemoteAddresses != "*" ?
                        rule.RemoteAddresses : "Any",
                    rule.ApplicationName,
                    rule.ServiceName,
                    rule.Profiles,
                    rule.Grouping,
                    rule.Description
                });
                e.Item = item;
            }
        }

        private void RulesListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void RulesListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = e.Item.ListView.Focused ? SystemColors.Highlight : SystemColors.ControlDark;
                foreColor = e.Item.ListView.Focused ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            else
            {
                string statusText = string.Empty;
                if (e.Item.Tag is AdvancedRuleViewModel advancedRule)
                {
                    statusText = advancedRule.Status;
                }
                else if (e.Item.Tag is AggregatedRuleViewModel aggregatedRule)
                {
                    statusText = aggregatedRule.Status;
                }

                bool isAllow = statusText.Contains("Allow", StringComparison.OrdinalIgnoreCase);
                bool isBlock = statusText.Contains("Block", StringComparison.OrdinalIgnoreCase);

                if (isAllow && isBlock)
                {
                    backColor = Color.FromArgb(255, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isAllow)
                {
                    backColor = Color.FromArgb(204, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isBlock)
                {
                    backColor = Color.FromArgb(255, 204, 204);
                    foreColor = Color.Black;
                }
                else
                {
                    backColor = e.Item.ListView.BackColor;
                    foreColor = e.Item.ListView.ForeColor;
                }
            }

            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                if (e.Item.ImageIndex != -1 && e.Item.ImageList != null)
                {
                    Image img = e.Item.ImageList.Images[e.Item.ImageIndex];
                    int imgX = e.Bounds.Left + (e.Bounds.Width - img.Width) / 2;
                    int imgY = e.Bounds.Top + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, imgX, imgY);
                }
                return;
            }

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PathEllipsis;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, flags);
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ListView listView) return;
            ListViewItem? itemUnderMouse = listView.GetItemAt(e.X, e.Y);

            if (_hoveredItem != itemUnderMouse)
            {
                if (_hoveredItem != null && _hoveredItem.ListView != null && _hoveredItem.Index >= 0)
                {
                    try { listView.Invalidate(_hoveredItem.Bounds); }
                    catch (ArgumentOutOfRangeException) { }
                }
                _hoveredItem = itemUnderMouse;
                if (_hoveredItem != null)
                {
                    try { listView.Invalidate(_hoveredItem.Bounds); }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            var hitTest = listView.HitTest(e.Location);
            var subItem = hitTest.SubItem;

            if (subItem != _lastHoveredSubItem)
            {
                _lastHoveredSubItem = subItem;
                if (subItem != null)
                {
                    int textWidth = TextRenderer.MeasureText(subItem.Text, listView.Font).Width;
                    if (textWidth > subItem.Bounds.Width)
                    {
                        _cellToolTip.Show(subItem.Text, listView, e.X + 10, e.Y + 10, 3000);
                    }
                    else
                    {
                        _cellToolTip.Hide(listView);
                    }
                }
                else
                {
                    _cellToolTip.Hide(listView);
                }
            }
        }

        private void ListView_MouseLeave(object sender, EventArgs e)
        {
            _cellToolTip.Hide(rulesListView);
            _lastHoveredSubItem = null;
            if (_hoveredItem != null)
            {
                if (sender is ListView listView)
                {
                    try { listView.Invalidate(_hoveredItem.Bounds); }
                    catch (ArgumentOutOfRangeException) { }
                }
                _hoveredItem = null;
            }
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _rulesSortColumn)
            {
                _rulesSortOrder = (_rulesSortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _rulesSortOrder = SortOrder.Ascending;
            }

            _rulesSortColumn = e.Column;
            ApplyRulesFilters();
        }

        private async void AdvFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (advFilterUwpCheck.Checked && !_uwpAppsScanned)
            {
                if (this.FindForm() is MainForm mainForm)
                {
                    await mainForm.ForceDataRefreshAsync(forceUwpScan: true);
                }
                _uwpAppsScanned = true;
            }
            await DisplayRulesAsync();
        }

        private void RulesListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (rulesListView.FocusedItem != null && rulesListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    rulesContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action || rulesListView.SelectedIndices.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>(rulesListView.SelectedIndices.Count);
            foreach (int index in rulesListView.SelectedIndices)
            {
                if (index >= 0 && index < _mainViewModel.VirtualRulesData.Count)
                {
                    items.Add(_mainViewModel.VirtualRulesData[index]);
                }
            }

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var firstRule = item.UnderlyingRules.FirstOrDefault();
                    if (firstRule == null) continue;

                    switch (firstRule.Type)
                    {
                        case RuleType.Program:
                            var appPayload = new ApplyApplicationRulePayload { AppPaths = [firstRule.ApplicationName], Action = action };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                            break;
                        case RuleType.Service:
                            var servicePayload = new ApplyServiceRulePayload { ServiceName = firstRule.ServiceName, Action = action };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload));
                            break;
                        case RuleType.UWP:
                            if (firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                            {
                                var pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                                var uwpApp = new UwpApp { Name = item.Name, PackageFamilyName = pfn };
                                var uwpPayload = new ApplyUwpRulePayload { UwpApps = [uwpApp], Action = action };
                                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyUwpRule, uwpPayload));
                            }
                            break;
                    }
                }
            }
        }

        private void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesListView.SelectedIndices.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>(rulesListView.SelectedIndices.Count);
            foreach (int index in rulesListView.SelectedIndices)
            {
                if (index >= 0 && index < _mainViewModel.VirtualRulesData.Count)
                {
                    items.Add(_mainViewModel.VirtualRulesData[index]);
                }
            }

            if (items.Count > 0)
            {
                var result = DarkModeForms.Messenger.MessageBox($"Are you sure you want to delete the {items.Count} selected rule(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                var wildcardRulesToDelete = items
                    .Where(i => i.Type == RuleType.Wildcard && i.WildcardDefinition != null)
                    .Select(i => i.WildcardDefinition!)
                    .ToList();
                var standardRuleNamesToDelete = items
                    .Where(i => i.Type != RuleType.Wildcard)
                    .SelectMany(i => i.UnderlyingRules.Select(r => r.Name))
                    .ToList();
                foreach (var wildcardRule in wildcardRulesToDelete)
                {
                    var payload = new DeleteWildcardRulePayload { Wildcard = wildcardRule };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteWildcardRules, payload));
                    _wildcardRuleService.RemoveRule(wildcardRule);
                }

                if (standardRuleNamesToDelete.Count > 0)
                {
                    var payload = new DeleteRulesPayload { RuleIdentifiers = standardRuleNamesToDelete };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, payload));
                }

                var nameSet = new HashSet<string>(standardRuleNamesToDelete);
                var wildcardSet = new HashSet<WildcardRule>(wildcardRulesToDelete);
                _mainViewModel.AllAggregatedRules.RemoveAll(r => nameSet.Contains(r.Name) || (r.WildcardDefinition != null && wildcardSet.Contains(r.WildcardDefinition)));
                ApplyRulesFilters();
            }
        }

        private void CreateRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new AddRuleSelectionForm(_actionsService, _wildcardRuleService, _backgroundTaskService);
            if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
            }
        }

        private void AdvancedRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService);
            if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
            }
        }

        private async void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            await DisplayRulesAsync();
        }

        private void RulesListView_Resize(object? sender, EventArgs e)
        {
            if (rulesListView.Columns.Count < 2) return;
            int totalColumnWidths = 0;
            for (int i = 0; i < rulesListView.Columns.Count - 1; i++)
            {
                totalColumnWidths += rulesListView.Columns[i].Width;
            }

            int lastColumnIndex = rulesListView.Columns.Count - 1;
            int lastColumnWidth = rulesListView.ClientSize.Width - totalColumnWidths;

            int minWidth = TextRenderer.MeasureText(rulesListView.Columns[lastColumnIndex].Text, rulesListView.Font).Width + 10;
            if (lastColumnWidth > minWidth)
            {
                rulesListView.Columns[lastColumnIndex].Width = lastColumnWidth;
            }
            else
            {
                rulesListView.Columns[lastColumnIndex].Width = minWidth;
            }
        }

        private void rulesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (rulesListView.SelectedIndices.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            int index = rulesListView.SelectedIndices[0];
            if (index < 0 || index >= _mainViewModel.VirtualRulesData.Count)
            {
                e.Cancel = true;
                return;
            }

            var rule = _mainViewModel.VirtualRulesData[index];
            string? appPath = rule.ApplicationName;

            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && File.Exists(appPath);
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesListView.SelectedIndices.Count > 0)
            {
                int index = rulesListView.SelectedIndices[0];
                if (index >= 0 && index < _mainViewModel.VirtualRulesData.Count)
                {
                    var rule = _mainViewModel.VirtualRulesData[index];
                    string? appPath = rule.ApplicationName;

                    if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
                    {
                        try
                        {
                            Process.Start("explorer.exe", $"/select, \"{appPath}\"");
                        }
                        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
                        {
                            DarkModeForms.Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        DarkModeForms.Messenger.MessageBox("The path for this item is not available or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesListView.SelectedIndices.Count > 0)
            {
                int index = rulesListView.SelectedIndices[0];
                if (index >= 0 && index < _mainViewModel.VirtualRulesData.Count)
                {
                    var rule = _mainViewModel.VirtualRulesData[index];
                    var details = new System.Text.StringBuilder();

                    details.AppendLine($"Rule Name: {rule.Name}");
                    details.AppendLine($"Type: {rule.Type}");
                    details.AppendLine($"Action: {rule.Status}");
                    details.AppendLine($"Direction: {rule.Direction}");
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

                    if (details.Length > 0)
                    {
                        Clipboard.SetText(details.ToString());
                    }
                }
            }
        }
    }
}