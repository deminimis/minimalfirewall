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

            if (!string.IsNullOrWhiteSpace(programPathTextBox.Text) && !string.IsNullOrWhiteSpace(serviceNameTextBox.Text))
            {
                MessageBox.Show("A rule cannot specify both a program path and a service name. Please choose one.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string groupName = groupComboBox.Text;
            if (string.IsNullOrWhiteSpace(groupName))
            {
                groupName = MFWConstants.MainRuleGroup;
            }
            else if (!groupName.EndsWith(MFWConstants.MfwRuleSuffix))
            {
                groupName += MFWConstants.MfwRuleSuffix;
            }

            if (protocolComboBox.SelectedItem is not ProtocolTypes selectedProtocol)
            {
                MessageBox.Show("A valid protocol must be selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
                ApplicationName = programPathTextBox.Text,
                ServiceName = serviceNameTextBox.Text,
                LocalPorts = ParsingUtility.ParseStringToList<PortRange>(localPortsTextBox.Text, PortRange.TryParse),
                RemotePorts = ParsingUtility.ParseStringToList<PortRange>(remotePortsTextBox.Text, PortRange.TryParse),
                LocalAddresses = ParsingUtility.ParseStringToList<IPAddressRange>(localAddressTextBox.Text, IPAddressRange.TryParse),
                RemoteAddresses = ParsingUtility.ParseStringToList<IPAddressRange>(remoteAddressTextBox.Text, IPAddressRange.TryParse),
                Profiles = GetProfileString(),
                Type = RuleType.Advanced
            };
            _actionsService.CreateAdvancedRule(rule, GetInterfaceTypes(), icmpTypesAndCodesTextBox.Text);
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
            var profiles = new List<string>();
            if (domainCheckBox.Checked) profiles.Add("Domain");
            if (privateCheckBox.Checked) profiles.Add("Private");
            if (publicCheckBox.Checked) profiles.Add("Public");
            if (profiles.Count == 3 || profiles.Count == 0) return "All";
            return string.Join(", ", profiles);
        }

        private string GetInterfaceTypes()
        {
            var types = new List<string>();
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