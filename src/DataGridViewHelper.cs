using System.Collections.Generic;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public static class DataGridViewHelper
    {
        private const int MinRestoredWidth = 20;

        // Columns the designer marked as Fill, recorded before restore resets every column to None.
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, HashSet<string>> _designedFill = new();

        public static void RestoreColumnSettings(DataGridView grid, Dictionary<string, ColumnState> settings, AppSettings appSettings)
        {
            settings ??= [];

            // Keep user widths; one text column absorbs unused viewport space.
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.AllowUserToResizeColumns = true;
            if (!_designedFill.TryGetValue(grid, out _))
            {
                _designedFill.Add(grid, grid.Columns.Cast<DataGridViewColumn>()
                    .Where(c => c.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill).Select(c => c.Name).ToHashSet());
            }
            foreach (DataGridViewColumn column in grid.Columns) column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            var sortedList = new List<KeyValuePair<string, ColumnState>>(settings);
            sortedList.Sort((a, b) => a.Value.DisplayIndex.CompareTo(b.Value.DisplayIndex));

            foreach (var kvp in sortedList)
            {
                if (grid.Columns[kvp.Key] is DataGridViewColumn col)
                {
                    if (kvp.Value.DisplayIndex >= 0 && kvp.Value.DisplayIndex < grid.Columns.Count)
                    {
                        col.DisplayIndex = kvp.Value.DisplayIndex;
                    }
                    // Widths below this were saved from a collapsed or hidden grid; keep the default instead.
                    if (kvp.Value.Width >= MinRestoredWidth) col.Width = kvp.Value.Width;
                    // Action buttons are not user-toggleable; app icons also follow the global icon setting.
                    if (col is not DataGridViewButtonColumn)
                    {
                        col.Visible = kvp.Value.Visible && (col is not DataGridViewImageColumn || appSettings.ShowAppIcons);
                    }
                }
            }

            if (!DataColumns(grid).Any(c => c.Visible) && DataColumns(grid).FirstOrDefault() is DataGridViewColumn first)
            {
                // Never restore a table with no readable column; persist the repair so it is not repeated.
                first.Visible = true;
                if (settings.TryGetValue(first.Name, out var state)) state.Visible = true;
            }
            UpdateFillColumn(grid, settings);
            ConfigureColumnMenu(grid, settings, appSettings);
        }

        public static bool IsColumnVisible(Dictionary<string, ColumnState>? settings, string columnName) =>
            settings is null || !settings.TryGetValue(columnName, out var state) || state.Visible;

        public static bool IsIconColumnVisible(AppSettings appSettings, Dictionary<string, ColumnState>? settings, string columnName) =>
            appSettings.ShowAppIcons && IsColumnVisible(settings, columnName);

        // Columns that carry row data: the menu lists them and at least one must stay visible.
        private static IEnumerable<DataGridViewColumn> DataColumns(DataGridView grid) =>
            grid.Columns.Cast<DataGridViewColumn>().Where(c => c is not DataGridViewImageColumn and not DataGridViewButtonColumn);

        private static void UpdateFillColumn(DataGridView grid, Dictionary<string, ColumnState> settings)
        {
            // A grid in Fill mode (Dashboard) distributes width by FillWeight itself.
            if (grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill)
            {
                return;
            }

            var visible = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
            // Prefer the designer's fill column (a text column can also hold a drawn toggle, as in Groups).
            var designed = _designedFill.TryGetValue(grid, out var names) ? names : [];
            var fillColumn = visible.LastOrDefault(c => designed.Contains(c.Name))
                ?? visible.LastOrDefault(c => c is DataGridViewTextBoxColumn)
                ?? visible.LastOrDefault(c => c is not DataGridViewImageColumn and not DataGridViewButtonColumn);

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column == fillColumn || column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
                {
                    continue;
                }

                // Leaving Fill keeps the stretched width; return to the user's own width instead.
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                if (settings.TryGetValue(column.Name, out var state) && state.Width >= MinRestoredWidth)
                {
                    column.Width = state.Width;
                }
            }

            if (fillColumn != null && fillColumn.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
            {
                // Remember the user's width before Fill stretches it, so it can be restored later.
                if (!settings.TryGetValue(fillColumn.Name, out var state))
                {
                    settings[fillColumn.Name] = state = new ColumnState { DisplayIndex = fillColumn.DisplayIndex };
                }
                if (state.Width < MinRestoredWidth) state.Width = fillColumn.Width;
                fillColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private static void ConfigureColumnMenu(DataGridView grid, Dictionary<string, ColumnState> settings, AppSettings appSettings)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("Columns") { Enabled = false });
            menu.Items.Add(new ToolStripSeparator());
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.HeaderCell.ContextMenuStrip = menu;
                if (column is DataGridViewButtonColumn)
                {
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(column.HeaderText)
                    ? (column is DataGridViewImageColumn ? "Icon" : column.Name) : column.HeaderText;
                var item = new ToolStripMenuItem(label.Replace("&", "&&")) { Tag = column };
                item.Click += (_, _) =>
                {
                    if (column.Visible && column is not DataGridViewImageColumn && DataColumns(grid).Count(c => c.Visible) <= 1) return;
                    if (!settings.TryGetValue(column.Name, out var state))
                        settings[column.Name] = state = new ColumnState { Width = column.Width, DisplayIndex = column.DisplayIndex };
                    state.Visible = !column.Visible;
                    column.Visible = state.Visible;
                    UpdateFillColumn(grid, settings);
                    appSettings.QueueSave();
                };
                menu.Items.Add(item);
            }
            menu.Opening += (_, _) =>
            {
                int visibleCount = DataColumns(grid).Count(c => c.Visible);
                foreach (ToolStripMenuItem item in menu.Items.OfType<ToolStripMenuItem>())
                {
                    if (item.Tag is not DataGridViewColumn column) continue;
                    item.Checked = column.Visible;
                    item.Enabled = (!column.Visible || column is DataGridViewImageColumn || visibleCount > 1)
                        && (column is not DataGridViewImageColumn || appSettings.ShowAppIcons);
                }
            };
            grid.TopLeftHeaderCell.ContextMenuStrip = menu;
            grid.Disposed += (_, _) => menu.Dispose();
        }

        public static void SaveColumnSettings(DataGridView grid, Dictionary<string, ColumnState> settings, AppSettings appSettings)
        {
            if (appSettings == null || settings == null) return;
            // A zero-width grid (not laid out yet, or collapsed) reports widths that must not overwrite the user's layout.
            if (grid.ClientSize.Width <= 0) return;

            bool changed = false;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (!settings.TryGetValue(col.Name, out var state))
                {
                    state = new ColumnState();
                    settings[col.Name] = state;
                }

                // A Fill column's width follows the viewport, so persisting it would overwrite the user's width.
                bool isFill = col.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.Fill;
                if (state.DisplayIndex != col.DisplayIndex || (!isFill && state.Width != col.Width))
                {
                    state.DisplayIndex = col.DisplayIndex;
                    if (!isFill) state.Width = col.Width;
                    changed = true;
                }
            }

            if (changed) appSettings.QueueSave();
        }
    }
}
