using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DarkModeForms
{
    public class FlatProgressBar : ProgressBar
    {
        private Timer marqueeTimer;
        private int marqueePosition = 0;
        private ProgressBarStyle style = ProgressBarStyle.Blocks;

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
            using (Brush backBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(backBrush, this.ClientRectangle);
            }

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
                    float percent = (float)(val - min) / (float)(max - min);
                    if (percent > 1) percent = 1;
                    if (percent < 0) percent = 0;

                    Rectangle rect = this.ClientRectangle;
                    rect.Width = (int)((float)rect.Width * percent);
                    g.FillRectangle(brush, rect);
                }
            }

            // Border
            Draw3DBorder(g);
        }

        public new int Minimum
        {
            get => min;
            set
            {
                min = value;
                if (min > max) max = min;
                if (val < min) val = min;

                base.Minimum = min;
                Invalidate();
            }
        }

        public new int Maximum
        {
            get => max;
            set
            {
                max = value;
                if (max < min) min = max;
                if (val > max) val = max;

                base.Maximum = max;
                Invalidate();
            }
        }

        public new int Value
        {
            get => val;
            set
            {
                int oldValue = val;
                val = value;
                if (val < min) val = min;
                if (val > max) val = max;

                base.Value = val;
                if (val != oldValue) Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int penWidth = (int)(1 * (g.DpiX / 96f));
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