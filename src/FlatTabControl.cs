using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class FlatTabControl : TabControl
    {
        private Dictionary<string, Image> _iconCache = new Dictionary<string, Image>();

        public FlatTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SizeMode = TabSizeMode.Fixed;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            float scaleFactor = this.DeviceDpi / 96f;
            int baseWidth = 70;
            int baseHeight = 120;
            var scaledSize = new Size((int)(baseWidth * scaleFactor), (int)(baseHeight * scaleFactor));
            if (this.ItemSize.Width < scaledSize.Width)
            {
                this.ItemSize = scaledSize;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawControl(e.Graphics);
        }

        internal void DrawControl(Graphics g)
        {
            if (!Visible) return;
            using (Brush bBackColor = new SolidBrush(Theme.Colors.Background))
            {
                g.FillRectangle(bBackColor, this.ClientRectangle);
            }

            for (int i = 0; i < this.TabCount; i++)
            {
                DrawTab(g, this.TabPages[i], i);
            }
        }

        internal void DrawTab(Graphics g, TabPage customTabPage, int nIndex)
        {
            Rectangle tabRect = this.GetTabRect(nIndex);
            bool isSelected = (this.SelectedIndex == nIndex);

            Color tabBackColor = isSelected ? Theme.Colors.Surface : Theme.Colors.SurfaceDark;
            Color textColor = isSelected ? Theme.Colors.TextActive : Theme.Colors.TextInactive;
            bool isDarkModeText = (0.299 * textColor.R + 0.587 * textColor.G + 0.114 * textColor.B) > 128;

            UIHelpers.SetHighQualityGraphics(g);

            if (this.Alignment == TabAlignment.Left)
            {
                using (Brush b = new SolidBrush(tabBackColor))
                {
                    g.FillRectangle(b, tabRect);
                }

                using (var p = new Pen(Theme.Colors.ControlDark))
                {
                    int offset = UIHelpers.Scale(1, g);
                    g.DrawRectangle(p, tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height - offset);
                }

                Rectangle textRect = tabRect;
                TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                if (this.ImageList != null && customTabPage.ImageIndex >= 0 && customTabPage.ImageIndex < this.ImageList.Images.Count)
                {
                    Image originalIcon = this.ImageList.Images[customTabPage.ImageIndex];
                    if (originalIcon != null)
                    {
                        int iconW = this.ImageList.ImageSize.Width;
                        int iconH = this.ImageList.ImageSize.Height;
                        int iconX = tabRect.X + (tabRect.Width - iconW) / 2;
                        int iconY = tabRect.Y + UIHelpers.Scale(15, g);

                        if (isDarkModeText && customTabPage.ImageKey != "locked.png")
                        {
                            string cacheKey = $"{customTabPage.ImageIndex}-{textColor.ToArgb()}";
                            if (!_iconCache.ContainsKey(cacheKey))
                            {
                                _iconCache[cacheKey] = RecolorImage(originalIcon, textColor);
                            }
                            g.DrawImage(_iconCache[cacheKey], new Rectangle(iconX, iconY, iconW, iconH));
                        }
                        else
                        {
                            g.DrawImage(originalIcon, new Rectangle(iconX, iconY, iconW, iconH));
                        }

                        int textPadding = UIHelpers.Scale(5, g);
                        int textTop = iconY + iconH + textPadding;
                        textRect = new Rectangle(
                            tabRect.X,
                            textTop,
                            tabRect.Width,
                            tabRect.Bottom - textTop - UIHelpers.Scale(5, g)
                        );
                        textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
                    }
                }

                if (isSelected)
                {
                    using (var p = new Pen(Theme.Colors.Accent, UIHelpers.Scale(3, g)))
                    {
                        g.DrawLine(p, tabRect.Right - UIHelpers.Scale(1, g), tabRect.Top, tabRect.Right - UIHelpers.Scale(1, g), tabRect.Bottom - UIHelpers.Scale(1, g));
                    }
                }

                TextRenderer.DrawText(g, customTabPage.Text, Font, textRect, textColor, textFlags);
            }
            else
            {
                int scaled3 = UIHelpers.Scale(3, g);
                Point[] points = new[]
                {
                    new Point(tabRect.Left, tabRect.Bottom),
                    new Point(tabRect.Left, tabRect.Top + scaled3),
                    new Point(tabRect.Left + scaled3, tabRect.Top),
                    new Point(tabRect.Right - scaled3, tabRect.Top),
                    new Point(tabRect.Right, tabRect.Top + scaled3),
                    new Point(tabRect.Right, tabRect.Bottom),
                    new Point(tabRect.Left, tabRect.Bottom)
                };

                using (Brush brush = new SolidBrush(tabBackColor))
                {
                    g.FillPolygon(brush, points);
                    using (var borderPen = new Pen(Theme.Colors.ControlDark))
                    {
                        g.DrawPolygon(borderPen, points);
                    }
                }

                if (isSelected)
                {
                    g.DrawLine(new Pen(Theme.Colors.Surface, UIHelpers.Scale(2, g)), new Point(tabRect.Left, tabRect.Bottom), new Point(tabRect.Right, tabRect.Bottom));
                }

                TextRenderer.DrawText(g, customTabPage.Text, Font, tabRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private Image RecolorImage(Image sourceImage, Color newColor)
        {
            var newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(newBitmap))
            {
                float r = newColor.R / 255f;
                float g_ = newColor.G / 255f;
                float b = newColor.B / 255f;
                var colorMatrix = new ColorMatrix(
                [
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 1, 0],
                    [r, g_, b, 0, 1]
                ]);
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                        0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var img in _iconCache.Values) img.Dispose();
                _iconCache.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
