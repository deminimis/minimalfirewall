// File: MainForm.cs
using DarkModeForms;
using NetFwTypeLib;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using MinimalFirewall.Groups;
using Firewall.Traffic.ViewModels;
using MinimalFirewall.TypedObjects;
using System.Windows.Forms;
using System.ComponentModel;
using MinimalFirewall;
using System.Collections.Concurrent;

namespace MinimalFirewall
{
    public partial class MainForm : Form
    {
        #region Fields
        private readonly INetFwPolicy2 _firewallPolicy;
        private readonly FirewallDataService _dataService;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallEventListenerService _eventListenerService;
        private readonly FirewallSentryService _firewallSentryService;
        private readonly FirewallRuleService _firewallRuleService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly ForeignRuleTracker _foreignRuleTracker;
        private readonly AppSettings _appSettings;
        private readonly StartupService _startupService;
        private readonly FirewallGroupManager _groupManager;
        private readonly IconService _iconService;
        private readonly PublisherWhitelistService _whitelistService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly MainViewModel _mainViewModel;
        private readonly Queue<PendingConnectionViewModel> _popupQueue = new();
        private volatile bool _isPopupVisible = false;
        private readonly object _popupLock = new();
        private readonly DarkModeCS dm;
        private System.Threading.Timer? _autoRefreshTimer;
        private readonly Dictionary<string, System.Threading.Timer> _tabUnloadTimers = new();
        private Image? _lockedGreenIcon;
        private Image? _unlockedWhiteIcon;
        private Image? _refreshWhiteIcon;
        private ToolStripMenuItem? lockdownTrayMenuItem;
        private Icon? _defaultTrayIcon;
        private Icon? _unlockedTrayIcon;
        private Icon? _alertTrayIcon;
        private bool _isRefreshingData = false;
        private bool _isSentryServiceStarted = false;
        private readonly bool _startMinimized;
        private StatusForm? _auditStatusForm = null;
        private CancellationTokenSource? _scanCts = null;
        #endregion

