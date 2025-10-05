// File: CreateAdvancedRuleForm.cs
using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System.ComponentModel;
using NetFwTypeLib;
using MinimalFirewall.Groups;

namespace MinimalFirewall
{
    public partial class CreateAdvancedRuleForm : Form
    {
        private readonly DarkModeCS dm;
        private readonly FirewallActionsService _actionsService;
        private readonly FirewallRuleViewModel _viewModel;
        private readonly FirewallGroupManager _groupManager;
        private readonly ToolTip _toolTip;
        public AdvancedRuleViewModel? RuleVm { get; private set; }
        private readonly AdvancedRuleViewModel? _originalRuleVm;
        public CreateAdvancedRuleForm(INetFwPolicy2 firewallPolicy, FirewallActionsService actionsService)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _actionsService = actionsService;
            _groupManager = new FirewallGroupManager(firewallPolicy);
            _toolTip = new ToolTip();

            _viewModel = new FirewallRuleViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            protocolComboBox.Items.AddRange([
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
            this.Load += (sender, e) =>
            {
                var workingArea = Screen.FromControl(this).WorkingArea;
                if (this.Height > workingArea.Height)
                {
                    this.Height = workingArea.Height;
                }
                this.CenterToParent();
            };
        }

        public CreateAdvancedRuleForm(INetFwPolicy2 firewallPolicy, FirewallActionsService actionsService, string appPath, string direction)
        : this(firewallPolicy, actionsService)
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

        public CreateAdvancedRuleForm(INetFwPolicy2 firewallPolicy, FirewallActionsService actionsService, AdvancedRuleViewModel ruleToEdit)
            : this(firewallPolicy, actionsService)
        {
            _originalRuleVm = ruleToEdit;
            this.Text = "Edit Advanced Rule";
            PopulateFormFromRule(ruleToEdit);
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

            if (!string.IsNullOrEmpty(rule.ServiceName))
            {
                serviceNameTextBox.Text = rule.ServiceName;
                programPathTextBox.Text = rule.ApplicationName;
                programPathTextBox.ReadOnly = true;
                browseButton.Enabled = false;
            }
            else
            {
                programPathTextBox.Text = rule.ApplicationName;
                serviceNameTextBox.Text = string.Empty;
                programPathTextBox.ReadOnly = false;
                browseButton.Enabled = true;
            }


            int protocolIndex = -1;
            var items = protocolComboBox.Items.OfType<ProtocolTypes>().ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Value == rule.Protocol)
                {
                    protocolIndex = i;
                    break;
                }
            }

            if (protocolIndex != -1)
                protocolComboBox.SelectedIndex = protocolIndex;
            else
                protocolComboBox.SelectedItem = ProtocolTypes.Any;
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
            var groupNames = new HashSet<string>(groups.Select(g => g.Name));

            groupNames.Add(MFWConstants.MainRuleGroup);
            groupNames.Add(MFWConstants.WildcardRuleGroup);

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
            if (string.IsNullOrWhiteSpace(ruleNameTextBox.Text))
            {
                MessageBox.Show("Rule name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!programPathTextBox.ReadOnly && !string.IsNullOrWhiteSpace(programPathTextBox.Text) && !string.IsNullOrWhiteSpace(serviceNameTextBox.Text))
            {
                MessageBox.Show("A rule cannot specify both a program path and a service name. Please choose one.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (protocolComboBox.SelectedItem is not ProtocolTypes selectedProtocol)
            {
                MessageBox.Show("A valid protocol must be selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("When creating a rule for a service with a specific protocol (like TCP or UDP), you must also specify concrete Local and Remote ports. Wildcards (*) are only allowed if the protocol is 'Any'.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string groupName = groupComboBox.Text;
            if (string.IsNullOrWhiteSpace(groupName))
            {
                groupName = MFWConstants.MainRuleGroup;
            }

            var rule = new AdvancedRuleViewModel
            {
                Name = ruleNameTextBox.Text,
                Description = descriptionTextBox.Text,
                IsEnabled = enabledCheckBox.Checked,
                Grouping = groupName,
                Status = allowRadioButton.Checked ? "Allow" : "Block",
                Direction = GetDirection(),
                Protocol = selectedProtocol.Value,
                ProtocolName = selectedProtocol.Name,
                ApplicationName = programPathTextBox.Text,
                ServiceName = serviceNameTextBox.Text,
                LocalPorts = string.IsNullOrWhiteSpace(localPortsTextBox.Text) ? "*" : localPortsTextBox.Text,
                RemotePorts = string.IsNullOrWhiteSpace(remotePortsTextBox.Text) ? "*" : remotePortsTextBox.Text,
                LocalAddresses = string.IsNullOrWhiteSpace(localAddressTextBox.Text) ? "*" : localAddressTextBox.Text,
                RemoteAddresses = string.IsNullOrWhiteSpace(remoteAddressTextBox.Text) ? "*" : remoteAddressTextBox.Text,
                Profiles = GetProfileString(),
                Type = RuleType.Advanced,
                InterfaceTypes = GetInterfaceTypes(),
                IcmpTypesAndCodes = icmpTypesAndCodesTextBox.Text
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

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddGroupButton_Click(object sender, EventArgs e)
        {
            string newGroupName = groupComboBox.Text;
            if (!string.IsNullOrWhiteSpace(newGroupName) && !newGroupName.EndsWith(MFWConstants.MfwRuleSuffix))
            {
                newGroupName += MFWConstants.MfwRuleSuffix;
            }

            if (!groupComboBox.Items.Contains(newGroupName))
            {
                groupComboBox.Items.Add(newGroupName);
                groupComboBox.SelectedItem = newGroupName;
            }
        }
    }
}