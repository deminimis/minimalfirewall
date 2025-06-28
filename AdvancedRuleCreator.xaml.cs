using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MinimalFirewall
{
    public partial class AdvancedRuleCreator : Window
    {
        public string RuleCommand { get; private set; }

        public AdvancedRuleCreator()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            RuleCommand = string.Empty;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(tabControl.SelectedItem is TabItem selectedTab)) return;
            var commandBuilder = new StringBuilder("New-NetFirewallRule");
            string displayName;
            const string validationError = "cannot be empty.";
            switch (selectedTab.Header.ToString())
            {
                case "TCP Port":
                    if (string.IsNullOrWhiteSpace(TcpPortsTextBox.Text)) { ShowError("TCP Port " + validationError); return; }
                    displayName = $"'MFW TCP Port {TcpPortsTextBox.Text}'";
                    commandBuilder.Append($" -DisplayName {displayName} -Protocol TCP -LocalPort {TcpPortsTextBox.Text}");
                    AppendSharedParameters(commandBuilder, TcpActionComboBox, TcpDirectionComboBox);
                    break;
                case "UDP Port":
                    if (string.IsNullOrWhiteSpace(UdpPortsTextBox.Text)) { ShowError("UDP Port " + validationError); return; }
                    displayName = $"'MFW UDP Port {UdpPortsTextBox.Text}'";
                    commandBuilder.Append($" -DisplayName {displayName} -Protocol UDP -LocalPort {UdpPortsTextBox.Text}");
                    AppendSharedParameters(commandBuilder, UdpActionComboBox, UdpDirectionComboBox);
                    break;
                case "IP Address":
                    if (string.IsNullOrWhiteSpace(RemoteIpTextBox.Text)) { ShowError("Remote IP Address " + validationError); return; }
                    displayName = $"'MFW IP {RemoteIpTextBox.Text}'";
                    commandBuilder.Append($" -DisplayName {displayName} -RemoteAddress {RemoteIpTextBox.Text}");
                    AppendSharedParameters(commandBuilder, IpActionComboBox, IpDirectionComboBox);
                    break;
                case "Block Service":
                    if (string.IsNullOrWhiteSpace(ServiceNameTextBox.Text)) { ShowError("Service Name " + validationError); return; }
                    string serviceName = ServiceNameTextBox.Text;
                    displayName = $"'MFW Block Service {serviceName}'";
                    commandBuilder.Append($" -DisplayName {displayName} -Service {serviceName} -Action Block -Direction Outbound");
                    break;
                case "Program + Remote IP":
                    if (string.IsNullOrWhiteSpace(ProgIpProgramPathTextBox.Text)) { ShowError("Program Path " + validationError); return; }
                    if (string.IsNullOrWhiteSpace(ProgIpRemoteIpTextBox.Text)) { ShowError("Remote IP Address " + validationError); return; }
                    string progIpPath = ProgIpProgramPathTextBox.Text;
                    string progIpName = Path.GetFileNameWithoutExtension(progIpPath);
                    displayName = $"'MFW {progIpName} to {ProgIpRemoteIpTextBox.Text}'";
                    commandBuilder.Append($" -DisplayName {displayName} -Program \"{progIpPath}\" -RemoteAddress {ProgIpRemoteIpTextBox.Text}");
                    AppendSharedParameters(commandBuilder, ProgIpActionComboBox, ProgIpDirectionComboBox);
                    break;
                case "Program + Port":
                    if (string.IsNullOrWhiteSpace(ProgPortProgramPathTextBox.Text)) { ShowError("Program Path " + validationError); return; }
                    if (string.IsNullOrWhiteSpace(ProgPortRemotePortTextBox.Text)) { ShowError("Remote Port(s) " + validationError); return; }
                    string progPortPath = ProgPortProgramPathTextBox.Text;
                    string progPortName = Path.GetFileNameWithoutExtension(progPortPath);
                    string remotePorts = ProgPortRemotePortTextBox.Text;
                    string protocol = (ProgPortProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    displayName = $"'MFW Block {progPortName} on {protocol} {remotePorts}'";
                    commandBuilder.Append($" -DisplayName {displayName} -Program \"{progPortPath}\" -RemotePort {remotePorts} -Protocol {protocol} -Action Block");
                    AppendSharedParameters(commandBuilder, null, ProgPortDirectionComboBox);
                    break;
                case "Allow LAN Only":
                    if (string.IsNullOrWhiteSpace(LanOnlyProgramPathTextBox.Text)) { ShowError("Program Path " + validationError); return; }
                    string lanOnlyPath = LanOnlyProgramPathTextBox.Text;
                    string lanOnlyName = Path.GetFileNameWithoutExtension(lanOnlyPath);
                    displayName = $"'MFW {lanOnlyName} (LAN Only)'";
                    commandBuilder.Append($" -DisplayName {displayName} -Program \"{lanOnlyPath}\" -RemoteAddress LocalSubnet -Action Allow");
                    break;
                case "Uninstall Rules":
                    return;
                default:
                    return;
            }

            commandBuilder.Append($" -Group '{MFWConstants.MainRuleGroup}'");
            RuleCommand = commandBuilder.ToString();
            DialogResult = true;
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete ALL rules created by Minimal Firewall (including wildcard rules)?\n\nThis action cannot be undone.",
                "Confirm Uninstall",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                RuleCommand = $"Get-NetFirewallRule -Group '{MFWConstants.MainRuleGroup}', '{MFWConstants.WildcardRuleGroup}' | Remove-NetFirewallRule";
                DialogResult = true;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a Program"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    string header = selectedTab.Header.ToString();
                    if (header == "Program + Remote IP")
                    {
                        ProgIpProgramPathTextBox.Text = openFileDialog.FileName;
                    }
                    else if (header == "Program + Port")
                    {
                        ProgPortProgramPathTextBox.Text = openFileDialog.FileName;
                    }
                    else if (header == "Allow LAN Only")
                    {
                        LanOnlyProgramPathTextBox.Text = openFileDialog.FileName;
                    }
                }
            }
        }

        private static void AppendSharedParameters(StringBuilder builder, ComboBox actionBox, ComboBox directionBox)
        {
            if (actionBox != null && actionBox.SelectedItem is ComboBoxItem actionItem)
            {
                builder.Append($" -Action {actionItem.Content}");
            }
            if (directionBox != null && directionBox.SelectedItem is ComboBoxItem directionItem)
            {
                builder.Append($" -Direction {directionItem.Content}");
            }
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}