using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DarkModeForms
{
    public class FlatTabControl : TabControl
    {
        [Description("Color for a decorative line"), Category("Appearance")]
        public Color LineColor { get; set; } = SystemColors.Highlight;

        [Description("Color for all Borders"), Category("Appearance")]
        public Color BorderColor { get; set; } = SystemColors.ControlDark;

        [Description("Back color for selected Tab"), Category("Appearance")]
        public Color SelectTabColor { get; set; } = SystemColors.ControlLight;

        [Description("Fore Color for Selected Tab"), Category("Appearance")]
        public Color SelectedForeColor { get; set; } = SystemColors.HighlightText;

        [Description("Back Color for un-selected tabs"), Category("Appearance")]
        public Color TabColor { get; set; } = SystemColors.ControlLight;

        [Description("Background color for the whole control"), Category("Appearance"), Browsable(true)]
        public override Color BackColor { get; set; } = SystemColors.Control;

        [Description("Fore Color for all Texts"), Category("Appearance")]
        public override Color ForeColor { get; set; } = SystemColors.ControlText;

        public FlatTabControl()
        {
            try
            {
                this.DrawMode = TabDrawMode.OwnerDrawFixed;
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawControl(e.Graphics);
        }

        internal void DrawControl(Graphics g)
        {
            try
            {
                if (!Visible)
                {
                    return;
                }

                using (Brush bBackColor = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(bBackColor, this.ClientRectangle);
                }

                for (int i = 0; i < this.TabCount; i++)
                {
                    DrawTab(g, this.TabPages[i], i);
                }
            }
            catch { }
        }

        internal void DrawTab(Graphics g, TabPage customTabPage, int nIndex)
        {
            Rectangle tabRect = this.GetTabRect(nIndex);
            bool isSelected = (this.SelectedIndex == nIndex);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (this.Alignment == TabAlignment.Left)
            {
                Color tabBackColor = isSelected ? SelectTabColor : this.TabColor;

                using (Brush b = new SolidBrush(tabBackColor))
                {
                    g.FillRectangle(b, tabRect);
                }
                using (Pen p = new Pen(this.BorderColor))
                {
                    g.DrawRectangle(p, tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height - 1);
                }

                if (this.ImageList != null && customTabPage.ImageIndex >= 0 && customTabPage.ImageIndex < this.ImageList.Images.Count)
                {
                    Image? icon = this.ImageList.Images[customTabPage.ImageIndex];
                    if (icon != null)
                    {
                        int iconHeight = this.ImageList.ImageSize.Height;
                        int iconWidth = this.ImageList.ImageSize.Width;
                        int iconX = tabRect.X + (tabRect.Width - iconWidth) / 2;
                        int iconY = tabRect.Y + (tabRect.Height - iconHeight) / 2;
                        g.DrawImage(icon, new Rectangle(iconX, iconY, iconWidth, iconHeight));
                    }
                }

                if (isSelected)
                {
                    using (Pen p = new Pen(this.LineColor, 2))
                    {
                        g.DrawLine(p, tabRect.Right - 1, tabRect.Top, tabRect.Right - 1, tabRect.Bottom - 1);
                    }
                }
            }
            else
            {
                Point[] points;
                points = new[]
                {
                    new Point(tabRect.Left, tabRect.Bottom),
                    new Point(tabRect.Left, tabRect.Top + 3),
                    new Point(tabRect.Left + 3, tabRect.Top),
                    new Point(tabRect.Right - 3, tabRect.Top),
                    new Point(tabRect.Right, tabRect.Top + 3),
                    new Point(tabRect.Right, tabRect.Bottom),
                    new Point(tabRect.Left, tabRect.Bottom)
                };

                using (Brush brush = new SolidBrush(isSelected ? SelectTabColor : this.TabColor))
                {
                    g.FillPolygon(brush, points);
                    using (Pen borderPen = new Pen(this.BorderColor))
                    {
                        g.DrawPolygon(borderPen, points);
                    }
                }

                if (isSelected)
                {
                    g.DrawLine(new Pen(SelectTabColor, 2), new Point(tabRect.Left, tabRect.Bottom), new Point(tabRect.Right, tabRect.Bottom));
                }

                TextRenderer.DrawText(g, customTabPage.Text, Font, tabRect, isSelected ? SelectedForeColor : ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
