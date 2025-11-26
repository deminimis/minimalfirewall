using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

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
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SizeMode = TabSizeMode.Fixed;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // High DPI Scaling Logic
            float scale = this.DeviceDpi / 96f;
            int baseWidth = 70;
            int baseHeight = 120;
            Size scaledSize = new Size((int)(baseWidth * scale), (int)(baseHeight * scale));

            if (this.ItemSize.Width < scaledSize.Width)
            {
                this.ItemSize = scaledSize;
            }
        }

        private int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawControl(e.Graphics);
        }

        internal void DrawControl(Graphics g)
        {
            if (!Visible) return;

            using (Brush bBackColor = new SolidBrush(this.BackColor))
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

            bool isDarkModeText = (SelectedForeColor.R + SelectedForeColor.G + SelectedForeColor.B) > 382;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (this.Alignment == TabAlignment.Left)
            {

                Color tabBackColor = isSelected ? SelectTabColor : this.TabColor;
                using (Brush b = new SolidBrush(tabBackColor))
                {
                    g.FillRectangle(b, tabRect);
                }


                using (Pen p = new Pen(this.BorderColor))
                {
                    int offset = Scale(1, g);
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
                        int iconY = tabRect.Y + Scale(15, g);


                        if (isDarkModeText && customTabPage.ImageKey != "locked.png")
                        {
                            using (Image whiteIcon = RecolorImage(originalIcon, SelectedForeColor))
                            {
                                g.DrawImage(whiteIcon, new Rectangle(iconX, iconY, iconW, iconH));
                            }
                        }
                        else
                        {
                            g.DrawImage(originalIcon, new Rectangle(iconX, iconY, iconW, iconH));
                        }

                        int textPadding = Scale(5, g);
                        int textTop = iconY + iconH + textPadding;
                        textRect = new Rectangle(
                            tabRect.X,
                            textTop,
                            tabRect.Width,
                            tabRect.Bottom - textTop - Scale(5, g)
                        );

                        textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
                    }
                }

                if (isSelected)
                {
                    using (Pen p = new Pen(this.LineColor, Scale(3, g)))
                    {
                        g.DrawLine(p, tabRect.Right - Scale(1, g), tabRect.Top, tabRect.Right - Scale(1, g), tabRect.Bottom - Scale(1, g));
                    }
                }

                Color textColor = isSelected ? SelectedForeColor : ForeColor;
                TextRenderer.DrawText(g, customTabPage.Text, Font, textRect, textColor, textFlags);
            }
            else
            {
                // Fallback for Top/Bottom tabs 
                int scaled3 = Scale(3, g);
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
                    g.DrawLine(new Pen(SelectTabColor, Scale(2, g)), new Point(tabRect.Left, tabRect.Bottom), new Point(tabRect.Right, tabRect.Bottom));
                }

                TextRenderer.DrawText(g, customTabPage.Text, Font, tabRect, isSelected ? SelectedForeColor : ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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

                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {r, g_, b, 0, 1}
                });

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                        0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }
    }
}