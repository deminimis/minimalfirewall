using DarkModeForms;
using Firewall.Traffic.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using MinimalFirewall.TypedObjects;
using System.Collections.Generic;
using System.Linq;

namespace MinimalFirewall
{
    public partial class SettingsControl : UserControl
    {
        private AppSettings _appSettings;
        private StartupService _startupService;
        private PublisherWhitelistService _whitelistService;
        private FirewallActionsService _actionsService;
        private UserActivityLogger _activityLogger;
        private MainViewModel _mainViewModel;
        private ImageList _appImageList;
        private DarkModeCS _dm;

        public event Action ThemeChanged;
        public event Action IconVisibilityChanged;
        public event Func<Task> DataRefreshRequested;
        public event Action AutoRefreshTimerChanged;
        public event Action? TrafficMonitorSettingChanged;

        public SettingsControl()
        {
            InitializeComponent();
        }

        public void Initialize(
            AppSettings appSettings,
            StartupService startupService,
            PublisherWhitelistService whitelistService,
            FirewallActionsService actionsService,
            UserActivityLogger activityLogger,
            MainViewModel mainViewModel,
            ImageList appImageList,
            string version,
            DarkModeCS dm)
        {
            _appSettings = appSettings;
            _startupService = startupService;
            _whitelistService = whitelistService;
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _mainViewModel = mainViewModel;
            _appImageList = appImageList;
            _dm = dm;

            versionLabel.Text = version;
            loggingSwitch.CheckedChanged += new System.EventHandler(this.loggingSwitch_CheckedChanged);

            if (_appImageList != null && _appImageList.Images.ContainsKey("coffee.png"))
            {
                coffeePictureBox.Image = _appImageList.Images["coffee.png"];
            }
        }

        public void ApplyThemeFixes()
        {
            if (_dm == null) return;

            // Helper function 
            void StyleButton(Button btn)
            {
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = _dm.OScolors.ControlDark;
                btn.ForeColor = _dm.IsDarkMode ? Color.White : SystemColors.ControlText;
            }

            StyleButton(deleteAllRulesButton);
            StyleButton(revertFirewallButton);
            StyleButton(managePublishersButton);
            StyleButton(openFirewallButton);
            StyleButton(openAppDataButton);
            StyleButton(checkForUpdatesButton);
            StyleButton(cleanUpOrphanedRulesButton);
            StyleButton(exportRulesButton);
            StyleButton(importMergeButton);
            StyleButton(importReplaceButton);
            StyleButton(exportDiagnosticButton);
        }

        public void LoadSettingsToUI()
        {
            if (_appSettings == null) return;

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

        public void SaveSettingsFromUI()
        {
            if (_appSettings == null) return;

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

            _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;

            if (!_appSettings.IsTrafficMonitorEnabled)
            {
                _mainViewModel?.TrafficMonitorViewModel?.StopMonitoring();
            }

            IconVisibilityChanged?.Invoke();
            _appSettings.Save();
        }

        public void ApplyTheme(bool isDark, DarkModeCS dm)
        {
            var linkColor = isDark ? Color.SkyBlue : SystemColors.HotTrack;

            helpLink.LinkColor = linkColor;
            reportProblemLink.LinkColor = linkColor;
            forumLink.LinkColor = linkColor;
            coffeeLinkLabel.LinkColor = linkColor;

            helpLink.VisitedLinkColor = linkColor;
            reportProblemLink.VisitedLinkColor = linkColor;
            forumLink.VisitedLinkColor = linkColor;
            coffeeLinkLabel.VisitedLinkColor = linkColor;

            if (_appImageList != null && _appImageList.Images.ContainsKey("coffee.png"))
            {
                Image coffeeImage = _appImageList.Images["coffee.png"];
                if (coffeeImage != null)
                {
                    Color coffeeColor = isDark ? Color.LightGray : Color.Black;
                    Image? oldImage = coffeePictureBox.Image;
                    coffeePictureBox.Image = DarkModeCS.RecolorImage(coffeeImage, coffeeColor);

                    // Only dispose if it's not the original resource from ImageList
                    if (oldImage != null && oldImage != coffeeImage)
                    {
                        oldImage.Dispose();
                    }
                }
            }
        }

        private void DarkModeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.Theme = darkModeSwitch.Checked ? "Dark" : "Light";
                ThemeChanged?.Invoke();
            }
        }

