using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MinimalFirewall
{
    public static class PathResolver
    {
        private static readonly Dictionary<string, string> _deviceMap = new Dictionary<string, string>();

        static PathResolver()
        {
            // Pre-load the device-to-drive-letter map when the application starts.
            var driveLetters = Directory.GetLogicalDrives().Select(d => d.Substring(0, 2));
            foreach (var drive in driveLetters)
            {
                var targetPath = new StringBuilder(260);
                if (QueryDosDevice(drive, targetPath, targetPath.Capacity) != 0)
                {
                    _deviceMap[targetPath.ToString()] = drive;
                }
            }
        }

        public static string ConvertDevicePathToDrivePath(string devicePath)
        {
            if (string.IsNullOrEmpty(devicePath))
            {
                return devicePath;
            }

            // Check if the path is already a standard drive path
            if (devicePath.Length > 1 && devicePath[1] == ':' && char.IsLetter(devicePath[0]))
            {
                return devicePath;
            }

            var matchingDevice = _deviceMap.Keys.FirstOrDefault(d => devicePath.StartsWith(d, StringComparison.OrdinalIgnoreCase));
            if (matchingDevice != null)
            {
                return _deviceMap[matchingDevice] + devicePath.Substring(matchingDevice.Length);
            }

            return devicePath; // Return original path if no mapping is found
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
    }
}