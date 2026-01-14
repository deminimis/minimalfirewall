using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Firewall.Traffic.ViewModels;
using System.Linq;
using NetFwTypeLib;
using MinimalFirewall.TypedObjects;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;

namespace MinimalFirewall
{
    public partial class LiveConnectionsControl : UserControl
    {
        private TrafficMonitorViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private IconService _iconService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private FirewallActionsService _actionsService = null!;

        private SortableBindingList<TcpConnectionViewModel> _sortableList = new();

        // Cached GDI+ objects 
        private static readonly Brush HoverOverlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
        private static readonly Color EstablishedColor = Color.FromArgb(204, 255, 204);
        private static readonly Color ListenColor = Color.FromArgb(255, 255, 204);

        private int _hoveredRowIndex = -1;

        public LiveConnectionsControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(
            TrafficMonitorViewModel viewModel,
            AppSettings appSettings,
            IconService iconService,
            BackgroundFirewallTaskService backgroundTaskService,
            FirewallActionsService actionsService)
        {
            _viewModel = viewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _backgroundTaskService = backgroundTaskService;
            _actionsService = actionsService;

            typeof(DataGridView).InvokeMember(
               "DoubleBuffered",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
               null,
               liveConnectionsDataGridView,
               new object[] { true });

            liveConnectionsDataGridView.VirtualMode = true;
            liveConnectionsDataGridView.AutoGenerateColumns = false;
            liveConnectionsDataGridView.DataSource = null;
            liveConnectionsDataGridView.CellValueNeeded += LiveConnectionsDataGridView_CellValueNeeded;

            _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections.ToList());

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateEnabledState();
            UpdateLiveConnectionsView();
        }