        #region Native Methods
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessWorkingSetSize(IntPtr process,
            IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
        #endregion

        #region Constructor and Initialization
        public MainForm(bool startMinimized = false)
        {
            _startMinimized = startMinimized;
            InitializeComponent();

            this.Opacity = 0;
            this.ShowInTaskbar = false;

            using (Graphics g = this.CreateGraphics())
            {
                float dpiScale = g.DpiY / 96f;
                if (dpiScale > 1f)
                {
                    int newTabWidth = (int)(mainTabControl.ItemSize.Width * dpiScale);
                    int newTabHeight = (int)(mainTabControl.ItemSize.Height * dpiScale);
                    mainTabControl.ItemSize = new Size(newTabWidth, newTabHeight);
                }
            }

            this.DoubleBuffered = true;
            this.Text = "Minimal Firewall";

            _appSettings = AppSettings.Load();
            dm = new DarkModeCS(this);
            if (this.components != null)
            {
                dm.Components = this.components.Components;
            }

            try
            {
                Type? firewallPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (firewallPolicyType != null)
                {
                    _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(firewallPolicyType)!;
                }
                else
                {
                    throw new InvalidOperationException("Firewall policy type could not be retrieved.");
                }
            }
            catch (Exception ex) when (ex is COMException or InvalidOperationException)
            {
                MessageBox.Show("Could not initialize Windows Firewall policy management. The application cannot continue.\n\nError: " + ex.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            _startupService = new StartupService();
            _groupManager = new FirewallGroupManager(_firewallPolicy);
            _iconService = new IconService { ImageList = this.appIconList };
            _whitelistService = new PublisherWhitelistService();
            _firewallRuleService = new FirewallRuleService(_firewallPolicy);
            _activityLogger = new UserActivityLogger { IsEnabled = _appSettings.IsLoggingEnabled };
            _wildcardRuleService = new WildcardRuleService();
            _foreignRuleTracker = new ForeignRuleTracker();
            _dataService = new FirewallDataService(_firewallRuleService, _wildcardRuleService);
            _firewallSentryService = new FirewallSentryService(_firewallRuleService);
            var trafficMonitorViewModel = new TrafficMonitorViewModel();
            _eventListenerService = new FirewallEventListenerService(_dataService, _wildcardRuleService, () => _mainViewModel.IsLockedDown, msg => _activityLogger.LogDebug(msg), _appSettings, _whitelistService);
            _actionsService = new FirewallActionsService(_firewallRuleService, _activityLogger, _eventListenerService, _foreignRuleTracker, _firewallSentryService, _whitelistService, _firewallPolicy, _wildcardRuleService, _dataService);
            _eventListenerService.ActionsService = _actionsService;
            _backgroundTaskService = new BackgroundFirewallTaskService(_actionsService, _activityLogger, _wildcardRuleService);
            _mainViewModel = new MainViewModel(_firewallRuleService, _wildcardRuleService, _backgroundTaskService, _dataService, _firewallSentryService, _foreignRuleTracker, trafficMonitorViewModel, _eventListenerService, _appSettings, _activityLogger);
            _backgroundTaskService.QueueCountChanged += OnQueueCountChanged;
            _mainViewModel.PopupRequired += OnPopupRequired;
            _mainViewModel.DashboardActionProcessed += OnDashboardActionProcessed;

            dashboardControl1.Initialize(_mainViewModel, _appSettings, _iconService, dm, _wildcardRuleService, _actionsService, _firewallPolicy);
            rulesControl1.Initialize(_mainViewModel, _actionsService, _firewallPolicy, _wildcardRuleService, _backgroundTaskService, _iconService, _appSettings, appIconList, dm);
            auditControl1.Initialize(_mainViewModel, _foreignRuleTracker, _firewallSentryService, dm);
            groupsControl1.Initialize(_groupManager, _backgroundTaskService, dm);
            liveConnectionsControl1.Initialize(_mainViewModel.TrafficMonitorViewModel, _appSettings, _iconService, _backgroundTaskService, appIconList);

            string versionInfo = "Version " + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(3);
            _mainViewModel.SystemChangesUpdated += () => {
                UpdateUiWithChangesCount();
            };

            settingsControl1.Initialize(_appSettings, _startupService, _whitelistService, _actionsService, _activityLogger, _mainViewModel, appImageList, versionInfo, dm);

            settingsControl1.ThemeChanged += UpdateThemeAndColors;
            settingsControl1.IconVisibilityChanged += UpdateIconColumnVisibility;
            settingsControl1.DataRefreshRequested += async () => await ForceDataRefreshAsync(true);
            settingsControl1.AutoRefreshTimerChanged += SetupAutoRefreshTimer;

            SetupTrayIcon();

            lockdownButton.BringToFront();
            rescanButton.BringToFront();
        }

        private void OnQueueCountChanged(int count)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnQueueCountChanged(count));
                return;
            }

            if (count == 0)
            {
                _mainViewModel.ClearRulesCache();
                _ = RefreshRulesListAsync();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyLastWindowState();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            DarkModeCS.ExcludeFromProcessing(rescanButton);
            rescanButton.BackColor = Color.Transparent;
            DarkModeCS.ExcludeFromProcessing(lockdownButton);
            lockdownButton.BackColor = Color.Transparent;
            lockdownButton.Paint += OwnerDrawnButton_Paint;
            rescanButton.Paint += OwnerDrawnButton_Paint;

            SetupAppIcons();
            if (!_startMinimized)
            {
                await DisplayCurrentTabData();
                UpdateThemeAndColors();

                this.ShowInTaskbar = true;

                var fadeTimer = new System.Windows.Forms.Timer();
                fadeTimer.Interval = 20;
                fadeTimer.Tick += (sender, args) =>
                {
                    this.Opacity += 0.1;
                    if (this.Opacity >= 1.0)
                    {
                        fadeTimer.Stop();
                        fadeTimer.Dispose();
                        this.Opacity = 1.0;
                    }
                };
                fadeTimer.Start();

                this.Activate();
            }
            else
            {
                Hide();
                await PrepareForTrayAsync();
            }

            _actionsService.CleanupTemporaryRulesOnStartup();
            if (_mainViewModel.IsLockedDown)
            {
                AdminTaskService.SetAuditPolicy(true);
            }

            _eventListenerService.Start();

            UpdateTrayStatus();
            string versionInfo = "Version " + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(3);
            _activityLogger.LogDebug("Application Started: " + versionInfo);
            settingsControl1.LoadSettingsToUI();
            SetupAutoRefreshTimer();
            UpdateIconColumnVisibility();
        }

        private Icon CreateRecoloredIcon(Icon originalIcon, Color color)
        {
            using var bmp = originalIcon.ToBitmap();
            using var recoloredImage = RecolorImage(bmp, color);
            IntPtr hIcon = ((Bitmap)recoloredImage).GetHicon();
            try
            {
                using var newIcon = Icon.FromHandle(hIcon);
                return (Icon)newIcon.Clone();
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        private void SetupAppIcons()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("MinimalFirewall.logo.ico"))
            {
                if (stream != null)
                {
                    var icon = new Icon(stream);
                    this.Icon = icon;
                    _defaultTrayIcon = icon;
                    _unlockedTrayIcon = CreateRecoloredIcon(icon, Color.Red);
                    _alertTrayIcon = CreateRecoloredIcon(icon, Color.Orange);
                    if (notifyIcon != null)
                    {
                        notifyIcon.Icon = _mainViewModel.IsLockedDown ? _defaultTrayIcon : _unlockedTrayIcon;
                    }
                }
            }

            appImageList.ImageSize = new Size(32, 32);
            mainTabControl.ImageList = appImageList;

            Image? lockedIcon = appImageList.Images["locked.png"];
            if (lockedIcon != null)
            {
                _lockedGreenIcon = DarkModeCS.RecolorImage(lockedIcon, Color.FromArgb(0, 200, 83));
            }
            Image? unlockedIcon = appImageList.Images["unlocked.png"];
            if (unlockedIcon != null)
            {
                _unlockedWhiteIcon = DarkModeCS.RecolorImage(unlockedIcon, Color.White);
            }
            Image? refreshIcon = appImageList.Images["refresh.png"];
            if (refreshIcon != null)
            {
                _refreshWhiteIcon = DarkModeCS.RecolorImage(refreshIcon, Color.White);
            }

            lockdownButton!.Text = string.Empty;
            rescanButton.Text = string.Empty;
            lockdownButton.AutoSize = false;
            rescanButton.AutoSize = false;
            using (var g = this.CreateGraphics())
            {
                int scaledSize = (int)(40 * (g.DpiY / 96f));
                lockdownButton.Size = new Size(scaledSize, scaledSize);
                rescanButton.Size = new Size(scaledSize, scaledSize);
            }
            lockdownButton.Image = null;
            rescanButton.Image = null;
            using (var stream = assembly.GetManifestResourceStream("MinimalFirewall.logo.png"))
            {
                if (stream != null)
                {
                    logoPictureBox.Image = Image.FromStream(stream);
                }
            }
        }
        #endregion

        #region Settings and Theme

        private void UpdateThemeAndColors()
        {
            this.SuspendLayout();
            bool isDark = _appSettings.Theme == "Dark";
            dm.ColorMode = isDark ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(isDark);
            rulesControl1.ApplyThemeFixes();
            auditControl1.ApplyThemeFixes();
            settingsControl1.ApplyTheme(isDark, dm);
            settingsControl1.ApplyThemeFixes();

            rescanButton.Invalidate();
            UpdateTrayStatus();
            this.ResumeLayout(true);
            this.Refresh();
            lockdownButton.FlatAppearance.BorderColor = this.BackColor;
            rescanButton.FlatAppearance.BorderColor = this.BackColor;

            lockdownButton.BringToFront();
            rescanButton.BringToFront();
        }

        private void UpdateIconColumnVisibility()
        {
            rulesControl1.UpdateIconColumnVisibility();
            dashboardControl1.SetIconColumnVisibility(_appSettings.ShowAppIcons);
            liveConnectionsControl1.UpdateIconColumnVisibility();
        }
        #endregion

        #region Core Logic and Backend Event Handlers
        private void UpdateTrayStatus()
        {
            bool locked = _mainViewModel.IsLockedDown;
            logoPictureBox.Visible = !locked;
            dashboardControl1.Visible = locked;

            lockdownButton.Invalidate();

            if (notifyIcon != null)
            {
                if (locked)
                {
                    notifyIcon.Icon = _defaultTrayIcon;
                }
                else if (_appSettings.AlertOnForeignRules && _mainViewModel.UnseenSystemChangesCount > 0)
                {
                    notifyIcon.Icon = _alertTrayIcon;
                }
                else
                {
                    notifyIcon.Icon = _unlockedTrayIcon;
                }
            }
        }

        private void UpdateUiWithChangesCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateUiWithChangesCount));
                return;
            }

            if (_appSettings.AlertOnForeignRules && _mainViewModel.UnseenSystemChangesCount > 0)
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, _mainViewModel.UnseenSystemChangesCount);
            }
            else
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, 0);
            }
            UpdateTrayStatus();
        }

        private void OnPopupRequired(PendingConnectionViewModel pending)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnPopupRequired(pending));
                return;
            }

            bool alreadyInPopupQueue = _popupQueue.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase) && p.Direction.Equals(pending.Direction, StringComparison.OrdinalIgnoreCase));
            if (alreadyInPopupQueue)
            {
                _activityLogger.LogDebug($"Ignoring duplicate pending connection for {pending.AppPath} (in popup queue)");
                return;
            }

            if (_appSettings.IsPopupsEnabled)
            {
                lock (_popupLock)
                {
                    _popupQueue.Enqueue(pending);
                }
                BeginInvoke(new Action(ProcessNextPopup));
            }
        }

        private void ProcessNextPopup()
        {
            lock (_popupLock)
            {
                if (_isPopupVisible || _popupQueue.Count == 0)
                {
                    return;
                }

                _isPopupVisible = true;
                var pending = _popupQueue.Dequeue();

                var notifier = new NotifierForm(pending, _appSettings.Theme == "Dark");
                notifier.FormClosed += Notifier_FormClosed;
                notifier.TopMost = true;
                notifier.Show();
            }
        }

        private void Notifier_FormClosed(object? sender, FormClosedEventArgs e)
        {
            try
            {
                if (sender is not NotifierForm notifier) return;
                notifier.FormClosed -= Notifier_FormClosed;

                var pending = notifier.PendingConnection;
                var result = notifier.Result;
                _mainViewModel.PendingConnections.Remove(pending);

                if (result == NotifierForm.NotifierResult.CreateWildcard)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, pending.AppPath);
                        if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            var newRule = new WildcardRule
                            {
                                FolderPath = wildcardDialog.FolderPath,
                                ExeName = wildcardDialog.ExeName,
                                Action = wildcardDialog.FinalAction
                            };
                            _wildcardRuleService.AddRule(newRule);
                            var payload = new ApplyApplicationRulePayload
                            {
                                AppPaths = [pending.AppPath],
                                Action = newRule.Action,
                                WildcardSourcePath = newRule.FolderPath
                            };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload));
                        }

                        lock (_popupLock)
                        {
                            _isPopupVisible = false;
                        }
                        BeginInvoke(new Action(ProcessNextPopup));
                    }));
                }
                else
                {
                    var payload = new ProcessPendingConnectionPayload
                    {
                        PendingConnection = pending,
                        Decision = result.ToString(),
                        Duration = (result == NotifierForm.NotifierResult.TemporaryAllow) ?
                            notifier.TemporaryDuration : default,
                        TrustPublisher = notifier.TrustPublisher
                    };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));

                    lock (_popupLock)
                    {
                        _isPopupVisible = false;
                    }
                    BeginInvoke(new Action(ProcessNextPopup));
                }
            }
            catch (Exception)
            {
            }
            finally // Explicitly dispose the NotifierForm to ensure memory is released
            {
                if (sender is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private void OnDashboardActionProcessed(PendingConnectionViewModel processedConnection)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnDashboardActionProcessed(processedConnection));
                return;
            }

            NotifierForm? notifierToClose = null;

            lock (_popupLock)
            {
                var newQueue = new Queue<PendingConnectionViewModel>(
                    _popupQueue.Where(p =>
                        !(p.AppPath.Equals(processedConnection.AppPath, StringComparison.OrdinalIgnoreCase) &&
                          p.Direction.Equals(processedConnection.Direction, StringComparison.OrdinalIgnoreCase))
                    )
                );
                _popupQueue.Clear();
                foreach (var item in newQueue)
                {
                    _popupQueue.Enqueue(item);
                }

                if (_isPopupVisible)
                {
                    var activeNotifier = Application.OpenForms.OfType<NotifierForm>().FirstOrDefault();
                    if (activeNotifier != null)
                    {
                        var pendingInPopup = activeNotifier.PendingConnection;
                        if (pendingInPopup.AppPath.Equals(processedConnection.AppPath, StringComparison.OrdinalIgnoreCase) &&
                            pendingInPopup.Direction.Equals(processedConnection.Direction, StringComparison.OrdinalIgnoreCase))
                        {
                            notifierToClose = activeNotifier;
                        }
                    }
                }
            }

            if (notifierToClose != null)
            {
                notifierToClose.Result = NotifierForm.NotifierResult.Ignore;
                notifierToClose.Close();
            }
        }
        #endregion

        #region System Tray & Lifecycle
        private void SetupTrayIcon()
        {
            lockdownTrayMenuItem = new ToolStripMenuItem("Toggle Lockdown", null, ToggleLockdownTrayMenuItem_Click);
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(lockdownTrayMenuItem);
            contextMenu.Items.Add(new ToolStripMenuItem("Show", null, ShowWindow));
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, ExitApplication));
            contextMenu.Opening += TrayContextMenu_Opening;
            notifyIcon = new NotifyIcon(this.components)
            {
                Icon = this.Icon,
                Text = "Minimal Firewall",
                Visible = true,
                ContextMenuStrip = contextMenu
            };
            notifyIcon.DoubleClick += ShowWindow;
        }

        private void ToggleLockdownTrayMenuItem_Click(object? sender, EventArgs e)
        {
            _actionsService.ToggleLockdown();
            UpdateTrayStatus();
        }

        private void TrayContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lockdownTrayMenuItem != null)
            {
                lockdownTrayMenuItem.Text = _mainViewModel.IsLockedDown ? "Disable Lockdown" : "Enable Lockdown";
            }
        }

        private void SetupAutoRefreshTimer()
        {
            _autoRefreshTimer?.Dispose();
            if (_appSettings.AutoRefreshIntervalMinutes > 0)
            {
                var interval = TimeSpan.FromMinutes(_appSettings.AutoRefreshIntervalMinutes);
                _autoRefreshTimer = new System.Threading.Timer(_ =>
                {
                    if (this.IsDisposed || !this.IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        this.Invoke(new Action(async () =>
                        {
                            if (this.Visible && (mainTabControl.SelectedTab?.Name is "rulesTabPage"))
                            {
                                await ForceDataRefreshAsync();
                            }
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }, null, interval, interval);
                _activityLogger.LogDebug($"Auto-refresh timer set to {_appSettings.AutoRefreshIntervalMinutes} minutes.");
            }
        }

        private void ApplyLastWindowState()
        {
            if (_appSettings.WindowSize.Width > 0 && _appSettings.WindowSize.Height > 0)
            {
                this.Size = _appSettings.WindowSize;
            }

            bool isVisible = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(_appSettings.WindowLocation))
                {
                    isVisible = true;
                    break;
                }
            }
            if (isVisible)
            {
                this.Location = _appSettings.WindowLocation;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            FormWindowState savedState = (FormWindowState)_appSettings.WindowState;
            if (savedState == FormWindowState.Minimized)
            {
                savedState = FormWindowState.Normal;
            }

            this.WindowState = savedState;
        }

        private async void ShowWindow(object? sender, EventArgs e)
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;

            ApplyLastWindowState();
            this.Show();
            this.Activate();
            _eventListenerService.Start();
            if (_isSentryServiceStarted)
            {
                _firewallSentryService.Start();
            }

            SetupAutoRefreshTimer();
            await DisplayCurrentTabData();
            await RefreshRulesListAsync();
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                _appSettings.WindowLocation = this.RestoreBounds.Location;
                _appSettings.WindowSize = this.RestoreBounds.Size;
                _appSettings.WindowState = (int)FormWindowState.Maximized;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                _appSettings.WindowLocation = this.Location;
                _appSettings.WindowSize = this.Size;
                _appSettings.WindowState = (int)FormWindowState.Normal;
            }
            else
            {
                _appSettings.WindowLocation = this.RestoreBounds.Location;
                _appSettings.WindowSize = this.RestoreBounds.Size;
                _appSettings.WindowState = (int)FormWindowState.Normal;
            }

            settingsControl1.SaveSettingsFromUI();
            bool isExiting = !(_appSettings.CloseToTray && e.CloseReason == CloseReason.UserClosing);

            if (!isExiting)
            {
                e.Cancel = true;
                this.Hide();
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = true;
                }
                PrepareForTrayAsync();
            }
            else
            {
                _scanCts?.Cancel();
                foreach (var timer in _tabUnloadTimers.Values)
                {
                    timer.Dispose();
                }
                _tabUnloadTimers.Clear();
                _mainViewModel.PopupRequired -= OnPopupRequired;
                _backgroundTaskService.QueueCountChanged -= OnQueueCountChanged;
                Application.Exit();
            }
        }

        public async Task PrepareForTrayAsync()
        {
            _scanCts?.Cancel();
            _firewallSentryService.Stop();
            _mainViewModel.TrafficMonitorViewModel.StopMonitoring();
            _autoRefreshTimer?.Dispose();

            _mainViewModel.ClearRulesData();
            _mainViewModel.PendingConnections.Clear();
            _mainViewModel.SystemChanges.Clear();
            auditControl1.ApplySearchFilter();
            groupsControl1.ClearGroups();

            _dataService.ClearCaches();
            _iconService.ClearCache();
            await Task.Run(() =>
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            });
        }
        #endregion

        #region Tab Loading and Filtering
        private async Task DisplayCurrentTabData()
        {
            if (mainTabControl is null) return;
            var selectedTab = mainTabControl.SelectedTab;
            if (selectedTab == null) return;

            this.SuspendLayout();
            if (selectedTab != liveConnectionsTabPage)
            {
                liveConnectionsControl1.OnTabDeselected();
            }

            try
            {
                switch (selectedTab.Name)
                {
                    case "dashboardTabPage":
                        break;
                    case "rulesTabPage":
                        await ForceDataRefreshAsync(true);
                        break;
                    case "systemChangesTabPage":
                        await ScanForSystemChangesAsync(true);
                        break;
                    case "groupsTabPage":
                        await groupsControl1.OnTabSelectedAsync();
                        break;
                    case "liveConnectionsTabPage":
                        liveConnectionsControl1.OnTabSelected();
                        break;
                }
            }
            catch (OperationCanceledException) { }
            this.ResumeLayout(true);
        }

        public async Task ForceDataRefreshAsync(bool forceUwpScan = false, bool showStatus = true, StatusForm? statusFormInstance = null)
        {
            if (_isRefreshingData) return;
            _scanCts?.Cancel();
            _scanCts = new CancellationTokenSource();
            var token = _scanCts.Token;
            StatusForm? statusForm = null;
            try
            {
                _isRefreshingData = true;
                statusForm = statusFormInstance;
                if (showStatus && statusForm == null && this.Visible)
                {
                    statusForm = new StatusForm("Scanning firewall rules...");
                    statusForm.Show(this);
                }

                var progress = new Progress<int>(p => statusForm?.UpdateProgress(p));
                UpdateUIForRefresh(showStatus);
                _iconService.ClearCache();

                await rulesControl1.RefreshDataAsync(forceUwpScan, progress, token);

                if (token.IsCancellationRequested)
                {
                    _mainViewModel.ClearRulesData();
                    GC.Collect();
                    return;
                }
            }
            finally
            {
                if (statusForm != null && statusFormInstance == null && !statusForm.IsDisposed)
                {
                    statusForm.Close();
                }
                _isRefreshingData = false;
                UpdateUIAfterRefresh(showStatus);
            }
        }


        private async Task RefreshRulesListAsync()
        {
            try
            {
                await rulesControl1.RefreshDataAsync();
                if (this.Visible)
                {
                    await DisplayCurrentTabData();
                }
            }
            catch (OperationCanceledException) { }
        }

        private void UpdateUIForRefresh(bool showStatus)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateUIForRefresh(showStatus));
                return;
            }

            rescanButton.Text = "Refreshing...";
            rescanButton.Enabled = false;
            lockdownButton.Enabled = false;
        }

        private void UpdateUIAfterRefresh(bool showStatus)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateUIAfterRefresh(showStatus));
                return;
            }

            rescanButton.Text = "";
            rescanButton.Enabled = true;
            lockdownButton.Enabled = true;
        }

        private async Task ScanForSystemChangesAsync(bool showStatusWindow = false, IProgress<int>? progress = null, CancellationToken token = default)
        {
            if (token == default)
            {
                _scanCts?.Cancel();
                _scanCts = new CancellationTokenSource();
                token = _scanCts.Token;
            }

            try
            {
                if (showStatusWindow && this.Visible)
                {
                    _auditStatusForm = new StatusForm("Scanning for system changes...");
                    var progressIndicator = new Progress<int>(p => _auditStatusForm?.UpdateProgress(p));
                    _auditStatusForm.Show(this);
                    progress = progressIndicator;
                }
                await _mainViewModel.ScanForSystemChangesAsync(token, progress);
            }
            finally
            {
                if (token.IsCancellationRequested)
                {
                    _mainViewModel.SystemChanges.Clear();
                    auditControl1.ApplySearchFilter();
                    GC.Collect();
                }
                if (_auditStatusForm?.IsDisposed == false) _auditStatusForm?.Close();
                _auditStatusForm = null;
            }
        }
        #endregion

        #region UI Event Handlers
        private async void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTab = mainTabControl.SelectedTab;
            if (selectedTab == null) return;

            if (_tabUnloadTimers.TryGetValue(selectedTab.Name, out var timer))
            {
                timer.Dispose();
                _tabUnloadTimers.Remove(selectedTab.Name);
            }

            if (selectedTab != liveConnectionsTabPage)
            {
                liveConnectionsControl1.OnTabDeselected();
            }

            try
            {
                switch (selectedTab.Name)
                {
                    case "dashboardTabPage":
                        break;
                    case "rulesTabPage":
                        await ForceDataRefreshAsync(true);
                        break;
                    case "systemChangesTabPage":
                        if (!_isSentryServiceStarted)
                        {
                            _firewallSentryService.Start();
                            _isSentryServiceStarted = true;
                        }
                        await ScanForSystemChangesAsync(true);
                        break;
                    case "groupsTabPage":
                        await groupsControl1.OnTabSelectedAsync();
                        break;
                    case "liveConnectionsTabPage":
                        liveConnectionsControl1.OnTabSelected();
                        break;
                }
            }
            catch (OperationCanceledException) { }
        }

        private void MainTabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == null) return;
            _scanCts?.Cancel();

            var tabsToUnload = new[] { "rulesTabPage", "systemChangesTabPage", "groupsTabPage" };
            if (tabsToUnload.Contains(e.TabPage.Name))
            {
                if (_tabUnloadTimers.TryGetValue(e.TabPage.Name, out var existingTimer))
                {
                    existingTimer.Dispose();
                }

                var timer = new System.Threading.Timer(UnloadTabData, e.TabPage.Name, 30000, Timeout.Infinite);
                _tabUnloadTimers[e.TabPage.Name] = timer;
            }
        }

        private void UnloadTabData(object? state)
        {
            if (state is not string tabName) return;
            this.BeginInvoke(new Action(() =>
            {
                if (mainTabControl.SelectedTab != null && mainTabControl.SelectedTab.Name == tabName)
                {
                    return;
                }

                switch (tabName)
                {
                    case "rulesTabPage":
                        _mainViewModel.ClearRulesData();
                        break;
                    case "systemChangesTabPage":
                        if (_auditStatusForm?.IsDisposed == false) _auditStatusForm?.Close();
                        _auditStatusForm = null;
                        _mainViewModel.SystemChanges.Clear();
                        auditControl1.ApplySearchFilter();
                        UpdateUiWithChangesCount();
                        _firewallSentryService.Stop();
                        _isSentryServiceStarted = false;
                        break;
                    case "groupsTabPage":
                        groupsControl1.ClearGroups();
                        break;
                }

                GC.Collect();
                if (_tabUnloadTimers.TryGetValue(tabName, out var timer))
                {
                    timer.Dispose();
                    _tabUnloadTimers.Remove(tabName);
                }
            }));
        }


        private async void RescanButton_Click(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab != null)
            {
                _activityLogger.LogDebug($"Rescan triggered for tab: {mainTabControl.SelectedTab.Text}");
            }

            try
            {
                if (mainTabControl.SelectedTab == systemChangesTabPage)
                {
                    await ScanForSystemChangesAsync(true);
                }
                else
                {
                    _mainViewModel.ClearRulesCache();
                    await ForceDataRefreshAsync(true);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void ToggleLockdownButton_Click(object sender, EventArgs e)
        {
            bool wasLocked = _mainViewModel.IsLockedDown;
            _actionsService.ToggleLockdown();
            UpdateTrayStatus();

            bool isNowLocked = _mainViewModel.IsLockedDown;
            if (wasLocked && !isNowLocked)
            {
                DismissAllPopups();
            }
        }

        private void DismissAllPopups()
        {
            foreach (var form in Application.OpenForms.OfType<NotifierForm>().ToList())
            {
                form.Close();
            }

            lock (_popupLock)
            {
                _popupQueue.Clear();
                _isPopupVisible = false;
            }
        }

        private void ArrowPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color arrowColor = (_appSettings.Theme == "Dark") ? Color.White : Color.Black;
            using (var arrowPen = new Pen(arrowColor, 2.5f))
            {
                arrowPen.EndCap = LineCap.ArrowAnchor;
                Point startPoint = new(arrowPictureBox.Width - 1, 0);
                Point endPoint = new(5, arrowPictureBox.Height - 5);
                Point controlPoint1 = new(arrowPictureBox.Width - 5, arrowPictureBox.Height / 2);
                Point controlPoint2 = new(arrowPictureBox.Width / 2, arrowPictureBox.Height);
                e.Graphics.DrawBezier(arrowPen, startPoint, controlPoint1, controlPoint2, endPoint);
            }
        }

        private void LockdownButton_MouseEnter(object? sender, EventArgs e)
        {
            lockdownButton.Invalidate();
        }

        private void LockdownButton_MouseLeave(object? sender, EventArgs e)
        {
            lockdownButton.Invalidate();
        }

        private void RescanButton_MouseEnter(object? sender, EventArgs e)
        {
            rescanButton.Invalidate();
        }

        private void RescanButton_MouseLeave(object? sender, EventArgs e)
        {
            rescanButton.Invalidate();
        }

        private static Image RecolorImage(Image sourceImage, Color newColor)
        {
            var newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                float r = newColor.R / 255f;
                float g_ = newColor.G / 255f;
                float b = newColor.B / 255f;
                var colorMatrix = new ColorMatrix(
                [
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 1, 0],
                    [r, g_, b, 0, 1]
                ]);
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                                0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        private void OwnerDrawnButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button) return;
            e.Graphics.Clear(this.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Image? imageToDraw = null;
            if (button.Name == "lockdownButton")
            {
                imageToDraw = _mainViewModel.IsLockedDown ?
                    _lockedGreenIcon : ((_appSettings.Theme == "Dark") ? _unlockedWhiteIcon : appImageList.Images["unlocked.png"]);
            }
            else if (button.Name == "rescanButton")
            {
                if (_isRefreshingData)
                {
                    TextRenderer.DrawText(e.Graphics, "...", button.Font, button.ClientRectangle, button.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    return;
                }
                imageToDraw = (_appSettings.Theme == "Dark") ?
                    _refreshWhiteIcon : appImageList.Images["refresh.png"];
            }

            if (imageToDraw != null)
            {
                int imgX = (button.ClientSize.Width - imageToDraw.Width) / 2;
                int imgY = (button.ClientSize.Height - imageToDraw.Height) / 2;
                e.Graphics.DrawImage(imageToDraw, imgX, imgY, imageToDraw.Width, imageToDraw.Height);
            }

            if (button.ClientRectangle.Contains(button.PointToClient(Cursor.Position)))
            {
                using var p = new Pen(dm.OScolors.Accent, 2);
                e.Graphics.DrawRectangle(p, 0, 0, button.Width - 1, button.Height - 1);
            }
        }
        #endregion
    }
}
