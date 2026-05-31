using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System.ComponentModel;
using NetFwTypeLib;
using MinimalFirewall.Groups;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace MinimalFirewall
{
    public partial class CreateAdvancedRuleForm : Form
    {
        // Constants for Protocol Numbers
        private const int ProtocolTcp = 6;
        private const int ProtocolUdp = 17;
        private const int ProtocolIcmpV4 = 1;
        private const int ProtocolIcmpV6 = 58;

        
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallRuleViewModel _viewModel;
        private readonly FirewallGroupManager _groupManager;
        private readonly AppSettings _appSettings;
        public AdvancedRuleViewModel? RuleVm { get; private set; }
        private readonly AdvancedRuleViewModel? _originalRuleVm;

        public CreateAdvancedRuleForm(FirewallActionsService actionsService, AppSettings appSettings)
        {
            InitializeComponent();

            _appSettings = appSettings;
            _actionsService = actionsService;
            _groupManager = new FirewallGroupManager();
            _viewModel = new FirewallRuleViewModel();

            InitializeState();

            
            ApplyDynamicTheme();

            BindDynamicUI();
        }

        public CreateAdvancedRuleForm(FirewallActionsService actionsService, string appPath, string direction, AppSettings appSettings)
               : this(actionsService, appSettings)
        {
            programPathTextBox.Text = appPath;
            if (direction.Equals("Inbound", StringComparison.OrdinalIgnoreCase))
            {
                inboundRadioButton.Checked = true;
            }
            else if (direction.Equals("Outbound", StringComparison.OrdinalIgnoreCase))
            {
                outboundRadioButton.Checked = true;
            }
            else
            {
                bothDirRadioButton.Checked = true;
            }
        }

        public CreateAdvancedRuleForm(FirewallActionsService actionsService, AdvancedRuleViewModel ruleToEdit, AppSettings appSettings)
            : this(actionsService, appSettings)
        {
            _originalRuleVm = ruleToEdit;
            Text = "Edit Advanced Rule";
            PopulateFormFromRule(ruleToEdit);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyDynamicLayout();
        }

        // Logic, theme, layout

        private void InitializeState()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ApplyDynamicTheme()
        {
            bool isDark = _appSettings.Theme == "Dark";
            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            BackColor = Theme.Colors.Background;
            ForeColor = Theme.Colors.TextInactive;

        }

        private void BindDynamicUI()
        {
            // Custom struct/type in code, not designer
            protocolComboBox.Items.AddRange(
            [
                ProtocolTypes.Any,
                ProtocolTypes.TCP,
                ProtocolTypes.UDP,
                ProtocolTypes.ICMPv4,
                ProtocolTypes.ICMPv6,
                ProtocolTypes.IGMP
            ]);
            protocolComboBox.SelectedItem = ProtocolTypes.Any;
            LoadFirewallGroups();
        }

        private void ApplyDynamicLayout()
        {
            var workingArea = Screen.FromControl(this).WorkingArea;
            if (Height > workingArea.Height)
            {
                Height = workingArea.Height;
            }
            CenterToParent();
        }

        private void PopulateFormFromRule(AdvancedRuleViewModel rule)
        {
            ruleNameTextBox.Text = rule.Name;
            descriptionTextBox.Text = rule.Description;
            enabledCheckBox.Checked = rule.IsEnabled;

            if (rule.Status == "Allow")
            {
                allowRadioButton.Checked = true;
            }
            else
            {
                blockRadioButton.Checked = true;
            }

            if (rule.Direction == (Directions.Incoming | Directions.Outgoing))
            {
                bothDirRadioButton.Checked = true;
            }
            else if (rule.Direction == Directions.Incoming)
            {
                inboundRadioButton.Checked = true;
            }
            else
            {
                outboundRadioButton.Checked = true;
            }

            programPathTextBox.Text = rule.ApplicationName;
            serviceNameTextBox.Text = (rule.ServiceName == "*" || string.IsNullOrEmpty(rule.ServiceName)) ? string.Empty : rule.ServiceName;


            bool found = false;
            foreach (var item in protocolComboBox.Items)
            {
                if (item is ProtocolTypes pt && pt.Value == rule.Protocol)
                {
                    protocolComboBox.SelectedItem = item;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                protocolComboBox.SelectedItem = ProtocolTypes.Any;
            }

            _viewModel.SelectedProtocol = protocolComboBox.SelectedItem is ProtocolTypes selectedPt ? selectedPt : ProtocolTypes.Any;

            localPortsTextBox.Text = rule.LocalPorts;
            remotePortsTextBox.Text = rule.RemotePorts;

            localAddressTextBox.Text = rule.LocalAddresses?.Replace("/255.255.255.255", "");
            remoteAddressTextBox.Text = rule.RemoteAddresses?.Replace("/255.255.255.255", "");

            domainCheckBox.Checked = rule.Profiles.Contains("Domain") || rule.Profiles == "All";
            privateCheckBox.Checked = rule.Profiles.Contains("Private") || rule.Profiles == "All";
            publicCheckBox.Checked = rule.Profiles.Contains("Public") || rule.Profiles == "All";

            groupComboBox.Text = rule.Grouping;
            lanCheckBox.Checked = rule.InterfaceTypes.Contains("Lan") || rule.InterfaceTypes == "All";
            wirelessCheckBox.Checked = rule.InterfaceTypes.Contains("Wireless") || rule.InterfaceTypes == "All";
            remoteAccessCheckBox.Checked = rule.InterfaceTypes.Contains("RemoteAccess") || rule.InterfaceTypes == "All";

            if (_viewModel.IsIcmpSectionVisible)
            {
                icmpTypesAndCodesTextBox.Text = rule.IcmpTypesAndCodes;
            }
        }

        private void LoadFirewallGroups()
        {
            var groups = _groupManager.GetAllGroups();

            var groupNames = new HashSet<string>(groups.Select(g => g.Name))
            {
                MFWConstants.MainRuleGroup,
                MFWConstants.WildcardRuleGroup
            };

            groupComboBox.Items.Clear();
            foreach (var name in groupNames.OrderBy(n => n))
            {
                groupComboBox.Items.Add(name);
            }

            groupComboBox.SelectedItem = MFWConstants.MainRuleGroup;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.IsPortSectionVisible))
            {
                portsGroupBox.Visible = _viewModel.IsPortSectionVisible;
            }
            else if (e.PropertyName == nameof(_viewModel.IsIcmpSectionVisible))
            {
                icmpGroupBox.Visible = _viewModel.IsIcmpSectionVisible;
            }
        }

        private void ProtocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (protocolComboBox.SelectedItem is ProtocolTypes selectedProtocol)
            {
                _viewModel.SelectedProtocol = selectedProtocol;
            }
        }

        private async void OkButton_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren())
            {
                Messenger.MessageBox("Please correct the validation errors before submitting.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ruleNameTextBox.Text))
            {
                Messenger.MessageBox("Rule name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ruleNameTextBox.Focus();
                return;
            }

            if (protocolComboBox.SelectedItem is not ProtocolTypes selectedProtocol)
            {
                Messenger.MessageBox("A valid protocol must be selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_originalRuleVm == null)
            {
                bool hasService = !string.IsNullOrWhiteSpace(serviceNameTextBox.Text);
                bool hasWildcardPorts = string.IsNullOrWhiteSpace(localPortsTextBox.Text) || localPortsTextBox.Text.Trim() == "*" ||
                                        string.IsNullOrWhiteSpace(remotePortsTextBox.Text) || remotePortsTextBox.Text.Trim() == "*";
                bool protocolIsNotAny = selectedProtocol.Value != ProtocolTypes.Any.Value;

                if (hasService && hasWildcardPorts && protocolIsNotAny)
                {
                    Messenger.MessageBox("When creating a rule for a service with a specific protocol (like TCP or UDP), you must also specify concrete Local and Remote ports. Wildcards (*) are only allowed if the protocol is 'Any'.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string groupName = groupComboBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(groupName))
            {
                groupName = MFWConstants.MainRuleGroup;
            }

            // Calculate values directly to avoid redundant assignment
            bool isTcpOrUdp = selectedProtocol.Value == ProtocolTcp || selectedProtocol.Value == ProtocolUdp;
            string finalLocalPorts = isTcpOrUdp ? (string.IsNullOrWhiteSpace(localPortsTextBox.Text) ? "*" : localPortsTextBox.Text) : "";
            string finalRemotePorts = isTcpOrUdp ? (string.IsNullOrWhiteSpace(remotePortsTextBox.Text) ? "*" : remotePortsTextBox.Text) : "";

            bool isIcmp = selectedProtocol.Value == ProtocolIcmpV4 || selectedProtocol.Value == ProtocolIcmpV6;
            string finalIcmp = isIcmp ? icmpTypesAndCodesTextBox.Text : "";

            // --- DNS resolution logic ---
            string rawRemoteAddress = remoteAddressTextBox.Text.Trim();
            string finalRemoteAddresses = "*";
            string domainTag = "";

            if (!string.IsNullOrWhiteSpace(rawRemoteAddress) && rawRemoteAddress != "*")
            {
                var resolvedIps = new List<string>();
                var domains = new List<string>();
                var parts = rawRemoteAddress.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Disable UI while resolving
                okButton.Enabled = false;
                okButton.Text = "Resolving...";

                foreach (var part in parts)
                {
                    bool isKeyword = new[] { "LocalSubnet", "DNS", "DHCP", "WINS", "DefaultGateway" }.Contains(part, StringComparer.OrdinalIgnoreCase);
                    bool isIpOrSubnet = Uri.CheckHostName(part) == UriHostNameType.IPv4 || Uri.CheckHostName(part) == UriHostNameType.IPv6 || part.Contains('/') || part.Contains('-');

                    if (isKeyword || isIpOrSubnet)
                    {
                        resolvedIps.Add(part);
                    }
                    else
                    {
                        try
                        {
                            var ips = await System.Net.Dns.GetHostAddressesAsync(part);
                            resolvedIps.AddRange(ips.Select(ip => ip.ToString()));
                            domains.Add(part);
                        }
                        catch (Exception ex)
                        {
                            Messenger.MessageBox($"Could not resolve domain '{part}'. Check spelling or internet connection.\n\nError: {ex.Message}", "DNS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            okButton.Enabled = true;
                            okButton.Text = "OK";
                            return; // Stop creation on failure
                        }
                    }
                }

                okButton.Enabled = true;
                okButton.Text = "OK";
                finalRemoteAddresses = resolvedIps.Count > 0 ? string.Join(",", resolvedIps.Distinct()) : "*";

                if (domains.Count > 0)
                {
                    domainTag = $" [MFW-Domain: {string.Join(",", domains)}]";
                }
            }
            // -----

            var rule = new AdvancedRuleViewModel
            {
                Name = ruleNameTextBox.Text.Trim(),
                Description = (descriptionTextBox.Text.Trim() + domainTag).Trim(),
                IsEnabled = enabledCheckBox.Checked,
                Grouping = groupName,
                Status = allowRadioButton.Checked ? "Allow" : "Block",
                Direction = GetDirection(),
                Protocol = selectedProtocol.Value,
                ProtocolName = selectedProtocol.Name,
                ApplicationName = programPathTextBox.Text.Trim(),
                ServiceName = serviceNameTextBox.Text.Trim(),

                LocalPorts = finalLocalPorts.Trim(),
                RemotePorts = finalRemotePorts.Trim(),

                LocalAddresses = (string.IsNullOrWhiteSpace(localAddressTextBox.Text) ? "*" : localAddressTextBox.Text).Trim(),
                RemoteAddresses = finalRemoteAddresses, // Use the resolved IPs here
                Profiles = GetProfileString(),
                Type = RuleType.Advanced,
                InterfaceTypes = GetInterfaceTypes(),

                IcmpTypesAndCodes = finalIcmp.Trim()
            };

            RuleVm = rule;

            DialogResult = DialogResult.OK;
            Close();
        }

        private Directions GetDirection() => inboundRadioButton.Checked ? Directions.Incoming : outboundRadioButton.Checked ? Directions.Outgoing : Directions.Incoming | Directions.Outgoing;

        private string GetProfileString()
        {
            var profiles = new List<string>(3);
            if (domainCheckBox.Checked)
            {
                profiles.Add("Domain");
            }

            if (privateCheckBox.Checked)
            {
                profiles.Add("Private");
            }

            if (publicCheckBox.Checked)
            {
                profiles.Add("Public");
            }

            if (profiles.Count == 3 || profiles.Count == 0)
            {
                return "All";
            }

            return string.Join(", ", profiles);
        }

        public string GetInterfaceTypes()
        {
            var types = new List<string>(3);
            if (remoteAccessCheckBox.Checked)
            {
                types.Add("RemoteAccess");
            }

            if (wirelessCheckBox.Checked)
            {
                types.Add("Wireless");
            }

            if (lanCheckBox.Checked)
            {
                types.Add("Lan");
            }

            if (types.Count == 3 || types.Count == 0)
            {
                return "All";
            }

            return string.Join(",", types);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a program"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                programPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void BrowseServiceButton_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                browseServiceButton.Enabled = false;

                var services = await Task.Run(() => SystemDiscoveryService.GetServicesWithExePaths());

                using var browseForm = new BrowseServicesForm(services, _appSettings);
                if (browseForm.ShowDialog(this) == DialogResult.OK && browseForm.SelectedService != null)
                {
                    serviceNameTextBox.Text = browseForm.SelectedService.ServiceName;
                    if (!string.IsNullOrEmpty(browseForm.SelectedService.ExePath))
                    {
                        programPathTextBox.Text = PathResolver.NormalizePath(browseForm.SelectedService.ExePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox($"Error loading services: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                browseServiceButton.Enabled = true;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddGroupButton_Click(object sender, EventArgs e)
        {
            string newGroupName = groupComboBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newGroupName) && !newGroupName.EndsWith(MFWConstants.MfwRuleSuffix))
            {
                newGroupName += MFWConstants.MfwRuleSuffix;
            }

            if (!string.IsNullOrWhiteSpace(newGroupName) && !groupComboBox.Items.Contains(newGroupName))
            {
                groupComboBox.Items.Add(newGroupName);
                groupComboBox.SelectedItem = newGroupName;
            }
        }

        private delegate bool ValidationDelegate(string input, out string errorMessage);

        private void ValidateTextBox(object sender, CancelEventArgs e, ValidationDelegate validationMethod)
        {
            if (sender is TextBox textBox)
            {
                if (!validationMethod(textBox.Text, out string errorMessage))
                {
                    errorProvider1.SetError(textBox, errorMessage);
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(textBox, string.Empty);
                }
            }
        }

        private void ValidatePortTextBox_Validating(object sender, CancelEventArgs e) => ValidateTextBox(sender, e, ValidationUtility.ValidatePortString);
        private void LocalAddressTextBox_Validating(object sender, CancelEventArgs e) => ValidateTextBox(sender, e, ValidationUtility.ValidateAddressString);
        private void RemoteAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            string input = remoteAddressTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input) || input == "*")
            {
                errorProvider1.SetError(remoteAddressTextBox, string.Empty);
                return;
            }

            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                bool isKeyword = new[] { "LocalSubnet", "DNS", "DHCP", "WINS", "DefaultGateway" }.Contains(part, StringComparer.OrdinalIgnoreCase);
                bool isIpOrSubnet = Uri.CheckHostName(part) == UriHostNameType.IPv4 || Uri.CheckHostName(part) == UriHostNameType.IPv6 || part.Contains('/') || part.Contains('-');
                bool isDomain = Uri.CheckHostName(part) == UriHostNameType.Dns;

                if (!isKeyword && !isIpOrSubnet && !isDomain)
                {
                    errorProvider1.SetError(remoteAddressTextBox, $"Invalid format (IP, CIDR, Keyword, or Domain expected): {part}");
                    e.Cancel = true;
                    return;
                }
            }
            errorProvider1.SetError(remoteAddressTextBox, string.Empty);
        }
        private void IcmpTypesAndCodesTextBox_Validating(object sender, CancelEventArgs e) => ValidateTextBox(sender, e, ValidationUtility.ValidateIcmpString);

        private void ProgramGroupBox_Enter(object sender, EventArgs e)
        {

        }
    }
}
