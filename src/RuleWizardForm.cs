// File: RuleWizardForm.cs
using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System.IO;
using NetFwTypeLib;
using System.Net;

namespace MinimalFirewall
{
    public partial class RuleWizardForm : Form
    {
        private enum WizardStep { Selection, GetAction, GetProgram, GetDirection, GetPorts, GetProtocol, GetName, Summary, GetService, GetFileShareIP, GetBlockDeviceIP, GetRestrictApp }
        private enum RuleTemplate { None, ProgramRule, PortRule, BlockService, AllowFileShare, BlockDevice, RestrictApp }

        private WizardStep _currentStep = WizardStep.Selection;
        private RuleTemplate _selectedTemplate = RuleTemplate.None;
        private readonly Stack<WizardStep> _history = new Stack<WizardStep>();

        private readonly FirewallActionsService _actionsService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly INetFwPolicy2 _firewallPolicy;
        private readonly DarkModeCS dm;

        private string _wizardAppPath = "";
        private string _wizardPorts = "";
        private int _wizardProtocol = 0;
        private string _wizardRuleName = "";
        private Actions _wizardAction = Actions.Allow;
        private Directions _wizardDirection = Directions.Outgoing;
        private string _wizardServiceName = "";
        private string _wizardRemoteIP = "";
        public RuleWizardForm(FirewallActionsService actionsService, WildcardRuleService wildcardRuleService, BackgroundFirewallTaskService backgroundTaskService, INetFwPolicy2 firewallPolicy)
        {
            InitializeComponent();
            dm = new DarkModeCS(this);
            _actionsService = actionsService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _firewallPolicy = firewallPolicy;

            GoToStep(WizardStep.Selection);
        }

        private void GoForwardTo(WizardStep newStep)
        {
            _history.Push(_currentStep);
            GoToStep(newStep);
        }

        private void GoBack()
        {
            if (_history.Count > 0)
            {
                var previousStep = _history.Pop();
                GoToStep(previousStep);
            }
        }

        private void GoToStep(WizardStep newStep)
        {
            _currentStep = newStep;
            pnlSelection.Visible = _currentStep == WizardStep.Selection;
            pnlGetAction.Visible = _currentStep == WizardStep.GetAction;
            pnlGetProgram.Visible = _currentStep == WizardStep.GetProgram;
            pnlGetDirection.Visible = _currentStep == WizardStep.GetDirection;
            pnlGetPorts.Visible = _currentStep == WizardStep.GetPorts;
            pnlGetProtocol.Visible = _currentStep == WizardStep.GetProtocol;
            pnlGetName.Visible = _currentStep == WizardStep.GetName;
            pnlSummary.Visible = _currentStep == WizardStep.Summary;
            pnlGetService.Visible = _currentStep == WizardStep.GetService;
            pnlGetFileShareIP.Visible = _currentStep == WizardStep.GetFileShareIP;
            pnlGetBlockDeviceIP.Visible = _currentStep == WizardStep.GetBlockDeviceIP;
            pnlGetRestrictApp.Visible = _currentStep == WizardStep.GetRestrictApp;
            backButton.Enabled = _history.Count > 0;
            nextButton.Text = "Next";

            switch (_currentStep)
            {
                case WizardStep.Selection:
                    this.Text = "Create New Rule";
                    mainHeaderLabel.Text = "What would you like to do?";
                    backButton.Enabled = false;
                    nextButton.Visible = false;
                    break;
                case WizardStep.GetAction:
                    this.Text = "Step 1: Choose Action";
                    mainHeaderLabel.Text = "Do you want to allow or block the program?";
                    nextButton.Visible = true;
                    break;
                case WizardStep.GetProgram:
                    this.Text = "Step 2: Select a Program";
                    mainHeaderLabel.Text = "Select the program's main executable file (.exe)";
                    nextButton.Visible = true;
                    break;
                case WizardStep.GetDirection:
                    this.Text = "Step 3: Choose Direction";
                    mainHeaderLabel.Text = "Apply this rule to which connection direction?";
                    nextButton.Text = "Finish";
                    break;
                case WizardStep.GetPorts:
                    this.Text = "Step 1: Enter Ports";
                    mainHeaderLabel.Text = "What port or port range is needed?";
                    nextButton.Visible = true;
                    break;
                case WizardStep.GetProtocol:
                    this.Text = "Step 2: Select Protocol";
                    mainHeaderLabel.Text = "What protocol does it use?";
                    break;
                case WizardStep.GetName:
                    this.Text = "Step 3: Name Your Rule";
                    mainHeaderLabel.Text = "Give your new rule a descriptive name.";
                    nextButton.Text = "Finish";
                    break;
                case WizardStep.GetService:
                    this.Text = "Step 1: Select a Service";
                    mainHeaderLabel.Text = "Select a Windows Service to block";
                    nextButton.Visible = true;
                    if (serviceListBox.Items.Count == 0)
                    {
                        var services = SystemDiscoveryService.GetServicesWithExePaths();
                        foreach (var service in services.OrderBy(s => s.DisplayName))
                        {
                            serviceListBox.Items.Add($"{service.DisplayName} ({service.ServiceName})");
                        }
                    }
                    break;
                case WizardStep.GetFileShareIP:
                    this.Text = "Step 1: Enter IP Address";
                    mainHeaderLabel.Text = "Enter the local IP of the trusted computer";
                    nextButton.Visible = true;
                    nextButton.Text = "Finish";
                    break;
                case WizardStep.GetBlockDeviceIP:
                    this.Text = "Step 1: Enter IP Address";
                    mainHeaderLabel.Text = "Enter the local IP of the device to block";
                    nextButton.Visible = true;
                    nextButton.Text = "Finish";
                    break;
                case WizardStep.GetRestrictApp:
                    this.Text = "Step 1: Select a Program";
                    mainHeaderLabel.Text = "Select the program to restrict to your local network";
                    nextButton.Visible = true;
                    nextButton.Text = "Finish";
                    break;
                case WizardStep.Summary:
                    this.Text = "Summary";
                    mainHeaderLabel.Text = "The following rule will be created:";
                    BuildSummary();
                    nextButton.Text = "Finish";
                    break;
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (!ValidateStep()) return;
            ProcessStepLogic();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void programRuleButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.ProgramRule;
            GoForwardTo(WizardStep.GetAction);
        }

        private void portRuleButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.PortRule;
            GoForwardTo(WizardStep.GetPorts);
        }

