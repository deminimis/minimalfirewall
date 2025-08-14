// DarkModeCS.cs
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class DarkModeCS
    {
        public struct DWMCOLORIZATIONcolors
        {
            public uint ColorizationColor,
              ColorizationAfterglow,
              ColorizationColorBalance,
              ColorizationAfterglowBalance,
              ColorizationBlurBalance,
              ColorizationGlassReflectionIntensity,
              ColorizationOpaqueBlend;
        }

        [Flags]
        public enum DWMWINDOWATTRIBUTE : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        }

        [DllImport("DwmApi")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);
        [DllImport("dwmapi.dll", EntryPoint = "#127")]
        public static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONcolors colors);
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
          int nLeftRect,
          int nTopRect,
          int nRightRect,
          int nBottomRect,
          int nWidthEllipse,
          int nHeightEllipse
        );
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        private const uint GW_CHILD = 5;

        private static readonly ControlStatusStorage controlStatusStorage = new();
        private static ControlEventHandler? ownerFormControlAdded;
        private static ControlEventHandler? controlControlAdded;
        private bool _IsDarkMode;

        public enum DisplayMode
        {
            SystemDefault,
            ClearMode,
            DarkMode
        }

        public DisplayMode ColorMode { get; set; } = DisplayMode.SystemDefault;
        public bool IsDarkMode => _IsDarkMode;
        public bool ColorizeIcons { get; set; } = true;
        public bool RoundedPanels { get; set; } = false;
        public Form OwnerForm { get; set; }
        public ComponentCollection? Components { get; set; }
        public OSThemeColors OScolors { get; set; }

        public DarkModeCS(Form _Form, bool _ColorizeIcons = true, bool _RoundedPanels = false)
        {
            OwnerForm = _Form;
            Components = null;
            ColorizeIcons = _ColorizeIcons;
            RoundedPanels = _RoundedPanels;

            OScolors = GetSystemColors(1);
            OwnerForm.HandleCreated += (sender, e) => ApplyTitleBarTheme();
            _Form.Load += (sender, e) =>
            {
                _IsDarkMode = isDarkMode();
                if (ColorMode != DisplayMode.SystemDefault)
                {
                    _IsDarkMode = ColorMode == DisplayMode.DarkMode;
                }

                ApplyTheme(_IsDarkMode);
            };
        }

        private void ApplyTitleBarTheme()
        {
            if (OwnerForm.Handle != IntPtr.Zero)
            {
                bool useDark = (ColorMode == DisplayMode.DarkMode) || (ColorMode == DisplayMode.SystemDefault && isDarkMode());
                int[] DarkModeOn = useDark ? [0x01] : [0x00];
                DwmSetWindowAttribute(OwnerForm.Handle, (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, DarkModeOn, 4);
            }
        }

        public bool isDarkMode()
        {
            return GetWindowsColorMode() <= 0;
        }

        public void ApplyTheme(bool pIsDarkMode = true)
        {
            try
            {
                _IsDarkMode = pIsDarkMode;
                OScolors = GetSystemColors(pIsDarkMode ? 0 : 1);

                ApplyTitleBarTheme();

                OwnerForm.BackColor = OScolors.Background;
                OwnerForm.ForeColor = OScolors.TextInactive;
                if (OwnerForm.Controls != null)
                {
                    foreach (Control _control in OwnerForm.Controls)
                    {
                        ThemeControl(_control);
                    }

                    ownerFormControlAdded ??= (sender, e) =>
                    {
                        if (e.Control != null)
                        {
                            ThemeControl(e.Control!);
                        }
                    };
                    OwnerForm.ControlAdded -= ownerFormControlAdded;
                    OwnerForm.ControlAdded += ownerFormControlAdded;
                }

                if (Components != null)
                {
                    foreach (var item in Components.OfType<ContextMenuStrip>())
                        ThemeControl(item);
                }
            }
            catch (Exception ex)
            {
                Messenger.MessageBox(ex, false);
            }
        }

        public void ApplyTheme(DisplayMode pColorMode)
        {
            if (ColorMode == pColorMode) return;
            ColorMode = pColorMode;
            _IsDarkMode = isDarkMode();
            if (ColorMode != DisplayMode.SystemDefault)
            {
                _IsDarkMode = ColorMode == DisplayMode.DarkMode;
            }

            ApplyTheme(_IsDarkMode);
        }

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (sender is not ListView listView) return;
            if (IsDarkMode)
            {
                using (var backBrush = new SolidBrush(OScolors.Surface))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }
                TextRenderer.DrawText(e.Graphics, e.Header!.Text, e.Font, e.Bounds, OScolors.TextActive, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        public void ThemeControl(Control control)
        {
            var info = controlStatusStorage.GetControlStatusInfo(control);
            if (info != null)
            {
                if (info.IsExcluded) return;
                if (info.LastThemeAppliedIsDark == IsDarkMode) return;
                info.LastThemeAppliedIsDark = IsDarkMode;
            }
            else
            {
                controlStatusStorage.RegisterProcessedControl(control, IsDarkMode);
            }

            BorderStyle BStyle = (IsDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D);
            controlControlAdded ??= (sender, e) =>
            {
                if (e.Control != null)
                {
                    ThemeControl(e.Control);
                }
            };
            control.ControlAdded -= controlControlAdded;
            control.ControlAdded += controlControlAdded;
            string Mode = IsDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";
            SetWindowTheme(control.Handle, Mode, null);

            control.GetType().GetProperty("BackColor")?.SetValue(control, OScolors.Control);
            control.GetType().GetProperty("ForeColor")?.SetValue(control, OScolors.TextActive);
            if (control is Label lbl && control.Parent != null)
            {
                control.BackColor = control.Parent.BackColor;
                control.GetType().GetProperty("BorderStyle")?.SetValue(control, BorderStyle.None);
                control.Paint += (sender, e) =>
                {
                    if (!control.Enabled && IsDarkMode && control.Parent != null)
                    {
                        e.Graphics.Clear(control.Parent.BackColor);
                        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                        using Brush B = new SolidBrush(control.ForeColor);
                        MethodInfo? mi = lbl.GetType().GetMethod("CreateStringFormat", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mi != null)
                        {
                            if (mi.Invoke(lbl, []) is StringFormat sf)
                            {
                                e.Graphics.DrawString(lbl.Text ?? "", lbl.Font, B, new PointF(1, 0), sf);
                            }
                        }
                    }
                };
            }
            else if (control is LinkLabel linkLabel && linkLabel.Parent != null)
            {
                linkLabel.BackColor = linkLabel.Parent.BackColor;
                linkLabel.LinkColor = OScolors.AccentLight;
                linkLabel.VisitedLinkColor = OScolors.Primary;
            }
            else if (control is TextBox)
            {
                control.GetType().GetProperty("BorderStyle")?.SetValue(control, BStyle);
            }
            else if (control is NumericUpDown)
            {
                Mode = IsDarkMode ? "DarkMode_ItemsView" : "ClearMode_ItemsView";
                SetWindowTheme(control.Handle, Mode, null);
            }
            else if (control is Button button)
            {
                button.FlatStyle = IsDarkMode ? FlatStyle.Flat : FlatStyle.Standard;
                button.FlatAppearance.CheckedBackColor = OScolors.Accent;
                button.BackColor = OScolors.Control;
                button.FlatAppearance.BorderColor = (button.FindForm()?.AcceptButton == button) ? OScolors.Accent : OScolors.Control;
                button.FlatAppearance.MouseOverBackColor = OScolors.ControlLight;
            }
            else if (control is ComboBox comboBox)
            {
                if (comboBox.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    comboBox.SelectionStart = comboBox.Text.Length;
                }
                control.BeginInvoke(new Action(() =>
                {
                    if (control is ComboBox invokedComboBox && !invokedComboBox.DropDownStyle.Equals(ComboBoxStyle.DropDownList))
                        invokedComboBox.SelectionLength = 0;
                }));

                if (!control.Enabled && IsDarkMode)
                {
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                }

                Mode = IsDarkMode ? "DarkMode_CFD" : "ClearMode_CFD";
                SetWindowTheme(control.Handle, Mode, null);
            }
            else if (control is TabPage tabPage)
            {
                tabPage.BackColor = OScolors.Surface;
            }
            else if (control is Panel panel && panel.Parent != null)
            {
                panel.BackColor = panel.Parent.BackColor;
                panel.BorderStyle = BorderStyle.None;
                if (panel.Parent is not TabControl && panel.Parent is not TableLayoutPanel)
                {
                    if (RoundedPanels)
                    {
                        SetRoundBorders(panel, 6, OScolors.SurfaceDark, 1);
                    }
                }
            }
            else if (control is GroupBox groupBox && groupBox.Parent != null)
            {
                groupBox.BackColor = groupBox.Parent.BackColor;
                groupBox.ForeColor = OScolors.TextActive;
                groupBox.Paint += (sender, e) =>
                {
                    if (sender is GroupBox senderGroupBox && !control.Enabled && IsDarkMode)
                    {
                        using Brush B = new SolidBrush(control.ForeColor);
                        e.Graphics.DrawString(senderGroupBox.Text, senderGroupBox.Font, B, new PointF(6, 0));
                    }
                };
            }
            else if (control is TableLayoutPanel tablePanel && tablePanel.Parent != null)
            {
                tablePanel.BackColor = tablePanel.Parent.BackColor;
                tablePanel.ForeColor = OScolors.TextInactive;
            }
            else if (control is TabControl tab && tab.Parent != null)
            {
                tab.Appearance = TabAppearance.Normal;
                tab.DrawMode = TabDrawMode.OwnerDrawFixed;
                tab.DrawItem += (sender, e) =>
                {
                    using (SolidBrush headerBrush = new SolidBrush(tab.Parent.BackColor))
                    {
                        e.Graphics.FillRectangle(headerBrush, new Rectangle(0, 0, tab.Width, tab.Height));
                    }

                    for (int i = 0; i < tab.TabPages.Count; i++)
                    {
                        TabPage tabPage = tab.TabPages[i];
                        if (tabPage.Tag == null)
                        {
                            tabPage.BackColor = OScolors.Surface;
                            tabPage.BorderStyle = BorderStyle.FixedSingle;
                            foreach (Control child in tabPage.Controls)
                            {
                                ThemeControl(child);
                            }
                            tabPage.ControlAdded += (_s, _e) =>
                            {
                                if (_e.Control != null) ThemeControl(_e.Control);
                            };
                            tabPage.Tag = "themed";
                        }
                        Rectangle tabRect = tab.GetTabRect(i);
                        bool isSelected = tab.SelectedIndex == i;
                        if (isSelected)
                        {
                            using (SolidBrush tabBackColor = new SolidBrush(OScolors.Surface))
                            {
                                e.Graphics.FillRectangle(tabBackColor, tabRect);
                            }
                        }
                        Image? icon = null;
                        if (tab.ImageList != null && tabPage.ImageIndex >= 0 && tabPage.ImageIndex < tab.ImageList.Images.Count)
                        {
                            icon = tab.ImageList.Images[tabPage.ImageIndex];
                        }
                        Rectangle textBounds;
                        TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                        Color textColor = isSelected ? OScolors.TextActive : OScolors.TextInactive;
                        if (tab.Alignment == TabAlignment.Left || tab.Alignment == TabAlignment.Right)
                        {
                            if (icon != null)
                            {
                                int iconHeight = tab.ImageList.ImageSize.Height;
                                int iconWidth = tab.ImageList.ImageSize.Width;
                                int iconX = tabRect.X + (tabRect.Width - iconWidth) / 2;
                                int iconY = tabRect.Y + 15;
                                Image imageToDraw = icon;
                                bool shouldDispose = false;
                                if (IsDarkMode && tabPage.ImageKey != "locked.png")
                                {
                                    imageToDraw = ChangeToColor(icon, Color.White);
                                    shouldDispose = true;
                                }
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
                };
            }
            else if (control is PictureBox pictureBox && pictureBox.Parent != null)
            {
                pictureBox.BackColor = pictureBox.Parent.BackColor;
                if (OScolors != null)
                {
                    pictureBox.ForeColor = OScolors.TextActive;
                }
                pictureBox.BorderStyle = BorderStyle.None;
            }
            else if (control is CheckBox checkBox && checkBox.Parent != null)
            {
                checkBox.BackColor = checkBox.Parent.BackColor;
                checkBox.ForeColor = control.Enabled ? OScolors.TextActive : OScolors.TextInactive;
                checkBox.Paint += (sender, e) =>
                {
                    if (sender is CheckBox senderCheckBox && !control.Enabled && IsDarkMode)
                    {
                        using Brush B = new SolidBrush(control.ForeColor);
                        e.Graphics.DrawString(senderCheckBox.Text, senderCheckBox.Font, B, new PointF(16, 0));
                    }
                };
            }
            else if (control is RadioButton radioButton && radioButton.Parent != null)
            {
                radioButton.BackColor = radioButton.Parent.BackColor;
                radioButton.ForeColor = control.Enabled ? OScolors.TextActive : OScolors.TextInactive;
                radioButton.Paint += (sender, e) =>
                {
                    if (sender is RadioButton senderRadioButton && !control.Enabled && IsDarkMode)
                    {
                        using Brush B = new SolidBrush(control.ForeColor);
                        e.Graphics.DrawString(senderRadioButton.Text, senderRadioButton.Font, B, new PointF(16, 0));
                    }
                };
            }
            else if (control is MenuStrip menuStrip)
            {
                menuStrip.RenderMode = ToolStripRenderMode.Professional;
                menuStrip.Renderer = new MyRenderer(new CustomColorTable(OScolors), ColorizeIcons)
                {
                    MyColors = OScolors
                };
            }
            else if (control is ToolStrip toolStrip)
            {
                toolStrip.RenderMode = ToolStripRenderMode.Professional;
                toolStrip.Renderer = new MyRenderer(new CustomColorTable(OScolors), ColorizeIcons) { MyColors = OScolors };
            }
            else if (control is ToolStripPanel toolStripPanel && toolStripPanel.Parent != null)
            {
                toolStripPanel.BackColor = toolStripPanel.Parent.BackColor;
            }
            else if (control is ToolStripDropDown dropDown)
            {
                dropDown.Opening -= Tsdd_Opening;
                dropDown.Opening += Tsdd_Opening;
            }
            else if (control is ContextMenuStrip contextMenu)
            {
                contextMenu.RenderMode = ToolStripRenderMode.Professional;
                contextMenu.Renderer = new MyRenderer(new CustomColorTable(OScolors), ColorizeIcons) { MyColors = OScolors };
                contextMenu.Opening -= Tsdd_Opening;
                contextMenu.Opening += Tsdd_Opening;
            }
            else if (control is MdiClient mdiClient)
            {
                mdiClient.BackColor = OScolors.Surface;
            }
            else if (control is PropertyGrid pGrid)
            {
                pGrid.BackColor = OScolors.Control;
                pGrid.ViewBackColor = OScolors.Control;
                pGrid.LineColor = OScolors.Surface;
                pGrid.ViewForeColor = OScolors.TextActive;
                pGrid.ViewBorderColor = OScolors.ControlDark;
                pGrid.CategoryForeColor = OScolors.TextActive;
                pGrid.CategorySplitterColor = OScolors.ControlLight;
            }
            else if (control is ListView lView)
            {
                if (lView.Name == "liveConnectionsListView")
                {
                    lView.OwnerDraw = false;
                }
                else
                {
                    lView.OwnerDraw = true;
                }

                lView.DrawColumnHeader -= ListView_DrawColumnHeader;
                lView.DrawColumnHeader += ListView_DrawColumnHeader;

                if (!lView.OwnerDraw)
                {
                    Mode = IsDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";
                    SetWindowTheme(control.Handle, Mode, null);
                }
            }
            else if (control is TreeView)
            {
                control.GetType().GetProperty("BorderStyle")?.SetValue(control, BorderStyle.None);
            }
            else if (control is DataGridView grid)
            {
                grid.EnableHeadersVisualStyles = false;
                grid.BorderStyle = BorderStyle.FixedSingle;
                grid.BackgroundColor = OScolors.Control;
                grid.GridColor = OScolors.Control;

                grid.Paint += (sender, e) =>
                {
                    if (sender is DataGridView dgv)
                    {
                        PropertyInfo? hsp = typeof(DataGridView).GetProperty("HorizontalScrollBar", BindingFlags.Instance | BindingFlags.NonPublic);
                        PropertyInfo? vsp = typeof(DataGridView).GetProperty("VerticalScrollBar", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (hsp?.GetValue(dgv) is HScrollBar hs && hs.Visible && vsp?.GetValue(dgv) is VScrollBar vs && vs.Visible)
                        {
                            using Brush brush = new SolidBrush(OScolors.SurfaceDark);
                            var w = vs.Size.Width;
                            var h = hs.Size.Height;
                            e.Graphics.FillRectangle(brush, dgv.ClientRectangle.X + dgv.ClientRectangle.Width - w - 1,
                              dgv.ClientRectangle.Y + dgv.ClientRectangle.Height - h - 1, w, h);
                        }
                    }
                };
                grid.DefaultCellStyle.BackColor = OScolors.Surface;
                grid.DefaultCellStyle.ForeColor = OScolors.TextActive;
                grid.ColumnHeadersDefaultCellStyle.BackColor = OScolors.Surface;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = OScolors.TextActive;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = OScolors.Surface;
                grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                grid.RowHeadersDefaultCellStyle.BackColor = OScolors.Surface;
                grid.RowHeadersDefaultCellStyle.ForeColor = OScolors.TextActive;
                grid.RowHeadersDefaultCellStyle.SelectionBackColor = OScolors.Surface;
                grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            }
            else if (control is RichTextBox richText && richText.Parent != null)
            {
                richText.BackColor = richText.Parent.BackColor;
                richText.BorderStyle = BorderStyle.None;
            }
            else if (control is FlowLayoutPanel flowLayout && flowLayout.Parent != null)
            {
                flowLayout.BackColor = flowLayout.Parent.BackColor;
            }

            if (control.ContextMenuStrip != null)
                ThemeControl(control.ContextMenuStrip);
            foreach (Control childControl in control.Controls)
            {
                ThemeControl(childControl);
            }
        }

        public static void ExcludeFromProcessing(Control control)
        {
            controlStatusStorage.ExcludeFromProcessing(control);
        }

        public static int GetWindowsColorMode(bool GetSystemColorModeInstead = false)
        {
            try
            {
                return (int?)Registry.GetValue(
                  @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                  GetSystemColorModeInstead ? "SystemUsesLightTheme" : "AppsUseLightTheme",
                  -1) ?? 1;
            }
            catch
            {
                return 1;
            }
        }

        public static Color GetWindowsAccentColor()
        {
            try
            {
                DWMCOLORIZATIONcolors colors = new DWMCOLORIZATIONcolors();
                DwmGetColorizationParameters(ref colors);

                if (IsWindows10orGreater())
                {
                    var color = colors.ColorizationColor;
                    var colorValue = long.Parse(color.ToString(), System.Globalization.NumberStyles.HexNumber);
                    var transparency = (colorValue >> 24) & 0xFF;
                    var red = (colorValue >> 16) & 0xFF;
                    var green = (colorValue >> 8) & 0xFF;
                    var blue = (colorValue >> 0) & 0xFF;
                    return Color.FromArgb((int)transparency, (int)red, (int)green, (int)blue);
                }
                return Color.CadetBlue;
            }
            catch (Exception)
            {
                return Color.CadetBlue;
            }
        }

        public static Color GetWindowsAccentOpaqueColor()
        {
            DWMCOLORIZATIONcolors colors = new DWMCOLORIZATIONcolors();
            DwmGetColorizationParameters(ref colors);

            if (IsWindows10orGreater())
            {
                var color = colors.ColorizationColor;
                var colorValue = long.Parse(color.ToString(), System.Globalization.NumberStyles.HexNumber);
                var red = (colorValue >> 16) & 0xFF;
                var green = (colorValue >> 8) & 0xFF;
                var blue = (colorValue >> 0) & 0xFF;
                return Color.FromArgb(255, (int)red, (int)green, (int)blue);
            }
            return Color.CadetBlue;
        }

        public static OSThemeColors GetSystemColors(int ColorMode = 0)
        {
            OSThemeColors _ret = new();
            if (ColorMode <= 0)
            {
                _ret.Background = Color.FromArgb(32, 32, 32);
                _ret.BackgroundDark = Color.FromArgb(18, 18, 18);
                _ret.BackgroundLight = ControlPaint.Light(_ret.Background);
                _ret.Surface = Color.FromArgb(43, 43, 43);
                _ret.SurfaceLight = Color.FromArgb(50, 50, 50);
                _ret.SurfaceDark = Color.FromArgb(29, 29, 29);
                _ret.TextActive = Color.White;
                _ret.TextInactive = Color.FromArgb(176, 176, 176);
                _ret.TextInAccent = GetReadableColor(_ret.Accent);
                _ret.Control = Color.FromArgb(55, 55, 55);
                _ret.ControlDark = ControlPaint.Dark(_ret.Control);
                _ret.ControlLight = Color.FromArgb(67, 67, 67);
                _ret.Primary = Color.FromArgb(3, 218, 198);
                _ret.Secondary = Color.MediumSlateBlue;
            }

            return _ret;
        }

        public static void SetRoundBorders(Control _Control, int Radius = 10, Color? borderColor = null, int borderSize = 2, bool underlinedStyle = false)
        {
            borderColor ??= Color.MediumSlateBlue;
            if (_Control?.Parent != null)
            {
                _Control.GetType().GetProperty("BorderStyle")?.SetValue(_Control, BorderStyle.None);
                _Control.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _Control.Width, _Control.Height, Radius, Radius));
                _Control.Paint += (sender, e) =>
                {
                    Graphics graph = e.Graphics;
                    if (Radius > 1 && _Control.Parent != null)
                    {
                        var rectBorderSmooth = _Control.ClientRectangle;
                        var rectBorder = Rectangle.Inflate(rectBorderSmooth, -borderSize, -borderSize);
                        int smoothSize = borderSize > 0 ? borderSize : 1;
                        using GraphicsPath pathBorderSmooth = GetFigurePath(rectBorderSmooth, Radius);
                        using GraphicsPath pathBorder = GetFigurePath(rectBorder, Radius - borderSize);
                        using Pen penBorderSmooth = new(_Control.Parent.BackColor, smoothSize);
                        using Pen penBorder = new((Color)borderColor, borderSize);

                        _Control.Region = new Region(pathBorderSmooth);
                        if (Radius > 15)
                        {
                            using GraphicsPath pathTxt = GetFigurePath(_Control.ClientRectangle, borderSize * 2);
                            _Control.Region = new Region(pathTxt);
                        }
                        graph.SmoothingMode = SmoothingMode.AntiAlias;
                        penBorder.Alignment = PenAlignment.Center;

                        if (underlinedStyle)
                        {
                            graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                            graph.SmoothingMode = SmoothingMode.None;
                            graph.DrawLine(penBorder, 0, _Control.Height - 1, _Control.Width, _Control.Height - 1);
                        }
                        else
                        {
                            graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                            graph.DrawPath(penBorder, pathBorder);
                        }
                    }
                };
            }
        }

        public static Bitmap ChangeToColor(Bitmap bmp, Color c)
        {
            Bitmap bmp2 = new(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(bmp2))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                float tR = c.R / 255f;
                float tG = c.G / 255f;
                float tB = c.B / 255f;

                System.Drawing.Imaging.ColorMatrix colorMatrix = new(new float[][]
                {
                    new float[] { 1,    0,  0,  0,  0 },
                    new float[] { 0,    1,  0,  0,  0 },
                    new float[] { 0,    0,  1,  0,  0 },
                    new float[] { 0,    0,  0,  1,  0 },
                    new float[] { tR,   tG, tB, 0,  1 }
                });

                System.Drawing.Imaging.ImageAttributes attributes = new();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
                  0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp2;
        }

        public static Image ChangeToColor(Image bmp, Color c) => ChangeToColor((Bitmap)bmp, c);
        private void Tsdd_Opening(object? sender, CancelEventArgs e)
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
                    ThemeControl(tsmi.DropDown);
                }
                tsmi.DropDownOpening -= Tsmi_DropDownOpening;
            }
        }

        private static bool IsWindows10orGreater()
        {
            return WindowsVersion() >= 10;
        }

        private static int WindowsVersion()
        {
            try
            {
                using var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string? productName = reg?.GetValue("ProductName")?.ToString();
                if (!string.IsNullOrEmpty(productName))
                {
                    var parts = productName.Split(' ');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int result))
                    {
                        return result;
                    }
                }
            }
            catch { }

            return Environment.OSVersion.Version.Major;
        }

        private static Color GetReadableColor(Color backgroundColor)
        {
            double normalizedR = backgroundColor.R / 255.0;
            double normalizedG = backgroundColor.G / 255.0;
            double normalizedB = backgroundColor.B / 255.0;
            double luminance = 0.299 * normalizedR + 0.587 * normalizedG + 0.114 * normalizedB;
            return luminance < 0.5 ? Color.FromArgb(182, 180, 215) : Color.FromArgb(34, 34, 34);
        }

        private static GraphicsPath GetFigurePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class OSThemeColors
    {
        public Color Background { get; set; } = SystemColors.Control;
        public Color BackgroundDark { get; set; } = SystemColors.ControlDark;
        public Color BackgroundLight { get; set; } = SystemColors.ControlLight;
        public Color Surface { get; set; } = SystemColors.ControlLightLight;
        public Color SurfaceDark { get; set; } = SystemColors.ControlLight;
        public Color SurfaceLight { get; set; } = Color.White;
        public Color TextActive { get; set; } = SystemColors.ControlText;
        public Color TextInactive { get; set; } = SystemColors.GrayText;
        public Color TextInAccent { get; set; } = SystemColors.HighlightText;
        public Color Control { get; set; } = SystemColors.ButtonFace;
        public Color ControlDark { get; set; } = SystemColors.ButtonShadow;
        public Color ControlLight { get; set; } = SystemColors.ButtonHighlight;
        public Color Accent { get; set; } = DarkModeCS.GetWindowsAccentColor();
        public Color AccentOpaque { get; set; } = DarkModeCS.GetWindowsAccentOpaqueColor();
        public Color AccentDark => ControlPaint.Dark(Accent);
        public Color AccentLight => ControlPaint.Light(Accent);
        public Color Primary { get; set; } = SystemColors.Highlight;
        public Color PrimaryDark => ControlPaint.Dark(Primary);
        public Color PrimaryLight => ControlPaint.Light(Primary);
        public Color Secondary { get; set; } = SystemColors.HotTrack;
        public Color SecondaryDark => ControlPaint.Dark(Secondary);
        public Color SecondaryLight => ControlPaint.Light(Secondary);
    }

    public class MyRenderer : ToolStripProfessionalRenderer
    {
        public bool ColorizeIcons { get; set; } = true;
        public OSThemeColors MyColors { get; set; }

        public MyRenderer(ProfessionalColorTable table, bool pColorizeIcons = true) : base(table)
        {
            ColorizeIcons = pColorizeIcons;
            MyColors = new OSThemeColors();
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            base.OnRenderGrip(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                using var p = new Pen(MyColors.ControlDark);
                e.Graphics.DrawRectangle(p, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
            }
            else
            {
                base.OnRenderToolStripBorder(e);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip != null)
            {
                e.ToolStrip!.BackColor = MyColors.Background;
            }
            base.OnRenderToolStripBackground(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is not ToolStripButton button) return;
            Graphics g = e.Graphics;
            Rectangle bounds = new(Point.Empty, e.Item.Size);

            Color gradientBegin = MyColors.Background;
            Color gradientEnd = MyColors.Background;
            using Pen BordersPencil = new(MyColors.Background);

            if (button.Pressed || button.Checked)
            {
                gradientBegin = MyColors.Control;
                gradientEnd = MyColors.Control;
            }
            else if (button.Selected)
            {
                gradientBegin = MyColors.Accent;
                gradientEnd = MyColors.Accent;
            }

            using (Brush b = new LinearGradientBrush(bounds, gradientBegin, gradientEnd, LinearGradientMode.Vertical))
            {
                g.FillRectangle(b, bounds);
            }

            g.DrawRectangle(BordersPencil, bounds);
            g.DrawLine(BordersPencil, bounds.X, bounds.Y, bounds.Width - 1, bounds.Y);
            g.DrawLine(BordersPencil, bounds.X, bounds.Y, bounds.X, bounds.Height - 1);
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new(Point.Empty, e.Item.Size);
            Color gradientBegin = MyColors.Background;
            Color gradientEnd = MyColors.Background;

            if (e.Item.Pressed)
            {
                gradientBegin = MyColors.Control;
                gradientEnd = MyColors.Control;
            }
            else if (e.Item.Selected)
            {
                gradientBegin = MyColors.Accent;
                gradientEnd = MyColors.Accent;
            }

            using Brush b = new LinearGradientBrush(bounds, gradientBegin, gradientEnd, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(b, bounds);
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new(Point.Empty, e.Item.Size);
            Color gradientBegin = MyColors.Background;
            Color gradientEnd = MyColors.Background;

            if (e.Item.Pressed)
            {
                gradientBegin = MyColors.Control;
                gradientEnd = MyColors.Control;
            }
            else if (e.Item.Selected)
            {
                gradientBegin = MyColors.Accent;
                gradientEnd = MyColors.Accent;
            }

            using (Brush b = new LinearGradientBrush(bounds, gradientBegin, gradientEnd, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(b, bounds);
            }

            int Padding = 2;
            Size cSize = new(8, 4);
            using Pen ChevronPen = new(MyColors.TextInactive, 2);
            Point P1 = new(bounds.Width - (cSize.Width + Padding), (bounds.Height / 2) - (cSize.Height / 2));
            Point P2 = new(bounds.Width - Padding, (bounds.Height / 2) - (cSize.Height / 2));
            Point P3 = new(bounds.Width - (cSize.Width / 2 + Padding), (bounds.Height / 2) + (cSize.Height / 2));

            e.Graphics.DrawLine(ChevronPen, P1, P3);
            e.Graphics.DrawLine(ChevronPen, P2, P3);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item != null)
            {
                e.TextColor = e.Item.Enabled ? MyColors.TextActive : MyColors.TextInactive;
            }
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderItemBackground(e);
            if (e.Item is ToolStripComboBox)
            {
                Rectangle rect = new(Point.Empty, e.Item.Size);
                using Pen p = new(MyColors.ControlLight, 1);
                e.Graphics.DrawRectangle(p, rect);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is not ToolStripMenuItem)
            {
                base.OnRenderMenuItemBackground(e);
                return;
            }

            Rectangle bounds = new(Point.Empty, e.Item.Size);
            Color gradientBegin = MyColors.Background;
            Color gradientEnd = MyColors.Background;
            bool DrawIt = false;
            if (e.Item.Pressed)
            {
                gradientBegin = MyColors.Control;
                gradientEnd = MyColors.Control;
                DrawIt = true;
            }
            else if (e.Item.Selected)
            {
                gradientBegin = MyColors.Accent;
                gradientEnd = MyColors.Accent;
                DrawIt = true;
            }

            if (DrawIt)
            {
                using Brush b = new LinearGradientBrush(bounds, gradientBegin, gradientEnd, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(b, bounds);
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null || e.Item == null)
            {
                base.OnRenderItemImage(e);
                return;
            }

            if (e.Item.GetType().FullName == "System.Windows.Forms.MdiControlStrip+ControlBoxMenuItem")
            {
                Color _ClearColor = e.Item.Enabled ? MyColors.TextActive : MyColors.SurfaceDark;
                using (Image adjustedImage = DarkModeCS.ChangeToColor(e.Image, _ClearColor))
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(adjustedImage, e.ImageRectangle);
                }
                return;
            }

            if (ColorizeIcons)
            {
                Color _ClearColor = e.Item.Enabled ? MyColors.TextInactive : MyColors.SurfaceDark;
                using (Image adjustedImage = DarkModeCS.ChangeToColor(e.Image, _ClearColor))
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(adjustedImage, e.ImageRectangle);
                }
            }
            else
            {
                base.OnRenderItemImage(e);
            }
        }
    }

    public class CustomColorTable : ProfessionalColorTable
    {
        public OSThemeColors Colors { get; set; }

        public CustomColorTable(OSThemeColors _Colors)
        {
            Colors = _Colors;
            UseSystemColors = false;
        }

        public override Color ImageMarginGradientBegin => Colors.Control;
        public override Color ImageMarginGradientMiddle => Colors.Control;
        public override Color ImageMarginGradientEnd => Colors.Control;
    }

    public class ControlStatusStorage
    {
        private readonly ConditionalWeakTable<Control, ControlStatusInfo> _controlsProcessed = new();
        public void ExcludeFromProcessing(Control control)
        {
            _controlsProcessed.Remove(control);
            _controlsProcessed.Add(control, new ControlStatusInfo() { IsExcluded = true });
        }

        public ControlStatusInfo? GetControlStatusInfo(Control control)
        {
            _controlsProcessed.TryGetValue(control, out ControlStatusInfo? info);
            return info;
        }

        public void RegisterProcessedControl(Control control, bool isDarkMode)
        {
            _controlsProcessed.Add(control,
                new ControlStatusInfo() { IsExcluded = false, LastThemeAppliedIsDark = isDarkMode });
        }
    }

    public class ControlStatusInfo
    {
        public bool IsExcluded { get; set; }
        public bool LastThemeAppliedIsDark { get; set; }
    }
}