        private void UnsubscribeEvents()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnsubscribeEvents();
            base.OnHandleDestroyed(e);
        }

        public void OnTabDeselected()
        {
            UnsubscribeEvents();
            if (_viewModel != null) _viewModel.StopMonitoring();
            UpdateEnabledState();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrafficMonitorViewModel.ActiveConnections))
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(UpdateLiveConnectionsView));
                }
                else
                {
                    UpdateLiveConnectionsView();
                }
            }
        }

        public void UpdateEnabledState()
        {
            if (_appSettings == null) return;
            bool isEnabled = _appSettings.IsTrafficMonitorEnabled;
            liveConnectionsDataGridView.Visible = isEnabled;
            disabledPanel.Visible = !isEnabled;
        }

        public void UpdateLiveConnectionsView()
        {
            if (_viewModel == null) return;

            string? selectedIdentifier = null;
            if (TryGetSelectedConnection(out var currentConn) && currentConn != null)
            {
                selectedIdentifier = currentConn.ProcessPath + currentConn.RemotePort;
            }

            var newList = _viewModel.ActiveConnections.ToList();

            // apply sorting
            if (_appSettings != null)
            {
                int sortCol = _appSettings.LiveConnectionsSortColumn;
                int sortOrd = _appSettings.LiveConnectionsSortOrder;

                if (sortCol > -1 && sortCol < liveConnectionsDataGridView.Columns.Count)
                {
                    var col = liveConnectionsDataGridView.Columns[sortCol];
                    if (!string.IsNullOrEmpty(col.DataPropertyName))
                    {
                        _sortableList = new SortableBindingList<TcpConnectionViewModel>(newList);
                        var dir = sortOrd == (int)SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        _sortableList.Sort(col.DataPropertyName, dir);
                        col.HeaderCell.SortGlyphDirection = (SortOrder)sortOrd;
                    }
                    else
                    {
                        _sortableList = new SortableBindingList<TcpConnectionViewModel>(newList);
                    }
                }
                else
                {
                    _sortableList = new SortableBindingList<TcpConnectionViewModel>(newList);
                }
            }
            else
            {
                _sortableList = new SortableBindingList<TcpConnectionViewModel>(newList);
            }

            liveConnectionsDataGridView.RowCount = _sortableList.Count;
            liveConnectionsDataGridView.Invalidate();

            if (selectedIdentifier != null)
            {
                var itemToSelect = _sortableList.FirstOrDefault(x => (x.ProcessPath + x.RemotePort) == selectedIdentifier);
                if (itemToSelect != null)
                {
                    int newIndex = _sortableList.IndexOf(itemToSelect);
                    if (newIndex >= 0 && newIndex < liveConnectionsDataGridView.RowCount)
                    {
                        liveConnectionsDataGridView.ClearSelection();
                        liveConnectionsDataGridView.Rows[newIndex].Selected = true;
                    }
                }
            }

            UpdateEnabledState();
        }

        private void LiveConnectionsDataGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= _sortableList.Count || e.RowIndex < 0) return;
            var conn = _sortableList[e.RowIndex];

            switch (liveConnectionsDataGridView.Columns[e.ColumnIndex].Name)
            {
                case "connIconColumn":
                    if (_appSettings != null && _appSettings.ShowAppIcons && !string.IsNullOrEmpty(conn.ProcessPath))
                    {
                        int iconIndex = _iconService.GetIconIndex(conn.ProcessPath);
                        if (iconIndex != -1 && _iconService.ImageList != null)
                        {
                            e.Value = _iconService.ImageList.Images[iconIndex];
                        }
                    }
                    break;
                case "connNameColumn": e.Value = conn.DisplayName; break;
                case "connStateColumn": e.Value = conn.State; break;
                case "connLocalAddrColumn": e.Value = conn.LocalAddress; break;
                case "connLocalPortColumn": e.Value = conn.LocalPort; break;
                case "connRemoteAddrColumn": e.Value = conn.RemoteAddress; break;
                case "connRemotePortColumn": e.Value = conn.RemotePort; break;
                case "connPathColumn": e.Value = conn.ProcessPath; break;
            }
        }

        public void UpdateIconColumnVisibility()
        {
            if (connIconColumn != null && _appSettings != null)
            {
                connIconColumn.Visible = _appSettings.ShowAppIcons;
                liveConnectionsDataGridView.InvalidateColumn(connIconColumn.Index);
            }
        }

        private void liveConnectionsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _sortableList.Count) return;
            var conn = _sortableList[e.RowIndex];

            // Color code rows based on connection state
            if (conn.State != null)
            {
                if (conn.State.Equals("Established", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = EstablishedColor;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (conn.State.Equals("Listen", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = ListenColor;
                    e.CellStyle.ForeColor = Color.Black;
                }
            }

            if (liveConnectionsDataGridView.Rows[e.RowIndex].Selected)
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

        // --- Mouse & Selection Handling ---

        private void liveConnectionsDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
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

        private void liveConnectionsDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = e.RowIndex;
                liveConnectionsDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void liveConnectionsDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = -1;
                liveConnectionsDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void liveConnectionsDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (!liveConnectionsDataGridView.Rows[e.RowIndex].Selected && e.RowIndex == _hoveredRowIndex)
            {
                e.Graphics.FillRectangle(HoverOverlayBrush, e.RowBounds);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            liveConnectionsDataGridView.ColumnHeaderMouseClick += (s, ev) =>
            {
                if (_appSettings != null && ev.ColumnIndex >= 0)
                {
                    int currentSort = _appSettings.LiveConnectionsSortOrder;
                    currentSort = currentSort == (int)SortOrder.Ascending ? (int)SortOrder.Descending : (int)SortOrder.Ascending;

                    _appSettings.LiveConnectionsSortColumn = ev.ColumnIndex;
                    _appSettings.LiveConnectionsSortOrder = currentSort;
                    _appSettings.Save();

                    UpdateLiveConnectionsView();
                }
            };
        }

        // --- Context Menu Actions ---

        private bool TryGetSelectedConnection(out TcpConnectionViewModel? connection)
        {
            connection = null;
            if (liveConnectionsDataGridView.SelectedRows.Count == 0) return false;

            int index = liveConnectionsDataGridView.SelectedRows[0].Index;
            if (index >= 0 && index < _sortableList.Count)
            {
                connection = _sortableList[index];
                return true;
            }
            return false;
        }

        private void liveConnectionsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (!TryGetSelectedConnection(out var connection) || connection == null)
            {
                e.Cancel = true;
                return;
            }

            killProcessToolStripMenuItem.Enabled = connection.KillProcessCommand.CanExecute(null);
            blockRemoteIPToolStripMenuItem.Enabled = connection.BlockRemoteIpCommand.CanExecute(null);
            bool pathExists = !string.IsNullOrEmpty(connection.ProcessPath) && File.Exists(connection.ProcessPath);
            openFileLocationToolStripMenuItem.Enabled = pathExists;
            createAdvancedRuleToolStripMenuItem.Enabled = pathExists;
        }

        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.KillProcessCommand.CanExecute(null))
            {
                connection.KillProcessCommand.Execute(null);
            }
        }

        private void blockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.BlockRemoteIpCommand.CanExecute(null))
            {
                connection.BlockRemoteIpCommand.Execute(null);
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && !string.IsNullOrEmpty(connection.ProcessPath))
            {
                using var dialog = new CreateAdvancedRuleForm(_actionsService, connection.ProcessPath, "", _appSettings);
                if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    if (dialog.RuleVm != null)
                    {
                        var payload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                    }
                }
            }
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && !string.IsNullOrEmpty(connection.ProcessPath))
            {
                if (!File.Exists(connection.ProcessPath) && !Directory.Exists(connection.ProcessPath))
                {
                    DarkModeForms.Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start("explorer.exe", $"/select, \"{connection.ProcessPath}\"");
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

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null)
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Live Connection");
                details.AppendLine($"Application: {connection.DisplayName}");
                details.AppendLine($"Path: {connection.ProcessPath}");
                details.AppendLine($"State: {connection.State}");
                details.AppendLine($"Local: {connection.LocalAddress}:{connection.LocalPort}");
                details.AppendLine($"Remote: {connection.RemoteAddress}:{connection.RemotePort}");
                Clipboard.SetText(details.ToString());
            }
        }
    }
}