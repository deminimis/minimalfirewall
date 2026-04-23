using DarkModeForms;
using MinimalFirewall.TypedObjects;
using System.IO;
using NetFwTypeLib;
using System.Net;
using System.Data;
using System.Linq;

namespace MinimalFirewall
{
    public partial class RuleWizardForm : Form
    {
        // Enums to track the state of the wizard and the type of rule being built
        private enum WizardStep { Selection, GetAction, GetProgram, GetDirection, GetPorts, GetProtocol, GetName, Summary, GetService, GetFileShareIP, GetBlockDeviceIP, GetRestrictApp, GetFolder }
        private enum RuleTemplate { None, ProgramRule, PortRule, BlockService, AllowFileShare, BlockDevice, RestrictApp, BatchProgramRule }

        // Constants 
        private const int ProtocolTCP = 6;
        private const int ProtocolUDP = 17;
        private const int ProtocolAny = 256;

        private WizardStep _currentStep = WizardStep.Selection;
        private RuleTemplate _selectedTemplate = RuleTemplate.None;
        private readonly Stack<WizardStep> _history = new Stack<WizardStep>();

        // Services injected via constructor
        private readonly FirewallActionsService _actionsService;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        private readonly DarkModeCS dm;
        private readonly AppSettings _appSettings;

        // Wizard temporary state data
        private string _wizardAppPath = "";
        private string _wizardFolderPath = "";
        private string _wizardPorts = "";
        private int _wizardProtocol = 0;
        private string _wizardRuleName = "";
        private Actions _wizardAction = Actions.Allow;
        private Directions _wizardDirection = Directions.Outgoing;
        private string _wizardServiceName = "";
        private string _wizardRemoteIP = "";

        public RuleWizardForm(FirewallActionsService actionsService, WildcardRuleService wildcardRuleService, BackgroundFirewallTaskService backgroundTaskService, AppSettings appSettings)
        {
            InitializeComponent();
            // Initialize Dark Mode theme
            dm = new DarkModeCS(this);
            dm.ColorMode = appSettings.Theme == "Dark" ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode;

            _actionsService = actionsService;
            _wildcardRuleService = wildcardRuleService;
            _backgroundTaskService = backgroundTaskService;
            _appSettings = appSettings;

            this.AcceptButton = nextButton;
            this.CancelButton = cancelButton;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dm.ApplyTheme(_appSettings.Theme == "Dark");
            GoToStep(WizardStep.Selection);
        }

        #region Navigation Logic

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

        private Panel SetupStepUI(Panel panel, string title, string headerText, string nextBtnText = "Next")
        {
            this.Text = title;
            mainHeaderLabel.Text = headerText;
            nextButton.Text = nextBtnText;
            return panel;
        }

