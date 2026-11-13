using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class ThemedTabControl : TabControl
    {
        private readonly System.Collections.Generic.Dictionary<string, Image> _darkIconCache = new();

        public ThemedTabControl()
        {
            Appearance = TabAppearance.Normal;
            DrawMode = TabDrawMode.OwnerDrawFixed;

            // Protected property accessed directly
            DoubleBuffered = true;
        }

        #region Flat sidebar (left-aligned tabs)
        // Left-aligned tabs are painted as a flat sidebar: no native 3D borders, a small icon and a
        // label per row, and an accent bar on the selected row. Top-aligned tabs keep OnDrawItem.
        private const int TcmAdjustRect = 0x1328;
        private int _hotIndex = -1;

        private bool IsSidebar => Alignment == TabAlignment.Left;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsSidebar)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct NativeRect { public int Left, Top, Right, Bottom; }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Let the pages fill everything right of the strip, removing the native border and margins.
            if (m.Msg == TcmAdjustRect && IsSidebar && m.WParam == IntPtr.Zero && !DesignMode)
            {
                var rect = System.Runtime.InteropServices.Marshal.PtrToStructure<NativeRect>(m.LParam);
                rect.Left = ItemSize.Height;
                rect.Top = 0;
                rect.Right = Width;
                rect.Bottom = Height;
                System.Runtime.InteropServices.Marshal.StructureToPtr(rect, m.LParam, false);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!IsSidebar)
            {
                base.OnPaint(e);
                return;
            }

            var colors = Theme.Colors;
            using (var strip = new SolidBrush(Parent?.BackColor ?? colors.Background))
            {
                e.Graphics.FillRectangle(strip, ClientRectangle);
            }

            for (int i = 0; i < TabPages.Count; i++)
            {
                DrawSidebarItem(e.Graphics, i);
            }
        }

        private void DrawSidebarItem(Graphics g, int index)
        {
            var colors = Theme.Colors;
            TabPage page = TabPages[index];
            Rectangle rect = GetTabRect(index);
            rect.Width = ItemSize.Height;
            bool selected = SelectedIndex == index;
            float dpi = g.DpiX / 96f;

            if (selected || index == _hotIndex)
            {
                using var back = new SolidBrush(selected ? colors.Surface : colors.SurfaceLight);
                g.FillRectangle(back, rect);
            }

            if (selected)
            {
                using var accent = new SolidBrush(colors.Accent);
                g.FillRectangle(accent, new Rectangle(rect.X, rect.Y + (int)(8 * dpi), (int)Math.Max(3, 3 * dpi), rect.Height - (int)(16 * dpi)));
            }

            int iconSize = (int)Math.Round(20 * dpi);
            int left = rect.X + (int)Math.Round(16 * dpi);
            if (ImageList != null && page.ImageIndex >= 0 && page.ImageIndex < ImageList.Images.Count)
            {
                Image icon = GetThemedIcon(page, ImageList.Images[page.ImageIndex]);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(icon, new Rectangle(left, rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize));
            }

            int textLeft = left + iconSize + (int)Math.Round(12 * dpi);
            var textRect = new Rectangle(textLeft, rect.Y, rect.Right - textLeft - (int)(8 * dpi), rect.Height);
            TextRenderer.DrawText(g, page.Text, Font, textRect, selected ? colors.TextActive : colors.TextInactive,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            if (selected && Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(rect, -3, -3), colors.TextActive, colors.Surface);
            }
        }

        private Image GetThemedIcon(TabPage page, Image icon)
        {
            bool skipRecolor = page.ImageKey == "locked.png" || page.Tag?.ToString() == "NoRecolor";
            if (Theme.Colors.Surface.R >= 128 || skipRecolor)
            {
                return icon;
            }

            string cacheKey = (string.IsNullOrEmpty(page.ImageKey) ? page.ImageIndex.ToString() : page.ImageKey) + "_dark";
            if (!_darkIconCache.TryGetValue(cacheKey, out Image? cached))
            {
                cached = RecolorImage(icon, Color.White);
                _darkIconCache[cacheKey] = cached;
            }
            return cached;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsSidebar)
            {
                return;
            }

            int hot = -1;
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (GetTabRect(i).Contains(e.Location))
                {
                    hot = i;
                    break;
                }
            }
            if (hot != _hotIndex)
            {
                _hotIndex = hot;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hotIndex != -1)
            {
                _hotIndex = -1;
                Invalidate();
            }
        }
        #endregion

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Parent == null || e.Index < 0) return;

            TabPage tabPage = TabPages[e.Index];
            if (tabPage.Tag == null)
            {
                tabPage.BorderStyle = BorderStyle.FixedSingle;
                tabPage.Tag = "themed";
            }

            Rectangle tabRect = GetTabRect(e.Index);
            bool isSelected = SelectedIndex == e.Index;

            // Reference the global theme palette
            var colors = Theme.Colors;
            // Check the active palette rather than the system setting
            bool isDarkMode = colors.Surface.R < 128;

            // Draw the background for this specific tab ONLY
            using (var tabBackColor = new SolidBrush(isSelected ? colors.Surface : (Parent?.BackColor ?? colors.Background)))
            {
                // Inflate to prevent default gray borders from bleeding through
                Rectangle fillRect = tabRect;
                fillRect.Inflate(1, 1);
                e.Graphics.FillRectangle(tabBackColor, fillRect);
            }

            // Paint the empty void ONLY after the very last tab.
            if (e.Index == TabPages.Count - 1)
            {
                using var bgBrush = new SolidBrush(Parent?.BackColor ?? colors.Background);
                if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
                {
                    var emptySpace = new Rectangle(0, tabRect.Bottom, tabRect.Right + 4, Height - tabRect.Bottom);
                    e.Graphics.FillRectangle(bgBrush, emptySpace);
                }
                else
                {
                    var emptySpace = new Rectangle(tabRect.Right, 0, Width - tabRect.Right, tabRect.Bottom + 4);
                    e.Graphics.FillRectangle(bgBrush, emptySpace);
                }
            }

            // Setup Icon & Text
            Image? icon = null;
            if (ImageList != null && tabPage.ImageIndex >= 0 && tabPage.ImageIndex < ImageList.Images.Count)
            {
                icon = ImageList.Images[tabPage.ImageIndex];
            }

            Rectangle textBounds;
            TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            Color textColor = isSelected ? colors.TextActive : colors.TextInactive;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // 4. Draw Icon & Text based on Alignment
            if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
            {
                if (icon != null && ImageList != null)
                {
                    float dpiScale = e.Graphics.DpiY / 96f;
                    int iconWidth = ImageList.ImageSize.Width;
                    int iconX = tabRect.X + (tabRect.Width - iconWidth) / 2;
                    Image imageToDraw = icon;

                    // Skip recoloring for items explicitly tagged or specific icons
                    bool skipRecolor = tabPage.ImageKey == "locked.png" ||
                        (tabPage.Tag?.ToString() == "NoRecolor");

                    if (isDarkMode && !skipRecolor)
                    {
                        // Safely create a unique cache key based on Index or Key
                        string cacheKey = (string.IsNullOrEmpty(tabPage.ImageKey) ? tabPage.ImageIndex.ToString() : tabPage.ImageKey) + "_dark";

                        if (!_darkIconCache.TryGetValue(cacheKey, out Image? cachedImage))
                        {
                            cachedImage = RecolorImage(icon, Color.White);
                            _darkIconCache[cacheKey] = cachedImage;
                        }
                        imageToDraw = cachedImage ?? icon;
                    }

                    int iconHeight = ImageList.ImageSize.Height;

                    // Vertically center icon and text as a group within the tab
                    int gap = (int)Math.Round(4 * dpiScale);
                    Size textSize = TextRenderer.MeasureText(
                        e.Graphics, tabPage.Text, tabPage.Font,
                        new Size(tabRect.Width, int.MaxValue),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
                    int totalContentHeight = iconHeight + gap + textSize.Height;
                    int iconY = tabRect.Y + Math.Max(0, (tabRect.Height - totalContentHeight) / 2);

                    if (imageToDraw != null)
                    {
                        e.Graphics.DrawImage(imageToDraw, new Rectangle(iconX, iconY, iconWidth, iconHeight));
                    }

                    textBounds = new Rectangle(
                        tabRect.X, iconY + iconHeight + gap, tabRect.Width,
                        tabRect.Bottom - (iconY + iconHeight + gap));
                    textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
                }
                else
                {
                    textBounds = tabRect;
                }
            }
            else
            {
                textBounds = tabRect;
            }

            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, textBounds, textColor, textFlags);
        }

        private static Bitmap RecolorImage(Image sourceImage, Color newColor)
        {
            var newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                float r = newColor.R / 255f;
                float cg = newColor.G / 255f;
                float b = newColor.B / 255f;
                var colorMatrix = new System.Drawing.Imaging.ColorMatrix(
                [
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 1, 0],
                    [r, cg, b, 0, 1]
                ]);
                using var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
                g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                            0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }
    }
}
