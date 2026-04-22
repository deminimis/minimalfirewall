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

namespace MinimalFirewall
{
    public partial class DashboardControl : UserControl
    {
        private MainViewModel _viewModel = null!;
        private AppSettings _appSettings = null!;
        private IconService _iconService = null!;
        private DarkModeCS _dm = null!;
        private WildcardRuleService _wildcardRuleService = null!;
        private FirewallActionsService _actionsService = null!;
        private BackgroundFirewallTaskService _backgroundTaskService = null!;
        private BindingSource _bindingSource = null!;

        private static readonly Color AllowColor = Color.FromArgb(204, 255, 204);
        private static readonly Color BlockColor = Color.FromArgb(255, 204, 204);
        private readonly SolidBrush _highlightOverlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
        private readonly string _layoutSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dashboard_layout.json");

        public DashboardControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            if (dashboardDataGridView != null)
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, dashboardDataGridView, new object[] { true });
                dashboardDataGridView.CellMouseDown += dashboardDataGridView_CellMouseDown;
                dashboardDataGridView.SelectionChanged += DashboardDataGridView_SelectionChanged;
            }
        }

        public void Initialize(MainViewModel viewModel, AppSettings appSettings, IconService iconService, DarkModeCS dm, WildcardRuleService wildcardRuleService, FirewallActionsService actionsService, BackgroundFirewallTaskService backgroundTaskService)
        {
            _viewModel = viewModel;
            _appSettings = appSettings;
            _dm = dm;
            _iconService = iconService;
            _wildcardRuleService = wildcardRuleService;
            _actionsService = actionsService;
            _backgroundTaskService = backgroundTaskService;

            dashboardDataGridView.AutoGenerateColumns = false;
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
            catch { }

            splitContainer.SplitterMoved += SplitContainer_SplitterMoved;

            if (_dm != null && detailsRichTextBox != null)
            {
                bool isDark = _dm.IsDarkMode;
                detailsRichTextBox.BackColor = isDark ? _dm.OScolors.Surface : Color.White;
                detailsRichTextBox.ForeColor = isDark ? _dm.OScolors.TextActive : Color.Black;
                detailsLabel.ForeColor = isDark ? Color.White : Color.Black;
            }
        }

        private async void DashboardDataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (detailsRichTextBox == null) return;
            detailsRichTextBox.Clear();

            var pending = GetSelectedPendingConnection();
            if (pending == null) return;

            bool isDark = _dm?.IsDarkMode == true;
            Color labelColor = isDark ? Color.LightSkyBlue : Color.Blue;
            Color valColor = isDark ? _dm!.OScolors.TextActive : Color.Black;
            Font boldFont = new Font(detailsRichTextBox.Font, FontStyle.Bold);

            void AppendLine(string label, string value)
            {
                detailsRichTextBox.SelectionStart = detailsRichTextBox.TextLength;
                detailsRichTextBox.SelectionLength = 0;
                detailsRichTextBox.SelectionColor = labelColor;
                detailsRichTextBox.SelectionFont = boldFont;
                detailsRichTextBox.AppendText(label + ": ");

                detailsRichTextBox.SelectionColor = valColor;
                detailsRichTextBox.SelectionFont = detailsRichTextBox.Font;
                detailsRichTextBox.AppendText(value + Environment.NewLine);
            }

            AppendLine("Application", pending.FileName);
            AppendLine("Path", pending.AppPath);
            AppendLine("PID", pending.ProcessId);
            if (!string.IsNullOrEmpty(pending.ProcessOwner)) AppendLine("Owner", pending.ProcessOwner);
            if (!string.IsNullOrEmpty(pending.ParentProcessId))
            {
                string parentDisplay = string.IsNullOrEmpty(pending.ParentProcessName) ? pending.ParentProcessId : $"{pending.ParentProcessName} (PID: {pending.ParentProcessId})";
                AppendLine("Parent Process", parentDisplay);
            }
            AppendLine("Service", string.IsNullOrEmpty(pending.ServiceName) ? "N/A" : pending.ServiceName);
            AppendLine("Direction", pending.Direction);
            string remote = string.IsNullOrEmpty(pending.RemoteAddress) ? "N/A" : $"{pending.RemoteAddress}:{pending.RemotePort}";
            AppendLine("Remote Address", remote);
            AppendLine("Protocol", pending.ProtocolDisplay);

            if (!string.IsNullOrEmpty(pending.CommandLine))
            {
                AppendLine("CMD", pending.CommandLine);
            }

            string appPathToVerify = pending.AppPath;
            if (!string.IsNullOrEmpty(appPathToVerify))
            {
                string pubName = await Task.Run(() =>
                {
                    SignatureValidationService.GetPublisherInfo(appPathToVerify, out string name);
                    return name;
                });

                if (!this.IsDisposed && GetSelectedPendingConnection() == pending && !string.IsNullOrEmpty(pubName))
                {
                    AppendLine("Publisher", pubName);
                }
            }
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
            dashboardDataGridView.SuspendLayout();
            var prevAutoSize = dashboardDataGridView.AutoSizeColumnsMode;
            dashboardDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            _bindingSource.ResetBindings(false);

            dashboardDataGridView.AutoSizeColumnsMode = prevAutoSize;
            dashboardDataGridView.ResumeLayout();
        }

        private void dashboardDataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (sender is not DataGridView grid) return;

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

        private void dashboardDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Icon Column Logic
            if (dashIconColumn != null && e.ColumnIndex == dashIconColumn.Index)
            {
                e.Value = null;

                if (_appSettings.ShowAppIcons &&
                    dashboardDataGridView.Rows[e.RowIndex].DataBoundItem is PendingConnectionViewModel pending)
                {
                    int iconIndex = _iconService.GetIconIndex(pending.AppPath);

                    if (iconIndex != -1 && _iconService.ImageList != null && iconIndex < _iconService.ImageList.Images.Count)
                    {
                        e.Value = _iconService.ImageList.Images[iconIndex];
                    }
                }
            }

            // Color Logic 
            if (allowButtonColumn != null && e.ColumnIndex == allowButtonColumn.Index)
            {
                e.CellStyle.BackColor = AllowColor;
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (blockButtonColumn != null && e.ColumnIndex == blockButtonColumn.Index)
            {
                e.CellStyle.BackColor = BlockColor;
                e.CellStyle.ForeColor = Color.Black;
            }
            else if (ignoreButtonColumn != null && e.ColumnIndex == ignoreButtonColumn.Index)
            {
                e.CellStyle.BackColor = _dm != null && _dm.IsDarkMode ?
                    Color.FromArgb(85, 85, 85) : Color.FromArgb(200, 200, 200);
            }

            // Selection Logic
            if (dashboardDataGridView.Rows[e.RowIndex].Selected)
            {
                // Prevent selection color from covering the action buttons
                if ((allowButtonColumn != null && e.ColumnIndex == allowButtonColumn.Index) ||
                    (blockButtonColumn != null && e.ColumnIndex == blockButtonColumn.Index) ||
                    (ignoreButtonColumn != null && e.ColumnIndex == ignoreButtonColumn.Index))
                {
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                }
                else
                {
                    // Custom light blue selection for all other cells
                    e.CellStyle.SelectionBackColor = Color.FromArgb(189, 222, 255);
                    e.CellStyle.SelectionForeColor = Color.Black;
                }
            }
            else
            {
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
        }

        private void dashboardDataGridView_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (sender is not DataGridView grid) return;
            if (grid.Rows[e.RowIndex].Selected) return;

            var clientPoint = grid.PointToClient(MousePosition);
            var hitTest = grid.HitTest(clientPoint.X, clientPoint.Y);

            if (e.RowIndex == hitTest.RowIndex)
            {
                e.Graphics.FillRectangle(_highlightOverlayBrush, e.RowBounds);
            }
        }

        private void dashboardDataGridView_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void dashboardDataGridView_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dashboardDataGridView.InvalidateRow(e.RowIndex);
            }
        }

        private void dashboardDataGridView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
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

        private void createWildcardRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath, _appSettings);
                if (wildcardDialog.ShowDialog(this.FindForm()) == DialogResult.OK)
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

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending)
            {
                using var dialog = new CreateAdvancedRuleForm(_actionsService, pending.AppPath!, pending.Direction!, _appSettings);
                if (dialog.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    // Clean up the alert from dashbaord
                    _viewModel.ProcessDashboardAction(pending, "Ignore");
                }
            }
        }

        private void openFileLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
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
            if (GetSelectedPendingConnection() is { } pending)
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Type: Pending Connection");
                details.AppendLine($"Application: {pending.FileName}");
                details.AppendLine($"Path: {pending.AppPath}");
                details.AppendLine($"PID: {pending.ProcessId}");
                if (!string.IsNullOrEmpty(pending.ProcessOwner))
                    details.AppendLine($"Owner: {pending.ProcessOwner}");
                if (!string.IsNullOrEmpty(pending.ParentProcessId))
                {
                    string parentDisplay = string.IsNullOrEmpty(pending.ParentProcessName) ? pending.ParentProcessId : $"{pending.ParentProcessName} (PID: {pending.ParentProcessId})";
                    details.AppendLine($"Parent Process: {parentDisplay}");
                }
                details.AppendLine($"Service: {pending.ServiceName}");
                details.AppendLine($"Direction: {pending.Direction}");
                if (!string.IsNullOrEmpty(pending.CommandLine))
                    details.AppendLine($"CMD: {pending.CommandLine}");
                Clipboard.SetText(details.ToString());
            }
        }

        private void showBlockingRuleInfoToolStripMenuItem_Click(object sender, EventArgs e)
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

                DarkModeForms.Messenger.MessageBox(message, "Blocking Rule Information", MessageBoxButtons.OK, DarkModeForms.MsgIcon.Info);
            }
        }

        private async void copyHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath))
                {
                    DarkModeForms.Messenger.MessageBox("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    DarkModeForms.Messenger.MessageBox("Could not calculate file hash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                copyHashToolStripMenuItem.Text = "Copy File Hash (SHA-256)";
                copyHashToolStripMenuItem.Enabled = true;
            }
        }

        private async void checkVirusTotalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetSelectedPendingConnection() is { } pending && !string.IsNullOrEmpty(pending.AppPath))
            {
                if (!File.Exists(pending.AppPath))
                {
                    DarkModeForms.Messenger.MessageBox("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                checkVirusTotalToolStripMenuItem.Text = "Calculating Hash...";
                checkVirusTotalToolStripMenuItem.Enabled = false;

                string hash = await SystemDiscoveryService.CalculateSHA256Async(pending.AppPath);

                checkVirusTotalToolStripMenuItem.Text = "Check on VirusTotal";
                checkVirusTotalToolStripMenuItem.Enabled = true;

                if (!string.IsNullOrEmpty(hash))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"https://www.virustotal.com/gui/file/{hash}",
                        UseShellExecute = true
                    });
                }
                else
                {
                    DarkModeForms.Messenger.MessageBox("Could not calculate file hash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch { }
        }

        public class DashboardLayoutSettings
        {
            public int SplitterDistance { get; set; }
        }
    }
}