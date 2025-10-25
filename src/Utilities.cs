// File: Utilities.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace MinimalFirewall
{
    public delegate bool TryParseHandler<T>(string value, [NotNullWhen(true)] out T? result);

    public static class ParsingUtility
    {
        public static List<T> ParseStringToList<T>(string? input, TryParseHandler<T> tryParse)
        {
            if (string.IsNullOrEmpty(input) || input.Trim() == "*")
            {
                return [];
            }
            var results = new List<T>();
            var parts = input.Split(',');
            foreach (var part in parts)
            {
                if (tryParse(part.Trim(), out T? result) && result != null)
                {
                    results.Add(result);
                }
            }
            return results;
        }
    }

    public static partial class PathResolver
    {
        private static readonly Dictionary<string, string> _deviceMap = [];
        static PathResolver()
        {
            var driveLetters = Directory.GetLogicalDrives().Select(d => d[0..2]);
            foreach (var drive in driveLetters)
            {
                var targetPath = new StringBuilder(260);
                if (QueryDosDevice(drive, targetPath, targetPath.Capacity) != 0)
                {
                    _deviceMap[targetPath.ToString()] = drive;
                }
            }
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            try
            {
                string expandedPath = Environment.ExpandEnvironmentVariables(path);
                if (Path.IsPathRooted(expandedPath))
                {
                    return Path.GetFullPath(expandedPath);
                }

                string basePath = AppContext.BaseDirectory;
                return Path.GetFullPath(Path.Combine(basePath, expandedPath));
            }
            catch (ArgumentException)
            {
                return path;
            }
        }

        public static string ConvertDevicePathToDrivePath(string devicePath)
        {
            if (string.IsNullOrEmpty(devicePath) || (devicePath.Length > 1 && devicePath[1] == ':' && char.IsLetter(devicePath[0])))
                return devicePath;
            var matchingDevice = _deviceMap.Keys.FirstOrDefault(d => devicePath.StartsWith(d, StringComparison.OrdinalIgnoreCase));
            return matchingDevice != null ? string.Concat(_deviceMap[matchingDevice], devicePath.AsSpan(matchingDevice.Length)) : devicePath;
        }

        [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
    }
}