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

namespace MinimalFirewall
{
    public partial class LiveConnectionsControl : UserControl
    {
        private TrafficMonitorViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private IconService _iconService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private FirewallActionsService _actionsService = null!;
        private BindingSource _bindingSource;
        private SortableBindingList<TcpConnectionViewModel> _sortableList;

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

            liveConnectionsDataGridView.AutoGenerateColumns = false;
            _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections);
            _bindingSource = new BindingSource { DataSource = _sortableList };
            liveConnectionsDataGridView.DataSource = _bindingSource;

            _viewModel.ActiveConnections.CollectionChanged += ActiveConnections_CollectionChanged;

            liveConnectionsDataGridView.ColumnHeaderMouseClick += liveConnectionsDataGridView_ColumnHeaderMouseClick;
        }

        private void ActiveConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateLiveConnectionsView(e)));
            }
            else
            {
                UpdateLiveConnectionsView(e);
            }
        }

        public void UpdateLiveConnectionsView(NotifyCollectionChangedEventArgs e = null)
        {
            if (e != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections);
                    _bindingSource.DataSource = _sortableList;
                }
                else
                {
                    _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections);
                    _bindingSource.DataSource = _sortableList;
                }
            }
            else
            {
                _sortableList = new SortableBindingList<TcpConnectionViewModel>(_viewModel.ActiveConnections);
                _bindingSource.DataSource = _sortableList;
            }

            _bindingSource.ResetBindings(false);
            ApplySorting();
            liveConnectionsDataGridView.Refresh();
        }


        public void OnTabDeselected()
        {
            _viewModel.StopMonitoring();
            _sortableList.Clear();
            _bindingSource.DataSource = null;
            liveConnectionsDataGridView.DataSource = null;
            liveConnectionsDataGridView.Refresh();
        }

        public void UpdateIconColumnVisibility()
        {
            if (connIconColumn != null)
            {
                connIconColumn.Visible = _appSettings.ShowAppIcons;
            }
        }

        private void ApplySorting()
        {
            int sortCol = _appSettings.LiveConnectionsSortColumn;
            var sortOrder = (SortOrder)_appSettings.LiveConnectionsSortOrder;

            if (sortCol > -1 && sortOrder != SortOrder.None && sortCol < liveConnectionsDataGridView.Columns.Count)
            {
                var column = liveConnectionsDataGridView.Columns[sortCol];
                var direction = sortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                _sortableList.Sort(column.DataPropertyName, direction);
                liveConnectionsDataGridView.Sort(column, direction);
            }
        }

        private void liveConnectionsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;

            if (grid.Rows[e.RowIndex].DataBoundItem is not TcpConnectionViewModel conn) return;

            if (grid.Columns[e.ColumnIndex].Name == "connIconColumn")
            {
                if (_appSettings.ShowAppIcons && !string.IsNullOrEmpty(conn.ProcessPath))
                {
                    int iconIndex = _iconService.GetIconIndex(conn.ProcessPath);
                    if (iconIndex != -1 && _iconService.ImageList != null)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
                return;
            }

            if (conn.State.Equals("Established", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(204, 255, 204);
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (conn.State.Equals("Listen", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 255, 204);
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

        private void liveConnectionsDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            var newColumn = liveConnectionsDataGridView.Columns[e.ColumnIndex];
            var sortOrder = liveConnectionsDataGridView.SortOrder;
            string propertyName = newColumn.DataPropertyName;

            if (string.IsNullOrEmpty(propertyName)) return;

            _appSettings.LiveConnectionsSortColumn = e.ColumnIndex;
            _appSettings.LiveConnectionsSortOrder = (int)sortOrder;

            var direction = sortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            _sortableList.Sort(propertyName, direction);
            _bindingSource.ResetBindings(false);
        }

        private bool TryGetSelectedConnection(out TcpConnectionViewModel? connection)
        {
            connection = null;
            if (liveConnectionsDataGridView.SelectedRows.Count == 0)
            {
                return false;
            }

            if (liveConnectionsDataGridView.SelectedRows[0].DataBoundItem is TcpConnectionViewModel conn)
            {
                connection = conn;
                return true;
            }
            return false;
        }

        private void killProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection.KillProcessCommand.CanExecute(null))
            {
                connection.KillProcessCommand.Execute(null);
            }
        }

        private void blockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && connection.BlockRemoteIpCommand.CanExecute(null))
            {
                connection.BlockRemoteIpCommand.Execute(null);
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedConnection(out var connection) && !string.IsNullOrEmpty(connection.ProcessPath))
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
            if (TryGetSelectedConnection(out var connection) && !string.IsNullOrEmpty(connection.ProcessPath) && File.Exists(connection.ProcessPath))
            {
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
            if (TryGetSelectedConnection(out var connection))
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

        private void liveConnectionsDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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
    }
}

