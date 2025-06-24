using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MinimalFirewall
{
    public static class IconCacheService
    {
        private static readonly Dictionary<string, ImageSource> _cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

        public static ImageSource GetIcon(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (_cache.TryGetValue(filePath, out ImageSource icon))
            {
                return icon;
            }

            var newIcon = ExtractIcon(filePath);
            _cache[filePath] = newIcon;
            return newIcon;
        }

        private static ImageSource ExtractIcon(string filePath)
        {
            try
            {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon != null)
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze();
                        return bitmapSource;
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}