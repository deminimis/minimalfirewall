using MinimalFirewall;
using System.Text.Json;

internal static class ColumnChecks
{
    public static void Run()
    {
        Verify(settings =>
        {
            var view = new ApplicationsControl();
            view.InitializeColumnSettings(settings);
            return view;
        });
        Verify(settings =>
        {
            var view = new RulesControl();
            view.InitializeColumnSettings(settings);
            return view;
        });
        Verify(settings =>
        {
            var view = new GroupsControl();
            view.Initialize(null!, null!, settings);
            return view;
        });
        Verify(settings =>
        {
            var view = new WildcardRulesControl();
            view.Initialize(null!, null!, settings);
            return view;
        });
        Verify(settings =>
        {
            var view = new AuditControl();
            var grid = Grids(view).Single();
            var source = new BindingSource();
            grid.Disposed += (_, _) => source.Dispose();
            grid.DataSource = source;
            view.InitializeColumnSettings(settings);
            source.DataSource = new SortableBindingList<FirewallRuleChange>([]);
            source.ResetBindings(false);
            return view;
        });
        VerifyWildcardButtons();
        VerifyIconPreference();
        VerifyFillAndRepair();
        Check(JsonSerializer.Deserialize("{\"Width\":125}", AppSettingsJsonContext.Default.ColumnState)!.Visible,
            "Existing settings without visibility must keep columns visible.");
        Console.WriteLine("Column menus toggle visibility and prevent hiding the last column. Visibility, widths and order survive reopening; tables remain filled.");
    }

