using MinimalFirewall;
using MinimalFirewall.TypedObjects;
using DarkModeForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!string.Equals(Path.GetFileName(Environment.ProcessPath), "ApplicationsChecks.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Run ApplicationsChecks.exe so its portable settings stay inside the test output directory.");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "settings.json"), "{}");
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        PopupChecks.Run();
        ColumnChecks.Run();

        static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        var first = new AdvancedRuleViewModel
        {
            Name = "Shared display name", ApplicationName = @"C:\One\browser.exe", Status = "Allow",
            IsEnabled = true, Direction = Directions.Outgoing, Profiles = "Private", RemotePorts = "443", AutoAllowedPublisher = "Example Publisher"
        };
        var second = new AdvancedRuleViewModel
        {
            Name = "Shared display name", ApplicationName = @"C:\Two\browser.exe", Status = "Block",
            IsEnabled = true, Direction = Directions.Incoming, Profiles = "Public", RemotePorts = "*"
        };
        var conflict = new AdvancedRuleViewModel
        {
            Name = "Scoped block", ApplicationName = @"c:\ONE\BROWSER.EXE", Status = "Block", IsEnabled = false,
            Direction = Directions.Incoming, Profiles = "Domain", RemotePorts = "80"
        };
        var timestamp = new DateTime(2026, 1, 2, 3, 4, 5);
        var attempt = new PendingConnectionViewModel
        {
            AppPath = @"C:\One\browser.exe", ObservedAt = timestamp, RemoteAddress = "192.0.2.1", RemotePort = "80"
        };
        var observedOnly = new PendingConnectionViewModel { AppPath = @"C:\Three\other.exe", ObservedAt = timestamp.AddMinutes(1) };
        var entries = ApplicationEntry.Build([first, second, conflict,
            new() { ApplicationName = "*", Status = "Block" },
            new() { ApplicationName = "", Status = "Allow" },
            new() { ApplicationName = "S-1-15-2-12345", Status = "Allow" },
            new() { ApplicationName = @"C:\*\wildcard.exe", Status = "Allow" }], [attempt, observedOnly]);

        Check(entries.Count == 3, "Keep separate executables; exclude global, wildcard and package identities.");
        var one = entries.Single(e => e.AppPath.Equals(first.ApplicationName, StringComparison.OrdinalIgnoreCase));
        Check(one.Name == "browser.exe", "Display executable name.");
        Check(one.Rules.Count == 2 && one.Rules.Contains(conflict), "Group case-insensitive executable identities without losing disabled rules.");
        Check(one.RuleSummary == "Mixed (2 rules)", "Conflicting rules must not become a global allow/block status.");
        Check(one.Rules[0].RemotePorts == "443" && one.Rules[1].Profiles == "Domain", "Keep original rule scopes intact.");
        Check(one.LatestAttempt == timestamp, "Show observed timestamp.");
        Check(one.Matches("BROWSER", 0) && one.Matches(@"c:\one", 0), "Search name and path, case-insensitively.");
        Check(!one.Matches("missing", 0), "Exclude nonmatching search.");
        Check(one.Matches("", 1) && one.Matches("", 2) && one.Matches("", 3), "Filter mixed application by observations and each explicit action.");
        var two = entries.Single(e => e.AppPath == second.ApplicationName);
        Check(!two.Matches("", 1) && !two.Matches("", 2) && two.Matches("", 3), "Do not mistake explicit blocks for observed blocks.");
        Check(two.LatestAttempt == null, "Do not invent an observation timestamp from a rule.");
        var three = entries.Single(e => e.AppPath == observedOnly.AppPath);
        Check(three.RuleSummary == "No explicit executable rules" && !three.Matches("", 3), "Observed blocks are not explicit block rules.");
        Check(three.Matches("", 1), "Include applications observed without explicit rules.");
        Check(ApplicationEntry.Build([conflict], []).Single().RuleSummary == "Block rule (disabled)", "Label disabled rule explicitly.");
        Check(ApplicationEntry.Build([first, first], []).Single().RuleSummary == "Multiple rules (2)", "Keep same-action scopes visible.");
        var latest = new PendingConnectionViewModel { AppPath = attempt.AppPath, ObservedAt = timestamp.AddMinutes(2) };
        Check(ApplicationEntry.Build([first], [latest, attempt]).Single().LatestAttempt == latest.ObservedAt, "Use latest timestamp, independent of input order.");
        Check(ApplicationEntry.Build([], []).Count == 0, "Handle an empty snapshot.");
        Check(ApplicationEntry.Build([new() { ApplicationName = @"C:\Tools\legacy.com" }], []).Count == 1, "Support executable paths beyond the .exe extension.");
        Check(ApplicationEntry.Build([new() { ApplicationName = @"relative\tool.exe" }], []).Count == 0, "Do not treat relative paths as executable identities.");
        var screenshotPath = Path.Combine(Environment.CurrentDirectory, "artifacts", "applications");
        Directory.CreateDirectory(screenshotPath);
        using (var before = new Form { ClientSize = new Size(870, 730), Text = "Existing Rules view (empty fixture)" })
        {
            var rules = new RulesControl { Dock = DockStyle.Fill };
            before.Controls.Add(rules);
            before.Show();
            MainForm.RefreshAllThemedControls(before);
            rules.ApplyThemeFixes();
            using var image = new Bitmap(before.Width, before.Height);
            before.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(screenshotPath, "before-rules-empty.png"));
        }

        foreach (bool dark in new[] { false, true })
        foreach (int percent in new[] { 100, 125, 150, 200 })
        {
            Theme.Colors = Theme.GetSystemColors(dark ? 0 : 1);
            using var form = new KeyboardForm { ClientSize = new Size(870, 730), Text = "Applications (synthetic fixture)" };
            var view = new ApplicationsControl();
            form.Controls.Add(view);
            form.Show();
            MainForm.RefreshAllThemedControls(form);
            view.ApplyThemeFixes();
            using var displayFont = new Font(view.Font.FontFamily, view.Font.SizeInPoints * percent / 100f);
            view.Font = displayFont;

            var snapshot = ApplicationEntry.Build([first, second, conflict], [attempt, observedOnly]);
            snapshot.AddRange(Enumerable.Range(0, 60).Select(i => new ApplicationEntry { AppPath = $@"C:\Fixture\Application{i:00}\tool.exe" }));
            view.DisplayApplications(snapshot);
            var controls = Descendants(view).ToList();
            var grid = controls.OfType<DataGridView>().Single(c => c.AccessibleName == "Applications");
            grid.Columns["AppPath"]!.Width = 500;
            grid.CurrentCell = grid.Rows[25].Cells["AppPath"];
            grid.Rows[25].Selected = true;
            grid.FirstDisplayedScrollingRowIndex = 20;
            grid.HorizontalScrollingOffset = 120;
            string selectedPath = ((ApplicationEntry)grid.Rows[25].Tag!).AppPath;
            string topPath = ((ApplicationEntry)grid.Rows[20].Tag!).AppPath;
            int horizontal = grid.HorizontalScrollingOffset;
            Check(horizontal > 0, "Exercise a genuinely scrolled horizontal viewport.");
            snapshot.Insert(0, new ApplicationEntry { AppPath = @"C:\New\new.exe" });
            view.DisplayApplications(snapshot);
            Check(((ApplicationEntry)grid.CurrentRow!.Tag!).AppPath == selectedPath, "Refresh must preserve selected executable when row positions change.");
            Check(((ApplicationEntry)grid.Rows[grid.FirstDisplayedScrollingRowIndex].Tag!).AppPath == topPath, "Refresh must preserve top visible executable.");
            Check(grid.HorizontalScrollingOffset == horizontal, "Refresh must preserve horizontal scroll.");
            Check(grid.CurrentCell!.OwningColumn!.Name == "AppPath", "Refresh must preserve focused column.");
            var identity = controls.OfType<TextBox>().Single(c => c.AccessibleName == "Executable path and publisher metadata");
            Check(identity.Text.Contains(selectedPath), "Details must follow selected executable.");
            view.DisplayApplications([]);
            Check(grid.Rows.Count == 0 && !identity.Text.Contains(selectedPath), "Empty result must clear stale selection and details.");
            Check(!controls.OfType<Button>().Single(c => c.Text == "&Allow…").Enabled, "Empty result must disable application actions.");

            view.DisplayApplications(ApplicationEntry.Build([first, second, conflict], [attempt, observedOnly]));
            grid.Columns["AppPath"]!.Width = 260;
            var ruleGrid = controls.OfType<DataGridView>().Single(c => c.AccessibleName == "Explicit executable rules");
            ruleGrid.CurrentCell = ruleGrid.Rows[1].Cells["Name"];
            ruleGrid.Rows[1].Selected = true;
            view.DisplayApplications(ApplicationEntry.Build([first, second, conflict], [attempt, observedOnly]));
            Check(((AdvancedRuleViewModel)ruleGrid.CurrentRow!.DataBoundItem!).Name == conflict.Name, "Refresh must preserve the selected individual rule.");
            Check(form.ActivateMnemonic('s') && controls.OfType<TextBox>().Single(c => c.AccessibleName == "Search applications").Focused, "Search mnemonic must focus search.");
            Check(form.ActivateMnemonic('f') && controls.OfType<ComboBox>().Single(c => c.AccessibleName == "Filter applications").Focused, "Filter mnemonic must focus filter.");
            Check(form.ActivateMnemonic('d') && controls.OfType<ComboBox>().Single(c => c.AccessibleName == "Rule direction").Focused, "Direction mnemonic must focus direction.");
            grid.Focus();
            Check(grid.Focused, "Application grid must accept keyboard focus.");
            Check(grid.Rows.Cast<DataGridViewRow>().Where(r => r.Displayed).All(r => r.Height > grid.Font.Height), "Visible rows must fit enlarged text without clipping.");
            Check(controls.OfType<DataGridView>().All(c => c.AllowUserToResizeColumns && c.TabStop), "Grids must allow resizing and keyboard access.");
            using var image = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(screenshotPath, $"applications-{(dark ? "dark" : "light")}-layout-{percent}.png"));
            controls.OfType<TabControl>().Single().SelectedIndex = 1;
            form.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(screenshotPath, $"applications-{(dark ? "dark" : "light")}-reports-layout-{percent}.png"));
        }

        Console.WriteLine("Applications model and UI checks passed. Screenshots: " + screenshotPath);
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}

internal sealed class KeyboardForm : Form
{
    public bool ActivateMnemonic(char key) => ProcessMnemonic(key);
}
