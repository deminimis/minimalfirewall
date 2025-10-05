// File: IconService.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public class IconService : Component
    {
        private ImageList? _imageList;
        private readonly Dictionary<string, int> _iconCache = new(StringComparer.OrdinalIgnoreCase);
        private int _defaultIconIndex = -1;
        private int _systemIconIndex = -1;

        #region Native Methods
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        #endregion

        public ImageList? ImageList
        {
            get => _imageList;
            set
            {
                _imageList = value;
                if (_imageList != null)
                {
                    AddDefaultIcon();
                    AddSystemIcon();
                }
            }
        }

        public IconService() { }

        public IconService(IContainer container)
        {
            container.Add(this);
        }

        private void AddDefaultIcon()
        {
            if (_imageList == null || _imageList.Images.ContainsKey("default")) return;
            try
            {
                var bmp = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                }
                _imageList.Images.Add("default", (Bitmap)bmp.Clone());
                bmp.Dispose();
                _defaultIconIndex = _imageList.Images.IndexOfKey("default");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconService] Failed to create default icon: {ex.Message}");
            }
        }

        private void AddSystemIcon()
        {
            if (_imageList == null || _imageList.Images.ContainsKey("system_icon")) return;
            try
            {
                if (_imageList.Images.ContainsKey("advanced.png"))
                {
                    Image systemImage = _imageList.Images["advanced.png"];
                    _imageList.Images.Add("system_icon", systemImage);
                    _systemIconIndex = _imageList.Images.IndexOfKey("system_icon");
                }
                else
                {
                    _systemIconIndex = _defaultIconIndex;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconService] Failed to create system icon: {ex.Message}");
                _systemIconIndex = _defaultIconIndex;
            }
        }

        public int GetIconIndex(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || _imageList == null)
            {
                return _systemIconIndex;
            }

            if (_iconCache.TryGetValue(filePath, out int cachedIndex))
            {
                return cachedIndex;
            }

            if (!File.Exists(filePath))
            {
                return _systemIconIndex;
            }

            IntPtr hIcon = IntPtr.Zero;
            try
            {
                var shinfo = new SHFILEINFO();
                const uint flags = SHGFI_ICON | SHGFI_LARGEICON;
                SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
                hIcon = shinfo.hIcon;

                if (hIcon != IntPtr.Zero)
                {
                    Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
                    _imageList.Images.Add(filePath, icon);

                    int newIndex = _imageList.Images.IndexOfKey(filePath);
                    _iconCache[filePath] = newIndex;
                    return newIndex;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconService] SHGetFileInfo failed for {filePath}: {ex.Message}");
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                {
                    DestroyIcon(hIcon);
                }
            }

            try
            {
                using (Icon? icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon != null)
                    {
                        _imageList.Images.Add(filePath, (Icon)icon.Clone());
                        int newIndex = _imageList.Images.IndexOfKey(filePath);
                        _iconCache[filePath] = newIndex;
                        return newIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconService] Fallback icon extraction failed for {filePath}: {ex.Message}");
            }

            _iconCache[filePath] = _systemIconIndex;
            return _systemIconIndex;
        }

        public void ClearCache()
        {
            if (_imageList == null)
            {
                _iconCache.Clear();
                return;
            }

            foreach (var key in _iconCache.Keys)
            {
                if (key != "default" && key != "system_icon")
                {
                    if (_imageList.Images.ContainsKey(key))
                    {
                        _imageList.Images.RemoveByKey(key);
                    }
                }
            }
            _iconCache.Clear();
        }
    }
}

