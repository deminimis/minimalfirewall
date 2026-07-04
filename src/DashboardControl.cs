using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text.Json;
using System.Diagnostics;

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
        private BindingSource _bindingSource = null!;
        private readonly string _layoutSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dashboard_layout.json");

        public DashboardControl()
        {
            InitializeComponent();
            DoubleBuffered = true;

            if (dashboardDataGridView != null)
            {
                dashboardDataGridView.CellMouseDown += DashboardDataGridView_CellMouseDown;
                dashboardDataGridView.SelectionChanged += DashboardDataGridView_SelectionChanged;
            }
        }

        public void Initialize(MainViewModel viewModel, AppSettings appSettings, IconService iconService, WildcardRuleService wildcardRuleService, FirewallActionsService actionsService, BackgroundFirewallTaskService backgroundTaskService)
        {
            _viewModel = viewModel;
            _appSettings = appSettings;
            _iconService = iconService;
            _wildcardRuleService = wildcardRuleService;
            _actionsService = actionsService;
            _backgroundTaskService = backgroundTaskService;

            dashboardDataGridView.AutoGenerateColumns = false;
            if (dashIconColumn != null) dashIconColumn.DefaultCellStyle.NullValue = new Bitmap(1, 1);
            _bindingSource = new BindingSource { DataSource = _viewModel.PendingConnections };
            dashboardDataGridView.DataSource = _bindingSource;

            _viewModel.PendingConnections.CollectionChanged += PendingConnections_CollectionChanged;
            LoadDashboardItems();

            try
            {
                if (File.Exists(_layoutSettingsPath))
                {
                    string json = File.ReadAllText(_layoutSettingsPath);
                    var settings = JsonSerializer.Deserialize<DashboardLayoutSettings>(json);
                    if (settings != null && settings.SplitterDistance > 0 && settings.SplitterDistance < splitContainer.Height)
                    {
                        splitContainer.SplitterDistance = settings.SplitterDistance;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] DashboardControl.Initialize failed to restore layout: {ex.Message}");
            }

            splitContainer.SplitterMoved += SplitContainer_SplitterMoved;

            ApplyThemeToControls();
        }

        private void ApplyThemeToControls()
        {
            if (detailsRichTextBox != null)
            {
                detailsRichTextBox.BackColor = Theme.Colors.Surface;
                detailsRichTextBox.ForeColor = Theme.Colors.TextActive;
                detailsLabel.ForeColor = Theme.Colors.TextActive;
            }
        }

        private async void DashboardDataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            var pending = GetSelectedPendingConnection();
            if (pending == null)
            {
                detailsRichTextBox?.Clear();
                return;
            }

            string? pubName = null;
            if (!string.IsNullOrEmpty(pending.AppPath))
            {
                pubName = await Task.Run(() =>
                {
                    SignatureValidationService.GetPublisherInfo(pending.AppPath, out string? name);
                    return name;
                });
            }

            // Safety check 
            if (IsDisposed || GetSelectedPendingConnection() != pending)
            {
                return;
            }

            RenderConnectionDetails(pending, pubName);
        }

        private void RenderConnectionDetails(PendingConnectionViewModel pending, string? pubName)
        {
            if (detailsRichTextBox == null)
            {
                return;
            }

            detailsRichTextBox.Clear();

            void AppendDetail(string label, string? value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                detailsRichTextBox.SelectionStart = detailsRichTextBox.TextLength;
                detailsRichTextBox.SelectionLength = 0;

                // Centralized Theme Styling
                detailsRichTextBox.SelectionColor = Theme.Colors.InfoText;
                detailsRichTextBox.SelectionFont = new Font(detailsRichTextBox.Font, FontStyle.Bold);
                detailsRichTextBox.AppendText(label + ": ");

                detailsRichTextBox.SelectionColor = Theme.Colors.TextActive;
                detailsRichTextBox.SelectionFont = detailsRichTextBox.Font;
                detailsRichTextBox.AppendText(value + Environment.NewLine);
            }

            AppendDetail("Application", pending.FileName);
            AppendDetail("Path", pending.AppPath);
            AppendDetail("PID", pending.ProcessId);
            AppendDetail("Owner", pending.ProcessOwner);

            string parentDisplay = string.IsNullOrEmpty(pending.ParentProcessName) ?
                pending.ParentProcessId : $"{pending.ParentProcessName} (PID: {pending.ParentProcessId})";
            AppendDetail("Parent Process", parentDisplay);

            AppendDetail("Service", string.IsNullOrEmpty(pending.ServiceName) ? "N/A" : pending.ServiceName);
            AppendDetail("Direction", pending.Direction);

            string remote = string.IsNullOrEmpty(pending.RemoteAddress) ? "N/A" : $"{pending.RemoteAddress}:{pending.RemotePort}";
            AppendDetail("Remote Address", remote);

            AppendDetail("Protocol", pending.ProtocolDisplay);
            AppendDetail("CMD", pending.CommandLine);
            AppendDetail("Publisher", pubName);
        }

        public void SetIconColumnVisibility(bool visible)
        {
            dashIconColumn?.Visible = visible;
        }

        private void PendingConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Don't try to update if the window is closing
            if (Disposing || IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(LoadDashboardItems));
            }
            else
            {
                LoadDashboardItems();
            }
        }

        private void LoadDashboardItems()
        {
            dashboardDataGridView.SuspendLayout();
            var prevAutoSize = dashboardDataGridView.AutoSizeColumnsMode;
            dashboardDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            _bindingSource.ResetBindings(false);

            dashboardDataGridView.AutoSizeColumnsMode = prevAutoSize;
            dashboardDataGridView.ResumeLayout();
        }

        private void DashboardDataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (sender is not DataGridView grid)
            {
                return;
            }

            if (grid.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending)
            {
                if (allowButtonColumn != null && e.ColumnIndex == allowButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Allow");
                }
                else if (blockButtonColumn != null && e.ColumnIndex == blockButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Block");
                }
                else if (ignoreButtonColumn != null && e.ColumnIndex == ignoreButtonColumn.Index)
                {
                    _viewModel.ProcessDashboardAction(pending, "Ignore");
                }
            }
        }

        private void DashboardDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            // Icon Column Logic
            if (dashIconColumn != null && e.ColumnIndex == dashIconColumn.Index)
            {
                e.Value = null;
                if (_appSettings.ShowAppIcons &&
                    dashboardDataGridView.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending &&
                    !string.IsNullOrEmpty(pending.AppPath))
                {
                    bool isUwp = pending.AppPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) || (!pending.AppPath.Contains('\\') && !pending.AppPath.Contains(':'));
                    int iconIndex = isUwp ? _iconService.GetUwpIconIndex(pending.AppPath) : _iconService.GetIconIndex(pending.AppPath);

                    if (iconIndex != -1 && _iconService.ImageList != null && iconIndex < _iconService.ImageList.Images.Count)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
            }

            ApplyDashboardCellTheme(e);
        }

        private void ApplyDashboardCellTheme(DataGridViewCellFormattingEventArgs e)
        {
            // Semantic Color Logic 
            if (allowButtonColumn != null && e.ColumnIndex == allowButtonColumn.Index)
            {
                e.CellStyle.BackColor = Theme.Colors.Success;
                e.CellStyle.ForeColor = Color.Black; 
            }
            else if (blockButtonColumn != null && e.ColumnIndex == blockButtonColumn.Index)
            {
                e.CellStyle.BackColor = Theme.Colors.Danger;
                e.CellStyle.ForeColor = Color.Black; 
            }
            else if (ignoreButtonColumn != null && e.ColumnIndex == ignoreButtonColumn.Index)
            {
                e.CellStyle.BackColor = Theme.Colors.Ignore;
                e.CellStyle.ForeColor = Color.Black; 
            }

            // Selection Logic
            if (dashboardDataGridView.Rows[e.RowIndex].Selected)
            {
                if ((allowButtonColumn != null && e.ColumnIndex == allowButtonColumn.Index) ||
                    (blockButtonColumn != null && e.ColumnIndex == blockButtonColumn.Index) ||
                    (ignoreButtonColumn != null && e.ColumnIndex == ignoreButtonColumn.Index))
                {
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                }
                else
                {
                    e.CellStyle.SelectionBackColor = Theme.Colors.SelectionInfo;
                    e.CellStyle.SelectionForeColor = Color.Black; 
                }
            }
            else
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
        }

        private void DashboardDataGridView_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (sender is not DataGridView grid)
            {
                return;
            }

            if (grid.Rows[e.RowIndex].Selected)
            {
                return;
            }

            var clientPoint = grid.PointToClient(MousePosition);
            var hitTest = grid.HitTest(clientPoint.X, clientPoint.Y);

            if (e.RowIndex == hitTest.RowIndex)
            {
                using var highlightBrush = new SolidBrush(Theme.Colors.HighlightOverlay);
                e.Graphics.FillRectangle(highlightBrush, e.RowBounds);
            }
        }

        private void DashboardDataGridView_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void DashboardDataGridView_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void DashboardDataGridView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            // If it's a right-click on a valid row, update the selection before the context menu opens
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dashboardDataGridView.ClearSelection();
                dashboardDataGridView.Rows[e.RowIndex].Selected = true;
                dashboardDataGridView.CurrentCell = dashboardDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
            }
        }

        private PendingConnectionViewModel? GetSelectedPendingConnection()
        {
            return dashboardDataGridView.SelectedRows.Count > 0
                ? dashboardDataGridView.SelectedRows[0].DataBoundItem as PendingConnectionViewModel
                : null;
        }

        private void ProcessActionForSelected(string action, bool trustPublisher = false)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                _viewModel.ProcessDashboardAction(pending, action, trustPublisher);
            }
        }

        

        private void PermanentAllowToolStripMenuItem_Click(object sender, EventArgs e) => ProcessActionForSelected("Allow");

        private void AllowAndTrustPublisherToolStripMenuItem_Click(object sender, EventArgs e) => ProcessActionForSelected("Allow", trustPublisher: true);

        private void PermanentBlockToolStripMenuItem_Click(object sender, EventArgs e) => ProcessActionForSelected("Block");

        private void IgnoreToolStripMenuItem_Click(object sender, EventArgs e) => ProcessActionForSelected("Ignore");

        private void TempAllowMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending &&
                sender is ToolStripMenuItem menuItem &&
                int.TryParse(menuItem.Tag?.ToString(), out int minutes))
            {
                _viewModel.ProcessTemporaryDashboardAction(pending, "TemporaryAllow", TimeSpan.FromMinutes(minutes));
            }
        }

        private void CreateWildcardRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath, _appSettings);
                if (wildcardDialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    _viewModel.CreateWildcardRule(pending, wildcardDialog.NewRule);
                }
            }
        }

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var pending = GetSelectedPendingConnection();
            if (pending == null)
            {
                e.Cancel = true;
                return;
            }

            allowAndTrustPublisherToolStripMenuItem.Visible =
                !string.IsNullOrEmpty(pending.AppPath) &&
                File.Exists(pending.AppPath) &&
                SignatureValidationService.IsSignatureTrusted(pending.AppPath, out _);
        }

        private void CreateAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                using var dialog = new CreateAdvancedRuleForm(_actionsService, pending.AppPath!, pending.Direction!, _appSettings);
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    // Clean up the alert from dashboard
                    _viewModel.ProcessDashboardAction(pending, "Ignore");
                }
            }
        }

        private void OpenFileLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath) && !Directory.Exists(pending.AppPath) && !pending.AppPath.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) && !pending.AppPath.Contains("SystemApps", StringComparison.OrdinalIgnoreCase))
                {
                    Messenger.MessageBox("The path for this item is no longer valid or does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start("explorer.exe", $"/select, \"{pending.AppPath}\"");
                }
                catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
                {
                    Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CopyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Pending Connection");
                details.AppendLine($"Application: {pending.FileName}");
                details.AppendLine($"Path: {pending.AppPath}");
                details.AppendLine($"PID: {pending.ProcessId}");
                if (!string.IsNullOrEmpty(pending.ProcessOwner))
                {
                    details.AppendLine($"Owner: {pending.ProcessOwner}");
                }

                if (!string.IsNullOrEmpty(pending.ParentProcessId))
                {
                    string parentDisplay = string.IsNullOrEmpty(pending.ParentProcessName) ? pending.ParentProcessId : $"{pending.ParentProcessName} (PID: {pending.ParentProcessId})";
                    details.AppendLine($"Parent Process: {parentDisplay}");
                }
                details.AppendLine($"Service: {pending.ServiceName}");
                details.AppendLine($"Direction: {pending.Direction}");
                if (!string.IsNullOrEmpty(pending.CommandLine))
                {
                    details.AppendLine($"CMD: {pending.CommandLine}");
                }

                Clipboard.SetText(details.ToString());
            }
        }

        private void ShowBlockingRuleInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                string filterId = string.IsNullOrEmpty(pending.FilterId) ? "N/A" : pending.FilterId;
                string layerId = string.IsNullOrEmpty(pending.LayerId) ? "N/A" : pending.LayerId;

                string message = $"Application: {pending.FileName}\n" +
                                 $"Direction: {pending.Direction}\n" +
                                 $"Remote: {pending.RemoteAddress}:{pending.RemotePort}\n\n" +
                                 $"Blocking Filter ID: {filterId}\n" +
                                 $"Blocking Layer ID: {layerId}\n\n" +
                                 "You can use these IDs to search within the advanced 'Windows Defender Firewall' console (wf.msc) or with PowerShell's " +
                                 "Get-NetFirewallRule / Get-NetFirewallFilter commands to find the specific rule/filter.";

                Messenger.MessageBox(message, "Blocking Rule Information", MessageBoxButtons.OK, DarkModeForms.MsgIcon.Info);
            }
        }

        private async void CopyHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath))
                {
                    Messenger.MessageBox("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                copyHashToolStripMenuItem.Text = "Calculating...";
                copyHashToolStripMenuItem.Enabled = false;

                string hash = await SystemDiscoveryService.CalculateSHA256Async(pending.AppPath);

                if (!string.IsNullOrEmpty(hash))
                {
                    Clipboard.SetText(hash);
                    copyHashToolStripMenuItem.Text = "Copied!";
                    await Task.Delay(1500);
                }
                else
                {
                    Messenger.MessageBox("Could not calculate file hash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                copyHashToolStripMenuItem.Text = "Copy File Hash (SHA-256)";
                copyHashToolStripMenuItem.Enabled = true;
            }
        }

        private async void CheckVirusTotalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath))
                {
                    Messenger.MessageBox("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                checkVirusTotalToolStripMenuItem.Text = "Calculating Hash...";
                checkVirusTotalToolStripMenuItem.Enabled = false;

                string hash = await SystemDiscoveryService.CalculateSHA256Async(pending.AppPath);

                checkVirusTotalToolStripMenuItem.Text = "Check on VirusTotal";
                checkVirusTotalToolStripMenuItem.Enabled = true;

                if (!string.IsNullOrEmpty(hash))
                {
                    Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"https://www.virustotal.com/gui/file/{hash}",
                        UseShellExecute = true
                    });
                }
                else
                {
                    Messenger.MessageBox("Could not calculate file hash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SplitContainer_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            try
            {
                var settings = new DashboardLayoutSettings { SplitterDistance = splitContainer.SplitterDistance };
                string json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_layoutSettingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] DashboardControl.SplitContainer_SplitterMoved failed to save layout: {ex.Message}");
            }
        }

        public class DashboardLayoutSettings
        {
            public int SplitterDistance { get; set; }
        }
    }
}
