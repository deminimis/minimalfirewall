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

namespace MinimalFirewall
{
    public partial class LiveConnectionsControl : UserControl
    {
        private TrafficMonitorViewModel _trafficMonitorViewModel;
        private AppSettings _appSettings;
        private IconService _iconService;
        private BackgroundFirewallTaskService _backgroundTaskService;
        private List<TcpConnectionViewModel> _virtualLiveConnectionsData = [];
        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;
        private ListViewItem? _hoveredItem = null;

        public LiveConnectionsControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(liveConnectionsListView, true);
        }

        public void Initialize(
            TrafficMonitorViewModel trafficMonitorViewModel,
            AppSettings appSettings,
            IconService iconService,
            BackgroundFirewallTaskService backgroundTaskService,
            ImageList appIconList)
        {
            _trafficMonitorViewModel = trafficMonitorViewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _backgroundTaskService = backgroundTaskService;
            liveConnectionsListView.SmallImageList = appIconList;
            _trafficMonitorViewModel.ActiveConnections.CollectionChanged += ActiveConnections_CollectionChanged;
            SetDefaultColumnWidths();
            this.liveConnectionsListView.Resize += new System.EventHandler(this.LiveConnectionsListView_Resize);
        }

        private void SetDefaultColumnWidths()
        {
            processNameColumn.Width = 200;
            localAddressColumn.Width = 150;
            localPortColumn.Width = 80;
            remoteAddressColumn.Width = 150;
            remotePortColumn.Width = 80;
            stateColumn.Width = 100;
        }

        public void OnTabSelected()
        {
            liveConnectionsListView.Visible = _appSettings.IsTrafficMonitorEnabled;
            liveConnectionsDisabledLabel.Visible = !_appSettings.IsTrafficMonitorEnabled;
            if (_appSettings.IsTrafficMonitorEnabled)
            {
                _trafficMonitorViewModel.StartMonitoring();
                UpdateLiveConnectionsView();
            }
            else
            {
                liveConnectionsListView.VirtualListSize = 0;
            }
        }

        public void OnTabDeselected()
        {
            _trafficMonitorViewModel.StopMonitoring();
        }

        public void UpdateIconColumnVisibility()
        {
            liveIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
        }

        private void ActiveConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Parent is TabPage parentTabPage && parentTabPage.Parent is TabControl parentTabControl && parentTabControl.SelectedTab == parentTabPage)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(UpdateLiveConnectionsView));
                }
                else
                {
                    UpdateLiveConnectionsView();
                }
            }
        }

        public void UpdateLiveConnectionsView()
        {
            IEnumerable<TcpConnectionViewModel> connections = _trafficMonitorViewModel.ActiveConnections;
            if (_sortOrder != SortOrder.None && _sortColumn != -1)
            {
                Func<TcpConnectionViewModel, object> keySelector = GetLiveConnectionKeySelector(_sortColumn);
                if (_sortOrder == SortOrder.Ascending)
                {
                    connections = connections.OrderBy(keySelector);
                }
                else
                {
                    connections = connections.OrderByDescending(keySelector);
                }
            }

            _virtualLiveConnectionsData = connections.ToList();
            liveConnectionsListView.VirtualListSize = _virtualLiveConnectionsData.Count;
            liveConnectionsListView.Invalidate();
        }

        private Func<TcpConnectionViewModel, object> GetLiveConnectionKeySelector(int columnIndex)
        {
            return columnIndex switch
            {
                2 => conn => conn.LocalAddress,
                3 => conn => conn.LocalPort,
                4 => conn => conn.RemoteAddress,
                5 => conn => conn.RemotePort,
                6 => conn => conn.State,
                _ => conn => conn.ProcessName,
            };
        }

        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedIndices.Count > 0)
            {
                int index = liveConnectionsListView.SelectedIndices[0];
                if (index >= 0 && index < _virtualLiveConnectionsData.Count)
                {
                    var vm = _virtualLiveConnectionsData[index];
                    vm.KillProcessCommand.Execute(null);
                }
            }
        }

        private void blockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedIndices.Count > 0)
            {
                int index = liveConnectionsListView.SelectedIndices[0];
                if (index >= 0 && index < _virtualLiveConnectionsData.Count)
                {
                    var vm = _virtualLiveConnectionsData[index];
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
            if (liveConnectionsListView.SelectedIndices.Count > 0)
            {
                int index = liveConnectionsListView.SelectedIndices[0];
                if (index >= 0 && index < _virtualLiveConnectionsData.Count)
                {
                    var vm = _virtualLiveConnectionsData[index];
                    var details = new System.Text.StringBuilder();

                    details.AppendLine($"Process Name: {vm.ProcessName}");
                    details.AppendLine($"Process Path: {vm.ProcessPath}");
                    details.AppendLine($"Local Endpoint: {vm.LocalAddress}:{vm.LocalPort}");
                    details.AppendLine($"Remote Endpoint: {vm.RemoteAddress}:{vm.RemotePort}");
                    details.AppendLine($"State: {vm.State}");

                    if (details.Length > 0)
                    {
                        Clipboard.SetText(details.ToString());
                    }
                }
            }
        }

        private void liveConnectionsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                _sortOrder = (_sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }

            _sortColumn = e.Column;
            UpdateLiveConnectionsView();
        }

        private void liveConnectionsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < _virtualLiveConnectionsData.Count)
            {
                var connection = _virtualLiveConnectionsData[e.ItemIndex];
                int iconIndex = _appSettings.ShowAppIcons && !string.IsNullOrEmpty(connection.ProcessPath) ?
                    _iconService.GetIconIndex(connection.ProcessPath) : -1;
                var item = new ListViewItem("", iconIndex) { Tag = connection };
                item.SubItems.AddRange(new[] {
                    connection.ProcessName,
                    connection.LocalAddress,
                    connection.LocalPort.ToString(),
                    connection.RemoteAddress,
                    connection.RemotePort.ToString(),
                    connection.State
                });
                e.Item = item;
            }
        }

        private void liveConnectionsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void liveConnectionsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
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
                backColor = e.Item.ListView.BackColor;
                foreColor = e.Item.ListView.ForeColor;
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

        private void liveConnectionsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (liveConnectionsListView.FocusedItem != null && liveConnectionsListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    liveConnectionsContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void liveConnectionsListView_MouseLeave(object sender, EventArgs e)
        {
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

        private void liveConnectionsListView_MouseMove(object sender, MouseEventArgs e)
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
        }

        private void LiveConnectionsListView_Resize(object? sender, EventArgs e)
        {
            if (liveConnectionsListView.Columns.Count < 2) return;
            int totalColumnWidths = 0;
            for (int i = 0; i < liveConnectionsListView.Columns.Count - 1; i++)
            {
                totalColumnWidths += liveConnectionsListView.Columns[i].Width;
            }

            int lastColumnIndex = liveConnectionsListView.Columns.Count - 1;
            int lastColumnWidth = liveConnectionsListView.ClientSize.Width - totalColumnWidths;

            int minWidth = TextRenderer.MeasureText(liveConnectionsListView.Columns[lastColumnIndex].Text, liveConnectionsListView.Font).Width + 10;
            if (lastColumnWidth > minWidth)
            {
                liveConnectionsListView.Columns[lastColumnIndex].Width = lastColumnWidth;
            }
            else
            {
                liveConnectionsListView.Columns[lastColumnIndex].Width = minWidth;
            }
        }
    }
}