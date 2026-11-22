using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public static class DataGridViewHelper
    {
        private const int MinimumColumnWidth = 30;
        private const int MaximumColumnWidth = 2000;

        public static void RestoreColumnSettings(
            DataGridView grid,
            Dictionary<string, ColumnState>? settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return;
            }

            grid.AllowUserToOrderColumns = true;
            grid.AllowUserToResizeColumns = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            foreach (var savedColumn in settings
                         .Where(item => grid.Columns.Contains(item.Key))
                         .OrderBy(item => item.Value.DisplayIndex))
            {
                DataGridViewColumn column = grid.Columns[savedColumn.Key]!;

                column.Width = Math.Clamp(
                    savedColumn.Value.Width,
                    MinimumColumnWidth,
                    MaximumColumnWidth);
            }

            var orderedColumns = grid.Columns
                .Cast<DataGridViewColumn>()
                .OrderBy(column =>
                    settings.TryGetValue(column.Name, out ColumnState? state) &&
                    state.DisplayIndex >= 0
                        ? state.DisplayIndex
                        : column.DisplayIndex)
                .ThenBy(column => column.Index)
                .ToList();

            for (int displayIndex = 0;
                 displayIndex < orderedColumns.Count;
                 displayIndex++)
            {
                orderedColumns[displayIndex].DisplayIndex = displayIndex;
            }
        }

        public static void SaveColumnSettings(
            DataGridView grid,
            Dictionary<string, ColumnState>? settings,
            AppSettings appSettings)
        {
            if (settings == null ||
                appSettings == null ||
                grid.IsDisposed ||
                grid.ClientSize.Width <= 0)
            {
                return;
            }

            bool changed = false;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                int width = Math.Clamp(
                    column.Width,
                    MinimumColumnWidth,
                    MaximumColumnWidth);

                if (!settings.TryGetValue(
                        column.Name,
                        out ColumnState? state))
                {
                    state = new ColumnState
                    {
                        DisplayIndex = column.DisplayIndex,
                        Width = width
                    };

                    settings[column.Name] = state;
                    changed = true;
                    continue;
                }

                if (state.DisplayIndex == column.DisplayIndex &&
                    state.Width == width)
                {
                    continue;
                }

                state.DisplayIndex = column.DisplayIndex;
                state.Width = width;
                changed = true;
            }

            if (changed)
            {
                appSettings.QueueSave();
            }
        }
    }
}
