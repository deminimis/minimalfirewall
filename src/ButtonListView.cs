// File: ButtonListView.cs
using DarkModeForms;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MinimalFirewall
{
    public class ListViewItemEventArgs : EventArgs
    {
        public ListViewItem Item { get; }
        public ListViewItemEventArgs(ListViewItem item) { Item = item; }
    }

    public partial class ButtonListView : ListView
    {
        public enum Mode { Dashboard, Audit }

        private ListViewItem? _hotItem;
        private Rectangle? _hotButtonBounds;
        private ListViewItem? _pressedItem;
        private Rectangle? _pressedButtonBounds;
        private ListViewItem? _hoveredItem;
        [Category("Behavior")]
        public Mode ViewMode { get; set; } = Mode.Dashboard;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkModeCS? DarkMode { get; set; }

        public event EventHandler<ListViewItemEventArgs>? AllowClicked;
        public event EventHandler<ListViewItemEventArgs>? BlockClicked;
        public event EventHandler<ListViewItemEventArgs>? IgnoreClicked;
        public event EventHandler<ListViewItemEventArgs>? AcceptClicked;
        public event EventHandler<ListViewItemEventArgs>? DeleteClicked;
        public ButtonListView()
        {
            this.OwnerDraw = true;
            this.DoubleBuffered = true;
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null) return;
            e.DrawDefault = false;

            Color backColor;
            Color foreColor;

            if (e.Item.Selected)
            {
                backColor = this.Focused ? SystemColors.Highlight : SystemColors.ControlDark;
                foreColor = this.Focused ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            else
            {
                backColor = this.BackColor;
                foreColor = this.ForeColor;

                if (this.ViewMode == Mode.Audit && e.Item.Tag is FirewallRuleChange change && change.Rule != null)
                {
                    switch (change.Type)
                    {
                        case ChangeType.New:
                            backColor = Color.FromArgb(204, 255, 204);
                            foreColor = Color.Black;
                            break;
                        case ChangeType.Modified:
                            backColor = change.Rule.Status.Contains("Allow", StringComparison.OrdinalIgnoreCase)
                                ? Color.FromArgb(204, 255, 204)
                                : Color.FromArgb(255, 204, 204);
                            foreColor = Color.Black;
                            break;
                        case ChangeType.Deleted:
                            backColor = Color.FromArgb(255, 204, 204);
                            foreColor = Color.Black;
                            break;
                    }
                }
            }

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
            if (_hoveredItem == e.Item && !e.Item.Selected)
            {
                using var overlayBrush = new SolidBrush(Color.FromArgb(25, Color.Black));
                e.Graphics.FillRectangle(overlayBrush, e.Bounds);
            }

            int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
            int iconColumnIndex = 0;

            if (e.ColumnIndex == iconColumnIndex && this.ViewMode == Mode.Dashboard)
            {
                if (e.Item.ImageIndex != -1 && e.Item.ImageList != null)
                {
                    Image img = e.Item.ImageList.Images[e.Item.ImageIndex];
                    int imgX = e.Bounds.Left + (e.Bounds.Width - img.Width) / 2;
                    int imgY = e.Bounds.Top + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, imgX, imgY);
                }
            }
            else if (e.ColumnIndex == buttonColumnIndex)
            {
                int buttonWidth = Scale(70, e.Graphics);
                int buttonHeight = Scale(22, e.Graphics);
                int buttonSpacing = Scale(5, e.Graphics);
                Point center = new Point(e.Bounds.Left + buttonSpacing, e.Bounds.Y + (e.Bounds.Height - buttonHeight) / 2);
                if (this.ViewMode == Mode.Dashboard)
                {
                    var allowButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    var blockButtonRect = new Rectangle(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    var ignoreButtonRect = new Rectangle(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    PushButtonState GetState(Rectangle rect) => _pressedButtonBounds == rect && _pressedItem == e.Item ? PushButtonState.Pressed : _hotButtonBounds == rect ? PushButtonState.Hot : PushButtonState.Normal;

                    DrawManualButton(e.Graphics, allowButtonRect, "Allow", GetState(allowButtonRect));
                    DrawManualButton(e.Graphics, blockButtonRect, "Block", GetState(blockButtonRect));
                    DrawManualButton(e.Graphics, ignoreButtonRect, "Ignore", GetState(ignoreButtonRect));
                }
                else
                {
                    var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    PushButtonState GetState(Rectangle rect) => _pressedButtonBounds == rect && _pressedItem == e.Item ? PushButtonState.Pressed : _hotButtonBounds == rect ? PushButtonState.Hot : PushButtonState.Normal;

                    DrawManualButton(e.Graphics, acceptButtonRect, "Accept", GetState(acceptButtonRect));
                    DrawManualButton(e.Graphics, deleteButtonRect, "Delete", GetState(deleteButtonRect));
                    DrawManualButton(e.Graphics, ignoreButtonRect, "Ignore", GetState(ignoreButtonRect));
                }
            }
            else
            {
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PathEllipsis;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, foreColor, flags);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ListViewItem? itemUnderMouse = this.GetItemAt(e.X, e.Y);

            if (_hoveredItem != itemUnderMouse)
            {
                if (_hoveredItem != null)
                {
                    this.Invalidate(_hoveredItem.Bounds);
                }
                _hoveredItem = itemUnderMouse;
                if (_hoveredItem != null)
                {
                    this.Invalidate(_hoveredItem.Bounds);
                }
            }

            var hitTest = this.HitTest(e.Location);
            Rectangle? newHotBounds = null;
            ListViewItem? currentItem = hitTest.Item;

            if (currentItem != null && hitTest.SubItem != null)
            {
                int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
                if (currentItem.SubItems.IndexOf(hitTest.SubItem) == buttonColumnIndex)
                {
                    int buttonWidth = Scale(70, CreateGraphics());
                    int buttonHeight = Scale(22, CreateGraphics());
                    int buttonSpacing = Scale(5, CreateGraphics());
                    Rectangle bounds = hitTest.SubItem.Bounds;
                    Point center = new Point(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);
                    if (this.ViewMode == Mode.Dashboard)
                    {
                        Rectangle allowButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                        if (allowButtonRect.Contains(e.Location)) newHotBounds = allowButtonRect;

                        Rectangle blockButtonRect = new Rectangle(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                        if (blockButtonRect.Contains(e.Location)) newHotBounds = blockButtonRect;
                        Rectangle ignoreButtonRect = new Rectangle(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                        if (ignoreButtonRect.Contains(e.Location)) newHotBounds = ignoreButtonRect;
                    }
                    else
                    {
                        var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                        if (acceptButtonRect.Contains(e.Location)) newHotBounds = acceptButtonRect;

                        var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                        if (deleteButtonRect.Contains(e.Location)) newHotBounds = deleteButtonRect;
                        var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                        if (ignoreButtonRect.Contains(e.Location)) newHotBounds = ignoreButtonRect;
                    }
                }
            }

            ListViewItem? newHotItem = newHotBounds.HasValue ? currentItem : null;
            if (_hotItem != newHotItem || _hotButtonBounds != newHotBounds)
            {
                ListViewItem? oldHotItem = _hotItem;
                _hotButtonBounds = newHotBounds;
                _hotItem = newHotItem;

                int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
                if (oldHotItem != null && oldHotItem.ListView != null)
                {
                    this.Invalidate(oldHotItem.SubItems[buttonColumnIndex].Bounds);
                }
                if (_hotItem != null)
                {
                    this.Invalidate(_hotItem.SubItems[buttonColumnIndex].Bounds);
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredItem != null)
            {
                this.Invalidate(_hoveredItem.Bounds);
                _hoveredItem = null;
            }
            if (_hotItem != null)
            {
                int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
                this.Invalidate(_hotItem.SubItems[buttonColumnIndex].Bounds);
                _hotItem = null;
                _hotButtonBounds = null;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            _pressedButtonBounds = null;
            var hitTest = this.HitTest(e.Location);
            if (hitTest.Item == null) return;
            if (_hotButtonBounds.HasValue && hitTest.Item == _hotItem)
            {
                _pressedItem = _hotItem;
                _pressedButtonBounds = _hotButtonBounds;
                int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
                this.Invalidate(hitTest.Item.SubItems[buttonColumnIndex].Bounds);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_pressedItem != null)
            {
                int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
                this.Invalidate(_pressedItem.SubItems[buttonColumnIndex].Bounds);
            }
            _pressedItem = null;
            _pressedButtonBounds = null;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            var hitTest = this.HitTest(e.Location);
            if (hitTest.Item == null || hitTest.SubItem == null) return;
            int buttonColumnIndex = (this.ViewMode == Mode.Dashboard) ? 1 : 0;
            if (hitTest.Item.SubItems.IndexOf(hitTest.SubItem) == buttonColumnIndex)
            {
                int buttonWidth = Scale(70, CreateGraphics());
                int buttonHeight = Scale(22, CreateGraphics());
                int buttonSpacing = Scale(5, CreateGraphics());
                Rectangle bounds = hitTest.SubItem.Bounds;
                Point center = new Point(bounds.Left + buttonSpacing, bounds.Y + (bounds.Height - buttonHeight) / 2);
                if (ViewMode == Mode.Dashboard)
                {
                    Rectangle allowButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    Rectangle blockButtonRect = new Rectangle(allowButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    Rectangle ignoreButtonRect = new Rectangle(blockButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    if (allowButtonRect.Contains(e.Location)) AllowClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                    else if (blockButtonRect.Contains(e.Location)) BlockClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                    else if (ignoreButtonRect.Contains(e.Location)) IgnoreClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                }
                else
                {
                    var acceptButtonRect = new Rectangle(center, new Size(buttonWidth, buttonHeight));
                    var deleteButtonRect = new Rectangle(acceptButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    var ignoreButtonRect = new Rectangle(deleteButtonRect.Right + buttonSpacing, center.Y, buttonWidth, buttonHeight);
                    if (acceptButtonRect.Contains(e.Location)) AcceptClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                    else if (deleteButtonRect.Contains(e.Location)) DeleteClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                    else if (ignoreButtonRect.Contains(e.Location)) IgnoreClicked?.Invoke(this, new ListViewItemEventArgs(hitTest.Item));
                }
            }
        }

        private void DrawManualButton(Graphics g, Rectangle bounds, string text, PushButtonState state)
        {
            if (DarkMode == null) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            bool isDark = DarkMode.IsDarkMode;
            Color defaultBackColor = isDark ? DarkMode.OScolors.SurfaceDark : ControlPaint.Dark(SystemColors.Window, 0.05f);
            Color textColor = isDark ? DarkMode.OScolors.TextActive : SystemColors.ControlText;
            Color currentBackColor = defaultBackColor;
            if (state == PushButtonState.Pressed)
            {
                currentBackColor = isDark ? ControlPaint.Light(defaultBackColor, 1.2f) : ControlPaint.Dark(defaultBackColor, 0.2f);
            }
            else if (state == PushButtonState.Hot)
            {
                currentBackColor = isDark ? ControlPaint.Light(defaultBackColor, 0.9f) : ControlPaint.Light(defaultBackColor, 0.2f);
            }

            using (var backBrush = new SolidBrush(currentBackColor))
            {
                g.FillRectangle(backBrush, bounds);
            }

            TextRenderer.DrawText(g, text, this.Font, bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));
    }
}