        private void GoToStep(WizardStep newStep)
        {
            _currentStep = newStep;

            foreach (Control c in this.Controls)
            {
                if (c is Panel p && p.Name.StartsWith("pnl") && p.Name != "bottomPanel" && p.Name != "topPanel")
                {
                    p.Visible = false;
                }
            }

            Panel activePanel = null;

            // Default button states
            backButton.Enabled = _history.Count > 0;
            nextButton.Visible = true;
            nextButton.Text = "Next";

            switch (_currentStep)
            {
                case WizardStep.Selection:
                    activePanel = SetupStepUI(pnlSelection, "Create New Rule", "What would you like to do?");
                    backButton.Enabled = false;
                    nextButton.Visible = false;
                    break;
                case WizardStep.GetFolder:
                    activePanel = SetupStepUI(pnlGetFolder, "Step 1: Select a Folder", "Select a folder to apply rules to all programs within it");
                    break;
                case WizardStep.GetAction:
                    activePanel = SetupStepUI(pnlGetAction, _selectedTemplate == RuleTemplate.BatchProgramRule ? "Step 2: Choose Action" : "Step 1: Choose Action", "Do you want to allow or block the program(s)?");
                    break;
                case WizardStep.GetProgram:
                    activePanel = SetupStepUI(pnlGetProgram, "Step 2: Select a Program", "Select the program's main executable file (.exe)");
                    break;
                case WizardStep.GetDirection:
                    activePanel = SetupStepUI(pnlGetDirection, "Step 3: Choose Direction", "Apply this rule to which connection direction?", "Finish");
                    break;
                case WizardStep.GetPorts:
                    activePanel = SetupStepUI(pnlGetPorts, "Step 1: Enter Ports", "What port or port range is needed?");
                    break;
                case WizardStep.GetProtocol:
                    activePanel = SetupStepUI(pnlGetProtocol, "Step 2: Select Protocol", "What protocol does it use?");
                    break;
                case WizardStep.GetName:
                    activePanel = SetupStepUI(pnlGetName, "Step 3: Name Your Rule", "Give your new rule a descriptive name.", "Finish");
                    break;
                case WizardStep.GetService:
                    activePanel = SetupStepUI(pnlGetService, "Step 1: Select a Service", "Select a Windows Service to block");
                    LoadServicesAsync();
                    break;
                case WizardStep.GetFileShareIP:
                    activePanel = SetupStepUI(pnlGetFileShareIP, "Step 1: Enter IP Address", "Enter the local IP of the trusted computer", "Finish");
                    break;
                case WizardStep.GetBlockDeviceIP:
                    activePanel = SetupStepUI(pnlGetBlockDeviceIP, "Step 1: Enter IP Address", "Enter the local IP of the device to block", "Finish");
                    break;
                case WizardStep.GetRestrictApp:
                    activePanel = SetupStepUI(pnlGetRestrictApp, "Step 1: Select a Program", "Select the program to restrict to your local network", "Finish");
                    break;
                case WizardStep.Summary:
                    activePanel = SetupStepUI(pnlSummary, "Summary", "The following rule will be created:", "Finish");
                    BuildSummary();
                    break;
            }

            if (activePanel != null)
            {
                activePanel.Visible = true;
                activePanel.Controls.OfType<TextBox>().FirstOrDefault()?.Focus();
            }
        }

        private void LoadServicesAsync()
        {
            if (serviceListBox.Items.Count > 0) return;

            serviceListBox.Items.Add("Loading services...");
            serviceListBox.Enabled = false;

            Task.Run(() =>
            {
                var services = SystemDiscoveryService.GetServicesWithExePaths()
                                    .OrderBy(s => s.DisplayName).ToList();

                if (this.IsDisposed || !this.IsHandleCreated) return;

                try
                {
                    this.Invoke(new Action(() =>
                    {
                        if (this.IsDisposed) return;
                        serviceListBox.Items.Clear();
                        foreach (var service in services)
                        {
                            serviceListBox.Items.Add($"{service.DisplayName} ({service.ServiceName})");
                        }
                        serviceListBox.Enabled = true;
                    }));
                }
                catch (ObjectDisposedException) { /* ignore if closed during invoke */ }
                catch (InvalidOperationException) { }
            });
        }

        #endregion

        #region Event Handlers - Navigation Buttons

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

        #endregion

        #region Event Handlers - Selection Buttons

        private void StartWizardTemplate(RuleTemplate template, WizardStep step)
        {
            _selectedTemplate = template;
            GoForwardTo(step);
        }

