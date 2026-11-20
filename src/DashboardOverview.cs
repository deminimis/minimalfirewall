// File: DashboardOverview.cs
using DarkModeForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MinimalFirewall
{
    // Summary strip above the pending list. It reads counts when painting and keeps only
    // 24 hourly counters and the last few decisions in memory, so it adds no background work.
    public sealed class DashboardOverview : Control
    {
        private const int Hours = 24;
        private const int MaxDecisions = 4;
        private const TextFormatFlags Flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

        private readonly MainViewModel _viewModel;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly int[] _alertsPerHour = new int[Hours];
        private readonly List<(DateTime Time, string Decision, string App)> _decisions = [];
        private DateTime _currentHour = StartOfHour(DateTime.Now);

        public DashboardOverview(MainViewModel viewModel, WildcardRuleService wildcardRuleService)
        {
            _viewModel = viewModel;
            _wildcardRuleService = wildcardRuleService;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Dock = DockStyle.Top;
            UpdateHeight();
        }

        public void AddAlerts(int count)
        {
            RollBuckets(_alertsPerHour, ref _currentHour, DateTime.Now);
            _alertsPerHour[Hours - 1] += count;
            Invalidate();
        }

        public void AddDecision(PendingConnectionViewModel pending, string decision)
        {
            _decisions.Insert(0, (DateTime.Now, decision == "TemporaryAllow" ? "Allow (temp)" : decision, $"{pending.FileName} ({pending.Direction})"));
            if (_decisions.Count > MaxDecisions)
            {
                _decisions.RemoveAt(MaxDecisions);
            }
            Invalidate();
        }

        // Shifts hourly counters so the last slot is the hour of "now"; slots older than 24 h drop out.
        internal static void RollBuckets(int[] buckets, ref DateTime currentHour, DateTime now)
        {
            DateTime hour = StartOfHour(now);
            int shift = (int)Math.Min(buckets.Length, Math.Max(0, (hour - currentHour).TotalHours));
            if (shift > 0)
            {
                Array.Copy(buckets, shift, buckets, 0, buckets.Length - shift);
                Array.Clear(buckets, buckets.Length - shift, shift);
            }
            currentHour = hour;
        }

        private static DateTime StartOfHour(DateTime time) => time.Date.AddHours(time.Hour);

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateHeight();
        }

        // Counters row, then the chart and the decisions side by side, five lines high.
        private void UpdateHeight() => Height = Font.Height * 8;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RollBuckets(_alertsPerHour, ref _currentHour, DateTime.Now);

            Graphics g = e.Graphics;
            int line = Font.Height;
            int pad = line / 2;
            int columnWidth = (ClientSize.Width - 3 * pad) / 2;
            if (columnWidth <= 0)
            {
                return;
            }

            Color dim = Theme.Colors.TextInactive;
            Color text = Theme.Colors.TextActive;

            int x = pad;
            foreach (var (label, value) in new[]
            {
                ("Pending", _viewModel.PendingConnections.Count),
                ("Alerts (24 h)", _alertsPerHour.Sum()),
                ("Audit changes", _viewModel.UnseenSystemChangesCount),
                ("Wildcard rules", _wildcardRuleService.GetRules().Count),
            })
            {
                x += DrawText(g, label, x, pad, dim) + pad / 2;
                x += DrawText(g, value.ToString(), x, pad, text) + line;
            }

            int top = pad + line + pad;
            DrawChart(g, new Rectangle(pad, top, columnWidth, line * 5), dim);
            DrawDecisions(g, new Rectangle(2 * pad + columnWidth, top, columnWidth, line * 5), dim, text);
        }

        private void DrawChart(Graphics g, Rectangle area, Color dim)
        {
            int line = Font.Height;
            int peak = _alertsPerHour.Max();
            DrawText(g, peak > 0 ? $"Alerts per hour, last 24 h (peak {peak})" : "Alerts per hour, last 24 h", area.X, area.Y, dim, area.Width);

            var bars = Rectangle.FromLTRB(area.Left, area.Top + line + line / 4, area.Right, area.Bottom - 1);
            float slot = bars.Width / (float)Hours;
            using (var brush = new SolidBrush(Theme.Colors.Accent))
            {
                for (int i = 0; i < Hours; i++)
                {
                    if (_alertsPerHour[i] == 0)
                    {
                        continue;
                    }
                    // At least 2 px, so a single alert stays visible next to a large peak.
                    float height = Math.Max(2, bars.Height * _alertsPerHour[i] / (float)peak);
                    g.FillRectangle(brush, bars.Left + i * slot + 1, bars.Bottom - height, Math.Max(1, slot - 2), height);
                }
            }
            using var pen = new Pen(dim);
            g.DrawLine(pen, bars.Left, bars.Bottom, bars.Right, bars.Bottom);
        }

        private void DrawDecisions(Graphics g, Rectangle area, Color dim, Color text)
        {
            int line = Font.Height;
            DrawText(g, "Recent decisions", area.X, area.Y, dim, area.Width);
            if (_decisions.Count == 0)
            {
                DrawText(g, "None since start", area.X, area.Y + line, dim, area.Width);
                return;
            }

            int y = area.Y + line;
            int marker = line / 2;
            // Proportional fonts do not align padded strings, so the decision gets a fixed column.
            int decisionWidth = Measure(g, "Allow (temp)");
            foreach (var (time, decision, app) in _decisions)
            {
                Color color = decision.StartsWith("Allow") ? Theme.Colors.Success : decision == "Block" ? Theme.Colors.Danger : Theme.Colors.Ignore;
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, area.X, y + (line - marker) / 2, marker, marker);
                }
                int x = area.X + marker * 2;
                x += DrawText(g, $"{time:HH:mm}", x, y, dim) + marker;
                DrawText(g, decision, x, y, text, decisionWidth);
                x += decisionWidth + marker;
                DrawText(g, app, x, y, text, area.Right - x);
                y += line;
            }
        }

        // Ellipsis applies only to drawing; measuring with it would truncate the measured text itself.
        private int DrawText(Graphics g, string value, int x, int y, Color color, int width = 0)
        {
            int measured = Measure(g, value);
            TextRenderer.DrawText(g, value, Font, new Rectangle(x, y, width > 0 ? width : measured, Font.Height), color, Flags);
            return measured;
        }

        private int Measure(Graphics g, string value) =>
            TextRenderer.MeasureText(g, value, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
    }
}
