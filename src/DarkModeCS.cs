using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkModeForms
{
    public partial class DarkModeCS : IDisposable
    {
        private static readonly ConditionalWeakTable<Control, NotificationInfo> _notificationInfo = [];

        private class NotificationInfo
        {
            public int Count { get; set; }
            public string OriginalText { get; set; } = string.Empty;
        }

        public static void SetNotificationCount(Control control, int count)
        {
            if (control is not TabPage tabPage) return;

            if (count > 0)
            {
                if (_notificationInfo.TryGetValue(tabPage, out var info))
                {
                    info.Count = count;
                    tabPage.Text = $"{info.OriginalText} ({count})";
                }
                else
                {
                    _notificationInfo.Add(tabPage, new NotificationInfo { Count = count, OriginalText = tabPage.Text });
                    tabPage.Text = $"{tabPage.Text} ({count})";
                }
            }
            else
            {
                if (_notificationInfo.TryGetValue(tabPage, out var info))
                {
                    tabPage.Text = info.OriginalText;
                    _notificationInfo.Remove(tabPage);
                }
            }
        }

        

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial int SendMessage(IntPtr hWnd, int wMsg, [MarshalAs(UnmanagedType.Bool)] bool wParam, int lParam);
        private const int WM_SETREDRAW = 0x000B;

   

        [LibraryImport("uxtheme.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

   

        [LibraryImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static partial IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [LibraryImport("user32.dll", EntryPoint = "GetWindow")]
        private static partial IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        private const uint GW_CHILD = 5;

        private static readonly ControlStatusStorage controlStatusStorage = new();
        private ControlEventHandler? ownerFormControlAdded;
        private ControlEventHandler? controlControlAdded;
        private bool _IsDarkMode;

        

        public Theme.DisplayMode ColorMode
        {
            get;
            set;
        } = Theme.DisplayMode.SystemDefault;
        public bool IsDarkMode => _IsDarkMode;
        public bool ColorizeIcons { get; set; } = true;
        public Form OwnerForm
        {
            get;
            set;
        }
        public ComponentCollection?
            Components
        {
            get; set;
        }
        public OSThemeColors OScolors
        {
            get;
            set;
        }

        public DarkModeCS(Form _Form, bool _ColorizeIcons = true)
        {
            OwnerForm = _Form;
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(OwnerForm, true, null);
            Components = null;
            ColorizeIcons = _ColorizeIcons;
            OScolors = Theme.GetSystemColors(Theme.IsSystemDarkMode() ? 0 : 1);
            OwnerForm.HandleCreated += (sender, e) => ApplyTitleBarTheme();
        }

        private static void SuspendDrawing(Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }

        private static void ResumeDrawing(Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }

        private void ApplyTitleBarTheme()
        {
            Theme.ApplyTitleBarTheme(OwnerForm.Handle, ColorMode);
        }


        public void ApplyTheme(bool pIsDarkMode = true)
        {
            try
            {
                _IsDarkMode = pIsDarkMode;
                OScolors = Theme.GetSystemColors(pIsDarkMode ? 0 : 1);
                Theme.Colors = OScolors; // Sync global theme palette

                SuspendDrawing(OwnerForm);
                OwnerForm.SuspendLayout();

                ApplyTitleBarTheme();
                OwnerForm.BackColor = OScolors.Background;
                OwnerForm.ForeColor = OScolors.TextInactive;
                if (OwnerForm.Controls != null)
                {
                    foreach (Control _control in OwnerForm.Controls)
                    {
                        ThemeControl(_control);
                    }

                    ownerFormControlAdded = (sender, e) =>
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
                    {
                        ThemeControl(item);
                    }
                }
                OwnerForm.ResumeLayout(true);
                ResumeDrawing(OwnerForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void ApplyTheme(Theme.DisplayMode pColorMode)
        {
            if (ColorMode == pColorMode)
            {
                return;
            }

            ColorMode = pColorMode;
            ApplyTheme(ColorMode == Theme.DisplayMode.SystemDefault ? Theme.IsSystemDarkMode() : ColorMode == Theme.DisplayMode.DarkMode);
        }

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (sender is not ListView listView)
            {
                return;
            }

            if (IsDarkMode)
            {
                using (var backBrush = new SolidBrush(OScolors.Surface))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }
                if (e.Header != null)
                {
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, OScolors.TextActive, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void ApplyThemeToHandle(IntPtr handle, string themeClass)
        {
            string mode = IsDarkMode ? $"DarkMode_{themeClass}" : $"ClearMode_{themeClass}";
            SetWindowTheme(handle, mode, null);
        }

        public void ThemeControl(Control control)
        {
            var info = controlStatusStorage.GetControlStatusInfo(control);
            if (info != null)
            {
                if (info.IsExcluded)
                {
                    return;
                }

                if (info.LastThemeAppliedIsDark == IsDarkMode)
                {
                    return;
                }

                info.LastThemeAppliedIsDark = IsDarkMode;
            }
            else
            {
                controlStatusStorage.RegisterProcessedControl(control, IsDarkMode);
            }
            control.SuspendLayout();
            BorderStyle BStyle = (IsDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D);
            controlControlAdded = (sender, e) =>
            {
                if (e.Control != null)
                {
                    ThemeControl(e.Control);
                }
            };
            control.ControlAdded += controlControlAdded;
            ApplyThemeToHandle(control.Handle, "Explorer");

            control.BackColor = OScolors.Control;
            control.ForeColor = OScolors.TextActive;

            if (control is Label lbl && control.Parent != null)
            {
                control.BackColor = control.Parent.BackColor;
                control.GetType().GetProperty("BorderStyle")?.SetValue(control, BorderStyle.None);
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
                ApplyThemeToHandle(control.Handle, "ItemsView");
            }
            else if (control is Button button)
            {
                button.FlatStyle = IsDarkMode ?
                    FlatStyle.Flat : FlatStyle.Standard;
                button.FlatAppearance.CheckedBackColor = OScolors.Accent;

                if (button.BackColor != Color.Transparent)
                {
                    button.BackColor = OScolors.Control;
                }

                button.FlatAppearance.BorderColor = (button.FindForm()?.AcceptButton == button) ? OScolors.Accent : OScolors.Control;
                button.FlatAppearance.MouseOverBackColor = OScolors.ControlLight;
            }
            else if (control is ComboBox comboBox)
            {
                if (comboBox.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    comboBox.SelectionStart = comboBox.Text.Length;
                }
                if (control.IsHandleCreated)
                {
                    control.BeginInvoke(new Action(() =>
                    {
                        if (control is ComboBox invokedComboBox && !invokedComboBox.DropDownStyle.Equals(ComboBoxStyle.DropDownList))
                        {
                            invokedComboBox.SelectionLength = 0;
                        }
                    }));
                }

                if (!control.Enabled && IsDarkMode)
                {
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                }

                ApplyThemeToHandle(control.Handle, "CFD");
            }
            else if (control is TabPage tabPage)
            {
                tabPage.BackColor = OScolors.Surface;
            }
            else if (control is Panel panel && panel.Parent != null)
            {
                panel.BackColor = panel.Parent.BackColor;
                panel.BorderStyle = BorderStyle.FixedSingle; // flat panel borders
            }
            else if (control is GroupBox groupBox && groupBox.Parent != null)
            {
                groupBox.BackColor = groupBox.Parent.BackColor;
                groupBox.ForeColor = OScolors.TextActive;
                groupBox.FlatStyle = FlatStyle.Flat;
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
                tab.DrawItem -= Tab_DrawItem;
                tab.DrawItem += Tab_DrawItem;
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
            else if (control is ButtonBase btnBase && (control is CheckBox || control is RadioButton) && btnBase.Parent != null)
            {
                btnBase.BackColor = btnBase.Parent.BackColor;
                btnBase.ForeColor = control.Enabled ? OScolors.TextActive : OScolors.TextInactive;
                btnBase.FlatStyle = FlatStyle.Flat;
                btnBase.FlatAppearance.BorderColor = OScolors.ControlDark;
                btnBase.FlatAppearance.CheckedBackColor = OScolors.Accent;
            }
            else if (control is ToolStrip toolStrip)
            {
                toolStrip.RenderMode = ToolStripRenderMode.Professional;
                toolStrip.Renderer = new MyRenderer(new CustomColorTable(OScolors), ColorizeIcons) { MyColors = OScolors };
                if (toolStrip is ToolStripDropDown dropDown)
                {
                    dropDown.Opening -= Tsdd_Opening;
                    dropDown.Opening += Tsdd_Opening;
                }
            }
            else if (control is ToolStripPanel toolStripPanel && toolStripPanel.Parent != null)
            {
                toolStripPanel.BackColor = toolStripPanel.Parent.BackColor;
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
                lView.OwnerDraw = true;

                lView.DrawColumnHeader -= ListView_DrawColumnHeader;
                lView.DrawColumnHeader += ListView_DrawColumnHeader;

                if (!lView.OwnerDraw)
                {
                    ApplyThemeToHandle(control.Handle, "Explorer");
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
                grid.GridColor = OScolors.ControlDark;

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
            else if (control is ProgressBar pBar)
            {
                ApplyThemeToHandle(pBar.Handle, "Explorer");
            }

            if (control.ContextMenuStrip != null)
            {
                ThemeControl(control.ContextMenuStrip);
            }

            foreach (Control childControl in control.Controls)
            {
                ThemeControl(childControl);
            }
            control.ResumeLayout(false);
        }

        private void Tab_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tab || tab.Parent == null)
            {
                return;
            }

            //  Check bounds before filling to prevent overflow
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
                    using var tabBackColor = new SolidBrush(OScolors.Surface);
                    e.Graphics.FillRectangle(tabBackColor, tabRect);
                }
                Image? icon = null;
                if (tab.ImageList != null && tabPage.ImageIndex >= 0 && tabPage.ImageIndex < tab.ImageList.Images.Count)
                {
                    icon = tab.ImageList.Images[tabPage.ImageIndex];
                }
                Rectangle textBounds;
                TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                Color textColor = isSelected ? OScolors.TextActive : OScolors.TextInactive;

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
                        if (IsDarkMode && tabPage.ImageKey != "locked.png")
                        {
                            imageToDraw = RecolorImage(icon, Color.White);
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

        

        

        public static void ExcludeFromProcessing(Control control)
        {
            controlStatusStorage.ExcludeFromProcessing(control);
        }

        

        

        

        

        

        public static Image RecolorImage(Image sourceImage, Color newColor)
        {
            var newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(newBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                float r = newColor.R / 255f;
                float g_ = newColor.G / 255f;
                float b = newColor.B / 255f;
                var colorMatrix = new ColorMatrix(
                [
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0],
                    [0, 0, 0, 1, 0],
                    [r, g_, b, 0, 1]
                ]);
                using var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                    0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }

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

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (OwnerForm != null && ownerFormControlAdded != null)

                    {
                        OwnerForm.ControlAdded -= ownerFormControlAdded;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    

    public class MyRenderer : ToolStripProfessionalRenderer
    {
        private readonly Dictionary<string, Image> _imageCache = [];

        public bool ColorizeIcons { get; set; } = true;

        private OSThemeColors _myColors;
        public OSThemeColors MyColors
        {
            get => _myColors;
            set
            {
                _myColors = value;
                foreach (var img in _imageCache.Values)
                {
                    img.Dispose();
                }

                _imageCache.Clear();
            }
        }

        public MyRenderer(ProfessionalColorTable table, bool pColorizeIcons = true) : base(table)
        {
            ColorizeIcons = pColorizeIcons;
            _myColors = new OSThemeColors();
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
            if (e.Item is not ToolStripButton button)
            {
                return;
            }

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

        private void DrawGradientItemBackground(Graphics g, ToolStripItem item, Rectangle bounds, bool drawOnlyOnInteraction)
        {
            Color gradientBegin = MyColors.Background;
            Color gradientEnd = MyColors.Background;
            bool interacted = false;

            if (item.Pressed)
            {
                gradientBegin = MyColors.Control;
                gradientEnd = MyColors.Control;
                interacted = true;
            }
            else if (item.Selected)
            {
                gradientBegin = MyColors.Accent;
                gradientEnd = MyColors.Accent;
                interacted = true;
            }

            if (!drawOnlyOnInteraction || interacted)
            {
                using Brush b = new LinearGradientBrush(bounds, gradientBegin, gradientEnd, LinearGradientMode.Vertical);
                g.FillRectangle(b, bounds);
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            DrawGradientItemBackground(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size), false);
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            Rectangle bounds = new(Point.Empty, e.Item.Size);
            DrawGradientItemBackground(e.Graphics, e.Item, bounds, false);

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
                e.TextColor = e.Item.Enabled ?
                    MyColors.TextActive : MyColors.TextInactive;
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

            DrawGradientItemBackground(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size), true);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null || e.Item == null)
            {
                base.OnRenderItemImage(e);
                return;
            }


            string stateKey = e.Item.Enabled ? "Enabled" : "Disabled";
            string cacheKey = $"{e.Image.GetHashCode()}-{stateKey}-{ColorizeIcons}";

            if (!_imageCache.TryGetValue(cacheKey, out Image? imageToDraw))
            {
                // Image creation logic
                if (e.Item.GetType().FullName?.Equals("System.Windows.Forms.MdiControlStrip+ControlBoxMenuItem") == true)
                {
                    Color _ClearColor = e.Item.Enabled ?
                        MyColors.TextActive : MyColors.SurfaceDark;
                    imageToDraw = DarkModeCS.RecolorImage(e.Image, _ClearColor);
                }
                else if (ColorizeIcons)
                {
                    Color _ClearColor = e.Item.Enabled ?
                        MyColors.TextInactive : MyColors.SurfaceDark;
                    imageToDraw = DarkModeCS.RecolorImage(e.Image, _ClearColor);
                }
                else
                {
                    base.OnRenderItemImage(e);
                    return;
                }

                if (imageToDraw != null)
                {
                    _imageCache[cacheKey] = imageToDraw;
                }
            }

            // Draw cached image
            if (imageToDraw != null)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawImage(imageToDraw, e.ImageRectangle);
            }
            else
            {
                base.OnRenderItemImage(e);
            }
        }
    }

    public class CustomColorTable(OSThemeColors _Colors) : ProfessionalColorTable
    {
        public OSThemeColors Colors { get; set; } = _Colors;

        public CustomColorTable(OSThemeColors _Colors, bool dummy) : this(_Colors)
        {
        }

        public override Color ImageMarginGradientBegin => Colors.Control;
        public override Color ImageMarginGradientMiddle => Colors.Control;
        public override Color ImageMarginGradientEnd => Colors.Control;
    }

    public class ControlStatusStorage
    {
        private readonly ConditionalWeakTable<Control, ControlStatusInfo> _controlsProcessed = [];
        public void ExcludeFromProcessing(Control control)
        {
            _controlsProcessed.Remove(control);
            _controlsProcessed.Add(control, new ControlStatusInfo() { IsExcluded = true });
        }

        public ControlStatusInfo?
        GetControlStatusInfo(Control control)
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
        public bool IsExcluded
        { get; set; }
        public bool LastThemeAppliedIsDark
        { get; set; }
    }

    
}

