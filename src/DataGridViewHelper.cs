using System.Collections.Generic;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public static class DataGridViewHelper
    {
        public static void RestoreColumnSettings(DataGridView grid, Dictionary<string, ColumnState> settings)
        {
            if (settings == null || settings.Count == 0) return;

            var sortedList = new List<KeyValuePair<string, ColumnState>>(settings);
            sortedList.Sort((a, b) => a.Value.DisplayIndex.CompareTo(b.Value.DisplayIndex));

            foreach (var kvp in sortedList)
            {
                if (grid.Columns.Contains(kvp.Key))
                {
                    var col = grid.Columns[kvp.Key];
                    if (kvp.Value.DisplayIndex >= 0 && kvp.Value.DisplayIndex < grid.Columns.Count)
                    {
                        col.DisplayIndex = kvp.Value.DisplayIndex;
                    }
                    if (kvp.Value.Width > 0) col.Width = kvp.Value.Width;
                }
            }
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
