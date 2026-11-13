using System.Collections.Generic;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public static class DataGridViewHelper
    {
        public static void RestoreColumnSettings(DataGridView grid, Dictionary<string, ColumnState> settings, AppSettings appSettings)
        {
            // Keep user widths; one text column absorbs unused viewport space.
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.AllowUserToResizeColumns = true;
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
                    if (kvp.Value.Width > 0) col.Width = kvp.Value.Width;
                    col.Visible = kvp.Value.Visible;
                }
            }

            if (grid.Columns.Count > 0 && !grid.Columns.Cast<DataGridViewColumn>().Any(c => c.Visible))
                grid.Columns[0].Visible = true;
            UpdateFillColumn(grid);
            ConfigureColumnMenu(grid, settings, appSettings);
        }

        public static bool IsColumnVisible(Dictionary<string, ColumnState> settings, string columnName) =>
            !settings.TryGetValue(columnName, out var state) || state.Visible;

        private static void UpdateFillColumn(DataGridView grid)
        {
            DataGridViewColumn? fillColumn = null;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                if (column.Visible && column is DataGridViewTextBoxColumn)
                    fillColumn = column;
            }
            fillColumn ??= grid.Columns.Cast<DataGridViewColumn>().LastOrDefault(c => c.Visible);
            if (fillColumn != null) fillColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private static void ConfigureColumnMenu(DataGridView grid, Dictionary<string, ColumnState> settings, AppSettings appSettings)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("Columns") { Enabled = false });
            menu.Items.Add(new ToolStripSeparator());
            foreach (DataGridViewColumn column in grid.Columns)
            {
                string label = string.IsNullOrWhiteSpace(column.HeaderText)
                    ? (column is DataGridViewImageColumn ? "Icon" : column.Name) : column.HeaderText;
                var item = new ToolStripMenuItem(label.Replace("&", "&&")) { Tag = column };
                item.Click += (_, _) =>
                {
                    if (column.Visible && column is not DataGridViewImageColumn
                        && grid.Columns.Cast<DataGridViewColumn>().Count(c => c.Visible && c is not DataGridViewImageColumn) <= 1) return;
                    if (!settings.TryGetValue(column.Name, out var state))
                        settings[column.Name] = state = new ColumnState { Width = column.Width, DisplayIndex = column.DisplayIndex };
                    state.Visible = !column.Visible;
                    column.Visible = state.Visible;
                    UpdateFillColumn(grid);
                    SaveColumnSettings(grid, settings, appSettings);
                    appSettings.QueueSave();
                };
                menu.Items.Add(item);
                column.HeaderCell.ContextMenuStrip = menu;
            }
            menu.Opening += (_, _) =>
            {
                int visibleCount = grid.Columns.Cast<DataGridViewColumn>().Count(c => c.Visible && c is not DataGridViewImageColumn);
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
            if (appSettings == null) return;

            bool changed = false;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (!settings.TryGetValue(col.Name, out var state))
                {
                    state = new ColumnState();
                    settings[col.Name] = state;
                }

                if (state.DisplayIndex != col.DisplayIndex || state.Width != col.Width)
                {
                    state.DisplayIndex = col.DisplayIndex;
                    state.Width = col.Width;
                    changed = true;
                }
            }

            if (changed) appSettings.QueueSave();
        }
    }
}