        private void programRuleButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.ProgramRule, WizardStep.GetAction);
        private void batchProgramRuleButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.BatchProgramRule, WizardStep.GetFolder);
        private void portRuleButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.PortRule, WizardStep.GetPorts);
        private void blockServiceButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.BlockService, WizardStep.GetService);
        private void allowFileShareButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.AllowFileShare, WizardStep.GetFileShareIP);
        private void blockDeviceButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.BlockDevice, WizardStep.GetBlockDeviceIP);
        private void restrictAppButton_Click(object sender, EventArgs e) => StartWizardTemplate(RuleTemplate.RestrictApp, WizardStep.GetRestrictApp);

        private void wildcardRuleButton_Click(object sender, EventArgs e)
        {
            using var wildcardDialog = new WildcardCreatorForm(_wildcardRuleService, _appSettings);
            if (wildcardDialog.ShowDialog(this) == DialogResult.OK)
            {
                var newRule = wildcardDialog.NewRule;
                _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.AddWildcardRule, newRule));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void advancedRuleButton_Click(object sender, EventArgs e)
        {
            using var dialog = new CreateAdvancedRuleForm(_actionsService, _appSettings);
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

        #endregion

        #region Event Handlers - Browsing & Helpers

        private void SelectExecutable(TextBox targetTextBox)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a program"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                targetTextBox.Text = openFileDialog.FileName;
            }
        }

        private void browseButton_Click(object sender, EventArgs e) => SelectExecutable(programPathTextBox);

        private void portsBrowseButton_Click(object sender, EventArgs e) => SelectExecutable(portsProgramPathTextBox);

        private void restrictAppBrowseButton_Click(object sender, EventArgs e) => SelectExecutable(restrictAppPathTextBox);

        private void batchBrowseFolderButton_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                batchFolderPathTextBox.Text = dialog.SelectedPath;
            }
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

        #endregion

        #region Validation Logic

        private string GetSelectedServiceName()
        {
            if (serviceListBox.SelectedItem != null)
            {
                string selected = serviceListBox.SelectedItem.ToString()!;
                return selected.Substring(selected.LastIndexOf('(') + 1).TrimEnd(')');
            }
            return serviceNameTextBox.Text;
        }
        private bool ShowError(string message, string title)
        {
            Messenger.MessageBox(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private bool ValidateFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(Environment.ExpandEnvironmentVariables(path)))
                return ShowError("Please select a valid program file.", "Invalid File");

            return true;
        }

        private bool ValidatePortString(string portString, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(portString) || portString == "*") return true;

            var parts = portString.Split(',');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrEmpty(trimmedPart)) continue;

                if (trimmedPart.Contains('-'))
                {
                    var rangeParts = trimmedPart.Split('-');
                    if (rangeParts.Length != 2 ||
                        !ushort.TryParse(rangeParts[0], out ushort start) ||
                        !ushort.TryParse(rangeParts[1], out ushort end) ||
                        start > end)
                    {
                        errorMessage = $"Invalid port range '{trimmedPart}'. Must be in the format 'start-end' (e.g., 80-88).";
                        return false;
                    }
                }
                else if (!ushort.TryParse(trimmedPart, out _))
                {
                    errorMessage = $"Invalid port number '{trimmedPart}'. Must be a number between 0 and 65535.";
                    return false;
                }
            }
            return true;
        }

        private bool ValidateStep()
        {
            switch (_currentStep)
            {
                case WizardStep.GetFolder:
                    if (string.IsNullOrWhiteSpace(batchFolderPathTextBox.Text) || !Directory.Exists(Environment.ExpandEnvironmentVariables(batchFolderPathTextBox.Text)))
                        return ShowError("Please select a valid folder.", "Invalid Folder");
                    if (!exeCheckBox.Checked && !dllCheckBox.Checked)
                        return ShowError("Please select at least one file type to apply rules to (.exe or .dll).", "No File Type Selected");
                    break;
                case WizardStep.GetProgram:
                    if (!ValidateFile(programPathTextBox.Text)) return false;
                    break;
                case WizardStep.GetPorts:
                    if (!ValidatePortString(portsTextBox.Text, out string portError))
                        return ShowError(portError, "Invalid Port");
                    if (restrictToProgramCheckBox.Checked && !ValidateFile(portsProgramPathTextBox.Text)) return false;
                    break;
                case WizardStep.GetName:
                    if (string.IsNullOrWhiteSpace(ruleNameTextBox.Text))
                        return ShowError("Please enter a name for the rule.", "Invalid Name");
                    break;
                case WizardStep.GetService:
                    string serviceName = GetSelectedServiceName();
                    if (string.IsNullOrWhiteSpace(serviceName))
                        return ShowError("Please select a service from the list or enter a service name.", "No Service Selected");
                    if (serviceListBox.Items.Count > 0 && serviceListBox.Enabled)
                    {
                        var services = SystemDiscoveryService.GetServicesWithExePaths();
                        if (!services.Any(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)))
                            return ShowError($"Service '{serviceName}' not found on this system.", "Invalid Service");
                    }
                    break;
                case WizardStep.GetFileShareIP:
                    if (!IPAddress.TryParse(fileShareIpTextBox.Text, out _))
                        return ShowError("Please enter a valid IP address.", "Invalid IP");
                    break;
                case WizardStep.GetBlockDeviceIP:
                    if (!IPAddress.TryParse(blockDeviceIpTextBox.Text, out _))
                        return ShowError("Please enter a valid IP address.", "Invalid IP");
                    break;
            }
            return true;
        }

        #endregion



        #region Step Processing Logic

        private void ProcessStepLogic()
        {
            switch (_currentStep)
            {
                case WizardStep.GetFolder:
                    _wizardFolderPath = batchFolderPathTextBox.Text;
                    GoForwardTo(WizardStep.GetAction);
                    break;

                case WizardStep.GetAction:
                    _wizardAction = allowActionRadioButton.Checked ? Actions.Allow : Actions.Block;
                    if (_selectedTemplate == RuleTemplate.BatchProgramRule)
                    {
                        GoForwardTo(WizardStep.GetDirection);
                    }
                    else
                    {
                        GoForwardTo(WizardStep.GetProgram);
                    }
                    break;

                case WizardStep.GetProgram:
                    _wizardAppPath = programPathTextBox.Text;
                    GoForwardTo(WizardStep.GetDirection);
                    break;

                case WizardStep.GetDirection:
                    if (inboundRadioButton.Checked) _wizardDirection = Directions.Incoming;
                    else if (outboundRadioButton.Checked) _wizardDirection = Directions.Outgoing;
                    else _wizardDirection = Directions.Incoming | Directions.Outgoing;

                    if (_selectedTemplate == RuleTemplate.BatchProgramRule)
                    {
                        CreateRule();
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        GoForwardTo(WizardStep.Summary);
                    }
                    break;

                case WizardStep.GetPorts:
                    _wizardPorts = portsTextBox.Text;
                    _wizardAppPath = restrictToProgramCheckBox.Checked ? portsProgramPathTextBox.Text : string.Empty;
                    GoForwardTo(WizardStep.GetProtocol);
                    break;

                case WizardStep.GetProtocol:
                    if (tcpRadioButton.Checked) _wizardProtocol = ProtocolTCP;
                    else if (udpRadioButton.Checked) _wizardProtocol = ProtocolUDP;
                    else _wizardProtocol = ProtocolAny;

                    ruleNameTextBox.Text = string.IsNullOrEmpty(_wizardAppPath)
                        ? $"Port {_wizardPorts}"
                        : $"{Path.GetFileNameWithoutExtension(_wizardAppPath)} Port {_wizardPorts}";
                    GoForwardTo(WizardStep.GetName);
                    break;

                case WizardStep.GetName:
                    _wizardRuleName = ruleNameTextBox.Text;
                    GoForwardTo(WizardStep.Summary);
                    break;

                case WizardStep.GetService:
                    _wizardServiceName = GetSelectedServiceName();
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
                    sb.AppendLine($"Protocol: {(_wizardProtocol == ProtocolTCP ? "TCP" : _wizardProtocol == ProtocolUDP ? "UDP" : "TCP & UDP")}");
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

        #endregion

        #region Rule Creation Logic

        private void EnqueueAdvancedRuleTask(AdvancedRuleViewModel viewModel)
        {
            var payload = new CreateAdvancedRulePayload { ViewModel = viewModel, InterfaceTypes = "All", IcmpTypesAndCodes = "" };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));
        }
        private void CreateRule()
        {
            switch (_selectedTemplate)
            {
                case RuleTemplate.BatchProgramRule: CreateBatchProgramRule(); break;
                case RuleTemplate.ProgramRule: CreateProgramRule(); break;
                case RuleTemplate.PortRule: CreatePortRule(); break;
                case RuleTemplate.BlockService: CreateServiceRule(); break;
                case RuleTemplate.AllowFileShare: CreateFileShareRule(); break;
                case RuleTemplate.BlockDevice: CreateBlockDeviceRule(); break;
                case RuleTemplate.RestrictApp: CreateRestrictAppRule(); break;
            }
        }

        private AdvancedRuleViewModel CreateBaseViewModel(string name, string status, Directions direction, int protocol, string description = "")
        {
            return new AdvancedRuleViewModel
            {
                Name = name,
                Description = description,
                IsEnabled = true,
                Status = status,
                Direction = direction,
                Protocol = (short)protocol,
                LocalPorts = "*",
                RemotePorts = "*",
                LocalAddresses = "*",
                RemoteAddresses = "*",
                ApplicationName = string.Empty,
                ServiceName = string.Empty,
                Grouping = MFWConstants.MainRuleGroup,
                Profiles = "All",
                Type = RuleType.Advanced,
                InterfaceTypes = "All",
                IcmpTypesAndCodes = ""
            };
        }

        private void CreateBatchProgramRule()
        {
            var searchPatterns = new List<string>();
            if (exeCheckBox.Checked) searchPatterns.Add("*.exe");
            if (dllCheckBox.Checked) searchPatterns.Add("*.dll");

            var executables = SystemDiscoveryService.GetFilesInFolder(_wizardFolderPath, searchPatterns);
            if (executables.Count == 0)
            {
                Messenger.MessageBox($"No matching files found in '{_wizardFolderPath}' or its subfolders.", "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }
            string batchAction = $"{_wizardAction} ({_wizardDirection})";
            var batchPayload = new ApplyApplicationRulePayload { AppPaths = executables, Action = batchAction };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule, batchPayload));
            Messenger.MessageBox($"{executables.Count} rules have been queued for creation.", "Task Queued", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void CreateProgramRule()
        {
            string progName = Path.GetFileNameWithoutExtension(_wizardAppPath);
            string actionStr = _wizardAction == Actions.Allow ? "Allow" : "Block";

            var programVm = CreateBaseViewModel(progName, actionStr, _wizardDirection, ProtocolAny, $"Rule created via Wizard for {progName}");
            programVm.ProtocolName = "Any";
            programVm.ApplicationName = _wizardAppPath;
            programVm.Type = RuleType.Program;

            EnqueueAdvancedRuleTask(programVm);
        }

        private void CreatePortRule()
        {
            var vm = CreateBaseViewModel(_wizardRuleName, "Allow", Directions.Incoming | Directions.Outgoing, _wizardProtocol);
            vm.LocalPorts = _wizardPorts;
            vm.ApplicationName = string.IsNullOrEmpty(_wizardAppPath) ? string.Empty : _wizardAppPath;

            EnqueueAdvancedRuleTask(vm);
        }

        private void CreateServiceRule()
        {
            var servicePayload = new ApplyServiceRulePayload { ServiceName = _wizardServiceName, Action = "Block (All)" };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyServiceRule, servicePayload));
        }

        private void CreateFileShareRule()
        {
            var fileShareVm = CreateBaseViewModel($"File Sharing from {_wizardRemoteIP}", "Allow", Directions.Incoming, ProtocolTCP, "Allows inbound file sharing (SMB)");
            fileShareVm.LocalPorts = "445";
            fileShareVm.RemoteAddresses = _wizardRemoteIP;

            EnqueueAdvancedRuleTask(fileShareVm);
        }

        private void CreateBlockDeviceRule()
        {
            var blockDeviceVm = CreateBaseViewModel($"Inbound from {_wizardRemoteIP}", "Block", Directions.Incoming, ProtocolAny, "Blocks all inbound traffic from a specific local IP");
            blockDeviceVm.RemoteAddresses = _wizardRemoteIP;

            EnqueueAdvancedRuleTask(blockDeviceVm);
        }

        private void CreateRestrictAppRule()
        {
            string appName = Path.GetFileNameWithoutExtension(_wizardAppPath);
            var allowLocalVm = CreateBaseViewModel($"{appName} - Local Network Only", "Allow", Directions.Incoming | Directions.Outgoing, ProtocolAny, "Allows communication only within the local network. This rule only works as intended if Lockdown Mode is active.");
            allowLocalVm.ApplicationName = _wizardAppPath;
            allowLocalVm.RemoteAddresses = "10.0.0.0/8,172.16.0.0/12,192.168.0.0/16,LocalSubnet";

            EnqueueAdvancedRuleTask(allowLocalVm);
        }

        #endregion
    }
}