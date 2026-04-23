using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DarkModeForms
{
    public class FlatProgressBar : ProgressBar
    {
        private Timer marqueeTimer;
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
                this.Invalidate();
            }
        }

        public FlatProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            marqueeTimer = new Timer();
            marqueeTimer.Interval = 30;
            marqueeTimer.Tick += MarqueeTimer_Tick;
        }

        private void MarqueeTimer_Tick(object? sender, EventArgs e)
        {
            marqueePosition += 5;
            if (marqueePosition > this.Width)
            {
                marqueePosition = -this.Width / 2;
            }
            this.Invalidate();
        }

        private int min = 0;
        private int max = 100;
        private int val = 0;
        [DefaultValue(typeof(Color), "Green")]
        public Color BarColor { get; set; } = Color.Green;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            // Background
            using Brush backBrush = new SolidBrush(this.BackColor);
            g.FillRectangle(backBrush, this.ClientRectangle);

            // Foreground Bar
            using (SolidBrush brush = new SolidBrush(BarColor))
            {
                if (Style == ProgressBarStyle.Marquee)
                {
                    int marqueeWidth = this.Width / 3;
                    Rectangle marqueeRect = new Rectangle(marqueePosition, 0, marqueeWidth, this.Height);
                    if (marqueeRect.X < this.Width)
                        g.FillRectangle(brush, marqueeRect);
                }
                else
                {
                    float percent = Math.Clamp((float)(val - min) / (max - min), 0f, 1f);

                    Rectangle rect = this.ClientRectangle;
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
                if (val != oldValue) Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int penWidth = UIHelpers.Scale(1, g);
            if (penWidth < 1) penWidth = 1;

            using (Pen pen = new Pen(Color.DarkGray, penWidth))
            {
                g.DrawRectangle(pen, 0, 0, this.Width - penWidth, this.Height - penWidth);
            }
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