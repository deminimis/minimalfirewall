using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkModeForms
{
    public partial class ControlStyler
    {
        [LibraryImport("uxtheme.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        private readonly OSThemeColors _colors;
        private readonly bool _isDarkMode;

        public ControlStyler(OSThemeColors colors, bool isDarkMode)
        {
            _colors = colors;
            _isDarkMode = isDarkMode;
        }

        public void ApplyStyle(Control control)
        {
            // Apply base control styling
            StyleBaseControl(control);


            // Note > Derived classes MUST come before their base classes
            switch (control)
            {
                // Labels
                case LinkLabel llbl: StyleLinkLabel(llbl); break;
                case Label lbl: StyleLabel(lbl); break;

                // Panels & Containers
                case TableLayoutPanel tlp: StyleTableLayoutPanel(tlp); break;
                case FlowLayoutPanel flp: StyleFlowLayoutPanel(flp); break;
                case TabPage tp: StyleTabPage(tp); break;
                case Panel pnl: StylePanel(pnl); break;

                // TextBoxes
                case RichTextBox rtb: StyleRichTextBox(rtb); break;
                case TextBox txt: StyleTextBox(txt); break;

                // Buttons
                case CheckBox cbx: StyleButtonBase(cbx); break;
                case RadioButton rb: StyleButtonBase(rb); break;
                case Button btn: StyleButton(btn); break;

                // ToolStrips
                case ToolStrip ts: StyleToolStrip(ts); break;
                case ToolStripPanel tsp: StyleToolStripPanel(tsp); break;

                // Everything Else
                case NumericUpDown nud: StyleNumericUpDown(nud); break;
                case ComboBox cb: StyleComboBox(cb); break;
                case GroupBox gb: StyleGroupBox(gb); break;
                case TabControl tc: StyleTabControl(tc); break;
                case PictureBox pb: StylePictureBox(pb); break;
                case MdiClient mdi: StyleMdiClient(mdi); break;
                case PropertyGrid pg: StylePropertyGrid(pg); break;
                case ListView lv: StyleListView(lv); break;
                case TreeView tv: StyleTreeView(tv); break;
                case DataGridView dgv: StyleDataGridView(dgv); break;
                case ProgressBar pbar: StyleProgressBar(pbar); break;
            }

            if (control.ContextMenuStrip != null)
            {
                ApplyStyle(control.ContextMenuStrip);
            }

            // Recursively apply style to all child controls
            foreach (Control childControl in control.Controls)
            {
                ApplyStyle(childControl);
            }
        }

        private void ApplyThemeToHandle(IntPtr handle, string themeClass)
        {
            string mode = _isDarkMode ? $"DarkMode_{themeClass}" : $"ClearMode_{themeClass}";
            SetWindowTheme(handle, mode, null);
        }

        private void StyleBaseControl(Control control)
        {
            ApplyThemeToHandle(control.Handle, "Explorer");
            control.BackColor = _colors.Control;
            control.ForeColor = _colors.TextActive;
        }

        private void StyleLabel(Label lbl)
        {
            if (lbl.Parent != null) lbl.BackColor = lbl.Parent.BackColor;
            lbl.GetType().GetProperty("BorderStyle")?.SetValue(lbl, BorderStyle.None);
        }

        private void StyleLinkLabel(LinkLabel llbl)
        {
            if (llbl.Parent != null) llbl.BackColor = llbl.Parent.BackColor;
            llbl.LinkColor = _colors.AccentLight;
            llbl.VisitedLinkColor = _colors.Primary;
        }

        private void StyleTextBox(TextBox txt)
        {
            BorderStyle bStyle = _isDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
            txt.GetType().GetProperty("BorderStyle")?.SetValue(txt, bStyle);
        }

        private void StyleNumericUpDown(NumericUpDown nud)
        {
            ApplyThemeToHandle(nud.Handle, "ItemsView");
        }

        private void StyleButton(Button btn)
        {
            btn.FlatStyle = _isDarkMode ? FlatStyle.Flat : FlatStyle.Standard;
            btn.FlatAppearance.CheckedBackColor = _colors.Accent;

            if (btn.BackColor != Color.Transparent)
            {
                btn.BackColor = _colors.Control;
            }

            btn.FlatAppearance.BorderColor = (btn.FindForm()?.AcceptButton == btn) ? _colors.Accent : _colors.Control;
            btn.FlatAppearance.MouseOverBackColor = _colors.ControlLight;
        }

        private void StyleComboBox(ComboBox cb)
        {
            if (cb.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                cb.SelectionStart = cb.Text.Length;
            }

            if (cb.IsHandleCreated)
            {
                cb.BeginInvoke(new Action(() =>
                {
                    if (!cb.DropDownStyle.Equals(ComboBoxStyle.DropDownList))
                    {
                        cb.SelectionLength = 0;
                    }
                }));
            }

            if (!cb.Enabled && _isDarkMode)
            {
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            ApplyThemeToHandle(cb.Handle, "CFD");
        }

        private void StyleTabPage(TabPage tp)
        {
            tp.BackColor = _colors.Surface;
        }

        private void StylePanel(Panel pnl)
        {
            if (pnl.Parent != null) pnl.BackColor = pnl.Parent.BackColor;
            pnl.BorderStyle = BorderStyle.FixedSingle;
        }

        private void StyleGroupBox(GroupBox gb)
        {
            if (gb.Parent != null) gb.BackColor = gb.Parent.BackColor;
            gb.ForeColor = _colors.TextActive;
            gb.FlatStyle = FlatStyle.Flat;
        }

        private void StyleTableLayoutPanel(TableLayoutPanel tlp)
        {
            if (tlp.Parent != null) tlp.BackColor = tlp.Parent.BackColor;
            tlp.ForeColor = _colors.TextInactive;
        }

        private void StyleTabControl(TabControl tc)
        {
            tc.Appearance = TabAppearance.Normal;
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.DrawItem -= Tab_DrawItem;
            tc.DrawItem += Tab_DrawItem;
        }

        private void StylePictureBox(PictureBox pb)
        {
            if (pb.Parent != null) pb.BackColor = pb.Parent.BackColor;
            if (_colors != null) pb.ForeColor = _colors.TextActive;
            pb.BorderStyle = BorderStyle.None;
        }

        private void StyleButtonBase(ButtonBase btnBase)
        {
            if (btnBase.Parent != null) btnBase.BackColor = btnBase.Parent.BackColor;
            btnBase.ForeColor = btnBase.Enabled ? _colors.TextActive : _colors.TextInactive;
            btnBase.FlatStyle = FlatStyle.Flat;
            btnBase.FlatAppearance.BorderColor = _colors.ControlDark;
            btnBase.FlatAppearance.CheckedBackColor = _colors.Accent;
        }

        private void StyleToolStrip(ToolStrip ts)
        {
            ts.RenderMode = ToolStripRenderMode.Professional;
            ts.Renderer = new MyRenderer(new CustomColorTable(_colors), true) { MyColors = _colors };

            if (ts is ToolStripDropDown dropDown)
            {
                dropDown.Opening -= Tsdd_Opening;
                dropDown.Opening += Tsdd_Opening;
            }
        }

        private void StyleToolStripPanel(ToolStripPanel tsp)
        {
            if (tsp.Parent != null) tsp.BackColor = tsp.Parent.BackColor;
        }

        private void StyleMdiClient(MdiClient mdi)
        {
            mdi.BackColor = _colors.Surface;
        }

        private void StylePropertyGrid(PropertyGrid pg)
        {
            pg.BackColor = _colors.Control;
            pg.ViewBackColor = _colors.Control;
            pg.LineColor = _colors.Surface;
            pg.ViewForeColor = _colors.TextActive;
            pg.ViewBorderColor = _colors.ControlDark;
            pg.CategoryForeColor = _colors.TextActive;
            pg.CategorySplitterColor = _colors.ControlLight;
        }

        private void StyleListView(ListView lv)
        {
            lv.OwnerDraw = true;
            lv.DrawColumnHeader -= ListView_DrawColumnHeader;
            lv.DrawColumnHeader += ListView_DrawColumnHeader;

            if (!lv.OwnerDraw)
            {
                ApplyThemeToHandle(lv.Handle, "Explorer");
            }
        }

        private void StyleTreeView(TreeView tv)
        {
            tv.GetType().GetProperty("BorderStyle")?.SetValue(tv, BorderStyle.None);
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.BackgroundColor = _colors.Control;
            dgv.GridColor = _colors.ControlDark;

            dgv.DefaultCellStyle.BackColor = _colors.Surface;
            dgv.DefaultCellStyle.ForeColor = _colors.TextActive;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = _colors.Surface;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = _colors.TextActive;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = _colors.Surface;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.RowHeadersDefaultCellStyle.BackColor = _colors.Surface;
            dgv.RowHeadersDefaultCellStyle.ForeColor = _colors.TextActive;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = _colors.Surface;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        }

        private void StyleRichTextBox(RichTextBox rtb)
        {
            if (rtb.Parent != null) rtb.BackColor = rtb.Parent.BackColor;
            rtb.BorderStyle = BorderStyle.None;
        }

        private void StyleFlowLayoutPanel(FlowLayoutPanel flp)
        {
            if (flp.Parent != null) flp.BackColor = flp.Parent.BackColor;
        }

        private void StyleProgressBar(ProgressBar pbar)
        {
            ApplyThemeToHandle(pbar.Handle, "Explorer");
        }

        // --- Custom Drawing Methods ---

        private void Tab_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tab || tab.Parent == null) return;

            using var headerBrush = new SolidBrush(tab.Parent.BackColor);
            e.Graphics.FillRectangle(headerBrush, new Rectangle(0, 0, tab.Width, tab.Height));

            for (int i = 0; i < tab.TabPages.Count; i++)
            {
                TabPage tabPage = tab.TabPages[i];
                if (tabPage.Tag == null)
                {
                    tabPage.BorderStyle = BorderStyle.FixedSingle;
                    tabPage.Tag = "themed";
                }
                Rectangle tabRect = tab.GetTabRect(i);
                bool isSelected = tab.SelectedIndex == i;
                if (isSelected)
                {
                    using var tabBackColor = new SolidBrush(_colors.Surface);
                    e.Graphics.FillRectangle(tabBackColor, tabRect);
                }

                Image? icon = null;
                if (tab.ImageList != null && tabPage.ImageIndex >= 0 && tabPage.ImageIndex < tab.ImageList.Images.Count)
                {
                    icon = tab.ImageList.Images[tabPage.ImageIndex];
                }

                Rectangle textBounds;
                TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                Color textColor = isSelected ? _colors.TextActive : _colors.TextInactive;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (tab.Alignment == TabAlignment.Left || tab.Alignment == TabAlignment.Right)
                {
                    if (icon != null)
                    {
                        int iconWidth = tab.ImageList.ImageSize.Width;
                        int iconX = tabRect.X + (tabRect.Width - iconWidth) / 2;
                        int iconY = tabRect.Y + 15;
                        Image imageToDraw = icon;

                        bool shouldDispose = false;
                        if (_isDarkMode && tabPage.ImageKey != "locked.png")
                        {
                            imageToDraw = DarkModeCS.RecolorImage(icon, Color.White);
                            shouldDispose = true;
                        }
                        int iconHeight = tab.ImageList.ImageSize.Height;
                        e.Graphics.DrawImage(imageToDraw, new Rectangle(iconX, iconY, iconWidth, iconHeight));
                        if (shouldDispose)
                        {
                            imageToDraw.Dispose();
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
        }

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (sender is not ListView) return;

            if (_isDarkMode)
            {
                using (var backBrush = new SolidBrush(_colors.Surface))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }
                if (e.Header != null)
                {
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, _colors.TextActive, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void Tsdd_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is ToolStripDropDown tsdd)
            {
                foreach (ToolStripMenuItem toolStripMenuItem in tsdd.Items.OfType<ToolStripMenuItem>())
                {
                    toolStripMenuItem.DropDownOpening -= Tsmi_DropDownOpening;
                    toolStripMenuItem.DropDownOpening += Tsmi_DropDownOpening;
                }
            }
        }

        private void Tsmi_DropDownOpening(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                if (tsmi.DropDown != null && tsmi.DropDown.Items.Count > 0)
                {
                    ApplyStyle(tsmi.DropDown);
                }
                tsmi.DropDownOpening -= Tsmi_DropDownOpening;
            }
        }
    }
}