        private void startOnStartupSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null && _startupService != null)
            {
                _appSettings.StartOnSystemStartup = startOnStartupSwitch.Checked;
                _startupService.SetStartup(_appSettings.StartOnSystemStartup);
            }
        }

        private void PopupsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsPopupsEnabled = popupsSwitch.Checked;
            }
        }

        private void loggingSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsLoggingEnabled = loggingSwitch.Checked;
                _activityLogger.IsEnabled = _appSettings.IsLoggingEnabled;
            }
        }

        private void TrafficMonitorSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.IsTrafficMonitorEnabled = trafficMonitorSwitch.Checked;
                TrafficMonitorSettingChanged?.Invoke();
            }
        }

        private void ShowAppIconsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.ShowAppIcons = showAppIconsSwitch.Checked;
                IconVisibilityChanged?.Invoke();
            }
        }

        private void managePublishersButton_Click(object sender, EventArgs e)
        {
            using var form = new ManagePublishersForm(_whitelistService, _appSettings);
            form.ShowDialog(this.FindForm());
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
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open Windows Firewall console.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openAppDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                string standardAppDataPath = ConfigPathManager.GetStandardAppDataDirectory();

                if (!Directory.Exists(standardAppDataPath))
                {
                    Directory.CreateDirectory(standardAppDataPath);
                }

                Process.Start("explorer.exe", standardAppDataPath);
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open %AppData% folder.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            OpenLink("https://github.com/deminimis/minimalfirewall/releases");
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel { Tag: string url }) return;
            OpenLink(url);
        }

        private void CoffeeLink_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.buymeacoffee.com/deminimis");
        }

        private void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Could not open the link.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoffeePictureBox_MouseEnter(object? sender, EventArgs e) { }
        private void CoffeePictureBox_MouseLeave(object? sender, EventArgs e) { }

        private async void deleteAllRulesButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("This will permanently delete all firewall rules created by this application. This action cannot be undone. Are you sure you want to continue?",
                "Delete All Rules", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _actionsService.DeleteAllMfwRules();
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("All Minimal Firewall rules have been deleted.", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("DeleteAllRules", ex);
                    Messenger.MessageBox($"Failed to delete rules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void revertFirewallButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox("WARNING: This will reset your ENTIRE Windows Firewall configuration to its default state. " +
                "All custom rules, including those not created by this application, will be deleted. This action is irreversible.\n\n" +
                "Are you absolutely sure you want to continue?",
                "Revert Windows Firewall Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    AdminTaskService.ResetFirewall();
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("Windows Firewall has been reset to its default settings. It is recommended to restart the application.",
                        "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("RevertFirewall", ex);
                    Messenger.MessageBox($"Failed to reset firewall: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void cleanUpOrphanedRulesButton_Click(object sender, EventArgs e)
        {
            var result = Messenger.MessageBox(
                "This will scan for rules whose associated application no longer exists on disk and delete them.\n\nAre you sure you want to continue?",
                "Clean Up Orphaned Rules",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _mainViewModel.CleanUpOrphanedRulesAsync();
                }
                catch (Exception ex)
                {
                    _activityLogger?.LogException("CleanOrphanedRules", ex);
                    Messenger.MessageBox($"Failed to clean rules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void exportRulesButton_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Minimal Firewall Rules",
                FileName = $"mfw_rules_backup_{DateTime.Now:yyyyMMdd}.json"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonContent = await _actionsService.ExportAllMfwRulesAsync();
                    await File.WriteAllTextAsync(saveDialog.FileName, jsonContent);
                    Messenger.MessageBox("All rules have been successfully exported.", "Export Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox($"An error occurred during export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void importMergeButton_Click(object sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import and Add Rules"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonContent = await File.ReadAllTextAsync(openDialog.FileName);
                    await _actionsService.ImportRulesAsync(jsonContent, replace: false);
                    await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                    Messenger.MessageBox("Rules have been successfully imported and added.", "Import Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    Messenger.MessageBox($"An error occurred during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void importReplaceButton_Click(object sender, EventArgs e)
        {
            var confirmResult = Messenger.MessageBox(
                "WARNING: This will delete ALL current Minimal Firewall rules before importing the new ones. This action cannot be undone.\n\nAre you sure you want to continue?",
                "Confirm Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                using var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Import and Replace Rules"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(openDialog.FileName);
                        await _actionsService.ImportRulesAsync(jsonContent, replace: true);
                        await (DataRefreshRequested?.Invoke() ?? Task.CompletedTask);
                        Messenger.MessageBox("All rules have been replaced successfully.", "Import Complete", MessageBoxButtons.OK, MsgIcon.Success);
                    }
                    catch (Exception ex)
                    {
                        Messenger.MessageBox($"An error occurred during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task<string> ReadLastNLinesAsync(string filePath, int n)
        {
            if (!File.Exists(filePath))
            {
                return $"{Path.GetFileName(filePath)} not found.";
            }

            try
            {
                var lineQueue = new Queue<string>(n);

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (lineQueue.Count >= n)
                        {
                            lineQueue.Dequeue();
                        }
                        lineQueue.Enqueue(line);
                    }
                }
                return string.Join(Environment.NewLine, lineQueue);
            }
            catch (Exception ex)
            {
                return $"Error reading {Path.GetFileName(filePath)}: {ex.Message}";
            }
        }

        private async void exportDiagnosticButton_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip",
                Title = "Export Diagnostic Package",
                FileName = $"mfw_diagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string zipPath = saveDialog.FileName;
                var statusForm = new StatusForm("Gathering diagnostic data...", _appSettings);
                statusForm.Show(this.FindForm());
                Application.DoEvents(); 

                try
                {
                    if (File.Exists(zipPath)) File.Delete(zipPath);

                    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        statusForm.UpdateStatus("Adding configuration files...");
                        var configFiles = new List<string>
                        {
                            "settings.json",
                            "wildcard_rules.json",
                            "trusted_publishers.json",
                            "foreign_rules_baseline.json",
                            "temporary_rules.json",
                            "uwp_apps.json"
                        };

                        foreach (var fileName in configFiles)
                        {
                            string filePath = ConfigPathManager.GetConfigPath(fileName);

                            if (File.Exists(filePath))
                            {
                                archive.CreateEntryFromFile(filePath, fileName);
                            }
                        }
                        statusForm.UpdateProgress(20);

                        statusForm.UpdateStatus("Adding debug log...");
                        string debugLogPath = ConfigPathManager.GetConfigPath("debug_log.txt");
                        string logContent = await ReadLastNLinesAsync(debugLogPath, 1500);

                        var logEntry = archive.CreateEntry("debug_log_last1500.txt");
                        using (var writer = new StreamWriter(logEntry.Open()))
                        {
                            await writer.WriteAsync(logContent);
                        }
                        statusForm.UpdateProgress(40);

                        statusForm.UpdateStatus("Exporting current rules...");
                        string rulesJson = await _actionsService.ExportAllMfwRulesAsync();
                        var rulesEntry = archive.CreateEntry("current_mfw_rules.json");
                        using (var writer = new StreamWriter(rulesEntry.Open()))
                        {
                            await writer.WriteAsync(rulesJson);
                        }
                        statusForm.UpdateProgress(60);

                        statusForm.UpdateStatus("Gathering system info...");
                        var sysInfo = new StringBuilder();
                        sysInfo.AppendLine($"OS Version: {Environment.OSVersion}");
                        sysInfo.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
                        sysInfo.AppendLine($"App Version: {Assembly.GetExecutingAssembly().GetName()?.Version}");
                        sysInfo.AppendLine($"Timestamp: {DateTime.Now}");

                        var sysInfoEntry = archive.CreateEntry("system_info.txt");
                        using (var writer = new StreamWriter(sysInfoEntry.Open()))
                        {
                            await writer.WriteAsync(sysInfo.ToString());
                        }
                        statusForm.UpdateProgress(80);
                    }

                    statusForm.UpdateStatus("Finalizing package...");
                    statusForm.UpdateProgress(100);
                    statusForm.Close();
                    Messenger.MessageBox($"Diagnostic package exported successfully to:\n{zipPath}", "Export Complete", MessageBoxButtons.OK, MsgIcon.Success);
                }
                catch (Exception ex)
                {
                    statusForm?.Close();
                    _activityLogger.LogException("ExportDiagnosticPackage", ex);
                    Messenger.MessageBox($"An error occurred while exporting diagnostics:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}