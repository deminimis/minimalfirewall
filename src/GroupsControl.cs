// File: GroupsControl.cs
using DarkModeForms;
using MinimalFirewall.Groups;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public partial class GroupsControl : UserControl
    {
        private FirewallGroupManager?
        _groupManager;
        private BackgroundFirewallTaskService? _backgroundTaskService;
        private DarkModeCS? _dm;
        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;
        private ListViewItem?
        _hoveredItem = null;

        public GroupsControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(groupsListView, true);
        }

        public void Initialize(FirewallGroupManager groupManager, BackgroundFirewallTaskService backgroundTaskService, DarkModeCS dm)
        {
            _groupManager = groupManager;
            _backgroundTaskService = backgroundTaskService;
            _dm = dm;
            SetDefaultColumnWidths();
            this.groupsListView.Resize += new System.EventHandler(this.GroupsListView_Resize);
        }

        public void ClearGroups()
        {
            if (groupsListView != null)
            {
                groupsListView.Items.Clear();
            }
        }

        private void SetDefaultColumnWidths()
        {
            groupNameColumn.Width = 400;
        }

        public async Task OnTabSelectedAsync()
        {
            await DisplayGroupsAsync();
        }

        private async Task DisplayGroupsAsync()
        {
            if (groupsListView is null || _groupManager is null) return;
            var groups = await Task.Run(() => _groupManager.GetAllGroups());
            groupsListView.BeginUpdate();
            groupsListView.Items.Clear();
            foreach (var group in groups)
            {
                var item = new ListViewItem(new[] { group.Name, "" })
                {
                    Tag = group,
                };
                groupsListView.Items.Add(item);
            }
            groupsListView.EndUpdate();
        }

        private void DrawToggleSwitch(Graphics g, Rectangle bounds, bool isChecked)
        {
            if (_dm == null) return;
            int switchWidth = (int)(50 * (g.DpiY / 96f));
            int switchHeight = (int)(25 * (g.DpiY / 96f));
            int thumbSize = (int)(21 * (g.DpiY / 96f));
            int padding = (int)(10 * (g.DpiY / 96f));
            Rectangle switchRect = new Rectangle(
                bounds.X + padding,
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

        private void groupsListView_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var hitInfo = groupsListView.HitTest(e.Location);
            if (hitInfo.Item == null || hitInfo.SubItem == null) return;
            if (hitInfo.Item.SubItems.IndexOf(hitInfo.SubItem) == 1)
            {
                if (hitInfo.Item.Tag is FirewallGroup group && _backgroundTaskService != null)
                {
                    bool newState = !group.IsEnabled;
                    group.SetEnabledState(newState);
                    groupsListView.Invalidate(hitInfo.Item.Bounds);

                    var payload = new SetGroupEnabledStatePayload { GroupName = group.Name, IsEnabled = newState };
                    _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.SetGroupEnabledState, payload));
                }
            }
        }

        private async void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count > 0 && _backgroundTaskService != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete the \nselected group(s) and all associated rules?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    foreach (ListViewItem item in groupsListView.SelectedItems)
                    {
                        if (item.Tag is FirewallGroup group)
                        {
                            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.DeleteGroup, group.Name));
                        }
                    }
                    await DisplayGroupsAsync();
                }
            }
        }

        private void groupsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if ((e.State & ListViewItemStates.Focused) != 0)
            {
                e.DrawFocusRectangle();
            }
        }

        private void groupsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
            }
            else
            {
                backColor = e.Item.ListView.BackColor;
                foreColor = e.Item.ListView.ForeColor;
            }

            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(20, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            else if (e.ColumnIndex == 1)
            {
                if (e.Item.Tag is FirewallGroup group)
                {
                    DrawToggleSwitch(e.Graphics, e.Bounds, group.IsEnabled);
                }
            }
        }

        private void groupsListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem?
            itemUnderMouse = groupsListView.GetItemAt(e.X, e.Y);

            if (_hoveredItem != itemUnderMouse)
            {
                if (_hoveredItem != null && _hoveredItem.ListView != null && _hoveredItem.Index >= 0)
                {
                    try { groupsListView.Invalidate(_hoveredItem.Bounds); }
                    catch (ArgumentOutOfRangeException) { }
                }
                _hoveredItem = itemUnderMouse;
                if (_hoveredItem != null)
                {
                    try { groupsListView.Invalidate(_hoveredItem.Bounds); }
                    catch (ArgumentOutOfRangeException) { }
                }
            }
        }

        private void groupsListView_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredItem != null)
            {
                try { groupsListView.Invalidate(_hoveredItem.Bounds); }
                catch (ArgumentOutOfRangeException) { }
                _hoveredItem = null;
            }
        }

        private void groupsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                _sortOrder = (_sortOrder == SortOrder.Ascending) ?
                    SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }
            _sortColumn = e.Column;
            groupsListView.ListViewItemSorter = new ListViewItemComparer(e.Column, _sortOrder);
            groupsListView.Sort();
        }

        private void GroupsListView_Resize(object? sender, EventArgs e)
        {
            if (groupsListView.Columns.Count < 2) return;
            int totalColumnWidths = 0;
            for (int i = 0; i < groupsListView.Columns.Count - 1; i++)
            {
                totalColumnWidths += groupsListView.Columns[i].Width;
            }

            int lastColumnIndex = groupsListView.Columns.Count - 1;
            int lastColumnWidth = groupsListView.ClientSize.Width - totalColumnWidths;

            int minWidth = TextRenderer.MeasureText(groupsListView.Columns[lastColumnIndex].Text, groupsListView.Font).Width + 10;
            if (lastColumnWidth > minWidth)
            {
                groupsListView.Columns[lastColumnIndex].Width = lastColumnWidth;
            }
            else
            {
                groupsListView.Columns[lastColumnIndex].Width = minWidth;
            }
        }
    }
}