// File: DashboardControl.cs
using DarkModeForms;
using System;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Linq;
using MinimalFirewall.TypedObjects;
using System.Drawing;

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
        private BindingSource _bindingSource;

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

            dashboardDataGridView.AutoGenerateColumns = false;
            _bindingSource = new BindingSource { DataSource = _viewModel.PendingConnections };
            dashboardDataGridView.DataSource = _bindingSource;

            _viewModel.PendingConnections.CollectionChanged += PendingConnections_CollectionChanged;

            LoadDashboardItems();
        }

        public void SetIconColumnVisibility(bool visible)
        {
            if (dashIconColumn != null)
            {
                dashIconColumn.Visible = visible;
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
            _bindingSource.ResetBindings(false);
            dashboardDataGridView.Refresh();
        }

        private void dashboardDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the click is on a button cell and not on a header
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;
            var column = grid.Columns[e.ColumnIndex];

            if (grid.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending)
            {
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "allowButtonColumn")
                    {
                        _viewModel.ProcessDashboardAction(pending, "Allow");
                    }
                    else if (column.Name == "blockButtonColumn")
                    {
                        _viewModel.ProcessDashboardAction(pending, "Block");
                    }
                    else if (column.Name == "ignoreButtonColumn")
                    {
                        _viewModel.ProcessDashboardAction(pending, "Ignore");
                    }
                }
            }
        }

        private void dashboardDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;

            // Handle App Icons
            if (grid.Columns[e.ColumnIndex].Name == "dashIconColumn")
            {
                if (grid.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending && _appSettings.ShowAppIcons)
                {
                    int iconIndex = _iconService.GetIconIndex(pending.AppPath);
                    if (iconIndex != -1 && _iconService.ImageList != null)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
                return;
            }

            // Handle Button Colors
            var allowColumn = grid.Columns["allowButtonColumn"];
            var blockColumn = grid.Columns["blockButtonColumn"];
            var ignoreColumn = grid.Columns["ignoreButtonColumn"];

            if (e.ColumnIndex == allowColumn.Index)
            {
                e.CellStyle.BackColor = Color.FromArgb(204, 255, 204);
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (e.ColumnIndex == blockColumn.Index)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 204, 204);
                e.CellStyle.ForeColor = Color.Black;
            }

            // Selection Color
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

        private void dashboardDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var row = grid.Rows[e.RowIndex];

            // Draw hover effect
            if (row.Selected) return;

            var mouseOverRow = grid.HitTest(grid.PointToClient(MousePosition).X, grid.PointToClient(MousePosition).Y).RowIndex;
            if (e.RowIndex == mouseOverRow)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void dashboardDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void dashboardDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                grid.InvalidateRow(e.RowIndex);
            }
        }

        private void TempAllowMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending &&
                sender is ToolStripMenuItem menuItem &&
                int.TryParse(menuItem.Tag?.ToString(), out int minutes))
            {
                _viewModel.ProcessTemporaryDashboardAction(pending, "TemporaryAllow", TimeSpan.FromMinutes(minutes));
            }
        }

        private void PermanentAllowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Allow");
            }
        }

        private void AllowAndTrustPublisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Allow", trustPublisher: true);
            }
        }

        private void PermanentBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Block");
            }
        }

        private void IgnoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                _viewModel.ProcessDashboardAction(pending, "Ignore");
            }
        }

        private void createWildcardRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
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
            if (dashboardDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            if (dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                bool isSigned = SignatureValidationService.GetPublisherInfo(pending.AppPath, out _);
                allowAndTrustPublisherToolStripMenuItem.Visible = isSigned;
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                using var dialog = new
                    CreateAdvancedRuleForm(_firewallPolicy, _actionsService, pending.AppPath!, pending.Direction!);
                dialog.ShowDialog(this.FindForm());
            }
        }

        private void openFileLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending &&
                !string.IsNullOrEmpty(pending.AppPath) &&
                System.IO.File.Exists(pending.AppPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{pending.AppPath}\"");
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
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
