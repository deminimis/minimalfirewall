using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using DarkModeForms;
using Firewall.Traffic;
using Firewall.Traffic.ViewModels;
using MinimalFirewall.TypedObjects;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public partial class LiveConnectionsControl : UserControl
    {
        private TrafficMonitorViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private IconService _iconService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private FirewallActionsService _actionsService = null!;

        private SortableBindingList<TcpConnectionViewModel> _sortableList = [];

        // Behavioral State Handlers
        private int _hoveredRowIndex = -1;

        // Filter bar
        private readonly TableLayoutPanel _filterPanel = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 2, Padding = new Padding(4, 4, 4, 0) };
        private readonly TextBox _searchBox = new() { PlaceholderText = "Search connections...", Width = 240, Margin = new Padding(3, 4, 8, 4) };
        private readonly TextBox _filterBox = new() { Dock = DockStyle.Fill, Margin = new Padding(3, 2, 3, 4), PlaceholderText = "Advanced filter, e.g.  include:remote:tcp:443  exclude:process:svchost  exclude:both:tcpudp:192.168.0.0/16" };
        private readonly CheckBox _showTcp = new() { Text = "TCP" };
        private readonly CheckBox _showUdp = new() { Text = "UDP" };
        private readonly CheckBox _showIPv6 = new() { Text = "IPv6" };
        private readonly CheckBox _showListening = new() { Text = "Listening" };
        private readonly CheckBox _showLoopback = new() { Text = "Loopback" };
        private readonly CheckBox _autoRefresh = new() { Text = "Auto-refresh (2 s)" };
        private readonly ThemedButton _exportButton = new() { Text = "Export CSV...", AutoSize = true };
        private readonly ThemedLabel _countLabel = new() { AutoSize = true, Margin = new Padding(8, 8, 3, 0) };
        private readonly ToolTip _toolTip = new();
        private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 2000 };
        private LiveConnectionFilter _filter = LiveConnectionFilter.Parse("");

        // New-connection marking: keys of the previous scan and the rows that appeared since.
        private object? _lastSnapshot;
        private HashSet<string>? _previousKeys;
        private HashSet<string> _newKeys = [];
        private Font? _newRowFont;

        // Raised when the view needs a silent rescan (auto-refresh tick or a closed connection).
        public event Action? RefreshRequested;

        public LiveConnectionsControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            BuildFilterBar();
        }

        private void BuildFilterBar()
        {
            var options = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
            foreach (var box in new[] { _showTcp, _showUdp, _showIPv6, _showListening, _showLoopback, _autoRefresh })
            {
                box.AutoSize = true;
                box.Margin = new Padding(3, 6, 6, 0);
                options.Controls.Add(box);
            }
            options.Controls.Add(_exportButton);
            options.Controls.Add(_countLabel);

            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _filterPanel.Controls.Add(_searchBox, 0, 0);
            _filterPanel.Controls.Add(options, 1, 0);
            _filterPanel.Controls.Add(_filterBox, 0, 1);
            _filterPanel.SetColumnSpan(_filterBox, 2);
            Controls.Add(_filterPanel);
            // Docked first so the grid fills the remaining space.
            _filterPanel.SendToBack();

            _toolTip.SetToolTip(_filterBox,
                "Filters separated by spaces or ';' (CurrPorts syntax):\n" +
                "  include|exclude:local|remote|both:tcp|udp|tcpudp:<port, port range, IP, IP range or CIDR>\n" +
                "  include|exclude:process:<name or PID>\n" +
                "With include filters, only matching connections are shown; exclude filters always hide.");
            _toolTip.SetToolTip(_showListening, "Show TCP ports in the Listen state.");
            _toolTip.SetToolTip(_showLoopback, "Show connections bound to 127.0.0.1 or ::1.");
            _toolTip.SetToolTip(_autoRefresh, "Rescan every 2 seconds while this tab is visible. New connections are shown in bold.");

            _refreshTimer.Tick += (_, _) => RefreshRequested?.Invoke();
            VisibleChanged += (_, _) => UpdateRefreshTimer();
            _exportButton.Click += ExportButton_Click;
            Disposed += (_, _) =>
            {
                _refreshTimer.Dispose();
                _toolTip.Dispose();
                _newRowFont?.Dispose();
            };
        }

        private void BindFilterSettings()
        {
            _searchBox.Text = _appSettings.LiveConnectionsSearchText;
            _filterBox.Text = _appSettings.LiveConnectionsFilter;
            _filter = LiveConnectionFilter.Parse(_filterBox.Text);
            _searchBox.TextChanged += (_, _) => { _appSettings.LiveConnectionsSearchText = _searchBox.Text; UpdateLiveConnectionsView(); };
            _filterBox.TextChanged += (_, _) =>
            {
                _appSettings.LiveConnectionsFilter = _filterBox.Text;
                _filter = LiveConnectionFilter.Parse(_filterBox.Text);
                UpdateLiveConnectionsView();
            };

            void Bind(CheckBox box, bool value, Action<bool> save)
            {
                box.Checked = value;
                box.CheckedChanged += (_, _) => { save(box.Checked); UpdateLiveConnectionsView(); };
            }
            Bind(_showTcp, _appSettings.LiveConnectionsShowTcp, v => _appSettings.LiveConnectionsShowTcp = v);
            Bind(_showUdp, _appSettings.LiveConnectionsShowUdp, v => _appSettings.LiveConnectionsShowUdp = v);
            Bind(_showIPv6, _appSettings.LiveConnectionsShowIPv6, v => _appSettings.LiveConnectionsShowIPv6 = v);
            Bind(_showListening, _appSettings.LiveConnectionsShowListening, v => _appSettings.LiveConnectionsShowListening = v);
            Bind(_showLoopback, _appSettings.LiveConnectionsShowLoopback, v => _appSettings.LiveConnectionsShowLoopback = v);

            _autoRefresh.Checked = _appSettings.LiveConnectionsAutoRefresh;
            _autoRefresh.CheckedChanged += (_, _) => { _appSettings.LiveConnectionsAutoRefresh = _autoRefresh.Checked; UpdateRefreshTimer(); };
        }

        private void UpdateRefreshTimer()
        {
            _refreshTimer.Enabled = _autoRefresh.Checked && Visible && _appSettings != null && _appSettings.IsTrafficMonitorEnabled;
        }

        private bool IsShown(TcpConnectionViewModel conn)
        {
            var row = conn.Connection;
            if (row.IsUdp ? !_showUdp.Checked : !_showTcp.Checked) return false;
            if (!_showIPv6.Checked && row.LocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6) return false;
            if (!_showListening.Checked && !row.IsUdp && row.State == 2) return false;
            if (!_showLoopback.Checked && IPAddress.IsLoopback(row.LocalEndPoint.Address)) return false;
            if (!_filter.Matches(row.IsUdp, row.LocalEndPoint, row.RemoteEndPoint, conn.ProcessName, row.ProcessId)) return false;

            string search = _searchBox.Text.Trim();
            if (search.Length == 0) return true;
            string[] values = [conn.DisplayName, conn.ProcessPath, conn.Protocol, conn.State, conn.LocalAddress, conn.RemoteAddress,
                conn.LocalPort.ToString(), conn.RemotePort.ToString(), conn.ProcessId.ToString()];
            return values.Any(v => v.Contains(search, StringComparison.OrdinalIgnoreCase));
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

            

            liveConnectionsDataGridView.VirtualMode = true;
            liveConnectionsDataGridView.AutoGenerateColumns = false;
            if (connIconColumn != null) connIconColumn.DefaultCellStyle.NullValue = new Bitmap(1, 1);
            liveConnectionsDataGridView.DataSource = null;
            liveConnectionsDataGridView.CellValueNeeded += LiveConnectionsDataGridView_CellValueNeeded;

            _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections.ToList());

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            BindFilterSettings();
            UpdateEnabledState();
            UpdateLiveConnectionsView();

            liveConnectionsDataGridView.AllowUserToOrderColumns = true;
            DataGridViewHelper.RestoreColumnSettings(liveConnectionsDataGridView, _appSettings.LiveConnectionsColumns);

            // Columns added after a layout was saved: show them next to related columns rather than last.
            if (!_appSettings.LiveConnectionsColumns.ContainsKey(connPidColumn.Name)) connPidColumn.DisplayIndex = connNameColumn.DisplayIndex + 1;
            if (!_appSettings.LiveConnectionsColumns.ContainsKey(connProtocolColumn.Name)) connProtocolColumn.DisplayIndex = connStateColumn.DisplayIndex;

            liveConnectionsDataGridView.ColumnDisplayIndexChanged += (s, e) => DataGridViewHelper.SaveColumnSettings(liveConnectionsDataGridView, _appSettings.LiveConnectionsColumns, _appSettings);
            liveConnectionsDataGridView.ColumnWidthChanged += (s, e) => DataGridViewHelper.SaveColumnSettings(liveConnectionsDataGridView, _appSettings.LiveConnectionsColumns, _appSettings);
        }

        

        private void UnsubscribeEvents()
        {
            _viewModel?.PropertyChanged -= ViewModel_PropertyChanged;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnsubscribeEvents();
            base.OnHandleDestroyed(e);
        }

        public void OnTabDeselected()
        {
            UnsubscribeEvents();
            _viewModel?.StopMonitoring();
            // Coming back to the tab must not mark every connection as new.
            _previousKeys = null;
            _newKeys = [];
            UpdateEnabledState();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrafficMonitorViewModel.ActiveConnections))
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

        public void UpdateEnabledState()
        {
            if (_appSettings == null)
            {
                return;
            }

            bool isEnabled = _appSettings.IsTrafficMonitorEnabled;
            liveConnectionsDataGridView.Visible = isEnabled;
            _filterPanel.Visible = isEnabled;
            disabledPanel.Visible = !isEnabled;
            UpdateRefreshTimer();
        }

        public void UpdateLiveConnectionsView()
        {
            if (_viewModel == null)
            {
                return;
            }

            string? selectedIdentifier = null;
            if (TryGetSelectedConnection(out var currentConn) && currentConn != null)
            {
                selectedIdentifier = currentConn.Key;
            }

            var active = _viewModel.ActiveConnections;
            if (!ReferenceEquals(_lastSnapshot, active) || active.Count == 0)
            {
                // Only a new scan updates the marking; filter changes reuse it.
                var keys = active.Select(c => c.Key).ToHashSet();
                _newKeys = _previousKeys == null ? [] : [.. keys.Except(_previousKeys)];
                _previousKeys = keys.Count == 0 ? null : keys;
                _lastSnapshot = active;
            }

            var newList = active.Where(IsShown).ToList();
            _countLabel.Text = $"{newList.Count} of {active.Count} shown" +
                (_filter.InvalidFilters.Count > 0 ? $"  |  ignored filter: {string.Join(" ", _filter.InvalidFilters)}" : "");

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
                var itemToSelect = _sortableList.FirstOrDefault(x => x.Key == selectedIdentifier);
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
            if (e.RowIndex >= _sortableList.Count || e.RowIndex < 0)
            {
                return;
            }

            var conn = _sortableList[e.RowIndex];

            switch (liveConnectionsDataGridView.Columns[e.ColumnIndex].Name)
            {
                case "connIconColumn":
                    if (_appSettings != null && _appSettings.ShowAppIcons && !string.IsNullOrEmpty(conn.ProcessPath))
                    {
                        bool isUwp = conn.ProcessPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) || (!conn.ProcessPath.Contains('\\') && !conn.ProcessPath.Contains(':'));
                        int iconIndex = isUwp ? _iconService.GetUwpIconIndex(conn.ProcessPath) : _iconService.GetIconIndex(conn.ProcessPath);

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
                case "connProtocolColumn": e.Value = conn.Protocol; break;
                case "connPidColumn": e.Value = conn.ProcessId; break;
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

        private void LiveConnectionsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _sortableList.Count)
            {
                return;
            }

            var conn = _sortableList[e.RowIndex];

            if (_newKeys.Contains(conn.Key))
            {
                e.CellStyle.Font = _newRowFont ??= new Font(e.CellStyle.Font ?? liveConnectionsDataGridView.Font, FontStyle.Bold);
            }

            // Dynamic theme assignment based on active state criteria
            if (conn.State != null)
            {
                if (conn.State.Equals("Established", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Theme.Colors.ConnectionEstablished;
                    // Black text on light backgrounds
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (conn.State.Equals("Listen", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Theme.Colors.ConnectionListening;
                    // Black text on light background
                    e.CellStyle.ForeColor = Color.Black;
                }
            }

            if (liveConnectionsDataGridView.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.SelectionBackColor = Theme.Colors.SelectionInfo;
                e.CellStyle.SelectionForeColor = Color.Black;
            }
            else
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
        }

        // --- Mouse & Selection Handling ---

        private void LiveConnectionsDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
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

        private void LiveConnectionsDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = e.RowIndex;
                liveConnectionsDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void LiveConnectionsDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = -1;
                liveConnectionsDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void LiveConnectionsDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (!liveConnectionsDataGridView.Rows[e.RowIndex].Selected && e.RowIndex == _hoveredRowIndex)
            {
                using var brush = new SolidBrush(Theme.Colors.HighlightOverlay);
                e.Graphics.FillRectangle(brush, e.RowBounds);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            liveConnectionsDataGridView.ColumnHeaderMouseClick += (s, ev) =>
            {
                if (_appSettings != null && ev.ColumnIndex >= 0)
                {
                    int currentSort;

                    if (_appSettings.LiveConnectionsSortColumn == ev.ColumnIndex)
                    {
                        // Toggle direction
                        currentSort = _appSettings.LiveConnectionsSortOrder == (int)SortOrder.Ascending
                            ? (int)SortOrder.Descending
                            : (int)SortOrder.Ascending;
                    }
                    else
                    {
                        // New column, reset to Ascending
                        currentSort = (int)SortOrder.Ascending;
                    }

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
            if (liveConnectionsDataGridView.SelectedRows.Count == 0)
            {
                return false;
            }

            int index = liveConnectionsDataGridView.SelectedRows[0].Index;
            if (index >= 0 && index < _sortableList.Count)
            {
                connection = _sortableList[index];
                return true;
            }
            return false;
        }

        private void LiveConnectionsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (!TryGetSelectedConnection(out var connection) || connection == null)
            {
                e.Cancel = true;
                return;
            }

            killProcessToolStripMenuItem.Enabled = connection.KillProcessCommand.CanExecute(null);
            blockRemoteIPToolStripMenuItem.Enabled = connection.BlockRemoteIpCommand.CanExecute(null);
            closeConnectionToolStripMenuItem.Enabled = TcpTrafficTracker.CanClose(connection.Connection);
            onlyRemoteAddressToolStripMenuItem.Enabled = connection.HasRemoteAddress;
            hideRemoteAddressToolStripMenuItem.Enabled = connection.HasRemoteAddress;
            clearFilterToolStripMenuItem.Enabled = _filterBox.TextLength > 0;
            bool pathExists = !string.IsNullOrEmpty(connection.ProcessPath) && (File.Exists(connection.ProcessPath) || connection.ProcessPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) || connection.ProcessPath.Contains("SystemApps", StringComparison.OrdinalIgnoreCase));
            openFileLocationToolStripMenuItem.Enabled = pathExists;
            createAdvancedRuleToolStripMenuItem.Enabled = pathExists;
        }

        private void KillProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.KillProcessCommand.CanExecute(null))
            {
                connection.KillProcessCommand.Execute(null);
            }
        }

        private void CloseConnectionToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!TryGetSelectedConnection(out var connection) || connection == null || !TcpTrafficTracker.CanClose(connection.Connection))
            {
                return;
            }

            uint error = TcpTrafficTracker.CloseConnection(connection.Connection);
            if (error != 0)
            {
                Messenger.MessageBox($"Could not close the connection.\n\nWindows error {error}: {new Win32Exception((int)error).Message}", "Close Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RefreshRequested?.Invoke();
        }

        // Quick filters append a CurrPorts-style rule to the advanced filter.
        private void AddFilter(string filter)
        {
            _filterBox.Text = _filterBox.Text.Trim().Length == 0 ? filter : _filterBox.Text.TrimEnd() + " " + filter;
        }

        // Names with spaces or placeholders such as "PID: 12" cannot be written as a filter; use the PID instead.
        private static string ProcessFilterValue(TcpConnectionViewModel connection) =>
            connection.ProcessName.Length > 0 && connection.ProcessName.All(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-')
                ? connection.ProcessName
                : connection.ProcessId.ToString();

        private void OnlyProcessToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null) AddFilter($"include:process:{ProcessFilterValue(connection)}");
        }

        private void HideProcessToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null) AddFilter($"exclude:process:{ProcessFilterValue(connection)}");
        }

        private void OnlyRemoteAddressToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.HasRemoteAddress) AddFilter($"include:remote:tcpudp:{connection.RemoteAddress}");
        }

        private void HideRemoteAddressToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.HasRemoteAddress) AddFilter($"exclude:remote:tcpudp:{connection.RemoteAddress}");
        }

        private void ClearFilterToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            _filterBox.Text = "";
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"connections-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
            };
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            var csv = new StringBuilder();
            csv.AppendLine("Application,PID,Protocol,State,Local Address,Local Port,Remote Address,Remote Port,Path");
            foreach (var c in _sortableList)
            {
                string[] values = [c.DisplayName, c.ProcessId.ToString(), c.Protocol, c.State, c.LocalAddress, c.LocalPort.ToString(),
                    c.RemoteAddress, c.RemotePort.ToString(), c.ProcessPath];
                csv.AppendLine(string.Join(",", values.Select(CsvField)));
            }

            try
            {
                File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Messenger.MessageBox($"Could not export the connections.\n\nError: {ex.Message}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string CsvField(string value)
        {
            // A leading = + - @ would run as a formula when the file is opened in a spreadsheet.
            if (value.Length > 0 && "=+-@".Contains(value[0]))
            {
                value = "'" + value;
            }
            return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private void BlockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && connection.BlockRemoteIpCommand.CanExecute(null))
            {
                connection.BlockRemoteIpCommand.Execute(null);
            }
        }

        private void CreateAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && !string.IsNullOrEmpty(connection.ProcessPath))
            {
                using var dialog = new CreateAdvancedRuleForm(_actionsService, connection.ProcessPath, "", _appSettings);
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    if (dialog.RuleVm != null)
                    {
                        var payload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                        _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                    }
                }
            }
        }

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null && !string.IsNullOrEmpty(connection.ProcessPath))
            {
                if (!File.Exists(connection.ProcessPath) && !Directory.Exists(connection.ProcessPath) && !connection.ProcessPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) && !connection.ProcessPath.Contains("SystemApps", StringComparison.OrdinalIgnoreCase))
                {
                    Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start("explorer.exe", $"/select, \"{connection.ProcessPath}\"");
                }
                catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
                {
                    Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Messenger.MessageBox("The path for this item is not available or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection != null)
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Live Connection");
                details.AppendLine($"Application: {connection.DisplayName}");
                details.AppendLine($"Path: {connection.ProcessPath}");
                details.AppendLine($"PID: {connection.ProcessId}");
                details.AppendLine($"Protocol: {connection.Protocol}");
                details.AppendLine($"State: {connection.State}");
                details.AppendLine($"Local: {connection.LocalAddress}:{connection.LocalPort}");
                details.AppendLine($"Remote: {connection.RemoteAddress}:{connection.RemotePort}");
                Clipboard.SetText(details.ToString());
            }
        }
    }
}