        private void browseButton_Click(object sender, EventArgs e)
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

        private void portsBrowseButton_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a program"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                portsProgramPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void restrictAppBrowseButton_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a program"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                restrictAppPathTextBox.Text = openFileDialog.FileName;
            }
        }


        private void wildcardRuleButton_Click(object sender, EventArgs e)
        {
            using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService);
            if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
            {
                var newRule = new WildcardRule
                {
                    FolderPath = wildcardDialog.FolderPath,
                    ExeName = wildcardDialog.ExeName,
                    Action = wildcardDialog.FinalAction
                };
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AddWildcardRule, newRule));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void advancedRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new CreateAdvancedRuleForm(_firewallPolicy, _actionsService);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (dialog.RuleVm != null)
                {
                    var payload = new CreateAdvancedRulePayload { ViewModel = dialog.RuleVm, InterfaceTypes = dialog.RuleVm.InterfaceTypes, IcmpTypesAndCodes = dialog.RuleVm.IcmpTypesAndCodes };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void blockServiceButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.BlockService;
            GoForwardTo(WizardStep.GetService);
        }

        private void allowFileShareButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.AllowFileShare;
            GoForwardTo(WizardStep.GetFileShareIP);
        }

        private void blockDeviceButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.BlockDevice;
            GoForwardTo(WizardStep.GetBlockDeviceIP);
        }

        private void restrictAppButton_Click(object sender, EventArgs e)
        {
            _selectedTemplate = RuleTemplate.RestrictApp;
            GoForwardTo(WizardStep.GetRestrictApp);
        }

        private void restrictToProgramCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = restrictToProgramCheckBox.Checked;
            portsProgramPathTextBox.Visible = isChecked;
            portsBrowseButton.Visible = isChecked;
            if (!isChecked)
            {
                portsProgramPathTextBox.Text = string.Empty;
            }
        }

        private bool ValidateStep()
        {
            switch (_currentStep)
            {
                case WizardStep.GetProgram:
                    if (string.IsNullOrWhiteSpace(programPathTextBox.Text) || !File.Exists(Environment.ExpandEnvironmentVariables(programPathTextBox.Text)))
                    {
                        Messenger.MessageBox("Please select a valid program file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetPorts:
                    if (string.IsNullOrWhiteSpace(portsTextBox.Text))
                    {
                        Messenger.MessageBox("Please enter a port or port range.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    if (restrictToProgramCheckBox.Checked && (string.IsNullOrWhiteSpace(portsProgramPathTextBox.Text) || !File.Exists(Environment.ExpandEnvironmentVariables(portsProgramPathTextBox.Text))))
                    {
                        Messenger.MessageBox("Please select a valid program file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetName:
                    if (string.IsNullOrWhiteSpace(ruleNameTextBox.Text))
                    {
                        Messenger.MessageBox("Please enter a name for the rule.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetService:
                    if (serviceListBox.SelectedItem == null && string.IsNullOrWhiteSpace(serviceNameTextBox.Text))
                    {
                        Messenger.MessageBox("Please select a service from the list or enter a service name.", "No Service Selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetFileShareIP:
                    if (string.IsNullOrWhiteSpace(fileShareIpTextBox.Text) || !IPAddress.TryParse(fileShareIpTextBox.Text, out _))
                    {
                        Messenger.MessageBox("Please enter a valid IP address.", "Invalid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetBlockDeviceIP:
                    if (string.IsNullOrWhiteSpace(blockDeviceIpTextBox.Text) || !IPAddress.TryParse(blockDeviceIpTextBox.Text, out _))
                    {
                        Messenger.MessageBox("Please enter a valid IP address.", "Invalid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
                case WizardStep.GetRestrictApp:
                    if (string.IsNullOrWhiteSpace(restrictAppPathTextBox.Text) || !File.Exists(Environment.ExpandEnvironmentVariables(restrictAppPathTextBox.Text)))
                    {
                        Messenger.MessageBox("Please select a valid program file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void ProcessStepLogic()
        {
            switch (_currentStep)
            {
                case WizardStep.GetAction:
                    _wizardAction = allowActionRadioButton.Checked ? Actions.Allow : Actions.Block;
                    GoForwardTo(WizardStep.GetProgram);
                    break;

                case WizardStep.GetProgram:
                    _wizardAppPath = programPathTextBox.Text;
                    GoForwardTo(WizardStep.GetDirection);
                    break;

                case WizardStep.GetDirection:
                    if (inboundRadioButton.Checked) _wizardDirection = Directions.Incoming;
                    else if (outboundRadioButton.Checked) _wizardDirection = Directions.Outgoing;
                    else _wizardDirection = Directions.Incoming | Directions.Outgoing;
                    GoForwardTo(WizardStep.Summary);
                    break;
                case WizardStep.GetPorts:
                    _wizardPorts = portsTextBox.Text;
                    _wizardAppPath = restrictToProgramCheckBox.Checked ? portsProgramPathTextBox.Text : string.Empty;
                    GoForwardTo(WizardStep.GetProtocol);
                    break;

                case WizardStep.GetProtocol:
                    if (tcpRadioButton.Checked) _wizardProtocol = 6;
                    else if (udpRadioButton.Checked) _wizardProtocol = 17;
                    else _wizardProtocol = 256;
                    ruleNameTextBox.Text = string.IsNullOrEmpty(_wizardAppPath)
                        ? $"Allow Port {_wizardPorts}"
                        : $"Allow {Path.GetFileNameWithoutExtension(_wizardAppPath)} Port {_wizardPorts}";
                    GoForwardTo(WizardStep.GetName);
                    break;

                case WizardStep.GetName:
                    _wizardRuleName = ruleNameTextBox.Text;
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.GetService:
                    if (serviceListBox.SelectedItem != null)
                    {
                        string selected = serviceListBox.SelectedItem.ToString();
                        _wizardServiceName = selected.Substring(selected.LastIndexOf('(') + 1).TrimEnd(')');
                    }
                    else
                    {
                        _wizardServiceName = serviceNameTextBox.Text;
                    }
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.GetFileShareIP:
                    _wizardRemoteIP = fileShareIpTextBox.Text;
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.GetBlockDeviceIP:
                    _wizardRemoteIP = blockDeviceIpTextBox.Text;
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.GetRestrictApp:
                    _wizardAppPath = restrictAppPathTextBox.Text;
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.Summary:
                    CreateRule();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
            }
        }

        private void BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            switch (_selectedTemplate)
            {
                case RuleTemplate.ProgramRule:
                    sb.AppendLine($"Action: {_wizardAction}");
                    sb.AppendLine($"Direction: {_wizardDirection}");
                    sb.AppendLine($"Program: {_wizardAppPath}");
                    sb.AppendLine($"Details: All protocols, all ports");
                    break;
                case RuleTemplate.PortRule:
                    sb.AppendLine($"Rule Name: {_wizardRuleName}");
                    sb.AppendLine($"Action: Allow");
                    sb.AppendLine($"Direction: Incoming & Outgoing");
                    if (!string.IsNullOrEmpty(_wizardAppPath))
                    {
                        sb.AppendLine($"Program: {_wizardAppPath}");
                    }
                    sb.AppendLine($"Ports: {_wizardPorts}");
                    sb.AppendLine($"Protocol: {(_wizardProtocol == 6 ? "TCP" : _wizardProtocol == 17 ? "UDP" : "TCP & UDP")}");
                    break;
                case RuleTemplate.BlockService:
                    sb.AppendLine("Action: Block");
                    sb.AppendLine("Direction: Incoming & Outgoing");
                    sb.AppendLine($"Service: {_wizardServiceName}");
                    break;
                case RuleTemplate.AllowFileShare:
                    sb.AppendLine("Action: Allow");
                    sb.AppendLine("Direction: Inbound");
                    sb.AppendLine("Protocol: TCP");
                    sb.AppendLine("Local Port: 445 (File Sharing)");
                    sb.AppendLine($"From IP Address: {_wizardRemoteIP}");
                    break;
                case RuleTemplate.BlockDevice:
                    sb.AppendLine("Action: Block");
                    sb.AppendLine("Direction: Inbound");
                    sb.AppendLine("Protocol: Any");
                    sb.AppendLine($"From IP Address: {_wizardRemoteIP}");
                    break;
                case RuleTemplate.RestrictApp:
                    sb.AppendLine("Action: Allow on Local Network Only");
                    sb.AppendLine("(Note: This requires 'Lockdown Mode' to be active to block internet access.)");
                    sb.AppendLine("Direction: Inbound & Outbound");
                    sb.AppendLine($"Program: {_wizardAppPath}");
                    break;
            }
            summaryLabel.Text = sb.ToString();
        }

        private void CreateRule()
        {
            switch (_selectedTemplate)
            {
                case RuleTemplate.ProgramRule:
                    string action = $"{_wizardAction} ({_wizardDirection})";
                    var payload = new ApplyApplicationRulePayload { AppPaths = { _wizardAppPath }, Action = action };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, payload));
                    break;
                case RuleTemplate.PortRule:
                    var vm = new AdvancedRuleViewModel
                    {
                        Name = _wizardRuleName,
                        IsEnabled = true,
                        Status = "Allow",
                        Direction = Directions.Incoming | Directions.Outgoing,
                        Protocol = (short)_wizardProtocol,
                        LocalPorts = _wizardPorts,
                        ApplicationName = string.IsNullOrEmpty(_wizardAppPath) ? "*" : _wizardAppPath,
                        Grouping = MFWConstants.MainRuleGroup,
                        RemotePorts = "*",
                        LocalAddresses = "*",
                        RemoteAddresses = "*",
                        Profiles = "All",
                        Type = RuleType.Advanced
                    };
                    var advPayload = new CreateAdvancedRulePayload { ViewModel = vm, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, advPayload));
                    break;
                case RuleTemplate.BlockService:
                    var servicePayload = new ApplyServiceRulePayload { ServiceName = _wizardServiceName, Action = "Block (All)" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload));
                    break;
                case RuleTemplate.AllowFileShare:
                    var fileShareVm = new AdvancedRuleViewModel
                    {
                        Name = $"Allow File Sharing from {_wizardRemoteIP}",
                        Description = "Allows inbound file sharing (SMB)",
                        IsEnabled = true,
                        Status = "Allow",
                        Direction = Directions.Incoming,
                        Protocol = 6, // TCP
                        LocalPorts = "445",
                        RemoteAddresses = _wizardRemoteIP,
                        Grouping = MFWConstants.MainRuleGroup,
                        Type = RuleType.Advanced,
                        RemotePorts = "*",
                        LocalAddresses = "*",
                        Profiles = "All"
                    };
                    var fileSharePayload = new CreateAdvancedRulePayload { ViewModel = fileShareVm, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, fileSharePayload));
                    break;
                case RuleTemplate.BlockDevice:
                    var blockDeviceVm = new AdvancedRuleViewModel
                    {
                        Name = $"Block Inbound from {_wizardRemoteIP}",
                        Description = "Blocks all inbound traffic from a specific local IP",
                        IsEnabled = true,
                        Status = "Block",
                        Direction = Directions.Incoming,
                        Protocol = 256, // Any
                        RemoteAddresses = _wizardRemoteIP,
                        Grouping = MFWConstants.MainRuleGroup,
                        Type = RuleType.Advanced,
                        LocalPorts = "*",
                        RemotePorts = "*",
                        LocalAddresses = "*",
                        Profiles = "All"
                    };
                    var blockDevicePayload = new CreateAdvancedRulePayload { ViewModel = blockDeviceVm, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, blockDevicePayload));
                    break;
                case RuleTemplate.RestrictApp:
                    string appName = Path.GetFileNameWithoutExtension(_wizardAppPath);
                    var allowLocalVm = new AdvancedRuleViewModel
                    {
                        Name = $"{appName} - Allow Local Network",
                        Description = "Allows communication only within the local network. This rule only works as intended if Lockdown Mode is active.",
                        IsEnabled = true,
                        Status = "Allow",
                        Direction = Directions.Incoming | Directions.Outgoing,
                        ApplicationName = _wizardAppPath,
                        Protocol = 256,
                        RemoteAddresses = "10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,LocalSubnet",
                        Grouping = MFWConstants.MainRuleGroup,
                        Type = RuleType.Advanced,
                        LocalPorts = "*",
                        RemotePorts = "*",
                        LocalAddresses = "*",
                        Profiles = "All"
                    };
                    var allowPayload = new CreateAdvancedRulePayload { ViewModel = allowLocalVm, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, allowPayload));
                    break;
            }
        }
    }
}