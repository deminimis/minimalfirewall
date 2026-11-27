using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DarkModeForms;
using MinimalFirewall.TypedObjects;

[assembly: InternalsVisibleTo("ApplicationsChecks")]

namespace MinimalFirewall;

public sealed class ApplicationsControl : UserControl
{
    private readonly TextBox _search = new()
    {
        Dock = DockStyle.Fill,
        PlaceholderText = "Application name or executable path"
    };

    private readonly ComboBox _filter = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        FlatStyle = FlatStyle.Flat
    };

    private readonly ThemedDataGridView _applications = CreateGrid();
    private readonly ThemedDataGridView _rules = CreateGrid();
    private readonly ThemedDataGridView _attempts = CreateGrid();

    private readonly TextBox _identity = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true
    };

    private readonly ThemedLabel _publisher = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true
    };

    private readonly ThemedLabel _signature = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true
    };

    private readonly ThemedLabel _count = new()
    {
        AutoSize = true
    };

    private readonly ThemedLabel _replacementScope = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        Text =
            "Application-wide replacement removes every matching rule in " +
            "the selected direction, including scoped and third-party rules."
    };

    private readonly ComboBox _bothAction = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        FlatStyle = FlatStyle.Flat,
        Width = 90
    };

    private readonly ThemedButton _replaceOutboundAllow = new()
    {
        Text = "Replace &outbound rules with Allow…",
        AutoSize = true
    };

    private readonly ThemedButton _replaceInboundBlock = new()
    {
        Text = "Replace &inbound rules with Block…",
        AutoSize = true
    };

    private readonly ThemedButton _replaceBoth = new()
    {
        Text = "Replace &both directions…",
        AutoSize = true
    };

    private readonly ThemedButton _editSelectedRule = new()
    {
        Text = "&Edit selected rule…",
        AutoSize = true
    };

    private readonly ThemedButton _deleteSelectedRule = new()
    {
        Text = "Delete selected &rule…",
        AutoSize = true
    };

    private readonly ThemedButton _deleteAllApplicationRules = new()
    {
        Text = "Delete &all rules for this application…",
        AutoSize = true
    };
    private readonly ThemedButton _searchOnline = new() { Text = "Search &online", AutoSize = true };
    private readonly ThemedButton _virusTotal = new() { Text = "Check on &VirusTotal", AutoSize = true };
    private bool _selectedFileExists;

    private MainViewModel _viewModel = null!;
    private AppSettings _settings = null!;
    private IconService _icons = null!;
    private FirewallActionsService _actionsService = null!;
    private BackgroundFirewallTaskService _backgroundTaskService = null!;
    private bool _updating;
    private bool _busy;

    

    public ApplicationsControl()
    {
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7, 15);
        Font = SystemFonts.MessageBoxFont;
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        _filter.Items.AddRange(
        [
            "All applications",
            "Reported blocked attempts",
            "Explicit allow rules",
            "Explicit block rules"
        ]);
        _filter.SelectedIndex = 0;

        var searchBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 2
        };

        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        _search.TabIndex = 1;
        _filter.TabIndex = 3;

        searchBar.Controls.Add(
            new ThemedLabel
            {
                Text = "&Search applications",
                AutoSize = true,
                TabIndex = 0
            },
            0,
            0);

        searchBar.Controls.Add(
            new ThemedLabel
            {
                Text = "&Filter applications",
                AutoSize = true,
                TabIndex = 2
            },
            1,
            0);

        searchBar.Controls.Add(_search, 0, 1);
        searchBar.Controls.Add(_filter, 1, 1);

        _applications.Columns.Add(
            new DataGridViewImageColumn
            {
                Name = "Icon",
                HeaderText = "",
                Width = 32,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    NullValue = null
                }
            });

        AddColumns(
            _applications,
            ("Name", "Application", 140),
            ("AppPath", "Executable path", 260),
            ("RuleSummary", "Explicit executable rules", 200),
            ("LatestAttempt", "Latest reported block", 160));

        AddColumns(
            _rules,
            ("Name", "Rule", 200),
            ("IsEnabled", "Enabled", 70),
            ("Status", "Action", 70),
            ("Direction", "Direction", 100),
            ("Profiles", "Profiles", 110),
            ("ProtocolName", "Protocol", 80),
            ("ServiceName", "Service", 110),
            ("LocalPorts", "Local ports", 90),
            ("RemotePorts", "Remote ports", 100),
            ("LocalAddresses", "Local addresses", 140),
            ("RemoteAddresses", "Remote addresses", 140),
            ("InterfaceTypes", "Interfaces", 100),
            ("IcmpTypesAndCodes", "ICMP types/codes", 110),
            ("Description", "Description", 200));

        AddColumns(
            _attempts,
            ("ObservedAt", "Reported at", 160),
            ("Direction", "Direction", 100),
            ("ProtocolDisplay", "Protocol", 80),
            ("RemoteAddress", "Remote address", 150),
            ("RemotePort", "Remote port", 100),
            ("ServiceName", "Service", 140));

        var details = new ThemedTabControl
        {
            Dock = DockStyle.Fill
        };

        var rulesPage = new TabPage("Explicit executable rules")
        {
            Name = "applicationRulesPage"
        };

        var attemptsPage = new TabPage("Reported blocked attempts (session)")
        {
            Name = "applicationAttemptsPage"
        };

        rulesPage.Controls.Add(_rules);
        attemptsPage.Controls.Add(_attempts);
        details.TabPages.AddRange([rulesPage, attemptsPage]);

        var identity = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            Margin = new Padding(0, 8, 0, 8)
        };
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        identity.Controls.Add(_identity, 0, 0);
        identity.SetColumnSpan(_identity, 3);
        identity.Controls.Add(_publisher, 0, 1);
        identity.Controls.Add(_signature, 0, 2);
        // Beside the two metadata lines, so the buttons add no height and the grid keeps its rows at large text sizes.
        identity.Controls.Add(_searchOnline, 1, 1);
        identity.Controls.Add(_virusTotal, 2, 1);
        identity.SetRowSpan(_searchOnline, 2);
        identity.SetRowSpan(_virusTotal, 2);

        _bothAction.Items.AddRange(["Allow", "Block"]);
        _bothAction.SelectedIndex = 0;

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        actions.Controls.AddRange(
        [
            _replaceOutboundAllow,
    _replaceInboundBlock,
    _replaceBoth,
    new ThemedLabel
    {
        Text = "Both-directions &action:",
        AutoSize = true,
        Margin = new Padding(12, 8, 3, 3)
    },
      _bothAction,
      _editSelectedRule,
      _deleteSelectedRule,
      _deleteAllApplicationRules
        ]);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(searchBar, 0, 0);
        layout.Controls.Add(_count, 0, 1);
        layout.Controls.Add(_applications, 0, 2);
        layout.Controls.Add(identity, 0, 3);
        layout.Controls.Add(details, 0, 4);
        layout.Controls.Add(_replacementScope, 0, 5);
        layout.Controls.Add(actions, 0, 6);
        Controls.Add(layout);

        _search.AccessibleName = "Search applications";
        _filter.AccessibleName = "Filter applications";
        _applications.AccessibleName = "Applications";
        _identity.AccessibleName = "Executable path and publisher metadata";
        _rules.AccessibleName = "Explicit executable rules. Double-click a rule or press Enter to edit it.";
        _attempts.AccessibleName = "Reported blocked attempts in this session";
        _bothAction.AccessibleName = "Both-directions replacement action";
        _replaceOutboundAllow.AccessibleName =
            "Replace outbound rules with Allow";
        _replaceInboundBlock.AccessibleName =
            "Replace inbound rules with Block";
        _replaceBoth.AccessibleName =
            "Replace both directions";
        _editSelectedRule.AccessibleName =
            "Edit selected application rule";
        _deleteSelectedRule.AccessibleName =
            "Delete one selected application rule";
        _deleteAllApplicationRules.AccessibleName =
            "Delete every firewall rule for the selected application";

        _search.TextChanged += (_, _) => RefreshApplications();
        _filter.SelectedIndexChanged += (_, _) => RefreshApplications();
        _rules.CellDoubleClick += Rules_CellDoubleClick;
        _rules.KeyDown += Rules_KeyDown;

        _applications.SelectionChanged += (_, _) =>
        {
            if (!_updating)
            {
                ShowDetails();
                UpdateActions();
            }
        };

        _rules.SelectionChanged += (_, _) => UpdateActions();

        _replaceOutboundAllow.Click += async (_, _) =>
            await ReplaceApplicationRulesAsync(
                Directions.Outgoing,
                "Allow");

        _replaceInboundBlock.Click += async (_, _) =>
            await ReplaceApplicationRulesAsync(
                Directions.Incoming,
                "Block");

        _replaceBoth.Click += async (_, _) =>
            await ReplaceApplicationRulesAsync(
                Directions.Incoming | Directions.Outgoing,
                _bothAction.SelectedIndex == 1 ? "Block" : "Allow");

        _editSelectedRule.Click += async (_, _) =>
            await EditSelectedRuleAsync();
                _deleteSelectedRule.Click += async (_, _) =>
                    await DeleteSelectedRuleAsync();
                _deleteAllApplicationRules.Click += async (_, _) =>
                    await DeleteAllApplicationRulesAsync();
        _searchOnline.Click += (_, _) => SearchOnline();
        _virusTotal.Click += async (_, _) => await CheckVirusTotalAsync();

        _applications.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 ||
                _applications.Columns[e.ColumnIndex].Name != "Icon" ||
                _icons == null)
            {
                return;
            }

            if (_applications.Rows[e.RowIndex].Tag is not ApplicationEntry entry ||
                _icons.ImageList is not { } images)
            {
                return;
            }

            int index = _icons.GetIconIndex(entry.AppPath);
            e.Value = index >= 0 && index < images.Images.Count
                ? images.Images[index]
                : null;
            e.FormattingApplied = true;
        };

        UpdateActions();
    }

    public void Initialize(
    MainViewModel viewModel,
    AppSettings settings,
    IconService icons,
    FirewallActionsService actionsService,
    BackgroundFirewallTaskService backgroundTaskService)
    {
        _viewModel = viewModel;
        _settings = settings;
        _icons = icons;
        _actionsService = actionsService;
        _backgroundTaskService = backgroundTaskService;

        InitializeColumnSettings(settings);

        _viewModel.ReportedBlockedAttempts.CollectionChanged -= AttemptsChanged;
        _viewModel.ReportedBlockedAttempts.CollectionChanged += AttemptsChanged;

        RefreshApplications();
    }

    public void RefreshApplications()
    {
        if (_viewModel == null || IsDisposed)
        {
            return;
        }

        _applications.Columns["Icon"]!.Visible =
    _settings.ShowAppIcons;

        var entries = ApplicationEntry.Build(
                _viewModel.ApplicationRules,
                _viewModel.ReportedBlockedAttempts)
            .Where(entry =>
                entry.Matches(
                    _search.Text.Trim(),
                    _filter.SelectedIndex))
            .ToList();

        DisplayApplications(entries);
    }

    internal void InitializeColumnSettings(AppSettings settings)
    {
        _applications.AllowUserToOrderColumns = true;

        DataGridViewHelper.RestoreColumnSettings(
            _applications,
            settings.ApplicationsColumns);

        _applications.ColumnWidthChanged += (_, _) =>
            DataGridViewHelper.SaveColumnSettings(
                _applications,
                settings.ApplicationsColumns,
                settings);

        _applications.ColumnDisplayIndexChanged += (_, _) =>
            DataGridViewHelper.SaveColumnSettings(
                _applications,
                settings.ApplicationsColumns,
                settings);
    }

    internal void DisplayApplications(List<ApplicationEntry> entries)
    {
        string? selectedPath = SelectedApplication?.AppPath;
        string? selectedColumn =
            _applications.CurrentCell?.OwningColumn?.Name;

        int firstDisplayedIndex =
            _applications.FirstDisplayedScrollingRowIndex;

        string? firstDisplayedPath =
            firstDisplayedIndex >= 0
                ? (_applications.Rows[firstDisplayedIndex].Tag
                    as ApplicationEntry)?.AppPath
                : null;

        int horizontalOffset =
            _applications.HorizontalScrollingOffset;

        _updating = true;

        try
        {
            _applications.Rows.Clear();

            foreach (var entry in entries)
            {
                int rowIndex = _applications.Rows.Add(
                    null!,
                    entry.Name,
                    entry.AppPath,
                    entry.RuleSummary,
                    entry.LatestAttempt?.ToString("g") ??
                    "Not observed");

                _applications.Rows[rowIndex].Tag = entry;
            }

            _applications.ClearSelection();

            if (entries.Count > 0)
            {
                int selectedIndex = entries.FindIndex(entry =>
                    entry.AppPath.Equals(
                        selectedPath,
                        StringComparison.OrdinalIgnoreCase));

                var row =
                    _applications.Rows[Math.Max(0, selectedIndex)];

                _applications.CurrentCell =
                    selectedColumn != null &&
                    _applications.Columns[selectedColumn]!.Visible
                        ? row.Cells[selectedColumn]
                        : FirstVisibleCell(row);

                row.Selected = true;

                int topIndex = entries.FindIndex(entry =>
                    entry.AppPath.Equals(
                        firstDisplayedPath,
                        StringComparison.OrdinalIgnoreCase));

                _applications.FirstDisplayedScrollingRowIndex =
                    topIndex >= 0
                        ? topIndex
                        : Math.Clamp(
                            firstDisplayedIndex,
                            0,
                            entries.Count - 1);

                _applications.HorizontalScrollingOffset =
                    horizontalOffset;
            }
        }
        finally
        {
            _updating = false;
        }

        _count.Text = entries.Count == 0
            ? "No matching applications. Rules appear after refresh; reported blocks appear while monitoring."
            : $"{entries.Count} applications. Rules shown here target executables; global and package rules remain in Rules.";

        ShowDetails();
        UpdateActions();
    }

    public void ApplyThemeFixes()
    {
        foreach (var grid in new[]
        {
            _applications,
            _rules,
            _attempts
        })
        {
            grid.ApplyTheme();
            grid.DefaultCellStyle.SelectionBackColor =
                Theme.Colors.SelectionInfo;
            grid.DefaultCellStyle.SelectionForeColor =
                Color.Black;
        }
    }

    private ApplicationEntry? SelectedApplication =>
        _applications.CurrentRow?.Selected == true
            ? _applications.CurrentRow.Tag as ApplicationEntry
            : null;

    private AdvancedRuleViewModel? SelectedRule =>
        _rules.CurrentRow?.DataBoundItem
            as AdvancedRuleViewModel;

    private void ShowDetails()
    {
        var entry = SelectedApplication;

        string? selectedRuleName =
            SelectedRule?.ApplicationName.Equals(
                entry?.AppPath,
                StringComparison.OrdinalIgnoreCase) == true
                ? SelectedRule.Name
                : null;

        int firstRuleIndex =
            _rules.FirstDisplayedScrollingRowIndex;

        int ruleHorizontalOffset =
            _rules.HorizontalScrollingOffset;

        var publishers = entry?.Rules
            .Select(rule => rule.AutoAllowedPublisher)
            .Where(publisher =>
                !string.IsNullOrWhiteSpace(publisher))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        _identity.Text = entry?.AppPath ??
            "Select an application to inspect its rules and reported blocks.";

        _publisher.Text = entry == null
            ? ""
            : $"Publisher (rule metadata): " +
              $"{(publishers.Count > 0 ? string.Join(", ", publishers) : "Not available")}";

        _signature.Text = entry == null
            ? ""
            : "Signature status: Not available";

        _rules.DataSource =
            new BindingList<AdvancedRuleViewModel>(
                entry?.Rules ?? []);

        if (selectedRuleName != null && entry != null)
        {
            int selectedRuleIndex =
                entry.Rules.FindIndex(rule =>
                    rule.Name == selectedRuleName);

            if (selectedRuleIndex >= 0)
            {
                var row = _rules.Rows[selectedRuleIndex];

                _rules.CurrentCell = FirstVisibleCell(row);

                if (_rules.Rows.Count > 0)
                {
                    _rules.FirstDisplayedScrollingRowIndex =
                        Math.Clamp(
                            firstRuleIndex,
                            0,
                            _rules.Rows.Count - 1);
                }

                _rules.HorizontalScrollingOffset =
                    ruleHorizontalOffset;
            }
        }

        _attempts.DataSource =
    new BindingList<PendingConnectionViewModel>(
        entry?.Attempts
            .OrderByDescending(attempt =>
                attempt.ObservedAt)
            .ToList() ?? []);
        // Checked once per selection: rules can target executables that no longer exist.
        _selectedFileExists = entry != null && File.Exists(entry.AppPath);
    }

    private void UpdateActions()
    {
        bool hasApplication =
            !_busy && SelectedApplication != null;

        _replaceOutboundAllow.Enabled = hasApplication;
        _replaceInboundBlock.Enabled = hasApplication;
        _replaceBoth.Enabled = hasApplication;
        _bothAction.Enabled = hasApplication;

        _editSelectedRule.Enabled =
        !_busy && SelectedRule != null;
            _deleteSelectedRule.Enabled =
                !_busy && SelectedRule != null;
            _deleteAllApplicationRules.Enabled =
                hasApplication;
        _searchOnline.Enabled = SelectedApplication != null;
        _virusTotal.Enabled = _selectedFileExists && SelectedApplication != null;
    }

    // Sends only the SHA-256 hash; VirusTotal offers an upload when the file is unknown.
    private async Task CheckVirusTotalAsync()
    {
        if (SelectedApplication is not { } entry || !_selectedFileExists) return;
        _virusTotal.Enabled = false;
        string hash = await SystemDiscoveryService.CalculateSHA256Async(entry.AppPath);
        UpdateActions();
        if (string.IsNullOrEmpty(hash))
        {
            Messenger.MessageBox("Could not calculate file hash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo($"https://www.virustotal.com/gui/file/{hash}") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARN] ApplicationsControl.CheckVirusTotalAsync failed: {ex.Message}");
        }
    }

    // Same query as the connection popup; only sent on explicit click.
    private void SearchOnline()
    {
        if (SelectedApplication is not { } entry) return;
        try
        {
            Process.Start(new ProcessStartInfo(NotifierForm.BuildSearchUrl(entry.AppPath, _settings.SearchEngine)) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARN] ApplicationsControl.SearchOnline failed: {ex.Message}");
        }
    }

    private async Task ReplaceApplicationRulesAsync(
    Directions directions,
    string action)
    {
        ApplicationEntry? entry = SelectedApplication;

        if (entry == null || _busy)
        {
            return;
        }

        ApplicationRuleReplacementPreview preview;

        try
        {
            preview =
                FirewallRuleService.PreviewApplicationRuleReplacement(
                    entry.AppPath,
                    directions,
                    action);
        }
        catch (Exception ex)
        {
            Messenger.MessageBox(
                "The replacement preview could not be created.\r\n\r\n" +
                ex.Message,
                "Application rule replacement",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                pIsDarkMode: Theme.Colors.Surface.R < 128);

            return;
        }

        string directionText =
            directions ==
            (Directions.Incoming | Directions.Outgoing)
                ? "Both directions"
                : directions == Directions.Incoming
                    ? "Inbound"
                    : "Outbound";

        string warning =
            $"Replace {directionText.ToLowerInvariant()} rules with " +
            $"{action}\r\n\r\n" +
            $"Application:\r\n{preview.AppPath}\r\n\r\n" +
            $"Matching rules: {preview.MatchingRuleCount}\r\n" +
            $"Inbound rules: {preview.InboundRuleCount}\r\n" +
            $"Outbound rules: {preview.OutboundRuleCount}\r\n" +
            $"Third-party rules included: " +
            $"{(preview.IncludesThirdPartyRules ? "Yes" : "No")}" +
            "\r\n\r\n" +
            $"Profiles: {FormatPreviewValues(preview.Profiles)}\r\n" +
            $"Protocols: {FormatPreviewValues(preview.Protocols)}\r\n" +
            $"Local ports: " +
            $"{FormatPreviewValues(preview.LocalPorts)}\r\n" +
            $"Remote ports: " +
            $"{FormatPreviewValues(preview.RemotePorts)}\r\n" +
            $"Local addresses: " +
            $"{FormatPreviewValues(preview.LocalAddresses)}\r\n" +
            $"Remote addresses: " +
            $"{FormatPreviewValues(preview.RemoteAddresses)}\r\n" +
            $"Services: {FormatPreviewValues(preview.Services)}" +
            "\r\n\r\n" +
            $"All {preview.MatchingRuleCount} matching rule(s) will be " +
            $"removed and replaced with application-wide {action} " +
            "rule(s) covering all profiles, protocols, ports, " +
            "addresses, and services." +
            "\r\n\r\n" +
            "The replacement is validated before deletion. If creation " +
            "or verification fails, Minimal Firewall will attempt to " +
            "restore the removed rules." +
            "\r\n\r\nContinue?";

        DialogResult confirmation = Messenger.MessageBox(
            warning,
            $"Confirm replacement with {action}",
            MsgIcon.Warning,
            MessageBoxButtons.YesNo,
            pIsDarkMode: Theme.Colors.Surface.R < 128,
            defaultButton: MessageBoxDefaultButton.Button2,
            owner: FindForm());

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        await QueueMutationAndRefreshAsync(
            () => _backgroundTaskService.EnqueueTask(
                new FirewallTask(
                    FirewallTaskType.ReplaceApplicationRules,
                    new ReplaceApplicationRulesPayload
                    {
                        AppPath = entry.AppPath,
                        Directions = directions,
                        Action = action
                    },
                    $"Replacing {directionText.ToLowerInvariant()} " +
                    $"rules with {action}...")),
            "Could not replace the application rules.",
            "Application rule replacement stopped");
    }

    private static string FormatPreviewValues(
        IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return "(none)";
        }

        const int displayLimit = 8;

        string displayedValues =
            string.Join(", ", values.Take(displayLimit));

        return values.Count > displayLimit
            ? $"{displayedValues}, and " +
              $"{values.Count - displayLimit} more"
            : displayedValues;
    }

    private async Task QueueMutationAndRefreshAsync(
        Action enqueue,
        string errorPrefix,
        string errorTitle)
        {
            if (_busy || IsDisposed || Disposing)
            {
                return;
            }

            _busy = true;
            UpdateActions();

            try
            {
                _backgroundTaskService.TakeLastOperationFailure();

                enqueue();

                await _backgroundTaskService.WhenIdleAsync();

                Exception? failure =
                    _backgroundTaskService.TakeLastOperationFailure();

                if (failure != null)
                {
                    throw failure;
                }

                if (IsDisposed || Disposing)
                {
                    return;
                }

                _viewModel.ClearRulesCache();

                await _viewModel.RefreshRulesDataAsync(
                    CancellationToken.None);

                if (!IsDisposed && !Disposing)
                {
                    RefreshApplications();
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                {
                    Messenger.MessageBox(
                        errorPrefix + "\r\n\r\n" + ex.Message,
                        errorTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        pIsDarkMode: Theme.Colors.Surface.R < 128);
                }
            }
            finally
            {
                _busy = false;

                if (!IsDisposed && !Disposing)
                {
                    UpdateActions();
                }
            }
        }


    private async void Rules_CellDoubleClick(
    object? sender,
    DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        await EditSelectedRuleAsync();
    }

    private async void Rules_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
        await EditSelectedRuleAsync();
    }

    private async Task DeleteSelectedRuleAsync()
    {
        AdvancedRuleViewModel? rule = SelectedRule;

        if (rule == null || _busy)
        {
            return;
        }

        DialogResult confirmation = Messenger.MessageBox(
            "Delete one selected rule\r\n\r\n" +
            $"Rule name:\r\n{rule.Name}\r\n\r\n" +
            $"Application:\r\n{rule.ApplicationName}\r\n\r\n" +
            $"Direction: {rule.Direction}\r\n" +
            $"Action: {rule.Status}\r\n" +
            $"Protocol: {rule.ProtocolName}\r\n\r\n" +
            "Only this individual rule will be deleted. Rules with the same " +
            "name but different settings will not be affected. Deletion is " +
            "stopped if the selected rule cannot be identified uniquely.\r\n\r\n" +
            "Continue?",
            "Delete selected rule",
            MsgIcon.Warning,
            MessageBoxButtons.YesNo,
            pIsDarkMode: Theme.Colors.Surface.R < 128,
            defaultButton: MessageBoxDefaultButton.Button2,
            owner: FindForm());

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        await QueueMutationAndRefreshAsync(
            () => _backgroundTaskService.EnqueueTask(
                new FirewallTask(
                    FirewallTaskType.DeleteSelectedRule,
                    new DeleteSelectedRulePayload
                    {
                        Rule = rule
                    },
                    $"Deleting selected rule '{rule.Name}'...")),
            "The selected rule was not deleted.",
            "Delete selected rule stopped");
    }

    private async Task DeleteAllApplicationRulesAsync()
    {
        ApplicationEntry? entry = SelectedApplication;

        if (entry == null || _busy)
        {
            return;
        }

        ApplicationRuleDeletionPreview preview;

        try
        {
            preview =
                FirewallRuleService.PreviewApplicationRuleDeletion(
                    entry.AppPath);
        }
        catch (Exception ex)
        {
            Messenger.MessageBox(
                "The application-wide deletion preview could not be " +
                "created.\r\n\r\n" + ex.Message,
                "Delete all rules for this application",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                pIsDarkMode: Theme.Colors.Surface.R < 128);
            return;
        }

        if (preview.MatchingRuleCount == 0)
        {
            Messenger.MessageBox(
                "No firewall rules currently target this executable.",
                "Delete all rules for this application",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                pIsDarkMode: Theme.Colors.Surface.R < 128);
            return;
        }

        DialogResult confirmation = Messenger.MessageBox(
            BuildApplicationDeletionWarning(preview),
            "Delete all rules for this application",
            MsgIcon.Warning,
            MessageBoxButtons.YesNo,
            pIsDarkMode: Theme.Colors.Surface.R < 128,
            defaultButton: MessageBoxDefaultButton.Button2,
            owner: FindForm());

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        await QueueMutationAndRefreshAsync(
            () => _backgroundTaskService.EnqueueTask(
                new FirewallTask(
                    FirewallTaskType.DeleteApplicationRules,
                    new DeleteApplicationRulesPayload
                    {
                        AppPaths = [preview.AppPath]
                    },
                    "Deleting all firewall rules for " +
                    $"{Path.GetFileName(preview.AppPath)}...")),
            "Application-wide deletion did not complete.",
            "Delete all rules for this application stopped");
    }

    

    private static string BuildApplicationDeletionWarning(
        ApplicationRuleDeletionPreview preview)
    {
        const int nameLimit = 10;

        string displayedNames = string.Join(
            "\r\n",
            preview.RuleNames
                .Take(nameLimit)
                .Select(name => "• " + name));

        if (preview.RuleNames.Count > nameLimit)
        {
            displayedNames +=
                $"\r\n• ...and " +
                $"{preview.RuleNames.Count - nameLimit} more";
        }

        string types = preview.RuleTypes.Count == 0
            ? "(none)"
            : string.Join(
                "\r\n",
                preview.RuleTypes.Select(type => "• " + type));

        return
            "Delete all rules for this application\r\n\r\n" +
            $"Executable:\r\n{preview.AppPath}\r\n\r\n" +
            $"Total rules: {preview.MatchingRuleCount}\r\n" +
            $"Inbound: {preview.InboundRuleCount}\r\n" +
            $"Outbound: {preview.OutboundRuleCount}\r\n" +
            $"Allow: {preview.AllowRuleCount}\r\n" +
            $"Block: {preview.BlockRuleCount}\r\n" +
            $"Minimal Firewall rules: " +
            $"{preview.MinimalFirewallRuleCount}\r\n" +
            $"Windows, manual, or third-party rules: " +
            $"{preview.OtherRuleCount}\r\n\r\n" +
            "Affected rule types:\r\n" +
            $"{types}\r\n\r\n" +
            "Affected rule names:\r\n" +
            $"{displayedNames}\r\n\r\n" +
            "WARNING: This deletes every Windows Firewall rule whose " +
            "application path matches this executable. Windows-created, " +
            "manually created, and third-party rules may be included. " +
            "This is not equivalent to deleting one selected rule.\r\n\r\n" +
            "Continue?";
    }

    private async Task EditSelectedRuleAsync()
    {
        AdvancedRuleViewModel? originalRule = SelectedRule;

        if (originalRule == null || _busy)
        {
            return;
        }

        using var dialog = new CreateAdvancedRuleForm(
            _actionsService,
            originalRule,
            _settings);

        if (dialog.ShowDialog(FindForm()) != DialogResult.OK ||
            dialog.RuleVm == null ||
            originalRule.HasSameSettings(dialog.RuleVm))
        {
            return;
        }

        AdvancedRuleViewModel updatedRule = dialog.RuleVm;

        await QueueMutationAndRefreshAsync(
            () => _backgroundTaskService.EnqueueTask(
                new FirewallTask(
                    FirewallTaskType.EditAdvancedRule,
                    new EditAdvancedRulePayload
                    {
                        OriginalRule = originalRule,
                        UpdatedRule = updatedRule
                    },
                    $"Editing firewall rule '{originalRule.Name}'...")),
            "The selected rule was not edited.",
            "Rule edit stopped");
    }

    private void AttemptsChanged(
    object? sender,
    NotifyCollectionChangedEventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            try
            {
                BeginInvoke(new Action(() => AttemptsChanged(sender, e)));
            }
            catch (ObjectDisposedException)
            {
                // The control was disposed while the notification was queued.
            }
            catch (InvalidOperationException)
            {
                // The handle was destroyed while the notification was queued.
            }

            return;
        }

        // Do not rebuild all application rows while the Applications tab is hidden.
        if (Visible)
        {
            RefreshApplications();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _viewModel != null)
        {
            _viewModel.ReportedBlockedAttempts.CollectionChanged -=
                AttemptsChanged;
        }

        base.Dispose(disposing);
    }

    private static DataGridViewCell? FirstVisibleCell(
        DataGridViewRow row) =>
        row.Cells
            .Cast<DataGridViewCell>()
            .Where(cell => cell.OwningColumn.Visible)
            .MinBy(cell => cell.OwningColumn.DisplayIndex);

    private static ThemedDataGridView CreateGrid() =>
        new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToOrderColumns = true,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode
                    .DisplayedCellsExceptHeaders,
            ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None
        };

    private static void AddColumns(
        DataGridView grid,
        params (string Property, string Header, int Width)[] columns)
    {
        foreach (var column in columns)
        {
            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = column.Property,
                    DataPropertyName = column.Property,
                    HeaderText = column.Header,
                    Width = column.Width,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });
        }
    }
}
