using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DarkModeForms
{
    public class FlatProgressBar : ProgressBar
    {
        private readonly Timer marqueeTimer;
        private int marqueePosition = 0;
        private ProgressBarStyle style = ProgressBarStyle.Blocks;

        [DefaultValue(ProgressBarStyle.Blocks)]
        public new ProgressBarStyle Style
        {
            get { return style; }
            set
            {
                style = value;
                if (style == ProgressBarStyle.Marquee)
                {
                    marqueeTimer.Start();
                }
                else
                {
                    marqueeTimer.Stop();
                }
                Invalidate();
            }
        }

        public FlatProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            marqueeTimer = new Timer { Interval = 30 };
            marqueeTimer.Tick += MarqueeTimer_Tick;
        }

        private void MarqueeTimer_Tick(object? sender, EventArgs e)
        {
            marqueePosition += 5;
            if (marqueePosition > Width)
            {
                marqueePosition = -Width / 2;
            }
            Invalidate();
        }

        private int min = 0;
        private int max = 100;
        private int val = 0;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // Background
            using Brush backBrush = new SolidBrush(Theme.Colors.ControlLight);
            g.FillRectangle(backBrush, ClientRectangle);

            // Foreground Bar
            using (var brush = new SolidBrush(Theme.Colors.Primary)) // Centralized accent color
            {
                if (Style == ProgressBarStyle.Marquee)
                {
                    int marqueeWidth = Width / 3;
                    var marqueeRect = new Rectangle(marqueePosition, 0, marqueeWidth, Height);
                    if (marqueeRect.X < Width)
                    {
                        g.FillRectangle(brush, marqueeRect);
                    }
                }
                else
                {
                    float percent = Math.Clamp((float)(val - min) / (max - min), 0f, 1f);

                    Rectangle rect = ClientRectangle;
                    rect.Width = (int)((float)rect.Width * percent);
                    g.FillRectangle(brush, rect);
                }
            }

            // Border
            Draw3DBorder(g);
        }

        [DefaultValue(0)]
        public new int Minimum
        {
            get => min;
            set
            {
                min = value;
                max = Math.Max(max, min);
                val = Math.Max(val, min);

                base.Minimum = min;
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public new int Maximum
        {
            get => max;
            set
            {
                max = value;
                min = Math.Min(min, max);
                val = Math.Min(val, max);

                base.Maximum = max;
                Invalidate();
            }
        }

        [DefaultValue(0)]
        public new int Value
        {
            get => val;
            set
            {
                int oldValue = val;
                val = Math.Clamp(value, min, max);

                base.Value = val;
                if (val != oldValue)
                {
                    Invalidate();
                }
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int penWidth = (int)(1 * (g.DpiX / 96f));
            if (penWidth < 1)
            {
                penWidth = 1;
            }

            using var pen = new Pen(Theme.Colors.ControlDark, penWidth);
            g.DrawRectangle(pen, 0, 0, Width - penWidth, Height - penWidth);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                marqueeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
