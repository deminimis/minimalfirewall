using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkModeForms
{
    public partial class Theme
    {
        public enum DisplayMode
        {
            SystemDefault,
            ClearMode,
            DarkMode
        }

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

        [LibraryImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
        private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, [In] int[] attrValue, int attrSize);

        [LibraryImport("dwmapi.dll", EntryPoint = "#127")]
        private static partial void DwmGetColorizationParameters(ref DWMCOLORIZATIONcolors colors);

        // Global Theme Palette
        public static OSThemeColors Colors { get; set; } = GetSystemColors(IsSystemDarkMode() ? 0 : 1);

        public static void ApplyTitleBarTheme(IntPtr handle, DisplayMode colorMode)
        {
            if (handle != IntPtr.Zero)
            {
                bool useDark = (colorMode == DisplayMode.DarkMode) ||
                               (colorMode == DisplayMode.SystemDefault && IsSystemDarkMode());
                int[] DarkModeOn = useDark ? [0x01] : [0x00];
                DwmSetWindowAttribute(handle, (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, DarkModeOn, 4);
            }
        }

        public static bool IsSystemDarkMode()
        {
            return GetWindowsColorMode() <= 0;
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
                var colors = new DWMCOLORIZATIONcolors();
                DwmGetColorizationParameters(ref colors);

                if (IsWindows10orGreater())
                {
                    var colorValue = colors.ColorizationColor;
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
            Color c = GetWindowsAccentColor();
            return Color.FromArgb(255, c.R, c.G, c.B);
        }

        public static OSThemeColors GetSystemColors(int ColorMode = 0)
        {
            OSThemeColors _ret = new();
            if (ColorMode <= 0)
            {
                // Dark Mode
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

                // Semantic Colors (Dark with light theme accents)
                _ret.Success = Color.FromArgb(204, 255, 204);
                _ret.SuccessText = Color.Black;
                _ret.Danger = Color.FromArgb(255, 204, 204);
                _ret.DangerText = Color.Black;
                _ret.Warning = Color.FromArgb(255, 255, 204);
                _ret.InfoText = Color.Black;
                _ret.Ignore = Color.FromArgb(200, 200, 200);
                _ret.SelectionInfo = Color.FromArgb(189, 222, 255);
                _ret.HighlightOverlay = Color.FromArgb(40, Color.White); 
                _ret.LinkText = Color.SkyBlue;
                _ret.GraphicAccent = Color.White;
                // Dark Mode Palette Extensions
                _ret.ConnectionEstablished = Color.FromArgb(204, 255, 204);
                _ret.ConnectionListening = Color.FromArgb(255, 255, 204);
                _ret.PathLabelBackground = Color.FromArgb(45, 45, 48);
            }
            else
            {
                // Light Mode 
                _ret.Background = Color.FromArgb(243, 243, 243);
                _ret.BackgroundDark = Color.FromArgb(229, 229, 229);
                _ret.BackgroundLight = Color.FromArgb(250, 250, 250);
                _ret.Surface = Color.FromArgb(255, 255, 255);
                _ret.SurfaceLight = Color.White;
                _ret.SurfaceDark = Color.FromArgb(240, 240, 240);
                _ret.TextActive = Color.FromArgb(32, 32, 32);
                _ret.TextInactive = Color.FromArgb(105, 105, 105);
                _ret.TextInAccent = Color.White;
                _ret.Control = Color.FromArgb(240, 240, 240);
                _ret.ControlDark = Color.FromArgb(215, 215, 215);
                _ret.ControlLight = Color.FromArgb(250, 250, 250);
                _ret.Primary = Color.FromArgb(0, 120, 215);
                _ret.Secondary = Color.MediumSlateBlue;

                // Semantic Colors (Light)
                _ret.Success = Color.FromArgb(204, 255, 204);
                _ret.SuccessText = Color.DarkGreen;
                _ret.Danger = Color.FromArgb(255, 204, 204);
                _ret.DangerText = Color.Maroon;
                _ret.Warning = Color.FromArgb(255, 255, 204);
                _ret.InfoText = Color.Blue;
                _ret.Ignore = Color.FromArgb(200, 200, 200);
                _ret.SelectionInfo = Color.FromArgb(189, 222, 255);
                _ret.HighlightOverlay = Color.FromArgb(25, Color.Black);
                _ret.LinkText = SystemColors.HotTrack;
                _ret.GraphicAccent = Color.Black;
                // Light Mode Palette Extensions
                _ret.ConnectionEstablished = Color.FromArgb(204, 255, 204);
                _ret.ConnectionListening = Color.FromArgb(255, 255, 204);
                _ret.PathLabelBackground = Color.FromArgb(230, 230, 235);
            }

            return _ret;
        }

        public static bool IsWindows10orGreater()
        {
            return WindowsVersion() >= 10;
        }

        private static int WindowsVersion()
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                return Environment.OSVersion.Version.Major;
            }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (key?.GetValue("CurrentMajorVersionNumber") is int majorInt)
                {
                    return majorInt;
                }

                if (key?.GetValue("ProductName")?.ToString()?.Contains("Windows 1") == true)
                {
                    return 10;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] WindowsVersion failed to read registry version: {ex.Message}");
            }
            return 10;
        }

        public static Color GetReadableColor(Color backgroundColor)
        {
            double normalizedR = backgroundColor.R / 255.0;
            double normalizedG = backgroundColor.G / 255.0;
            double normalizedB = backgroundColor.B / 255.0;
            double luminance = 0.299 * normalizedR + 0.587 * normalizedG + 0.114 * normalizedB;
            return luminance < 0.5 ? Color.FromArgb(182, 180, 215) : Color.FromArgb(34, 34, 34);
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
        public Color Accent { get; set; } = Theme.GetWindowsAccentColor();
        public Color AccentOpaque { get; set; } = Theme.GetWindowsAccentOpaqueColor();
        public Color AccentDark => ControlPaint.Dark(Accent);
        public Color AccentLight => ControlPaint.Light(Accent);
        public Color Primary { get; set; } = SystemColors.Highlight;
        public Color PrimaryDark => ControlPaint.Dark(Primary);
        public Color PrimaryLight => ControlPaint.Light(Primary);
        public Color Secondary { get; set; } = SystemColors.HotTrack;
        public Color SecondaryDark => ControlPaint.Dark(Secondary);
        public Color SecondaryLight => ControlPaint.Light(Secondary);

        // Centralized Application Theme Extensions
        public Color ConnectionEstablished { get; set; }
        public Color ConnectionListening { get; set; }
        public Color PathLabelBackground { get; set; }

        // Semantic Application Colors
        public Color Success { get; set; }
        public Color SuccessText { get; set; }
        public Color Danger { get; set; }
        public Color DangerText { get; set; }
        public Color Warning { get; set; }
        public Color InfoText { get; set; }
        public Color Ignore { get; set; }
        public Color SelectionInfo { get; set; }
        public Color HighlightOverlay { get; set; }
        public Color LinkText { get; set; }
        public Color GraphicAccent { get; set; }
    }
}
