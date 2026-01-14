using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public partial class DashboardControl : UserControl
    {
        private MainViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private IconService _iconService = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private FirewallActionsService _actionsService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private BindingSource _bindingSource;

        private static readonly Color AllowColor = Color.FromArgb(204, 255, 204);
        private static readonly Color BlockColor = Color.FromArgb(255, 204, 204);
        private static readonly Color HighlightOverlay = Color.FromArgb(25, Color.Black);

        public DashboardControl()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            // FORCE Double Buffering on the DataGridView 
            if (dashboardDataGridView != null)
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, dashboardDataGridView, new object[] { true });
            }
        }

        public void Initialize(MainViewModel viewModel, AppSettings appSettings, IconService iconService, DarkModeCS dm, WildcardRuleService wildcardRuleService, FirewallActionsService actionsService, BackgroundFirewallTaskService backgroundTaskService)
        {
            _viewModel = viewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _wildcardRuleService = wildcardRuleService;
            _actionsService = actionsService;
            _backgroundTaskService = backgroundTaskService;

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

        private void PendingConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Don't try to update if the window is closing
            if (this.Disposing || this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(LoadDashboardItems));
            }
            else
            {
                LoadDashboardItems();
            }
        }

        private void LoadDashboardItems()
        {
            _bindingSource.ResetBindings(false);
        }

        private void dashboardDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;

            if (grid.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending)
            {
                if (e.ColumnIndex == allowButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Allow");
                }
                else if (e.ColumnIndex == blockButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Block");
                }
                else if (e.ColumnIndex == ignoreButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Ignore");
                }
            }
        }

        private void dashboardDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Icon Column Logic
            if (e.ColumnIndex == dashIconColumn.Index)
            {
                if (_appSettings.ShowAppIcons &&
                    dashboardDataGridView.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending)
                {
                    int iconIndex = _iconService.GetIconIndex(pending.AppPath);
                    if (iconIndex != -1 && _iconService.ImageList != null)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
                return;
            }

            // Color Logic 
            if (e.ColumnIndex == allowButtonColumn.Index)
            {
                e.CellStyle.BackColor = AllowColor;
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (e.ColumnIndex == blockButtonColumn.Index)
            {
                e.CellStyle.BackColor = BlockColor;
                e.CellStyle.ForeColor = Color.Black;
            }

            // Selection Logic
            if (dashboardDataGridView.Rows[e.RowIndex].Selected)
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

            if (grid.Rows[e.RowIndex].Selected) return;

            var clientPoint = grid.PointToClient(MousePosition);
            var hitTest = grid.HitTest(clientPoint.X, clientPoint.Y);

            if (e.RowIndex == hitTest.RowIndex)
            {
                using var overlayBrush = new SolidBrush(HighlightOverlay);
                e.Graphics.FillRectangle(overlayBrush, e.RowBounds);
            }
        }

        private void dashboardDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void dashboardDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
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
                using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath, _appSettings);
                if (wildcardDialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    var newRule = wildcardDialog.NewRule;
                    _viewModel.CreateWildcardRule(pending, newRule);
                }
            }
        }

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            if (dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                if (!string.IsNullOrEmpty(pending.AppPath) && File.Exists(pending.AppPath))
                {
                    bool isSigned = SignatureValidationService.GetPublisherInfo(pending.AppPath, out _);
                    allowAndTrustPublisherToolStripMenuItem.Visible = isSigned;
                }
                else
                {
                    allowAndTrustPublisherToolStripMenuItem.Visible = false;
                }
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                using var dialog = new CreateAdvancedRuleForm(_actionsService, pending.AppPath!, pending.Direction!, _appSettings);
                dialog.ShowDialog(this.FindForm());
            }
        }

        private void openFileLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending &&
                !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath) && !Directory.Exists(pending.AppPath))
                {
                    DarkModeForms.Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{pending.AppPath}\"");
                }
                catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
                {
                    DarkModeForms.Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                DarkModeForms.Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void showBlockingRuleInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.SelectedRows.Count > 0 &&
                dashboardDataGridView.SelectedRows[0].DataBoundItem is PendingConnectionViewModel pending)
            {
                string filterId = string.IsNullOrEmpty(pending.FilterId) ? "N/A" : pending.FilterId;
                string layerId = string.IsNullOrEmpty(pending.LayerId) ? "N/A" : pending.LayerId;

                string message = $"Application: {pending.FileName}\n" +
                                 $"Direction: {pending.Direction}\n" +
                                 $"Remote: {pending.RemoteAddress}:{pending.RemotePort}\n\n" +
                                 $"Blocking Filter ID: {filterId}\n" +
                                 $"Blocking Layer ID: {layerId}\n\n" +
                                 "You can use these IDs to search within the advanced 'Windows Defender Firewall' console (wf.msc) or with PowerShell's Get-NetFirewallRule / Get-NetFirewallFilter commands to find the specific rule/filter.";

                DarkModeForms.Messenger.MessageBox(message, "Blocking Rule Information", MessageBoxButtons.OK, DarkModeForms.MsgIcon.Info);
            }
        }
    }
}