using DarkModeForms;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationsChecks")]

namespace MinimalFirewall;

public sealed class ApplicationsControl : UserControl
{
    private readonly TextBox _search = new() { Dock = DockStyle.Fill, PlaceholderText = "Application name or executable path" };
    private readonly ComboBox _filter = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
    private readonly ThemedDataGridView _applications = CreateGrid();
    private readonly ThemedDataGridView _rules = CreateGrid();
    private readonly ThemedDataGridView _attempts = CreateGrid();
    private readonly TextBox _identity = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly ThemedLabel _publisher = new() { Dock = DockStyle.Fill, AutoSize = true };
    private readonly ThemedLabel _signature = new() { Dock = DockStyle.Fill, AutoSize = true };
    private readonly ThemedLabel _count = new() { AutoSize = true };
    private readonly ThemedLabel _scope = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly ComboBox _direction = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Width = 130 };
    private readonly ThemedButton _allow = new() { Text = "&Allow…", AutoSize = true };
    private readonly ThemedButton _block = new() { Text = "&Block…", AutoSize = true };
    private readonly ThemedButton _edit = new() { Text = "&Edit selected rule…", AutoSize = true };
    private MainViewModel _viewModel = null!;
    private FirewallActionsService _actions = null!;
    private BackgroundFirewallTaskService _background = null!;
    private AppSettings _settings = null!;
    private IconService _icons = null!;
    private bool _updating;
    private bool _busy;

    public event Func<Task>? DataRefreshRequested;

    public ApplicationsControl()
    {
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7, 15);
        Font = SystemFonts.MessageBoxFont;
        Dock = DockStyle.Fill;
        Padding = new Padding(8);
        _filter.Items.AddRange(["All applications", "Reported blocked attempts", "Explicit allow rules", "Explicit block rules"]);
        _filter.SelectedIndex = 0;
        _direction.Items.AddRange(["Outbound", "Inbound", "Both directions"]);
        _direction.SelectedIndex = 0;

        var searchBar = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 2 };
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        _search.TabIndex = 1;
        _filter.TabIndex = 3;
        searchBar.Controls.Add(new ThemedLabel { Text = "&Search applications", AutoSize = true, TabIndex = 0 }, 0, 0);
        searchBar.Controls.Add(new ThemedLabel { Text = "&Filter applications", AutoSize = true, TabIndex = 2 }, 1, 0);
        searchBar.Controls.Add(_search, 0, 1);
        searchBar.Controls.Add(_filter, 1, 1);

        _applications.Columns.Add(new DataGridViewImageColumn
        {
            Name = "Icon", HeaderText = "", Width = 32, ImageLayout = DataGridViewImageCellLayout.Zoom,
            DefaultCellStyle = new DataGridViewCellStyle { NullValue = null }
        });
        AddColumns(_applications, ("Name", "Application", 140), ("AppPath", "Executable path", 260),
            ("RuleSummary", "Explicit executable rules", 200), ("LatestAttempt", "Latest reported block", 160));
        AddColumns(_rules, ("Name", "Rule", 200), ("IsEnabled", "Enabled", 70), ("Status", "Action", 70),
            ("Direction", "Direction", 100), ("Profiles", "Profiles", 110), ("ProtocolName", "Protocol", 80),
            ("ServiceName", "Service", 110), ("LocalPorts", "Local ports", 90), ("RemotePorts", "Remote ports", 100),
            ("LocalAddresses", "Local addresses", 140), ("RemoteAddresses", "Remote addresses", 140),
            ("InterfaceTypes", "Interfaces", 100), ("IcmpTypesAndCodes", "ICMP types/codes", 110), ("Description", "Description", 200));
        AddColumns(_attempts, ("ObservedAt", "Reported at", 160), ("Direction", "Direction", 100),
            ("ProtocolDisplay", "Protocol", 80), ("RemoteAddress", "Remote address", 150),
            ("RemotePort", "Remote port", 100), ("ServiceName", "Service", 140));

        var details = new ThemedTabControl { Dock = DockStyle.Fill };
        var rulesPage = new TabPage("Explicit executable rules") { Name = "applicationRulesPage" };
        var attemptsPage = new TabPage("Reported blocked attempts (session)");
        rulesPage.Controls.Add(_rules);
        attemptsPage.Controls.Add(_attempts);
        details.TabPages.AddRange([rulesPage, attemptsPage]);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        actions.Controls.Add(new ThemedLabel { Text = "&Direction:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        actions.Controls.AddRange([_direction, _allow, _block, _edit]);

        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Margin = new Padding(0, 8, 0, 8) };
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        identity.Controls.Add(_identity, 0, 0);
        identity.Controls.Add(_publisher, 0, 1);
        identity.Controls.Add(_signature, 0, 2);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
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
        layout.Controls.Add(_scope, 0, 5);
        layout.Controls.Add(actions, 0, 6);
        Controls.Add(layout);

        _search.AccessibleName = "Search applications";
        _filter.AccessibleName = "Filter applications";
        _applications.AccessibleName = "Applications";
        _identity.AccessibleName = "Executable path and publisher metadata";
        _rules.AccessibleName = "Explicit executable rules";
        _attempts.AccessibleName = "Reported blocked attempts in this session";
        _direction.AccessibleName = "Rule direction";
        _search.TextChanged += (_, _) => RefreshApplications();
        _filter.SelectedIndexChanged += (_, _) => RefreshApplications();
        _applications.SelectionChanged += (_, _) => { if (!_updating) ShowDetails(); };
        _applications.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || _applications.Columns[e.ColumnIndex].Name != "Icon" || _icons == null) return;
            if (_applications.Rows[e.RowIndex].Tag is ApplicationEntry entry && _icons.ImageList is { } images)
            {
                int index = _icons.GetIconIndex(entry.AppPath);
                e.Value = index >= 0 && index < images.Images.Count ? images.Images[index] : null;
                e.FormattingApplied = true;
            }
        };
        _rules.SelectionChanged += (_, _) => UpdateActions();
        _direction.SelectedIndexChanged += (_, _) => UpdateScope();
        _allow.Click += async (_, _) => await ApplyAsync("Allow");
        _block.Click += async (_, _) => await ApplyAsync("Block");
        _edit.Click += async (_, _) => await EditAsync();
        UpdateScope();
        UpdateActions();
    }

    public void Initialize(MainViewModel viewModel, FirewallActionsService actions, BackgroundFirewallTaskService background,
        AppSettings settings, IconService icons)
    {
        _viewModel = viewModel;
        _actions = actions;
        _background = background;
        _settings = settings;
        _icons = icons;
        InitializeColumnSettings(settings);
        _viewModel.ReportedBlockedAttempts.CollectionChanged += AttemptsChanged;
        RefreshApplications();
    }

    public void RefreshApplications()
    {
        if (_viewModel == null || IsDisposed) return;
        _applications.Columns["Icon"]!.Visible = _settings.ShowAppIcons && DataGridViewHelper.IsColumnVisible(_settings.ApplicationsColumns, "Icon");
        DisplayApplications(ApplicationEntry.Build(_viewModel.ApplicationRules, _viewModel.ReportedBlockedAttempts)
            .Where(e => e.Matches(_search.Text.Trim(), _filter.SelectedIndex)).ToList());
    }

    internal void InitializeColumnSettings(AppSettings settings)
    {
        foreach (var (grid, columns) in new[]
        {
            (_applications, settings.ApplicationsColumns), (_rules, settings.ApplicationRulesColumns),
            (_attempts, settings.ApplicationAttemptsColumns)
        })
        {
            DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
            grid.ColumnWidthChanged += (_, _) => DataGridViewHelper.SaveColumnSettings(grid, columns, settings);
            grid.ColumnDisplayIndexChanged += (_, _) => DataGridViewHelper.SaveColumnSettings(grid, columns, settings);
        }
    }

    internal void DisplayApplications(List<ApplicationEntry> entries)
    {
        string? selected = SelectedApplication?.AppPath;
        string? selectedColumn = _applications.CurrentCell?.OwningColumn?.Name;
        int first = _applications.FirstDisplayedScrollingRowIndex;
        string? firstPath = first >= 0 ? (_applications.Rows[first].Tag as ApplicationEntry)?.AppPath : null;
        int horizontal = _applications.HorizontalScrollingOffset;
        _updating = true;
        try
        {
            _applications.Rows.Clear();
            foreach (var entry in entries)
            {
                int row = _applications.Rows.Add(null!, entry.Name, entry.AppPath, entry.RuleSummary,
                    entry.LatestAttempt?.ToString("g") ?? "Not observed");
                _applications.Rows[row].Tag = entry;
            }
            _applications.ClearSelection();
            if (entries.Count > 0)
            {
                int selectedIndex = entries.FindIndex(e => e.AppPath.Equals(selected, StringComparison.OrdinalIgnoreCase));
                var row = _applications.Rows[Math.Max(0, selectedIndex)];
                _applications.CurrentCell = row.Cells[selectedColumn != null && _applications.Columns[selectedColumn]!.Visible ? selectedColumn : "Name"];
                row.Selected = true;
                int top = entries.FindIndex(e => e.AppPath.Equals(firstPath, StringComparison.OrdinalIgnoreCase));
                _applications.FirstDisplayedScrollingRowIndex = top >= 0 ? top : Math.Clamp(first, 0, entries.Count - 1);
                _applications.HorizontalScrollingOffset = horizontal;
            }
        }
        finally { _updating = false; }
        _count.Text = entries.Count == 0 ? "No matching applications. Rules appear after refresh; reported blocks appear while monitoring."
            : $"{entries.Count} applications. Rules shown here target executables; global and package rules remain in Rules.";
        ShowDetails();
    }

    public void ApplyThemeFixes()
    {
        foreach (var grid in new[] { _applications, _rules, _attempts })
        {
            grid.ApplyTheme();
            grid.DefaultCellStyle.SelectionBackColor = Theme.Colors.SelectionInfo;
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        }
    }

    private ApplicationEntry? SelectedApplication => _applications.CurrentRow?.Selected == true
        ? _applications.CurrentRow.Tag as ApplicationEntry : null;
    private AdvancedRuleViewModel? SelectedRule => _rules.CurrentRow?.DataBoundItem as AdvancedRuleViewModel;

    private void ShowDetails()
    {
        var entry = SelectedApplication;
        string? selectedRule = SelectedRule?.ApplicationName.Equals(entry?.AppPath, StringComparison.OrdinalIgnoreCase) == true
            ? SelectedRule.Name : null;
        int firstRule = _rules.FirstDisplayedScrollingRowIndex;
        int horizontal = _rules.HorizontalScrollingOffset;
        var publishers = entry?.Rules.Select(r => r.AutoAllowedPublisher).Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        _identity.Text = entry?.AppPath ?? "Select an application to inspect its rules and reported blocks.";
        _publisher.Text = entry == null ? "" : $"Publisher (rule metadata): {(publishers.Count > 0 ? string.Join(", ", publishers) : "Not available")}";
        _signature.Text = entry == null ? "" : "Signature status: Not available";
        _rules.DataSource = new BindingList<AdvancedRuleViewModel>(entry?.Rules ?? []);
        if (selectedRule != null && entry != null)
        {
            int index = entry.Rules.FindIndex(r => r.Name == selectedRule);
            if (index >= 0)
            {
                _rules.CurrentCell = _rules.Rows[index].Cells[0];
                _rules.FirstDisplayedScrollingRowIndex = Math.Clamp(firstRule, 0, _rules.Rows.Count - 1);
                _rules.HorizontalScrollingOffset = horizontal;
            }
        }
        _attempts.DataSource = new BindingList<PendingConnectionViewModel>(entry?.Attempts.OrderByDescending(a => a.ObservedAt).ToList() ?? []);
        UpdateActions();
    }

    private void UpdateScope() => _scope.Text = $"Allow/Block: {_direction.Text.ToLowerInvariant()}, all profiles, protocols, ports and addresses. Replaces existing rules for that direction.";

    private void UpdateActions()
    {
        _allow.Enabled = _block.Enabled = !_busy && SelectedApplication != null;
        _edit.Enabled = !_busy && SelectedRule != null;
        _direction.Enabled = !_busy;
    }

    private async Task ApplyAsync(string action)
    {
        var entry = SelectedApplication;
        if (entry == null || _busy) return;
        string direction = _direction.SelectedIndex == 2 ? "(All)" : $"({_direction.Text})";
        string warning = $"{action} {entry.AppPath}\r\n\r\nDirection: {_direction.Text}.\r\nAll network profiles, protocols, ports and addresses; all services using this executable.\r\n\r\nExisting rules for this executable in the selected direction(s), including scoped and third-party rules, will be replaced.";
        if (action == "Allow") warning += "\r\nExisting Minimal Firewall general block rules for this executable may also be removed in either direction.";
        warning += "\r\n\r\nOther firewall rules may still affect traffic. Continue?";
        if (Messenger.MessageBox(warning, $"Confirm {action}", MsgIcon.Warning, MessageBoxButtons.YesNo,
            pIsDarkMode: Theme.Colors.Surface.R < 128, defaultButton: MessageBoxDefaultButton.Button2, owner: FindForm()) != DialogResult.Yes) return;
        await QueueAndRefreshAsync(() => _background.EnqueueTask(new FirewallTask(FirewallTaskType.ApplyApplicationRule,
            new ApplyApplicationRulePayload { AppPaths = [entry.AppPath], Action = $"{action} {direction}" })));
    }

    private async Task EditAsync()
    {
        var rule = SelectedRule;
        if (rule == null || _busy) return;
        using var dialog = new CreateAdvancedRuleForm(_actions, rule, _settings);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.RuleVm is not { } updated || rule.HasSameSettings(updated)) return;
        await QueueAndRefreshAsync(() =>
        {
            _background.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteAdvancedRules,
                new DeleteRulesPayload { RuleIdentifiers = [rule.Name] }));
            _background.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule,
                new CreateAdvancedRulePayload { ViewModel = updated, InterfaceTypes = updated.InterfaceTypes, IcmpTypesAndCodes = updated.IcmpTypesAndCodes }));
        });
    }

    private async Task QueueAndRefreshAsync(Action enqueue)
    {
        _busy = true;
        UpdateActions();
        try
        {
            enqueue();
            await _background.WhenIdleAsync();
            if (!IsDisposed && DataRefreshRequested != null) await DataRefreshRequested.Invoke();
        }
        catch (Exception ex)
        {
            if (!IsDisposed) Messenger.MessageBox($"Could not update application rules: {ex.Message}", "Applications",
                MessageBoxButtons.OK, MessageBoxIcon.Error, pIsDarkMode: Theme.Colors.Surface.R < 128);
        }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private void AttemptsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Visible) RefreshApplications();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _viewModel != null) _viewModel.ReportedBlockedAttempts.CollectionChanged -= AttemptsChanged;
        base.Dispose(disposing);
    }

    private static ThemedDataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, AllowUserToOrderColumns = true, RowHeadersVisible = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
    };

    private static void AddColumns(DataGridView grid, params (string Property, string Header, int Width)[] columns)
    {
        foreach (var column in columns)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = column.Property, DataPropertyName = column.Property, HeaderText = column.Header,
                Width = column.Width, SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }
    }
}
