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
        private Color borderColor = Color.Gray;
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    Invalidate();
                }
            }
        }

        private Color buttonColor = Color.LightGray;
        [DefaultValue(typeof(Color), "LightGray")]
        public Color ButtonColor
        {
            get { return buttonColor; }
            set
            {
                if (buttonColor != value)
                {
                    buttonColor = value;
                    Invalidate();
                }
            }
        }

        public FlatComboBox()
        {
            // double buffering to reduce flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        private int Scale(int value, Graphics g) => (int)(value * (g.DpiX / 96f));

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xF && DropDownStyle != ComboBoxStyle.Simple)
            {
                base.WndProc(ref m);

                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    var clientRect = ClientRectangle;
                    var dropDownButtonWidth = SystemInformation.HorizontalScrollBarArrowWidth;

                    if (dropDownButtonWidth < Scale(12, g)) dropDownButtonWidth = Scale(16, g);

                    var dropDownRect = new Rectangle(clientRect.Width - dropDownButtonWidth, 0, dropDownButtonWidth, clientRect.Height);

                    using (var backBrush = new SolidBrush(this.Enabled ? this.BackColor : SystemColors.Control))
                    {
                    }

                    #region DropDown Button
                    using (var b = new SolidBrush(Enabled ? ButtonColor : SystemColors.Control))
                    {
                        g.FillRectangle(b, dropDownRect);
                    }
                    #endregion

                    #region Chevron
                    Point middle = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);
                    Size cSize = new Size(Scale(8, g), Scale(4, g));
                    var chevron = new Point[]
                    {
                        new Point(middle.X - (cSize.Width / 2), middle.Y - (cSize.Height / 2)),
                        new Point(middle.X + (cSize.Width / 2), middle.Y - (cSize.Height / 2)),
                        new Point(middle.X, middle.Y + (cSize.Height / 2))
                    };
                    using (var chevronPen = new Pen(BorderColor, Scale(2, g)))
                    {
                        g.DrawLine(chevronPen, chevron[0], chevron[2]);
                        g.DrawLine(chevronPen, chevron[1], chevron[2]);
                    }
                    #endregion

                    #region Borders
                    using (var p = new Pen(Enabled ? BorderColor : SystemColors.ControlDark, Scale(1, g)))
                    {
                        Rectangle borderRect = new Rectangle(0, 0, clientRect.Width - 1, clientRect.Height - 1);
                        g.DrawRectangle(p, borderRect);

                        // Divider line for button
                        g.DrawLine(p, dropDownRect.Left, dropDownRect.Top, dropDownRect.Left, dropDownRect.Bottom);
                    }
                    #endregion
                }
                return; 
            }

            base.WndProc(ref m);
        }
    }
}