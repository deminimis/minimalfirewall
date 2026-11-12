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
        Console.WriteLine("Column widths and order survive resize, serialization and reopening.");
    }

    private static void Verify(Func<AppSettings, Control> create)
    {
        var settings = new AppSettings();
        var expected = new Dictionary<string, List<(string Name, int Width, int Index)>>();
        using (var host = new Form { ClientSize = new Size(870, 730) })
        {
            var view = create(settings);
            view.Dock = DockStyle.Fill;
            host.Controls.Add(view);
            host.Show();
            Application.DoEvents();
            foreach (var grid in Grids(view))
            {
                Check(grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None, "Fill mode must not overwrite user widths.");
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    column.Width = 250 + column.Index * 7;
                    Check(column.Width == 250 + column.Index * 7, $"{column.Name}: width must not be clamped.");
                }
                grid.Columns[0].DisplayIndex = grid.Columns.Count - 1;
                expected[grid.AccessibleName ?? grid.Name] = Snapshot(grid);
            }
            host.ClientSize = new Size(1050, 700);
            Application.DoEvents();
            foreach (var grid in Grids(view))
                Check(expected[grid.AccessibleName ?? grid.Name].SequenceEqual(Snapshot(grid)), "Resizing a window must preserve column sizes and order.");
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

    private static List<(string Name, int Width, int Index)> Snapshot(DataGridView grid) =>
        grid.Columns.Cast<DataGridViewColumn>().Select(c => (c.Name, c.Width, c.DisplayIndex)).ToList();

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
