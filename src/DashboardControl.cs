// File: DashboardControl.cs
using DarkModeForms;
using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public partial class DashboardControl : UserControl
    {
        private MainViewModel _viewModel;
        private AppSettings _appSettings;
        private IconService _iconService;
        private WildcardRuleService _wildcardRuleService;
        private FirewallActionsService _actionsService;
        private NetFwTypeLib.INetFwPolicy2 _firewallPolicy;

        public DashboardControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(MainViewModel viewModel, AppSettings appSettings, IconService iconService, DarkModeCS dm, WildcardRuleService wildcardRuleService, FirewallActionsService actionsService, NetFwTypeLib.INetFwPolicy2 firewallPolicy)
        {
            _viewModel = viewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _wildcardRuleService = wildcardRuleService;
            _actionsService = actionsService;
            _firewallPolicy = firewallPolicy;

            dashboardListView.DarkMode = dm;
            dashboardListView.AllowClicked += Dashboard_AllowClicked;
            dashboardListView.BlockClicked += Dashboard_BlockClicked;
            dashboardListView.IgnoreClicked += Dashboard_IgnoreClicked;

            _viewModel.PendingConnections.CollectionChanged += PendingConnections_CollectionChanged;

            LoadDashboardItems();
        }

        public void SetIconColumnVisibility(bool visible)
        {
            if (dashIconColumn != null)
            {
                dashIconColumn.Width = visible ? 32 : 0;
            }
        }

        private void PendingConnections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(LoadDashboardItems));
            }
            else
            {
                LoadDashboardItems();
            }
        }

        private void LoadDashboardItems()
        {
            if (dashboardListView == null) return;

            dashboardListView.BeginUpdate();
            dashboardListView.Items.Clear();

            foreach (var pending in _viewModel.PendingConnections)
            {
                int iconIndex = -1;
                if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(pending.AppPath))
                {
                    iconIndex = _iconService.GetIconIndex(pending.AppPath);
                }
                var item = new ListViewItem("", iconIndex) { Tag = pending };
                item.SubItems.AddRange(new[] { "", pending.FileName, pending.ServiceName, pending.Direction, pending.AppPath });
                dashboardListView.Items.Add(item);
            }
            dashboardListView.EndUpdate();
        }

        private void Dashboard_AllowClicked(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Allow");
            }
        }

        private void Dashboard_BlockClicked(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Block");
            }
        }

        private void Dashboard_IgnoreClicked(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Ignore");
            }
        }

        private void TempAllowMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending &&
                sender is ToolStripMenuItem menuItem &&
                int.TryParse(menuItem.Tag?.ToString(), out int minutes))
            {
                var payload = new ProcessPendingConnectionPayload
                {
                    PendingConnection = pending,
                    Decision = "TemporaryAllow",
                    Duration = TimeSpan.FromMinutes(minutes)
                };

                _viewModel.PendingConnections.Remove(pending);
                _actionsService.ProcessPendingConnection(payload.PendingConnection, payload.Decision, payload.Duration, payload.TrustPublisher);
            }
        }

        private void PermanentAllowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Allow");
            }
        }

        private void AllowAndTrustPublisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                var payload = new ProcessPendingConnectionPayload { PendingConnection = pending, Decision = "Allow", TrustPublisher = true };
                _viewModel.PendingConnections.Remove(pending);
                _actionsService.ProcessPendingConnection(payload.PendingConnection, payload.Decision, payload.Duration, payload.TrustPublisher);
            }
        }

        private void PermanentBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Block");
            }
        }

        private void IgnoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Ignore");
            }
        }

        private void createWildcardRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath);
                if (wildcardDialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    var newRule = new WildcardRule
                    {
                        FolderPath = wildcardDialog.FolderPath,
                        ExeName = wildcardDialog.ExeName,
                        Action = wildcardDialog.FinalAction
                    };
                    _wildcardRuleService.AddRule(newRule);
                    _viewModel.PendingConnections.Remove(pending);
                }
            }
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dashboardListView.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            if (dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                bool isSigned = SignatureValidationService.GetPublisherInfo(pending.AppPath, out _);
                allowAndTrustPublisherToolStripMenuItem.Visible = isSigned;
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService, pending.AppPath!, pending.Direction!);
                dialog.ShowDialog(this.FindForm());
            }
        }

        private void openFileLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending &&
                !string.IsNullOrEmpty(pending.AppPath) &&
                System.IO.File.Exists(pending.AppPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{pending.AppPath}\"");
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardListView.SelectedItems.Count > 0 &&
                dashboardListView.SelectedItems[0].Tag is PendingConnectionViewModel pending)
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Pending Connection");
                details.AppendLine($"Application: {pending.FileName}");
                details.AppendLine($"Path: {pending.AppPath}");
                details.AppendLine($"Service: {pending.ServiceName}");
                details.AppendLine($"Direction: {pending.Direction}");
                Clipboard.SetText(details.ToString());
            }
        }
    }
}