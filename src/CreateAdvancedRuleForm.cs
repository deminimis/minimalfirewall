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

        private readonly DarkModeCS dm;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallRuleViewModel _viewModel;
        private readonly FirewallGroupManager _groupManager;
        private readonly ToolTip _toolTip;
        private readonly AppSettings _appSettings;
        public AdvancedRuleViewModel? RuleVm { get; private set; }
        private readonly AdvancedRuleViewModel? _originalRuleVm;

        public CreateAdvancedRuleForm(FirewallActionsService actionsService, AppSettings appSettings)
        {
            InitializeComponent();
            _appSettings = appSettings;


            dm = new DarkModeCS(this)
            {
                ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode
            };
            dm.ApplyTheme(appSettings.Theme == "Dark");

            _actionsService = actionsService;
            _groupManager = new FirewallGroupManager();
            _toolTip = new ToolTip();
            _viewModel = new FirewallRuleViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;


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
            _toolTip.SetToolTip(groupComboBox, "Select an existing group, or type a new name to create a new group.");
            _toolTip.SetToolTip(serviceNameTextBox, "Enter the exact service name (not display name).");
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
            this.Text = "Edit Advanced Rule";
            PopulateFormFromRule(ruleToEdit);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var workingArea = Screen.FromControl(this).WorkingArea;
            if (this.Height > workingArea.Height)
            {
                this.Height = workingArea.Height;
            }
            this.CenterToParent();
        }

        private void PopulateFormFromRule(AdvancedRuleViewModel rule)
        {
            ruleNameTextBox.Text = rule.Name;
            descriptionTextBox.Text = rule.Description;
            enabledCheckBox.Checked = rule.IsEnabled;

            if (rule.Status == "Allow")
                allowRadioButton.Checked = true;
            else
                blockRadioButton.Checked = true;

            if (rule.Direction == (Directions.Incoming | Directions.Outgoing))
                bothDirRadioButton.Checked = true;
            else if (rule.Direction == Directions.Incoming)
                inboundRadioButton.Checked = true;
            else
                outboundRadioButton.Checked = true;

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

            _viewModel.SelectedProtocol = (ProtocolTypes)protocolComboBox.SelectedItem;

            localPortsTextBox.Text = rule.LocalPorts;
            remotePortsTextBox.Text = rule.RemotePorts;

            localAddressTextBox.Text = rule.LocalAddresses;
            remoteAddressTextBox.Text = rule.RemoteAddresses;

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

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
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
            string finalLocalPorts;
            string finalRemotePorts;
            string finalIcmp;

            if (selectedProtocol.Value == ProtocolTcp || selectedProtocol.Value == ProtocolUdp)
            {
                finalLocalPorts = string.IsNullOrWhiteSpace(localPortsTextBox.Text) ? "*" : localPortsTextBox.Text;
                finalRemotePorts = string.IsNullOrWhiteSpace(remotePortsTextBox.Text) ? "*" : remotePortsTextBox.Text;
            }
            else
            {
                finalLocalPorts = "";
                finalRemotePorts = "";
            }

            if (selectedProtocol.Value == ProtocolIcmpV4 || selectedProtocol.Value == ProtocolIcmpV6)
            {
                finalIcmp = icmpTypesAndCodesTextBox.Text;
            }
            else
            {
                finalIcmp = "";
            }

            var rule = new AdvancedRuleViewModel
            {
                Name = ruleNameTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
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
                RemoteAddresses = (string.IsNullOrWhiteSpace(remoteAddressTextBox.Text) ? "*" : remoteAddressTextBox.Text).Trim(),
                Profiles = GetProfileString(),
                Type = RuleType.Advanced,
                InterfaceTypes = GetInterfaceTypes(),

                IcmpTypesAndCodes = finalIcmp.Trim()
            };

            this.RuleVm = rule;

            DialogResult = DialogResult.OK;
            Close();
        }

        private Directions GetDirection()
        {
            if (inboundRadioButton.Checked) return Directions.Incoming;
            if (outboundRadioButton.Checked) return Directions.Outgoing;
            return Directions.Incoming | Directions.Outgoing;
        }

        private string GetProfileString()
        {
            var profiles = new List<string>(3);
            if (domainCheckBox.Checked) profiles.Add("Domain");
            if (privateCheckBox.Checked) profiles.Add("Private");
            if (publicCheckBox.Checked) profiles.Add("Public");
            if (profiles.Count == 3 || profiles.Count == 0) return "All";
            return string.Join(", ", profiles);
        }

        public string GetInterfaceTypes()
        {
            var types = new List<string>(3);
            if (remoteAccessCheckBox.Checked) types.Add("RemoteAccess");
            if (wirelessCheckBox.Checked) types.Add("Wireless");
            if (lanCheckBox.Checked) types.Add("Lan");
            if (types.Count == 3 || types.Count == 0) return "All";
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

        private async void browseServiceButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
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
                this.Cursor = Cursors.Default;
                browseServiceButton.Enabled = true;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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

        private void localPortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
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

        private void remotePortsTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidatePortString(textBox.Text, out string errorMessage))
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

        private void localAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidateAddressString(textBox.Text, out string errorMessage))
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

        private void remoteAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidateAddressString(textBox.Text, out string errorMessage))
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

        private void icmpTypesAndCodesTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!ValidationUtility.ValidateIcmpString(textBox.Text, out string errorMessage))
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
    }
}