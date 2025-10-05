// File: GroupsControl.cs
using DarkModeForms;
using MinimalFirewall.Groups;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Drawing;

namespace MinimalFirewall
{
    public partial class GroupsControl : UserControl
    {
        private FirewallGroupManager?
        _groupManager;
        private BackgroundFirewallTaskService? _backgroundTaskService;
        private DarkModeCS? _dm;
        private BindingSource _bindingSource;
        public GroupsControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void Initialize(FirewallGroupManager groupManager, BackgroundFirewallTaskService backgroundTaskService, DarkModeCS dm)
        {
            _groupManager = groupManager;
            _backgroundTaskService = backgroundTaskService;
            _dm = dm;

            groupsDataGridView.AutoGenerateColumns = false;
            _bindingSource = new BindingSource();
            groupsDataGridView.DataSource = _bindingSource;
        }

        public void ClearGroups()
        {
            _bindingSource.DataSource = null;
        }

        public async Task OnTabSelectedAsync()
        {
            await DisplayGroupsAsync();
        }

        private async Task DisplayGroupsAsync()
        {
            if (groupsDataGridView is null || _groupManager is null) return;
            var groups = await Task.Run(() => _groupManager.GetAllGroups());
            _bindingSource.DataSource = new SortableBindingList<FirewallGroup>(groups);
            groupsDataGridView.Refresh();
        }

        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groupsDataGridView.SelectedRows.Count > 0 && _backgroundTaskService != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete the \nselected group(s) and all associated rules?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    var rowsToDelete = new List<DataGridViewRow>();
                    foreach (DataGridViewRow row in groupsDataGridView.SelectedRows)
                    {
                        if (row.DataBoundItem is FirewallGroup group)

                        {
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteGroup, group.Name));
                            rowsToDelete.Add(row);
                        }
                    }

                    foreach (var row in rowsToDelete)
                    {
                        groupsDataGridView.Rows.Remove(row);
                    }
                }
            }
        }

        private void groupsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == groupsDataGridView.Columns["groupEnabledColumn"].Index)
            {
                if (groupsDataGridView.Rows[e.RowIndex].DataBoundItem is FirewallGroup group && _backgroundTaskService != null)

                {
                    bool newState = !group.IsEnabled;
                    group.SetEnabledState(newState);

                    var payload = new SetGroupEnabledStatePayload { GroupName = group.Name, IsEnabled = newState };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.SetGroupEnabledState, payload));

                    groupsDataGridView.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
        }

        private void groupsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == groupsDataGridView.Columns["groupEnabledColumn"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                if (groupsDataGridView.Rows[e.RowIndex].DataBoundItem is FirewallGroup group)
                {
                    DrawToggleSwitch(e.Graphics, e.CellBounds, group.IsEnabled);
                }

                e.Handled = true;
            }
        }

        private void DrawToggleSwitch(Graphics g, Rectangle bounds, bool isChecked)
        {
            if (_dm == null) return;
            int switchWidth = (int)(50 * (g.DpiY / 96f));
            int switchHeight = (int)(25 * (g.DpiY / 96f));
            int thumbSize = (int)(21 * (g.DpiY / 96f));
            int padding = (int)(10 * (g.DpiY / 96f));
            Rectangle switchRect = new Rectangle(
                bounds.X + (bounds.Width - switchWidth) / 2, // Centered
                bounds.Y + (bounds.Height - switchHeight) / 2,
                switchWidth,
                switchHeight);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color backColor = isChecked ? Color.FromArgb(0, 192, 0) : Color.FromArgb(200, 0, 0);
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(switchRect.X, switchRect.Y, switchRect.Height, switchRect.Height, 90, 180);
                path.AddArc(switchRect.Right - switchRect.Height, switchRect.Y, switchRect.Height, switchRect.Height, 270, 180);
                path.CloseFigure();
                g.FillPath(new SolidBrush(backColor), path);
            }

            int thumbX = isChecked ?
            switchRect.Right - thumbSize - (int)(2 * (g.DpiY / 96f)) : switchRect.X + (int)(2 * (g.DpiY / 96f));
            Rectangle thumbRect = new Rectangle(
                thumbX,
                switchRect.Y + (switchRect.Height - thumbSize) / 2,
                thumbSize,
                thumbSize);
            using (var thumbBrush = new SolidBrush(_dm.OScolors.TextActive))
            {
                g.FillEllipse(thumbBrush, thumbRect);
            }
        }

        private void groupsDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = groupsDataGridView.Columns[e.ColumnIndex];
            string propertyName = column.DataPropertyName;

            if (string.IsNullOrEmpty(propertyName)) return;

            var sortDirection = ListSortDirection.Ascending;
            if (groupsDataGridView.SortedColumn?.Name == column.Name && groupsDataGridView.SortOrder == SortOrder.Ascending)
            {
                sortDirection = ListSortDirection.Descending;
            }

            if (_bindingSource.DataSource is SortableBindingList<FirewallGroup> list)
            {
                list.Sort(propertyName, sortDirection);
            }

            groupsDataGridView.Sort(column, sortDirection);
        }

        private void groupsDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                var grid = (DataGridView)sender;
                var clickedRow = grid.Rows[e.RowIndex];

                if (!clickedRow.Selected)
                {
                    grid.ClearSelection();
                    clickedRow.Selected = true;
                }
            }
        }

        public class SortableBindingList<T> : BindingList<T>
        {
            private PropertyDescriptor?
            _sortProperty;
            private ListSortDirection _sortDirection;

            public SortableBindingList(IList<T> list) : base(list) { }

            protected override bool SupportsSortingCore => true;
            protected override bool IsSortedCore => _sortProperty != null;
            protected override PropertyDescriptor? SortPropertyCore => _sortProperty;
            protected override ListSortDirection SortDirectionCore => _sortDirection;
            protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
            {
                _sortProperty = prop;
                _sortDirection = direction;

                if (Items is List<T> items)
                {
                    items.Sort((a, b) =>
                    {
                        var valueA = prop.GetValue(a);

                        var valueB = prop.GetValue(b);

                        int result = (valueA as IComparable)?.CompareTo(valueB) ?? 0;
                        return direction == ListSortDirection.Ascending ? result : -result;

                    });

                    ResetBindings();
                }
            }

            public void Sort(string propertyName, ListSortDirection direction)
            {
                var prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
                if (prop != null)
                {
                    ApplySortCore(prop, direction);
                }
            }
        }
    }
}