    private static void Verify(Func<AppSettings, Control> create)
    {
        var settings = new AppSettings();
        var expected = new Dictionary<string, List<(string Name, int Width, int Index, bool Visible)>>();
        using (var host = new Form { ClientSize = new Size(870, 730) })
        {
            var view = create(settings);
            view.Dock = DockStyle.Fill;
            host.Controls.Add(view);
            host.Show();
            Application.DoEvents();
            foreach (var grid in Grids(view))
            {
                if (grid.Visible) CheckFilled(grid);
                VerifyColumnMenu(grid);
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    column.Width = 250 + column.Index * 7;
                    if (column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
                        Check(column.Width == 250 + column.Index * 7, $"{column.Name}: width must not be clamped.");
                }
                grid.Columns[0].DisplayIndex = grid.Columns.Count - 1;
                var hidden = grid.Columns.Cast<DataGridViewColumn>().Last(c => c is DataGridViewTextBoxColumn);
                ToggleColumn(grid, hidden);
                Check(!hidden.Visible, "Column must hide from the header menu.");
                if (view is AuditControl && grid.DataSource is BindingSource source)
                {
                    var columns = grid.Columns.Cast<DataGridViewColumn>().ToList();
                    source.DataSource = new SortableBindingList<FirewallRuleChange>([]);
                    source.ResetBindings(false);
                    Application.DoEvents();
                    Check(columns.SequenceEqual(grid.Columns.Cast<DataGridViewColumn>()), "Audit refresh must preserve column instances and header menus.");
                    Check(!hidden.Visible, "Audit refresh must preserve hidden columns.");
                    ToggleColumn(grid, hidden);
                    Check(hidden.Visible, "Audit menu must still show a hidden column after refresh.");
                    ToggleColumn(grid, hidden);
                }
                if (grid.Visible) CheckFilled(grid);
                expected[grid.AccessibleName ?? grid.Name] = Snapshot(grid);
            }
            host.ClientSize = new Size(1050, 700);
            Application.DoEvents();
            foreach (var grid in Grids(view))
                Check(expected[grid.AccessibleName ?? grid.Name].SequenceEqual(Snapshot(grid)), "Resizing a window must preserve column sizes and order.");
            host.ClientSize = new Size(2400, 730);
            Application.DoEvents();
            foreach (var grid in Grids(view).Where(g => g.Visible)) CheckFilled(grid);
        }
        string json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
        var loaded = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings)!;
        using var reopened = new Form { ClientSize = new Size(870, 730) };
        var restored = create(loaded);
        restored.Dock = DockStyle.Fill;
        reopened.Controls.Add(restored);
        reopened.Show();
        Application.DoEvents();
        foreach (var grid in Grids(restored))
            Check(expected[grid.AccessibleName ?? grid.Name].SequenceEqual(Snapshot(grid)), $"{grid.Name}: persisted column sizes and order must be restored.");
    }

    private static List<(string Name, int Width, int Index, bool Visible)> Snapshot(DataGridView grid) =>
        grid.Columns.Cast<DataGridViewColumn>().Select(c => (c.Name,
            c.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill ? 0 : c.Width, c.DisplayIndex, c.Visible)).ToList();

    private static void ToggleColumn(DataGridView grid, DataGridViewColumn column)
    {
        var menu = column.HeaderCell.ContextMenuStrip;
        Check(menu != null, "Each column header must provide the column menu.");
        menu!.Show(grid, Point.Empty);
        var item = menu.Items.OfType<ToolStripMenuItem>().Single(i => ReferenceEquals(i.Tag, column));
        Check(item.Checked == column.Visible, "Menu checkmark must match current visibility.");
        item.PerformClick();
        menu.Close();
    }

    private static void VerifyColumnMenu(DataGridView grid)
    {
        var original = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible && c is not DataGridViewButtonColumn).OrderBy(c => c is DataGridViewImageColumn).ToList();
        foreach (var column in original.Skip(1)) ToggleColumn(grid, column);
        var remaining = original[0];
        ToggleColumn(grid, remaining);
        Check(remaining.Visible, "The last visible column must not be hidden.");
        foreach (var column in original.Skip(1)) ToggleColumn(grid, column);
        Check(original.All(c => c.Visible), "Hidden columns must remain available in the menu.");
    }

    private static void VerifyIconPreference()
    {
        var settings = new AppSettings();
        using var host = new Form();
        using var view = new RulesControl { Dock = DockStyle.Fill };
        view.InitializeColumnSettings(settings);
        host.Controls.Add(view);
        host.Show();
        var grid = Grids(view).Single();
        var icon = grid.Columns.Cast<DataGridViewColumn>().Single(c => c is DataGridViewImageColumn);
        ToggleColumn(grid, icon);
        view.UpdateIconColumnVisibility();
        Check(!icon.Visible, "Refresh must preserve a hidden icon column.");
        settings.ShowAppIcons = false;
        view.UpdateIconColumnVisibility();
        settings.ShowAppIcons = true;
        view.UpdateIconColumnVisibility();
        Check(!icon.Visible, "Global icon settings must preserve per-table preference.");
        ToggleColumn(grid, icon);
        Check(icon.Visible, "Icon column must be restorable through its menu.");
    }

    // Regressions from review: fill column choice, width restore, button columns, all-hidden repair.
    private static void VerifyFillAndRepair()
    {
        var settings = new AppSettings();
        using var host = new Form { ClientSize = new Size(900, 300) };
        using var grid = new DataGridView { Dock = DockStyle.Fill };
        grid.Columns.Add(new DataGridViewImageColumn { Name = "Icon", Width = 32 });
        foreach (var name in new[] { "A", "B", "C" }) grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = name, Width = 100 });
        grid.Columns.Add(new DataGridViewButtonColumn { Name = "Act", HeaderText = "Act", Width = 60 });
        host.Controls.Add(grid);
        host.Show();

        var columns = new Dictionary<string, ColumnState>();
        DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
        Check(grid.Columns["C"]!.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill, "Last text column must fill.");
        var menu = grid.Columns["A"]!.HeaderCell.ContextMenuStrip!;
        Check(!menu.Items.OfType<ToolStripMenuItem>().Any(i => ReferenceEquals(i.Tag, grid.Columns["Act"])), "Button columns must not be in the column menu.");

        DataGridViewHelper.SaveColumnSettings(grid, columns, settings);
        Check(columns["C"].Width == 100, "Fill width must not be persisted.");

        ToggleColumn(grid, grid.Columns["C"]!);
        Check(grid.Columns["B"]!.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill, "Fill must move to the previous text column.");
        ToggleColumn(grid, grid.Columns["C"]!);
        Check(grid.Columns["B"]!.AutoSizeMode == DataGridViewAutoSizeColumnMode.None && grid.Columns["B"]!.Width == 100,
            "A column leaving Fill must return to its own width.");

        grid.Columns["C"]!.DisplayIndex = 1;
        DataGridViewHelper.SaveColumnSettings(grid, columns, settings);
        DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
        Check(grid.Columns["B"]!.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill, "Fill must follow display order, not declaration order.");

        foreach (var name in new[] { "Icon", "A", "B", "C" }) columns[name].Visible = false;
        DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
        Check(grid.Columns["A"]!.Visible && columns["A"].Visible, "All-hidden settings must show and persist a text column, not the icon.");

        settings.ShowAppIcons = false;
        columns["Icon"].Visible = true;
        DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
        Check(!grid.Columns["Icon"]!.Visible, "Icon column must respect ShowAppIcons when restored.");
        Check(DataGridViewHelper.IsColumnVisible(null, "A"), "Null settings must count as visible.");

        // Collapsed widths saved by older builds (Audit had 5 px columns) must not be restored.
        settings.ShowAppIcons = true;
        foreach (var name in new[] { "A", "B", "C" }) { columns[name].Visible = true; columns[name].Width = 5; }
        grid.Columns["A"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        grid.Columns["A"]!.Width = 100;
        DataGridViewHelper.RestoreColumnSettings(grid, columns, settings);
        Check(grid.Columns["A"]!.Width == 100, "Collapsed saved widths must be ignored on restore.");

        // A zero-width grid must not overwrite saved widths.
        columns["A"].Width = 140;
        grid.Dock = DockStyle.None; grid.Width = 0;
        grid.Columns["A"]!.Width = 30;
        DataGridViewHelper.SaveColumnSettings(grid, columns, settings);
        Check(columns["A"].Width == 140, "Saving from a zero-width grid must keep the stored width.");
        grid.Dock = DockStyle.Fill;
    }

    private static void CheckFilled(DataGridView grid)
    {
        int width = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).Sum(c => c.Width);
        Check(width >= grid.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2,
            $"{grid.Name}: columns must fill the available table width.");
    }

    private static void VerifyWildcardButtons()
    {
        foreach (bool dark in new[] { false, true })
        foreach (int percent in new[] { 100, 125, 150, 200 })
        {
            DarkModeForms.Theme.Colors = DarkModeForms.Theme.GetSystemColors(dark ? 0 : 1);
            using var host = new Form { ClientSize = new Size(700, 480) };
            using var view = new WildcardRulesControl { Dock = DockStyle.Fill };
            using var font = new Font(view.Font.FontFamily, view.Font.SizeInPoints * percent / 100f);
            view.Font = font;
            host.Controls.Add(view);
            host.Show();
            Application.DoEvents();
            var buttons = view.Controls.Find("topPanel", true).Single().Controls.OfType<Button>().ToList();
            foreach (var button in buttons)
            {
                Check(button.Height >= button.GetPreferredSize(Size.Empty).Height, $"{button.Name}: text must fit vertically.");
                Check(button.Parent!.ClientRectangle.Contains(button.Bounds), $"{button.Name}: button must fit inside toolbar.");
                Check(!buttons.Any(other => other != button && other.Bounds.IntersectsWith(button.Bounds)), "Toolbar buttons must not overlap.");
            }
            var directory = Path.Combine(Environment.CurrentDirectory, "artifacts", "applications");
            Directory.CreateDirectory(directory);
            using var image = new Bitmap(host.Width, host.Height);
            host.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(directory, $"wildcard-{(dark ? "dark" : "light")}-font-{percent}.png"));
        }
    }

    private static IEnumerable<DataGridView> Grids(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is DataGridView grid) yield return grid;
            foreach (var descendant in Grids(child)) yield return descendant;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
