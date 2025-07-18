﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace MinimalFirewall
{
    public partial class MainWindow : Window
    {
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
            this.Loaded += MainWindow_Loaded;

            InitializeTrayIcon();
            this.Closing += MainWindow_Closing;
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = GetAppIcon(),
                Visible = true,
                Text = "Minimal Firewall"
            };

            _notifyIcon.MouseDoubleClick += ShowWindow;
            var contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Show", ShowWindow);
            contextMenu.MenuItems.Add("Exit", ExitApplication);
            _notifyIcon.ContextMenu = contextMenu;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.IsCloseToTrayEnabled)
            {
                e.Cancel = true;
                this.Hide();
                vm.ClearCachesForTray();
            }
            else
            {
                _notifyIcon?.Dispose();
            }
        }

        private void ShowWindow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();

            if (DataContext is MainViewModel vm && vm.IsCacheCleared)
            {
                vm.SlowRefresh();
            }
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            System.Windows.Application.Current.Shutdown();
        }

        private Icon GetAppIcon()
        {
            try
            {
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return System.Drawing.Icon.ExtractAssociatedIcon(exePath);
            }
            catch
            {
                return SystemIcons.Shield;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

        private void CreateWildcardFromPending_Click(object sender, RoutedEventArgs e)
        {
            if (PendingConnectionsListView.SelectedItem is PendingConnectionViewModel pending && DataContext is MainViewModel vm)
            {
                vm.HandleWildcardCreationRequest(Path.GetDirectoryName(pending.AppPath), pending);
            }
        }

        private void EditProgramRules_Click(object sender, RoutedEventArgs e)
        {
            HandleRuleEdit(ProgramsListView.SelectedItems.Cast<FirewallRuleViewModel>().Select(vm => vm.ApplicationName));
        }

        private void EditUwpRules_Click(object sender, RoutedEventArgs e)
        {
            HandleUwpRuleEdit(UwpAppsListView.SelectedItems.Cast<UwpApp>().ToList());
        }

        private void DeleteAppRules_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                var programPaths = ProgramsListView.SelectedItems.Cast<FirewallRuleViewModel>().Select(vm => vm.ApplicationName).ToList();
                var servicePaths = ServicesListView.SelectedItems.Cast<FirewallRuleViewModel>().Select(vm => vm.ApplicationName).ToList();

                var combinedPaths = new List<string>();
                combinedPaths.AddRange(programPaths);
                combinedPaths.AddRange(servicePaths);
                mainViewModel.DeleteApplicationRules(combinedPaths);
            }
        }

        private void DeleteUwpRules_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var packageFamilyNames = UwpAppsListView.SelectedItems.Cast<UwpApp>().Select(app => app.PackageFamilyName).ToList();
                vm.DeleteUwpRules(packageFamilyNames);
            }
        }

        private void EditServiceRules_Click(object sender, RoutedEventArgs e)
        {
            HandleRuleEdit(ServicesListView.SelectedItems.Cast<FirewallRuleViewModel>().Select(vm => vm.ApplicationName));
        }

        private void CreateProgramRules_Click(object sender, RoutedEventArgs e)
        {
            HandleRuleEdit(UndefinedProgramsListView.SelectedItems.Cast<ProgramViewModel>().Select(vm => vm.ExePath));
        }

        private void CreateAdvancedRule_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AdvancedRuleCreator();
            if (dialog.ShowDialog() == true && DataContext is MainViewModel vm)
            {
                vm.CreatePowerShellRule(dialog.RuleCommand);
            }
        }

        private void OpenFirewallConsole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("wf.msc");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Could not open Windows Firewall console.\n\nError: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAdvancedRules_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is MainViewModel vm)) return;
            if (!(sender is System.Windows.Controls.MenuItem menuItem)) return;
            if (!(menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu)) return;

            IEnumerable<string> ruleNamesToDelete = null;

            if (contextMenu.PlacementTarget == AdvancedRulesListView)
            {
                ruleNamesToDelete = AdvancedRulesListView.SelectedItems.Cast<AdvancedRuleViewModel>().Select(r => r.Name);
            }
            else if (contextMenu.PlacementTarget == ForeignRulesListView)
            {
                ruleNamesToDelete = ForeignRulesListView.SelectedItems.Cast<AdvancedRuleViewModel>().Select(r => r.Name);
            }

            if (ruleNamesToDelete != null && ruleNamesToDelete.Any())
            {
                vm.DeleteAdvancedRules(ruleNamesToDelete.ToList());
            }
        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is System.Windows.Controls.MenuItem menuItem) || !(menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu)) return;
            if (!(contextMenu.PlacementTarget is System.Windows.Controls.ListView listView)) return;

            var selectedItem = listView.SelectedItem;
            if (selectedItem == null) return;

            string exePath = null;
            if (selectedItem is FirewallRuleViewModel fwRuleVm) exePath = fwRuleVm.ApplicationName;
            else if (selectedItem is PendingConnectionViewModel pendingVm) exePath = pendingVm.AppPath;
            else if (selectedItem is ProgramViewModel programVm) exePath = programVm.ExePath;

            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                try
                {
                    Process.Start("explorer.exe", $"/select, \"{exePath}\"");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(this, "Could not open file location.\n\nError: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (!string.IsNullOrEmpty(exePath))
            {
                System.Windows.MessageBox.Show(this, "The file path could not be found:\n\n" + exePath, "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HandleRuleEdit(IEnumerable<string> appPaths)
        {
            var distinctPaths = appPaths.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
            if (distinctPaths.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select one or more items to edit/create.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EditRuleWindow(distinctPaths);
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.ApplyApplicationRuleChange(distinctPaths, dialog.SelectedAction);
                }
            }
        }

        private void HandleUwpRuleEdit(List<UwpApp> uwpApps)
        {
            if (uwpApps.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select one or more UWP apps to edit/create.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var appNames = uwpApps.Select(app => app.Name).ToList();
            var dialog = new EditRuleWindow(appNames);
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ApplyUwpRuleChange(uwpApps, dialog.SelectedAction);
                }
            }
        }

        private void AllowTempButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.DataContext = btn.DataContext;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void AllowTempMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem &&
                menuItem.DataContext is PendingConnectionViewModel pending &&
                DataContext is MainViewModel vm &&
                int.TryParse(menuItem.Tag.ToString(), out var minutes))
            {
                vm.AllowPendingConnectionTemporarily(pending, minutes);
            }
        }

        private void SortableColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader headerClicked && headerClicked.Column != null && DataContext is MainViewModel vm)
            {
                if (FindParent<System.Windows.Controls.ListView>(headerClicked) is System.Windows.Controls.ListView listView)
                {
                    var direction = headerClicked == _lastHeaderClicked && _lastDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                    if (headerClicked.Column.DisplayMemberBinding is System.Windows.Data.Binding binding)
                    {
                        vm.SortCollection(listView.Name, binding.Path.Path, direction);
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            return parentObject as T ?? FindParent<T>(parentObject);
        }

        private void FileMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (DataContext is MainViewModel vm)
                {
                    string header = selectedTab.Header?.ToString();
                    if (header == "UWP Apps")
                    {
                        await vm.LoadUwpAppsOnDemandAsync();
                    }
                    else if (header == "Foreign Rules")
                    {
                        await vm.LoadForeignRulesOnDemandAsync();
                    }
                    else if (header == "Wildcard Rules")
                    {
                        vm.SyncWildcardRules(true);
                    }
                }
            }
        }

        private async void RescanForeignRules_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RescanForeignRulesAsync();
            }
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var item = FindParent<System.Windows.Controls.ListViewItem>(source);
                if (item == null)
                {
                    e.Handled = true;
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://coff.ee/deminimis");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Could not open the link.\n\nError: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}