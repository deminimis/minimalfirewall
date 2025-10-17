// File: LiveConnectionsControl.cs
using MinimalFirewall.TypedObjects;
using System.Collections.Specialized;
using System.Windows.Forms;
using Firewall.Traffic.ViewModels;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public partial class LiveConnectionsControl : UserControl
    {
        private TrafficMonitorViewModel _trafficMonitorViewModel;
        private AppSettings _appSettings;
        private IconService _iconService;
        private BackgroundFirewallTaskService _backgroundTaskService;
        private BindingSource _bindingSource;
        private FirewallActionsService _actionsService;
        private INetFwPolicy2 _firewallPolicy;

        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;

        public LiveConnectionsControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(
            TrafficMonitorViewModel trafficMonitorViewModel,
            AppSettings appSettings,
            IconService iconService,
            BackgroundFirewallTaskService backgroundTaskService,
            FirewallActionsService actionsService,
            INetFwPolicy2 firewallPolicy)
        {
            _trafficMonitorViewModel =
                       trafficMonitorViewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _backgroundTaskService = backgroundTaskService;
            _actionsService = actionsService;
            _firewallPolicy = firewallPolicy;

            liveConnectionsDataGridView.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            liveConnectionsDataGridView.DataSource = _bindingSource;

            _sortColumn = _appSettings.LiveConnectionsSortColumn;
            _sortOrder = (SortOrder)_appSettings.LiveConnectionsSortOrder;
        }

        public void OnTabDeselected()
        {
            _trafficMonitorViewModel.StopMonitoring();
        }

        public void UpdateIconColumnVisibility()
        {
            liveIconColumn.Visible = _appSettings.ShowAppIcons;
        }

        public void UpdateLiveConnectionsView()
        {
            var connections = _trafficMonitorViewModel.ActiveConnections.ToList();
            var sortableList = new SortableBindingList<TcpConnectionViewModel>(connections);

            if (_sortOrder != SortOrder.None && _sortColumn != -1)
            {
                string propertyName = liveConnectionsDataGridView.Columns[_sortColumn].DataPropertyName;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    sortableList.Sort(propertyName, _sortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
                }
            }

            _bindingSource.DataSource = sortableList;
            _bindingSource.ResetBindings(false);
            liveConnectionsDataGridView.Refresh();
        }

        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0)
            {
                if (liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel vm)
                {
                    vm.KillProcessCommand.Execute(null);
                }
            }
        }

        private void blockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0)
            {
                if (liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel vm)
                {
                    var rule = new AdvancedRuleViewModel
                    {
                        Name = $"Block {vm.RemoteAddress}",
                        Description = "Blocked from Live Connections",
                        IsEnabled = true,
                        Grouping = MFWConstants.MainRuleGroup,
                        Status = "Block",
                        Direction = Directions.Outgoing,
                        Protocol = 6,
                        LocalPorts = "*",
                        RemotePorts = "*",
                        LocalAddresses = "*",
                        RemoteAddresses = vm.RemoteAddress,
                        Profiles = "All",
                        Type = RuleType.Advanced
                    };
                    var payload = new CreateAdvancedRulePayload { ViewModel = rule, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                }
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0)
            {
                var details = new System.Text.StringBuilder();

                foreach (DataGridViewRow row in liveConnectionsDataGridView.SelectedRows)
                {
                    if (row.DataBoundItem is TcpConnectionViewModel vm)
                    {
                        if (details.Length > 0)
                        {
                            details.AppendLine();
                            details.AppendLine();
                        }

                        details.AppendLine($"Process Name: {vm.ProcessName}");
                        details.AppendLine($"Process Path: {vm.ProcessPath}");
                        details.AppendLine($"Local Endpoint: {vm.LocalAddress}:{vm.LocalPort}");
                        details.AppendLine($"Remote Endpoint: {vm.RemoteAddress}:{vm.RemotePort}");
                        details.AppendLine($"State: {vm.State}");
                    }
                }

                if (details.Length > 0)
                {
                    Clipboard.SetText(details.ToString());
                }
            }
        }

        private void liveConnectionsDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == _sortColumn)
            {
                _sortOrder = (_sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }

            _sortColumn = e.ColumnIndex;
            _appSettings.LiveConnectionsSortColumn = _sortColumn;
            _appSettings.LiveConnectionsSortOrder = (int)_sortOrder;

            UpdateLiveConnectionsView();
        }

        private void liveConnectionsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;

            if (grid.Columns[e.ColumnIndex].Name == "liveIconColumn")
            {
                if (grid.Rows[e.RowIndex].DataBoundItem is TcpConnectionViewModel conn && _appSettings.ShowAppIcons && !string.IsNullOrEmpty(conn.ProcessPath))
                {
                    int iconIndex = _iconService.GetIconIndex(conn.ProcessPath);
                    if (iconIndex != -1 && _iconService.ImageList != null)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
            }
        }

        private void liveConnectionsDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];

            if (row.Selected) return;

            var mouseOverRow = grid.HitTest(grid.PointToClient(MousePosition).X, grid.PointToClient(MousePosition).Y).RowIndex;
            if (e.RowIndex == mouseOverRow)
            {
                using var overlayBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(25, System.Drawing.Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void liveConnectionsDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void liveConnectionsDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void liveConnectionsDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
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

        private void liveConnectionsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count == 0 || liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is not TcpConnectionViewModel vm)
            {
                e.Cancel = true;
                return;
            }

            bool pathExists = !string.IsNullOrEmpty(vm.ProcessPath) && !vm.ProcessPath.Equals("N/A (Access Denied)") && File.Exists(vm.ProcessPath);

            killProcessToolStripMenuItem.Enabled = vm.KillProcessCommand.CanExecute(null);
            createRuleToolStripMenuItem.Enabled = pathExists;
            openFileLocationToolStripMenuItem.Enabled = pathExists;
            filePropertiesToolStripMenuItem.Enabled = pathExists;
        }

        private void createRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0 && liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel vm)
            {
                if (!string.IsNullOrEmpty(vm.ProcessPath) && File.Exists(vm.ProcessPath))
                {
                    using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService, vm.ProcessPath, "Outbound", _appSettings);
                    dialog.ShowDialog(this.FindForm());
                }
                else
                {
                    MessageBox.Show("Cannot create a rule because the process path is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0 && liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel vm)
            {
                try
                {
                    Process.Start("explorer.exe", $"/select, \"{vm.ProcessPath}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void filePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsDataGridView.SelectedRows.Count > 0 && liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel vm)
            {
                ShowFileProperties(vm.ProcessPath);
            }
        }

        #region P/Invoke for File Properties
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const uint SEE_MASK_INVOKEIDLIST = 12;

        private static void ShowFileProperties(string filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = filename;
            info.nShow = 5;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }
        #endregion
    }
}