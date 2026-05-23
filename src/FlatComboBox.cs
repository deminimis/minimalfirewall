using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class FlatComboBox : ComboBox
    {
        

        public FlatComboBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xF && DropDownStyle != ComboBoxStyle.Simple)
            {
                base.WndProc(ref m);
                using var g = Graphics.FromHwnd(Handle);
                UIHelpers.SetHighQualityGraphics(g);

                var clientRect = ClientRectangle;
                    var dropDownButtonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
                    if (dropDownButtonWidth < UIHelpers.Scale(12, g))
                {
                    dropDownButtonWidth = UIHelpers.Scale(16, g);
                }

                var dropDownRect = new Rectangle(clientRect.Width - dropDownButtonWidth, 0, dropDownButtonWidth, clientRect.Height);
                    var textPadding = UIHelpers.Scale(4, g);
                    var textBackRect = new Rectangle(clientRect.Left, clientRect.Top, Math.Max(0, clientRect.Width - dropDownButtonWidth), clientRect.Height);
                    var textRect = new Rectangle(clientRect.Left + textPadding, clientRect.Top, Math.Max(0, clientRect.Width - dropDownButtonWidth - (textPadding * 2)), clientRect.Height);

                    #region Selected Text
                    using (var b = new SolidBrush(Theme.Colors.Control))
                    {
                        g.FillRectangle(b, textBackRect);
                    }

                    TextRenderer.DrawText(g, Text, Font, textRect, Theme.Colors.TextActive, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                    #endregion

                    #region DropDown Button
                    using (var b = new SolidBrush(Enabled ? Theme.Colors.Surface : Theme.Colors.ControlLight))
                    {
                        g.FillRectangle(b, dropDownRect);
                    }
                    #endregion

                    #region Chevron
                    var middle = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);
                    var cSize = new Size(UIHelpers.Scale(8, g), UIHelpers.Scale(4, g));
                Point[] chevron =
                [
                    new(middle.X - (cSize.Width / 2), middle.Y - (cSize.Height / 2)),
                        new(middle.X + (cSize.Width / 2), middle.Y - (cSize.Height / 2)),
                        new(middle.X, middle.Y + (cSize.Height / 2))
                ];
                using (var chevronPen = new Pen(Theme.Colors.TextInactive, UIHelpers.Scale(2, g)))
                    {
                        g.DrawLine(chevronPen, chevron[0], chevron[2]);
                        g.DrawLine(chevronPen, chevron[1], chevron[2]);
                    }
                #endregion

                #region Borders
                using var p = new Pen(Enabled ? Theme.Colors.ControlDark : Theme.Colors.ControlLight, UIHelpers.Scale(1, g));
                var borderRect = new Rectangle(0, 0, clientRect.Width - 1, clientRect.Height - 1);
                g.DrawRectangle(p, borderRect);

                g.DrawLine(p, dropDownRect.Left, dropDownRect.Top, dropDownRect.Left, dropDownRect.Bottom);
                #endregion
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }
    }

    internal static class UIHelpers
    {
        public static int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));

        public static void SetHighQualityGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }
    }
}
