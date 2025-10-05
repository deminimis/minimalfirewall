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
        private INetFwPolicy2 _firewallPolicy = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private IconService _iconService = null!;
        private AppSettings _appSettings = null!;
        private DarkModeCS _dm = null!;

        private int _rulesSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private BindingSource _bindingSource;
        public event Func<Task> DataRefreshRequested;
        public RulesControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
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

            programFilterCheckBox.Checked = _appSettings.FilterPrograms;
            serviceFilterCheckBox.Checked = _appSettings.FilterServices;
            uwpFilterCheckBox.Checked = _appSettings.FilterUwp;
            wildcardFilterCheckBox.Checked = _appSettings.FilterWildcards;
            systemFilterCheckBox.Checked = _appSettings.FilterSystem;
            rulesSearchTextBox.Text = _appSettings.RulesSearchText;
            _rulesSortColumn = _appSettings.RulesSortColumn;
            _rulesSortOrder = (SortOrder)_appSettings.RulesSortOrder;

            rulesDataGridView.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            rulesDataGridView.DataSource = _bindingSource;
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
            _bindingSource.DataSource = _mainViewModel.VirtualRulesData;
            _bindingSource.ResetBindings(false);
            rulesDataGridView.Refresh();
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;
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
            if (advIconColumn != null)
            {
                advIconColumn.Visible = _appSettings.ShowAppIcons;
            }
        }

        private async Task DisplayRulesAsync()
        {
            ApplyRulesFilters();
            await Task.CompletedTask;
        }

        private void filterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
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
            _mainViewModel.ApplyRulesFilters(rulesSearchTextBox.Text, enabledTypes, _rulesSortColumn, _rulesSortOrder, showSystemRules);
        }

        private void rulesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
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

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action || rulesDataGridView.SelectedRows.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>();
            foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
            {
                if (row.DataBoundItem is AggregatedRuleViewModel vm)
                {
                    items.Add(vm);
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
            if (rulesDataGridView.SelectedRows.Count == 1 && rulesDataGridView.SelectedRows[0].DataBoundItem is AggregatedRuleViewModel aggRule)
            {
                var originalRule = aggRule.UnderlyingRules.FirstOrDefault();
                if (originalRule == null)
                {
                    DarkModeForms.Messenger.MessageBox("Cannot edit this rule as it has no underlying rule definition.", "Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService, originalRule);
                if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    if (dialog.RuleVm != null)
                    {
                        if (originalRule.HasSameSettings(dialog.RuleVm))
                        {
                            return;
                        }

                        var deletePayload = new DeleteRulesPayload { RuleIdentifiers = aggRule.UnderlyingRules.Select(r => r.Name).ToList() };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, deletePayload));

                        var createPayload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, createPayload));

                        await Task.Delay(500);
                        DataRefreshRequested?.Invoke();
                    }
                }
            }
        }

        private void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>();
            foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
            {
                if (row.DataBoundItem is AggregatedRuleViewModel vm)
                {
                    items.Add(vm);
                }
            }

            if (items.Count > 0)
            {
                var result = DarkModeForms.Messenger.MessageBox($"Are you sure you want to delete the {items.Count} selected rule(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                _mainViewModel.DeleteRules(items);
            }
        }

        private void CreateRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new RuleWizardForm(_actionsService, _wildcardRuleService, _backgroundTaskService, _firewallPolicy);
            if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
            }
        }

        private async void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _appSettings.RulesSearchText = rulesSearchTextBox.Text;
            await DisplayRulesAsync();
        }

        private void rulesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            if (rulesDataGridView.SelectedRows[0].DataBoundItem is not AggregatedRuleViewModel rule)
            {
                e.Cancel = true;
                return;
            }

            string? appPath = rule.ApplicationName;
            openFileLocationToolStripMenuItem.Enabled = !string.IsNullOrEmpty(appPath) && File.Exists(appPath);

            var firstUnderlyingRule = rule.UnderlyingRules.FirstOrDefault();
            bool isEditableType = rule.Type == RuleType.Program || rule.Type == RuleType.Service ||
                                  rule.Type == RuleType.Advanced;
            bool hasTarget = firstUnderlyingRule != null &&
                             ((!string.IsNullOrEmpty(firstUnderlyingRule.ApplicationName) && firstUnderlyingRule.ApplicationName != "*") ||
                              !string.IsNullOrEmpty(firstUnderlyingRule.ServiceName));
            editRuleToolStripMenuItem.Enabled = rulesDataGridView.SelectedRows.Count == 1 && isEditableType && hasTarget;
        }


        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count > 0 && rulesDataGridView.SelectedRows[0].DataBoundItem is AggregatedRuleViewModel rule)
            {
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

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesDataGridView.SelectedRows.Count > 0)
            {
                var details = new System.Text.StringBuilder();

                foreach (DataGridViewRow row in rulesDataGridView.SelectedRows)
                {
                    if (row.DataBoundItem is AggregatedRuleViewModel rule)
                    {
                        if (details.Length > 0)
                        {
                            details.AppendLine();
                            details.AppendLine();
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
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;

            if (grid.Rows[e.RowIndex].DataBoundItem is not AggregatedRuleViewModel rule) return;
            if (grid.Columns[e.ColumnIndex].Name == "advIconColumn")
            {
                if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(rule.ApplicationName))
                {
                    int iconIndex = _iconService.GetIconIndex(rule.ApplicationName);
                    if (iconIndex != -1 && _iconService.ImageList != null)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
                return;
            }

            bool hasAllow = rule.InboundStatus.Contains("Allow") || rule.OutboundStatus.Contains("Allow");
            bool hasBlock = rule.InboundStatus.Contains("Block") || rule.OutboundStatus.Contains("Block");

            if (hasAllow && hasBlock)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 255, 204);
            }
            else if (hasAllow)
            {
                e.CellStyle.BackColor = Color.FromArgb(204, 255, 204);
            }
            else if (hasBlock)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 204, 204);
            }

            if (hasAllow || hasBlock)
            {
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

        private void rulesDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == _rulesSortColumn)
            {
                _rulesSortOrder = (_rulesSortOrder == SortOrder.Ascending) ?
                                  SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _rulesSortOrder = SortOrder.Ascending;
            }

            _rulesSortColumn = e.ColumnIndex;
            _appSettings.RulesSortColumn = _rulesSortColumn;
            _appSettings.RulesSortOrder = (int)_rulesSortOrder;

            ApplyRulesFilters();
        }

        private void rulesDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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

        private void rulesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void rulesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }
    }
}