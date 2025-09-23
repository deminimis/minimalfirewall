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
using System.Collections.Specialized;
using MinimalFirewall.TypedObjects;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.ComponentModel;

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
        private System.Threading.Timer? _sentryRefreshDebounceTimer;
        private List<FirewallRuleChange> _masterSystemChanges = [];
        private bool _systemChangesLoaded = false;
        private Image? _lockedGreenIcon;
        private Image? _unlockedWhiteIcon;
        private Image? _refreshWhiteIcon;
        private ListViewItem? _hoveredItem = null;
        private readonly TrafficMonitorViewModel _trafficMonitorViewModel;
        private int _rulesSortColumn = -1;
        private int _dashboardSortColumn = -1;
        private int _systemChangesSortColumn = -1;
        private int _groupsSortColumn = -1;
        private int _liveConnectionsSortColumn = -1;
        private SortOrder _rulesSortOrder = SortOrder.None;
        private SortOrder _dashboardSortOrder = SortOrder.None;
        private SortOrder _systemChangesSortOrder = SortOrder.None;
        private SortOrder _groupsSortOrder = SortOrder.None;
        private SortOrder _liveConnectionsSortOrder = SortOrder.None;
        private ToolStripMenuItem? lockdownTrayMenuItem;
        private Icon? _defaultTrayIcon;
        private Icon? _unlockedTrayIcon;
        private int _unseenSystemChangesCount = 0;
        private Icon? _alertTrayIcon;
        private bool _isRefreshingData = false;
        private bool _isSentryServiceStarted = false;
        private bool _uwpAppsScanned = false;

        private List<AggregatedRuleViewModel> _virtualRulesData = [];
        private List<AggregatedRuleViewModel> _allAggregatedRules = [];
        private List<TcpConnectionViewModel> _virtualLiveConnectionsData = [];
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
        public MainForm()
        {
            InitializeComponent();
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
            rulesListView.Resize += ListView_Resize;
            systemChangesListView.Resize += ListView_Resize;
            groupsListView.Resize += ListView_Resize;
            liveConnectionsListView.Resize += ListView_Resize;

            this.Opacity = 0;
            this.ShowInTaskbar = false;
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(rulesListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(groupsListView, true);
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(liveConnectionsListView, true);
            this.Text = "Minimal Firewall";
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

            _appSettings = AppSettings.Load();
            _startupService = new StartupService();
            _groupManager = new FirewallGroupManager(_firewallPolicy);
            _iconService = new IconService { ImageList = this.appIconList };
            _whitelistService = new PublisherWhitelistService();
            versionLabel.Text = "Version " + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(3);
            _firewallRuleService = new FirewallRuleService(_firewallPolicy);
            _activityLogger = new UserActivityLogger { IsEnabled = _appSettings.IsLoggingEnabled };
            _wildcardRuleService = new WildcardRuleService();
            _foreignRuleTracker = new ForeignRuleTracker();
            _dataService = new FirewallDataService(_firewallRuleService, _wildcardRuleService);
            _firewallSentryService = new FirewallSentryService(_firewallRuleService);
            _trafficMonitorViewModel = new TrafficMonitorViewModel();
            _eventListenerService = new FirewallEventListenerService(_dataService, _wildcardRuleService, IsLockedDown, msg => _activityLogger.LogDebug(msg), _appSettings, _whitelistService);
            _actionsService = new FirewallActionsService(_firewallRuleService, _activityLogger, _eventListenerService, _foreignRuleTracker, _firewallSentryService, _whitelistService, _firewallPolicy);
            _eventListenerService.ActionsService = _actionsService;
            _backgroundTaskService = new BackgroundFirewallTaskService(_actionsService, _activityLogger, _wildcardRuleService);
            _mainViewModel = new MainViewModel(_firewallRuleService, _wildcardRuleService, _backgroundTaskService);

            _backgroundTaskService.QueueCountChanged += OnQueueCountChanged;
            _firewallSentryService.RuleSetChanged += OnRuleSetChanged;
            _eventListenerService.PendingConnectionDetected += OnPendingConnectionDetected;
            _trafficMonitorViewModel.ActiveConnections.CollectionChanged += ActiveConnections_CollectionChanged;
            rulesListView.SmallImageList = this.appIconList;
            liveConnectionsListView.SmallImageList = this.appIconList;

            systemChangesListView.ViewMode = ButtonListView.Mode.Audit;
            systemChangesListView.DarkMode = dm;

            systemChangesListView.AcceptClicked += SystemChanges_AcceptClicked;
            systemChangesListView.DeleteClicked += SystemChanges_DeleteClicked;
            systemChangesListView.IgnoreClicked += SystemChanges_IgnoreClicked;

            dashboardControl1.Initialize(_mainViewModel, _appSettings, _iconService, dm, _wildcardRuleService, _actionsService, _firewallPolicy);

            SetupTrayIcon();
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
                _ = RefreshRulesListAsync();
            }
        }

        private void ListView_Resize(object? sender, EventArgs e)
        {
            if (sender is not ListView listView || listView.Columns.Count == 0 || listView.ClientSize.Width == 0)
            {
                return;
            }

            ColumnHeader lastColumn = listView.Columns[^1];

            int totalWidthOfOtherColumns = 0;
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                totalWidthOfOtherColumns += listView.Columns[i].Width;
            }

            int newLastColumnWidth = listView.ClientSize.Width - totalWidthOfOtherColumns;
            const int minColumnWidth = 100;
            if (newLastColumnWidth < minColumnWidth)
            {
                newLastColumnWidth = minColumnWidth;
            }

            lastColumn.Width = newLastColumnWidth;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyLastWindowState();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            using var statusForm = new StatusForm("Scanning firewall rules...");
            statusForm.Show(this);
            Application.DoEvents();

            DarkModeCS.ExcludeFromProcessing(rescanButton);
            rescanButton.BackColor = Color.Transparent;
            DarkModeCS.ExcludeFromProcessing(lockdownButton);
            lockdownButton.BackColor = Color.Transparent;
            lockdownButton.Paint += OwnerDrawnButton_Paint;
            rescanButton.Paint += OwnerDrawnButton_Paint;

            SetupAppIcons();
            _sentryRefreshDebounceTimer = new System.Threading.Timer(DebouncedSentryRefresh, null, Timeout.Infinite, Timeout.Infinite);

            await ForceDataRefreshAsync(showStatus: false);

            await DisplayCurrentTabData();
            UpdateThemeAndColors();

            statusForm.Close();

            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Activate();

            _actionsService.CleanupTemporaryRulesOnStartup();
            if (IsLockedDown())
            {
                AdminTaskService.SetAuditPolicy(true);
            }

            _eventListenerService.Start();

            UpdateTrayStatus();
            _activityLogger.LogDebug("Application Started: " + versionLabel.Text);
            LoadSettingsToUI();
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
                        notifyIcon.Icon = IsLockedDown() ? _defaultTrayIcon : _unlockedTrayIcon;
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
        private void DarkModeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
            UpdateThemeAndColors();
        }

        private void UpdateThemeAndColors()
        {
            this.SuspendLayout();
            bool isDark = _appSettings.Theme == "Dark";
            dm.ColorMode = isDark ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;
            dm.ApplyTheme(isDark);
            if (isDark)
            {
                createRuleButton.ForeColor = Color.White;
                advancedRuleButton.ForeColor = Color.White;
            }

            var linkColor = isDark ? Color.SkyBlue : SystemColors.HotTrack;
            helpLink.LinkColor = linkColor;
            reportProblemLink.LinkColor = linkColor;
            forumLink.LinkColor = linkColor;
            coffeeLinkLabel.LinkColor = linkColor;

            helpLink.VisitedLinkColor = linkColor;
            reportProblemLink.VisitedLinkColor = linkColor;
            forumLink.VisitedLinkColor = linkColor;
            coffeeLinkLabel.VisitedLinkColor = linkColor;

            Image? coffeeImage = appImageList.Images["coffee.png"];
            if (coffeeImage != null)
            {
                Color coffeeColor = isDark ? Color.LightGray : Color.Black;
                Image? oldImage = coffeePictureBox.Image;
                coffeePictureBox.Image = DarkModeCS.RecolorImage(coffeeImage, coffeeColor);
                oldImage?.Dispose();
            }

            rescanButton.Invalidate();
            UpdateTrayStatus();
            this.ResumeLayout(true);
            this.Refresh();
            lockdownButton.FlatAppearance.BorderColor = this.BackColor;
            rescanButton.FlatAppearance.BorderColor = this.BackColor;
        }

        private void LoadSettingsToUI()
        {
            closeToTraySwitch.Checked = _appSettings.CloseToTray;
            startOnStartupSwitch.Checked = _appSettings.StartOnSystemStartup;
            darkModeSwitch.Checked = _appSettings.Theme == "Dark";
            popupsSwitch.Checked = _appSettings.IsPopupsEnabled;
            loggingSwitch.Checked = _appSettings.IsLoggingEnabled;
            autoRefreshTextBox.Text = _appSettings.AutoRefreshIntervalMinutes.ToString();
            trafficMonitorSwitch.Checked = _appSettings.IsTrafficMonitorEnabled;
            showAppIconsSwitch.Checked = _appSettings.ShowAppIcons;
            autoAllowSystemTrustedCheck.Checked = _appSettings.AutoAllowSystemTrusted;
            auditAlertsSwitch.Checked = _appSettings.AlertOnForeignRules;
            managePublishersButton.Enabled = true;
        }

        private void SaveSettingsFromUI()
        {
            _appSettings.CloseToTray = closeToTraySwitch.Checked;
            _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
            _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
            _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            _appSettings.IsLoggingEnabled = loggingSwitch.Checked;
            if (int.TryParse(autoRefreshTextBox.Text, out int val) && val >= 1)
            {
                _appSettings.AutoRefreshIntervalMinutes = val;
            }
            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            _appSettings.AutoAllowSystemTrusted = autoAllowSystemTrustedCheck.Checked;
            _appSettings.AlertOnForeignRules = auditAlertsSwitch.Checked;

            _startupService.SetStartup(_appSettings.StartOnSystemStartup);
            _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;
            if (_appSettings.IsTrafficMonitorEnabled)
            {
                _trafficMonitorViewModel.StartMonitoring();
            }
            else
            {
                _trafficMonitorViewModel.StopMonitoring();
            }

            UpdateIconColumnVisibility();
            _appSettings.Save();
        }

        private void managePublishersButton_Click(object sender, EventArgs e)
        {
            using var form = new ManagePublishersForm(_whitelistService);
            form.ShowDialog(this);
        }

        private void PopupsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            }
        }

        private void TrafficMonitorSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
            liveConnectionsListView.Visible = _appSettings.IsTrafficMonitorEnabled;
            liveConnectionsDisabledLabel.Visible = !_appSettings.IsTrafficMonitorEnabled;

            if (_appSettings.IsTrafficMonitorEnabled)
            {
                if (mainTabControl.SelectedTab == liveConnectionsTabPage)
                {
                    _trafficMonitorViewModel.StartMonitoring();
                }
            }
            else
            {
                _trafficMonitorViewModel.StopMonitoring();
            }
        }

        private void ShowAppIconsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
            UpdateIconColumnVisibility();
            DisplayCurrentTabData();
        }

        private void UpdateIconColumnVisibility()
        {
            advIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
            dashboardControl1.SetIconColumnVisibility(_appSettings.ShowAppIcons);
            liveIconColumn.Width = _appSettings.ShowAppIcons ? 32 : 0;
        }
        #endregion

        #region Core Logic and Backend Event Handlers
        public bool IsLockedDown() => _firewallRuleService.GetDefaultOutboundAction() == NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        private void UpdateTrayStatus()
        {
            bool locked = IsLockedDown();
            logoPictureBox.Visible = !locked;
            dashboardControl1.Visible = locked;

            lockdownButton.Invalidate();

            if (notifyIcon != null)
            {
                if (locked)
                {
                    notifyIcon.Icon = _defaultTrayIcon;
                }
                else if (_appSettings.AlertOnForeignRules && _unseenSystemChangesCount > 0)
                {
                    notifyIcon.Icon = _alertTrayIcon;
                }
                else
                {
                    notifyIcon.Icon = _unlockedTrayIcon;
                }
            }
        }

        private void OnRuleSetChanged()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(OnRuleSetChanged));
                return;
            }

            OnRuleSetChanged_LiveUpdate();
        }

        private void OnRuleSetChanged_LiveUpdate()
        {
            if (!_appSettings.AlertOnForeignRules)
            {
                _systemChangesLoaded = false;
                return;
            }

            if (mainTabControl.SelectedTab == systemChangesTabPage && this.ContainsFocus)
            {
                _systemChangesLoaded = false;
                return;
            }

            _activityLogger.LogDebug("Sentry: Firewall rule change detected. Debouncing for 1 second.");
            _sentryRefreshDebounceTimer?.Change(1000, Timeout.Infinite);
        }

        private async void DebouncedSentryRefresh(object? state)
        {
            _activityLogger.LogDebug("Sentry: Debounce timer elapsed. Checking for foreign rules.");
            var changes = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker));

            if (this.IsDisposed || !this.IsHandleCreated) return;
            this.Invoke(() =>
            {
                _masterSystemChanges = changes;
                _unseenSystemChangesCount = changes.Count;
                _systemChangesLoaded = true;
                UpdateUiWithChangesCount();
            });
        }

        private void UpdateUiWithChangesCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateUiWithChangesCount));
                return;
            }

            if (_appSettings.AlertOnForeignRules && _unseenSystemChangesCount > 0)
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, _unseenSystemChangesCount);
            }
            else
            {
                systemChangesTabPage.Text = "Audit";
                dm.SetNotificationCount(systemChangesTabPage, 0);
            }
            UpdateTrayStatus();
        }

        private void OnPendingConnectionDetected(PendingConnectionViewModel pending)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnPendingConnectionDetected(pending));
                return;
            }

            bool alreadyInPopupQueue = _popupQueue.Any(p => p.AppPath.Equals(pending.AppPath, StringComparison.OrdinalIgnoreCase));
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
            else
            {
                _mainViewModel.AddPendingConnection(pending);
            }
        }

        private void ActiveConnections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedTab == liveConnectionsTabPage)
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
                var payload = new ProcessPendingConnectionPayload
                {
                    PendingConnection = pending,
                    Decision = result.ToString(),
                    Duration = (result == NotifierForm.NotifierResult.TemporaryAllow) ? notifier.TemporaryDuration : default,
                    TrustPublisher = notifier.TrustPublisher
                };
                if (result == NotifierForm.NotifierResult.CreateWildcard)
                {
                    payload.Decision = "Ignore";
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));
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
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AddWildcardRule, newRule));
                        }
                    }));
                }
                else
                {
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ProcessPendingConnection, payload));
                }

                lock (_popupLock)
                {
                    _isPopupVisible = false;
                }

                BeginInvoke(new Action(ProcessNextPopup));
            }
            catch (Exception)
            {
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
                lockdownTrayMenuItem.Text = IsLockedDown() ? "Disable Lockdown" : "Enable Lockdown";
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
            ApplyLastWindowState();
            this.Show();
            this.Activate();
            _eventListenerService.Start();
            if (_isSentryServiceStarted)
            {
                _firewallSentryService.Start();
            }

            if (_appSettings.IsTrafficMonitorEnabled)
            {
                _trafficMonitorViewModel.StartMonitoring();
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

            SaveSettingsFromUI();
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
                _firewallSentryService.RuleSetChanged -= OnRuleSetChanged;
                _eventListenerService.PendingConnectionDetected -= OnPendingConnectionDetected;
                _trafficMonitorViewModel.ActiveConnections.CollectionChanged -= ActiveConnections_CollectionChanged;
                _backgroundTaskService.QueueCountChanged -= OnQueueCountChanged;
                Application.Exit();
            }
        }

        public async Task PrepareForTrayAsync()
        {
            _firewallSentryService.Stop();
            _trafficMonitorViewModel.StopMonitoring();
            _autoRefreshTimer?.Dispose();

            _mainViewModel.PendingConnections.Clear();
            _masterSystemChanges.Clear();

            _virtualRulesData.Clear();
            _virtualLiveConnectionsData.Clear();
            rulesListView.VirtualListSize = 0;
            systemChangesListView.Items.Clear();
            liveConnectionsListView.VirtualListSize = 0;
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
                _trafficMonitorViewModel.StopMonitoring();
            }

            switch (selectedTab.Name)
            {
                case "dashboardTabPage":
                    break;
                case "rulesTabPage":
                    await DisplayRulesAsync();
                    break;
                case "systemChangesTabPage":
                    ApplySystemChangesSearchFilter();
                    break;
                case "groupsTabPage":
                    await DisplayGroupsAsync();
                    break;
                case "liveConnectionsTabPage":
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
                    break;
            }
            this.ResumeLayout(true);
        }

        private async Task ForceDataRefreshAsync(bool forceUwpScan = false, bool showStatus = true)
        {
            if (_isRefreshingData) return;
            try
            {
                _isRefreshingData = true;
                UpdateUIForRefresh(showStatus);

                await RefreshRulesListAsync();
                _systemChangesLoaded = false;
                if (this.Visible && mainTabControl.SelectedTab == systemChangesTabPage)
                {
                    await ScanForSystemChangesAsync(false);
                }
            }
            finally
            {
                _isRefreshingData = false;
                UpdateUIAfterRefresh(showStatus);
            }
        }

        private async Task RefreshRulesListAsync()
        {
            _allAggregatedRules = await _dataService.GetAggregatedRulesAsync();
            if (this.Visible)
            {
                await DisplayCurrentTabData();
            }
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
            createRuleButton.Enabled = false;
            advancedRuleButton.Enabled = false;
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
            createRuleButton.Enabled = true;
            advancedRuleButton.Enabled = true;
            lockdownButton.Enabled = true;
        }

        private async Task DisplayRulesAsync()
        {
            ApplyRulesFilters(_allAggregatedRules);
            await Task.CompletedTask;
        }

        private async Task DisplayGroupsAsync()
        {
            if (groupsListView is null) return;
            var groups = await Task.Run(() => _groupManager.GetAllGroups());
            groupsListView.BeginUpdate();
            groupsListView.Items.Clear();
            foreach (var group in groups)
            {
                var item = new ListViewItem(new[] { group.Name, "" })
                {
                    Tag = group,
                };
                groupsListView.Items.Add(item);
            }
            groupsListView.EndUpdate();
        }

        private void ApplyRulesFilters(List<AggregatedRuleViewModel> rules)
        {
            var enabledTypes = new HashSet<RuleType>();
            if (advFilterProgramCheck.Checked) enabledTypes.Add(RuleType.Program);
            if (advFilterServiceCheck.Checked) enabledTypes.Add(RuleType.Service);
            if (advFilterUwpCheck.Checked) enabledTypes.Add(RuleType.UWP);
            if (advFilterWildcardCheck.Checked) enabledTypes.Add(RuleType.Wildcard);
            if (advFilterAdvancedCheck.Checked) enabledTypes.Add(RuleType.Advanced);

            IEnumerable<AggregatedRuleViewModel> filteredRules = rules;
            if (enabledTypes.Count > 0)
            {
                filteredRules = rules.Where(r => enabledTypes.Contains(r.Type));
            }

            if (!string.IsNullOrWhiteSpace(rulesSearchTextBox.Text))
            {
                filteredRules = filteredRules.Where(r =>
                    r.Name.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    r.ApplicationName.Contains(rulesSearchTextBox.Text, StringComparison.OrdinalIgnoreCase));
            }

            if (_rulesSortOrder != SortOrder.None && _rulesSortColumn != -1)
            {
                Func<AggregatedRuleViewModel, object> keySelector = GetRuleKeySelector(_rulesSortColumn);
                if (_rulesSortOrder == SortOrder.Ascending)
                {
                    filteredRules = filteredRules.OrderBy(keySelector);
                }
                else
                {
                    filteredRules = filteredRules.OrderByDescending(keySelector);
                }
            }

            _virtualRulesData = filteredRules.ToList();
            rulesListView.VirtualListSize = _virtualRulesData.Count;
            rulesListView.Invalidate();
        }

        private async Task ScanForSystemChangesAsync(bool showStatusWindow = false)
        {
            StatusForm? statusWindow = null;
            if (showStatusWindow && this.Visible)
            {
                statusWindow = new StatusForm("Scanning for system changes...");
                statusWindow.Show(this);
                Application.DoEvents();
            }

            _masterSystemChanges = await Task.Run(() => _firewallSentryService.CheckForChanges(_foreignRuleTracker));
            _systemChangesLoaded = true;
            ApplySystemChangesSearchFilter();
            statusWindow?.Close();
        }

        private void ApplySystemChangesSearchFilter()
        {
            if (systemChangesListView is null) return;
            systemChangesListView.BeginUpdate();
            systemChangesListView.Items.Clear();
            string searchText = auditSearchTextBox.Text;

            var filteredChanges = string.IsNullOrWhiteSpace(searchText)
                ? _masterSystemChanges
                : _masterSystemChanges.Where(c => c.Rule?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                                                   c.Rule?.ApplicationName.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
            foreach (var change in filteredChanges)
            {
                var item = new ListViewItem(
                    new[]
                    {
                        "",
                        change.Rule?.Name ?? "N/A",
                        change.Rule?.Status,
                        change.Rule?.Direction.ToString(),
                        change.Rule?.ProtocolName,
                        change.Rule?.ApplicationName,
                        change.Rule?.RemoteAddresses.Any() == true ? string.Join(",", change.Rule.RemoteAddresses) : "Any",
                        change.Rule?.Description
                    })
                {
                    Tag = change
                };
                systemChangesListView.Items.Add(item);
            }
            systemChangesListView.EndUpdate();
        }
        #endregion

        #region UI Event Handlers
        private async void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedTab = mainTabControl.SelectedTab;
            if (selectedTab == null) return;

            if (selectedTab != liveConnectionsTabPage)
            {
                _trafficMonitorViewModel.StopMonitoring();
            }

            switch (selectedTab.Name)
            {
                case "dashboardTabPage":
                    break;
                case "rulesTabPage":
                    await DisplayRulesAsync();
                    break;
                case "systemChangesTabPage":
                    if (!_isSentryServiceStarted)
                    {
                        _firewallSentryService.Start();
                        _isSentryServiceStarted = true;
                    }
                    if (!_systemChangesLoaded)
                    {
                        await ScanForSystemChangesAsync(true);
                    }
                    break;
                case "groupsTabPage":
                    await DisplayGroupsAsync();
                    break;
                case "liveConnectionsTabPage":
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
                    break;
            }
        }

        private async void RescanButton_Click(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab != null)
            {
                _activityLogger.LogDebug($"Rescan triggered for tab: {mainTabControl.SelectedTab.Text}");
            }
            if (mainTabControl.SelectedTab == systemChangesTabPage)
            {
                await ScanForSystemChangesAsync(true);
            }
            else
            {
                await ForceDataRefreshAsync(true);
            }
        }

        private void ToggleLockdownButton_Click(object sender, EventArgs e)
        {
            bool wasLocked = IsLockedDown();
            _actionsService.ToggleLockdown();
            UpdateTrayStatus();

            bool isNowLocked = IsLockedDown();
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

        private void CreateRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new AddRuleSelectionForm(_actionsService, _wildcardRuleService);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
            }
        }

        private void AdvancedRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
            }
        }

        private async void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            await DisplayCurrentTabData();
        }

        private void OpenFirewallButton_Click(object sender, EventArgs e)
        {
            try
            {
                string wfPath = Path.Combine(Environment.SystemDirectory, "wf.msc");
                var startInfo = new ProcessStartInfo(wfPath)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
            {
                Messenger.MessageBox($"Could not open Windows Firewall console.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel { Tag: string url }) return;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeeLink_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.buymeacoffee.com/deminimis") { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeePictureBox_MouseEnter(object? sender, EventArgs e)
        {
        }

        private void CoffeePictureBox_MouseLeave(object? sender, EventArgs e)
        {
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

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/deminimis/minimalfirewall/releases") { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void RebuildBaselineButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("This will clear all acknowledged rules and rebuild the firewall baseline from its current state. Are you sure?", "Rebuild Baseline", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _foreignRuleTracker.Clear();
            _firewallSentryService.CreateBaseline();
            await ScanForSystemChangesAsync();
            _activityLogger.LogChange("Sentry", "Firewall baseline rebuilt.");
        }

        private async void AdvFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (advFilterUwpCheck.Checked && !_uwpAppsScanned)
            {
                await ForceDataRefreshAsync(forceUwpScan: true, showStatus: true);
                _uwpAppsScanned = true;
            }
            await DisplayRulesAsync();
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
        #endregion

        #region DPI Scaling Helper
        private int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));
        #endregion

        #region ListView Custom Drawing & Virtual Mode
        private void ListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (sender is not ListView listView) return;
            if (listView.Name == "rulesListView")
            {
                if (e.ItemIndex >= 0 && e.ItemIndex < _virtualRulesData.Count)
                {
                    var rule = _virtualRulesData[e.ItemIndex];
                    int iconIndex = _appSettings.ShowAppIcons && !string.IsNullOrEmpty(rule.ApplicationName)
                                    ? _iconService.GetIconIndex(rule.ApplicationName) : -1;

                    var item = new ListViewItem("", iconIndex) { Tag = rule };
                    item.SubItems.AddRange(new[]
                    {
                        rule.Name,
                        rule.IsEnabled.ToString(),
                        rule.Status,
                        rule.Direction.ToString(),
                        rule.ProtocolName,
                        rule.LocalPorts.Count > 0 ? string.Join(",", rule.LocalPorts) : "Any",
                        rule.RemotePorts.Count > 0 ? string.Join(",", rule.RemotePorts) : "Any",
                        rule.LocalAddresses.Count > 0 ? string.Join(",", rule.LocalAddresses) : "Any",
                        rule.RemoteAddresses.Count > 0 ? string.Join(",", rule.RemoteAddresses) : "Any",
                        rule.ApplicationName,
                        rule.ServiceName,
                        rule.Profiles,
                        rule.Grouping,
                        rule.Description
                    });
                    e.Item = item;
                }
            }
            else if (listView.Name == "liveConnectionsListView")
            {
                if (e.ItemIndex >= 0 && e.ItemIndex < _virtualLiveConnectionsData.Count)
                {
                    var connection = _virtualLiveConnectionsData[e.ItemIndex];
                    int iconIndex = _appSettings.ShowAppIcons && !string.IsNullOrEmpty(connection.ProcessPath)
                                    ? _iconService.GetIconIndex(connection.ProcessPath) : -1;

                    var item = new ListViewItem("", iconIndex) { Tag = connection };
                    item.SubItems.AddRange(new[] { connection.ProcessName, connection.RemoteAddress, connection.RemotePort.ToString() });
                    e.Item = item;
                }
            }
        }

        private void RulesListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void RulesListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
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
                string statusText = string.Empty;
                if (e.Item.Tag is AdvancedRuleViewModel advancedRule)
                {
                    statusText = advancedRule.Status;
                }
                else if (e.Item.Tag is AggregatedRuleViewModel aggregatedRule)
                {
                    statusText = aggregatedRule.Status;
                }

                bool isAllow = statusText.Contains("Allow", StringComparison.OrdinalIgnoreCase);
                bool isBlock = statusText.Contains("Block", StringComparison.OrdinalIgnoreCase);

                if (isAllow && isBlock)
                {
                    backColor = Color.FromArgb(255, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isAllow)
                {
                    backColor = Color.FromArgb(204, 255, 204);
                    foreColor = Color.Black;
                }
                else if (isBlock)
                {
                    backColor = Color.FromArgb(255, 204, 204);
                    foreColor = Color.Black;
                }
                else
                {
                    backColor = e.Item.ListView.BackColor;
                    foreColor = e.Item.ListView.ForeColor;
                }
            }

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
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

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ListView listView) return;
            ListViewItem? itemUnderMouse = listView.GetItemAt(e.X, e.Y);

            if (_hoveredItem != itemUnderMouse)
            {
                if (_hoveredItem != null)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
                _hoveredItem = itemUnderMouse;
                if (_hoveredItem != null)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
            }
        }

        private void ListView_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredItem != null)
            {
                if (sender is ListView listView)
                {
                    listView.Invalidate(_hoveredItem.Bounds);
                }
                _hoveredItem = null;
            }
        }
        #endregion

        #region ButtonListView Event Handlers
        private void SystemChanges_AcceptClicked(object? sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is FirewallRuleChange change)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptForeignRule, payload));
                systemChangesListView.Items.Remove(e.Item);
                _masterSystemChanges.Remove(change);
                _unseenSystemChangesCount = _masterSystemChanges.Count;
                UpdateUiWithChangesCount();
            }
        }

        private void SystemChanges_DeleteClicked(object? sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is FirewallRuleChange change)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteForeignRule, payload));
                systemChangesListView.Items.Remove(e.Item);
                _masterSystemChanges.Remove(change);
                _unseenSystemChangesCount = _masterSystemChanges.Count;
                UpdateUiWithChangesCount();
            }
        }

        private void SystemChanges_IgnoreClicked(object? sender, ListViewItemEventArgs e)
        {
            if (e.Item.Tag is FirewallRuleChange change)
            {
                var payload = new ForeignRuleChangePayload { Change = change };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcknowledgeForeignRule, payload));
                systemChangesListView.Items.Remove(e.Item);
                _masterSystemChanges.Remove(change);
                _unseenSystemChangesCount = _masterSystemChanges.Count;
                UpdateUiWithChangesCount();
            }
        }

        private void AcceptAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_masterSystemChanges.Count == 0) return;
            var result = Messenger.MessageBox($"Are you sure you want to accept all {_masterSystemChanges.Count} detected changes?", "Confirm Accept All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var payload = new AllForeignRuleChangesPayload { Changes = new List<FirewallRuleChange>(_masterSystemChanges) };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcceptAllForeignRules, payload));
                _masterSystemChanges.Clear();
                _unseenSystemChangesCount = 0;
                UpdateUiWithChangesCount();
                ApplySystemChangesSearchFilter();
            }
        }

        private void IgnoreAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_masterSystemChanges.Count == 0) return;
            var result = Messenger.MessageBox($"Are you sure you want to ignore all {_masterSystemChanges.Count} detected changes? They will not be shown again.", "Confirm Ignore All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var payload = new AllForeignRuleChangesPayload { Changes = new List<FirewallRuleChange>(_masterSystemChanges) };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AcknowledgeAllForeignRules, payload));
                _masterSystemChanges.Clear();
                _unseenSystemChangesCount = 0;
                UpdateUiWithChangesCount();
                ApplySystemChangesSearchFilter();
            }
        }
        #endregion

        #region Groups Tab Custom Drawing & Events
        private void GroupsListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var hitInfo = groupsListView.HitTest(e.Location);
            if (hitInfo.Item == null || hitInfo.SubItem == null) return;
            if (hitInfo.Item.SubItems.IndexOf(hitInfo.SubItem) == 1)
            {
                if (hitInfo.Item.Tag is FirewallGroup group)
                {
                    group.IsEnabled = !group.IsEnabled;
                    groupsListView.Invalidate(hitInfo.Item.Bounds);
                }
            }
        }

        private async void DeleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete the selected group(s) and all associated rules?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    foreach (ListViewItem item in groupsListView.SelectedItems)
                    {
                        if (item.Tag is FirewallGroup group)
                        {
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteGroup, group.Name));
                        }
                    }
                    await DisplayGroupsAsync();
                }
            }
        }

        private void GroupsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if ((e.State & ListViewItemStates.Focused) != 0)
            {
                e.DrawFocusRectangle();
            }
        }

        private void GroupsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
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
                using var overlayBrush = new SolidBrush(Color.FromArgb(20, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            else if (e.ColumnIndex == 1)
            {
                if (e.Item.Tag is FirewallGroup group)
                {
                    DrawToggleSwitch(e.Graphics, e.Bounds, group.IsEnabled);
                }
            }
        }

        private void DrawToggleSwitch(Graphics g, Rectangle bounds, bool isChecked)
        {
            int switchWidth = Scale(50, g);
            int switchHeight = Scale(25, g);
            int thumbSize = Scale(21, g);
            int padding = Scale(10, g);
            Rectangle switchRect = new Rectangle(
                bounds.X + padding,
                bounds.Y + (bounds.Height - switchHeight) / 2,
                switchWidth,
                switchHeight);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color backColor = isChecked ? Color.FromArgb(0, 192, 0) : Color.FromArgb(200, 0, 0);
            using (var path = new GraphicsPath())
            {
                path.AddArc(switchRect.X, switchRect.Y, switchRect.Height, switchRect.Height, 90, 180);
                path.AddArc(switchRect.Right - switchRect.Height, switchRect.Y, switchRect.Height, switchRect.Height, 270, 180);
                path.CloseFigure();
                g.FillPath(new SolidBrush(backColor), path);
            }

            int thumbX = isChecked ? switchRect.Right - thumbSize - Scale(2, g) : switchRect.X + Scale(2, g);
            Rectangle thumbRect = new Rectangle(
                thumbX,
                switchRect.Y + (switchRect.Height - thumbSize) / 2,
                thumbSize,
                thumbSize);
            using (var thumbBrush = new SolidBrush(dm.OScolors.TextActive))
            {
                g.FillEllipse(thumbBrush, thumbRect);
            }
        }
        #endregion

        #region Rule Management
        private void RulesListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (rulesListView.FocusedItem != null && rulesListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    rulesContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void ApplyRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuItem || menuItem.Tag?.ToString() is not string action || rulesListView.SelectedIndices.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>(rulesListView.SelectedIndices.Count);
            foreach (int index in rulesListView.SelectedIndices)
            {
                if (index >= 0 && index < _virtualRulesData.Count)
                {
                    items.Add(_virtualRulesData[index]);
                }
            }

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var firstRule = item.UnderlyingRules.FirstOrDefault();
                    if (firstRule == null) continue;

                    switch (firstRule.Type)
                    {
                        case RuleType.Program:
                            var appPayload = new ApplyApplicationRulePayload { AppPaths = [firstRule.ApplicationName], Action = action };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, appPayload));
                            break;
                        case RuleType.Service:
                            var servicePayload = new ApplyServiceRulePayload { ServiceName = firstRule.ServiceName, Action = action };
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload));
                            break;
                        case RuleType.UWP:
                            if (firstRule.Description.Contains(MFWConstants.UwpDescriptionPrefix))
                            {
                                var pfn = firstRule.Description.Replace(MFWConstants.UwpDescriptionPrefix, "");
                                var uwpApp = new UwpApp { Name = item.Name, PackageFamilyName = pfn };
                                var uwpPayload = new ApplyUwpRulePayload { UwpApps = [uwpApp], Action = action };
                                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyUwpRule, uwpPayload));
                            }
                            break;
                    }
                }
            }
        }

        private void DeleteRuleMenuItem_Click(object sender, EventArgs e)
        {
            if (rulesListView.SelectedIndices.Count == 0) return;
            var items = new List<AggregatedRuleViewModel>(rulesListView.SelectedIndices.Count);
            foreach (int index in rulesListView.SelectedIndices)
            {
                if (index >= 0 && index < _virtualRulesData.Count)
                {
                    items.Add(_virtualRulesData[index]);
                }
            }

            if (items.Count > 0)
            {
                var result = Messenger.MessageBox($"Are you sure you want to delete the {items.Count} selected rule(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                var wildcardRulesToDelete = items
                    .Where(i => i.Type == RuleType.Wildcard && i.WildcardDefinition != null)
                    .Select(i => i.WildcardDefinition!)
                    .ToList();
                var standardRuleNamesToDelete = items
                    .Where(i => i.Type != RuleType.Wildcard)
                    .SelectMany(i => i.UnderlyingRules.Select(r => r.Name))
                    .ToList();
                foreach (var wildcardRule in wildcardRulesToDelete)
                {
                    var payload = new DeleteWildcardRulePayload { Wildcard = wildcardRule };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteWildcardRules, payload));
                    _wildcardRuleService.RemoveRule(wildcardRule);
                }

                if (standardRuleNamesToDelete.Count > 0)
                {
                    var payload = new DeleteRulesPayload { RuleIdentifiers = standardRuleNamesToDelete };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules, payload));
                }

                var nameSet = new HashSet<string>(standardRuleNamesToDelete);
                var wildcardSet = new HashSet<WildcardRule>(wildcardRulesToDelete);
                _virtualRulesData.RemoveAll(r => nameSet.Contains(r.Name) || (r.WildcardDefinition != null && wildcardSet.Contains(r.WildcardDefinition)));
                _allAggregatedRules.RemoveAll(r => nameSet.Contains(r.Name) || (r.WildcardDefinition != null && wildcardSet.Contains(r.WildcardDefinition)));
                rulesListView.VirtualListSize = _virtualRulesData.Count;
                rulesListView.Invalidate();
            }
        }
        #endregion

        #region Live Connections & Sorting
        private void LiveConnectionsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (liveConnectionsListView.FocusedItem != null && liveConnectionsListView.FocusedItem.Bounds.Contains(e.Location))
                {
                    liveConnectionsContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void LiveConnectionsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private void LiveConnectionsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
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

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
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

        private void KillProcessToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void BlockRemoteIPToolStripMenuItem_Click(object sender, EventArgs e)
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
                        RemoteAddresses = [new(System.Net.IPAddress.Parse(vm.RemoteAddress))],
                        Profiles = "All",
                        Type = RuleType.Advanced
                    };
                    var payload = new CreateAdvancedRulePayload { ViewModel = rule, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                }
            }
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (sender is not ListView listView) return;
            int sortColumn = e.Column;
            SortOrder sortOrder;

            int currentSortColumn = 0;
            SortOrder currentSortOrder = SortOrder.None;
            switch (listView.Name)
            {
                case "rulesListView":
                    currentSortColumn = _rulesSortColumn;
                    currentSortOrder = _rulesSortOrder;
                    break;
                case "dashboardListView":
                    currentSortColumn = _dashboardSortColumn;
                    currentSortOrder = _dashboardSortOrder;
                    break;
                case "systemChangesListView":
                    currentSortColumn = _systemChangesSortColumn;
                    currentSortOrder = _systemChangesSortOrder;
                    break;
                case "groupsListView":
                    currentSortColumn = _groupsSortColumn;
                    currentSortOrder = _groupsSortOrder;
                    break;
                case "liveConnectionsListView":
                    currentSortColumn = _liveConnectionsSortColumn;
                    currentSortOrder = _liveConnectionsSortOrder;
                    break;
                default:
                    return;
            }

            if (e.Column == currentSortColumn)
            {
                sortOrder = (currentSortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sortOrder = SortOrder.Ascending;
            }

            switch (listView.Name)
            {
                case "rulesListView":
                    _rulesSortColumn = sortColumn;
                    _rulesSortOrder = sortOrder;
                    ApplyRulesFilters(_allAggregatedRules);
                    break;
                case "dashboardListView":
                    _dashboardSortColumn = sortColumn;
                    _dashboardSortOrder = sortOrder;
                    listView.ListViewItemSorter = new ListViewItemComparer(e.Column > 1 ? e.Column : 2, sortOrder);
                    listView.Sort();
                    break;
                case "systemChangesListView":
                    _systemChangesSortColumn = sortColumn;
                    _systemChangesSortOrder = sortOrder;
                    listView.ListViewItemSorter = new ListViewItemComparer(e.Column > 0 ? e.Column : 1, sortOrder);
                    listView.Sort();
                    break;
                case "groupsListView":
                    _groupsSortColumn = sortColumn;
                    _groupsSortOrder = sortOrder;
                    listView.ListViewItemSorter = new ListViewItemComparer(e.Column, sortOrder);
                    listView.Sort();
                    break;
                case "liveConnectionsListView":
                    _liveConnectionsSortColumn = sortColumn;
                    _liveConnectionsSortOrder = sortOrder;
                    UpdateLiveConnectionsView();
                    break;
            }
        }
        #endregion

        #region Context Menu Handlers
        private bool TryGetSelectedAppContext(out string? appPath, out string? direction)
        {
            appPath = null;
            direction = "Outbound";

            ListView? activeListView = mainTabControl.SelectedTab?.Controls.OfType<ListView>().FirstOrDefault();
            if (activeListView?.SelectedIndices.Count == 0 && activeListView?.SelectedItems.Count == 0)
            {
                if (activeListView?.Name == "dashboardListView")
                {
                    var dashboard = mainTabControl.SelectedTab?.Controls.OfType<DashboardControl>().FirstOrDefault();
                    if (dashboard != null)
                    {
                        activeListView = dashboard.Controls.OfType<ButtonListView>().FirstOrDefault();
                        if (activeListView?.SelectedItems.Count == 0) return false;
                    }
                    else return false;
                }
                else return false;
            }

            if (activeListView.VirtualMode)
            {
                int index = activeListView.SelectedIndices[0];
                if (activeListView.Name == "rulesListView" && index < _virtualRulesData.Count)
                {
                    var rule = _virtualRulesData[index];
                    appPath = rule.ApplicationName;
                }
                else if (activeListView.Name == "liveConnectionsListView" && index < _virtualLiveConnectionsData.Count)
                {
                    var conn = _virtualLiveConnectionsData[index];
                    appPath = conn.ProcessPath;
                }
            }
            else
            {
                var selectedItem = activeListView.SelectedItems[0];
                if (selectedItem.Tag == null) return false;
                switch (selectedItem.Tag)
                {
                    case PendingConnectionViewModel pending:
                        appPath = pending.AppPath;
                        direction = pending.Direction;
                        break;
                    case FirewallRuleChange change:
                        appPath = change.Rule?.ApplicationName;
                        break;
                    default:
                        return false;
                }
            }

            return !string.IsNullOrEmpty(appPath);
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is not ContextMenuStrip contextMenu) return;
            if (!TryGetSelectedAppContext(out string? appPath, out _))
            {
                e.Cancel = true;
                return;
            }

            bool isProgramOrWildcard = !string.IsNullOrEmpty(appPath);
            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item.Name.Contains("openFileLocation"))
                {
                    item.Enabled = isProgramOrWildcard && (File.Exists(appPath) || Directory.Exists(appPath));
                }
                if (item.Name.Contains("createAdvancedRule"))
                {
                    item.Enabled = isProgramOrWildcard;
                }
                if (item.Name.Contains("allowAndTrustPublisher"))
                {
                    bool isSigned = SignatureValidationService.GetPublisherInfo(appPath, out _);
                    item.Visible = isSigned;
                }
            }
        }

        private void createAdvancedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedAppContext(out string? appPath, out string? direction))
            {
                using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService, appPath!, direction!);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                }
            }
        }

        private void copyRemoteAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveConnectionsListView.SelectedIndices.Count > 0)
            {
                int index = liveConnectionsListView.SelectedIndices[0];
                if (index >= 0 && index < _virtualLiveConnectionsData.Count)
                {
                    var vm = _virtualLiveConnectionsData[index];
                    Clipboard.SetText(vm.RemoteAddress);
                }
            }
        }

        private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView? activeListView = mainTabControl.SelectedTab?.Controls.OfType<ListView>().FirstOrDefault();
            if (activeListView == null || (activeListView.SelectedItems.Count == 0 && activeListView.SelectedIndices.Count == 0)) return;
            var details = new StringBuilder();
            object? tag = null;

            if (activeListView.VirtualMode)
            {
                int index = activeListView.SelectedIndices[0];
                if (activeListView.Name == "rulesListView" && index < _virtualRulesData.Count) tag = _virtualRulesData[index];
                else if (activeListView.Name == "liveConnectionsListView" && index < _virtualLiveConnectionsData.Count) tag = _virtualLiveConnectionsData[index];
            }
            else
            {
                tag = activeListView.SelectedItems[0].Tag;
            }

            if (tag == null) return;
            switch (tag)
            {
                case PendingConnectionViewModel pending:
                    details.AppendLine($"Type: Pending Connection");
                    details.AppendLine($"Application: {pending.FileName}");
                    details.AppendLine($"Path: {pending.AppPath}");
                    details.AppendLine($"Service: {pending.ServiceName}");
                    details.AppendLine($"Direction: {pending.Direction}");
                    break;
                case AggregatedRuleViewModel agg:
                    details.AppendLine($"Type: {agg.Type} Rule");
                    details.AppendLine($"Name: {agg.Name}");
                    if (agg.Type == RuleType.Wildcard && agg.WildcardDefinition != null)
                    {
                        details.AppendLine($"Folder Path: {agg.ApplicationName}");
                        details.AppendLine($"Exe Filter: {agg.WildcardDefinition.ExeName}");
                    }
                    else
                    {
                        details.AppendLine($"Application: {agg.ApplicationName}");
                        details.AppendLine($"Service: {agg.ServiceName}");
                    }
                    details.AppendLine($"Action: {agg.Status} ({agg.Direction})");
                    details.AppendLine($"Protocol: {agg.ProtocolName}");
                    details.AppendLine($"Remote Addresses: {(agg.RemoteAddresses.Any() ? string.Join(", ", agg.RemoteAddresses) : "Any")}");
                    details.AppendLine($"Description: {agg.Description}");
                    break;
                case TcpConnectionViewModel live:
                    details.AppendLine($"Type: Live Connection");
                    details.AppendLine($"Process: {live.ProcessName}");
                    details.AppendLine($"Path: {live.ProcessPath}");
                    details.AppendLine($"Remote Address: {live.RemoteAddress}");
                    details.AppendLine($"Remote Port: {live.RemotePort}");
                    break;
                case FirewallRuleChange change:
                    details.AppendLine($"Type: Audited Change ({change.Type})");
                    if (change.Rule != null)
                    {
                        details.AppendLine($"Rule Name: {change.Rule.Name}");
                        details.AppendLine($"Application: {change.Rule.ApplicationName}");
                        details.AppendLine($"Action: {change.Rule.Status}");
                        details.AppendLine($"Direction: {change.Rule.Direction}");
                        details.AppendLine($"Protocol: {change.Rule.ProtocolName}");
                        details.AppendLine($"Remote Addresses: {(change.Rule.RemoteAddresses.Any() ? string.Join(", ", change.Rule.RemoteAddresses) : "Any")}");
                    }
                    break;
            }

            if (details.Length > 0)
            {
                Clipboard.SetText(details.ToString());
            }
        }

        private void OpenFileLocationMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedAppContext(out string? appPath, out _))
            {
                Messenger.MessageBox("The path for this item is not available.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool pathExists = File.Exists(appPath) || Directory.Exists(appPath);
            if (!pathExists)
            {
                Messenger.MessageBox("The file or folder path for this item could not be found.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start("explorer.exe", $"/select, \"{appPath}\"");
            }
            catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
            {
                Messenger.MessageBox($"Could not open file location.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Dashboard Context Menu Handlers (REMOVED - MOVED TO DASHBOARDCONTROL)
        #endregion

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
                imageToDraw = IsLockedDown() ? _lockedGreenIcon : ((_appSettings.Theme == "Dark") ? _unlockedWhiteIcon : appImageList.Images["unlocked.png"]);
            }
            else if (button.Name == "rescanButton")
            {
                if (_isRefreshingData)
                {
                    TextRenderer.DrawText(e.Graphics, "...", button.Font, button.ClientRectangle, button.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    return;
                }
                imageToDraw = (_appSettings.Theme == "Dark") ? _refreshWhiteIcon : appImageList.Images["refresh.png"];
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

        #region Virtual Mode Sorting Helpers
        private Func<AggregatedRuleViewModel, object> GetRuleKeySelector(int columnIndex)
        {
            return columnIndex switch
            {
                2 => rule => rule.IsEnabled,
                3 => rule => rule.Status,
                4 => rule => rule.Direction,
                5 => rule => rule.ProtocolName,
                10 => rule => rule.ApplicationName,
                11 => rule => rule.ServiceName,
                12 => rule => rule.Profiles,
                13 => rule => rule.Grouping,
                14 => rule => rule.Description,
                _ => rule => rule.Name,
            };
        }

        private Func<TcpConnectionViewModel, object> GetLiveConnectionKeySelector(int columnIndex)
        {
            return columnIndex switch
            {
                2 => conn => conn.RemoteAddress,
                3 => conn => conn.RemotePort,
                _ => conn => conn.ProcessName,
            };
        }

        private void UpdateLiveConnectionsView()
        {
            IEnumerable<TcpConnectionViewModel> connections = _trafficMonitorViewModel.ActiveConnections;
            if (_liveConnectionsSortOrder != SortOrder.None && _liveConnectionsSortColumn != -1)
            {
                Func<TcpConnectionViewModel, object> keySelector = GetLiveConnectionKeySelector(_liveConnectionsSortColumn);
                if (_liveConnectionsSortOrder == SortOrder.Ascending)
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
        #endregion
    }
}