// FlatProgressBar.cs
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
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            marqueeTimer = new Timer();
            marqueeTimer.Interval = 30;
            marqueeTimer.Tick += MarqueeTimer_Tick;
        }

        private void MarqueeTimer_Tick(object? sender, EventArgs e)
        {
            marqueePosition += 5;
            if (marqueePosition > this.Width)
            {
                marqueePosition = -100;
            }
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        int min = 0;
        int max = 100;
        int val = 0;
        Color BarColor = Color.Green;
        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush brush = new SolidBrush(BarColor))
            using (Brush BackBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(BackBrush, this.ClientRectangle);
                if (Style == ProgressBarStyle.Marquee)
                {
                    int marqueeWidth = this.Width / 4;
                    Rectangle marqueeRect = new Rectangle(marqueePosition, 0, marqueeWidth, this.Height);
                    g.FillRectangle(brush, marqueeRect);
                }
                else
                {
                    float percent = (float)(val - min) / (float)(max - min);
                    Rectangle rect = this.ClientRectangle;
                    rect.Width = (int)((float)rect.Width * percent);
                    g.FillRectangle(brush, rect);
                }

                Draw3DBorder(g);
            }
        }

        public new int Minimum
        {
            get
            {
                return min;
            }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value > max)
                {
                    max = value;
                }

                min = value;
                if (val < min)
                {
                    val = min;
                }

                this.Invalidate();
            }
        }

        public new int Maximum
        {
            get
            {
                return max;
            }

            set
            {
                if (value < min)
                {
                    min = value;
                }

                max = value;
                if (val > max)
                {
                    val = max;
                }

                this.Invalidate();
            }
        }

        public new int Value
        {
            get
            {
                return val;
            }

            set
            {
                int oldValue = val;
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    val = max;
                }
                else
                {
                    val = value;
                }

                float percent;
                Rectangle newValueRect = this.ClientRectangle;
                Rectangle oldValueRect = this.ClientRectangle;

                percent = (float)(val - min) / (float)(max - min);
                newValueRect.Width = (int)((float)newValueRect.Width * percent);

                percent = (float)(oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)((float)oldValueRect.Width * percent);
                Rectangle updateRect = new Rectangle();

                if (newValueRect.Width > oldValueRect.Width)
                {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                }
                else
                {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }

                updateRect.Height = this.Height;
                this.Invalidate(updateRect);
            }
        }

        public Color ProgressBarColor
        {
            get
            {
                return BarColor;
            }

            set
            {
                BarColor = value;
                this.Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g)
        {
            int PenWidth = (int)Pens.White.Width;
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.DarkGray,
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
                new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
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