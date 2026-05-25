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
            bool isDarkMode = Theme.IsSystemDarkMode();

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
                    int iconWidth = ImageList.ImageSize.Width;
                    int iconX = tabRect.X + (tabRect.Width - iconWidth) / 2;
                    int iconY = tabRect.Y + 15;
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
                    if (imageToDraw != null)
                    {
                        e.Graphics.DrawImage(imageToDraw, new Rectangle(iconX, iconY, iconWidth, iconHeight));
                    }

                    textBounds = new Rectangle(tabRect.X, iconY + iconHeight, tabRect.Width, tabRect.Height - iconHeight - 20);
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
