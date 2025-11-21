using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;

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

    public static class ValidationUtility
    {
        public static bool ValidatePortString(string portString, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(portString) || portString == "*") return true;

            var parts = portString.Split(',');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrEmpty(trimmedPart)) continue;

                if (trimmedPart.Contains('-'))
                {
                    var rangeParts = trimmedPart.Split('-');
                    if (rangeParts.Length != 2 ||
                        !ushort.TryParse(rangeParts[0], out ushort start) ||
                        !ushort.TryParse(rangeParts[1], out ushort end) ||
                        start > end)
                    {
                        errorMessage = $"Invalid port range '{trimmedPart}'. Must be in 'start-end' format (e.g., 80-88).";
                        return false;
                    }
                }
                else if (!ushort.TryParse(trimmedPart, out _))
                {
                    errorMessage = $"Invalid port number '{trimmedPart}'. Must be a number between 0 and 65535.";
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateAddressString(string addressString, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(addressString) || addressString == "*") return true;

            var parts = addressString.Split(',');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrEmpty(trimmedPart)) continue;

                if (trimmedPart.Equals("LocalSubnet", StringComparison.OrdinalIgnoreCase) ||
                    trimmedPart.Equals("DNS", StringComparison.OrdinalIgnoreCase) ||
                    trimmedPart.Equals("DHCP", StringComparison.OrdinalIgnoreCase) ||
                    trimmedPart.Equals("WINS", StringComparison.OrdinalIgnoreCase) ||
                    trimmedPart.Equals("DefaultGateway", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!IPAddress.TryParse(trimmedPart, out _) && !TypedObjects.IPAddressRange.TryParse(trimmedPart, out _))
                {
                    errorMessage = $"Invalid IP address, range, or keyword: '{trimmedPart}'.";
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateIcmpString(string icmpString, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(icmpString) || icmpString == "*") return true;
            var parts = icmpString.Split(',');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrEmpty(trimmedPart)) continue;

                var icmpParts = trimmedPart.Split(':');
                if (icmpParts.Length > 2)
                {
                    errorMessage = $"Invalid ICMP format '{trimmedPart}'. Use 'type' or 'type:code'.";
                    return false;
                }

                if (!byte.TryParse(icmpParts[0], out _))
                {
                    errorMessage = $"Invalid ICMP type '{icmpParts[0]}'. Must be a number between 0 and 255.";
                    return false;
                }

                if (icmpParts.Length == 2 && !byte.TryParse(icmpParts[1], out _))
                {
                    errorMessage = $"Invalid ICMP code '{icmpParts[1]}'. Must be a number between 0 and 255.";
                    return false;
                }
            }
            return true;
        }
    }


    public static partial class PathResolver
    {
        private static readonly Dictionary<string, string> _deviceMap = [];
        private static readonly Dictionary<string, string> _envVarMap = [];

        static PathResolver()
        {
            try
            {
                var driveLetters = Directory.GetLogicalDrives().Select(d => d[0..2]);
                foreach (var drive in driveLetters)
                {
                    var targetPath = new StringBuilder(260);
                    if (QueryDosDevice(drive, targetPath, targetPath.Capacity) != 0)
                    {
                        string rawDevicePath = targetPath.ToString();
                        int nullIndex = rawDevicePath.IndexOf('\0');
                        if (nullIndex >= 0)
                        {
                            rawDevicePath = rawDevicePath.Substring(0, nullIndex);
                        }
                        rawDevicePath = rawDevicePath.TrimEnd('\\');

                        if (!string.IsNullOrEmpty(rawDevicePath))
                        {
                            _deviceMap[rawDevicePath] = drive;
                        }
                    }
                }
            }
            catch { /* Ignore errors during static init to prevent crash */ }

            var specialFolders = new[]
            {
                Environment.SpecialFolder.UserProfile,
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolder.CommonApplicationData,
                Environment.SpecialFolder.System,
                Environment.SpecialFolder.ProgramFiles,
                Environment.SpecialFolder.ProgramFilesX86,
                Environment.SpecialFolder.Windows
            };

            foreach (var folder in specialFolders)
            {
                try
                {
                    string path = Environment.GetFolderPath(folder, Environment.SpecialFolderOption.DoNotVerify);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string envVar = $"%{folder}%";
                        _envVarMap[path] = envVar;
                    }
                }
                catch { }
            }
        }

        public static string ConvertToEnvironmentPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return absolutePath;

            foreach (var kvp in _envVarMap.OrderByDescending(x => x.Key.Length))
            {
                if (absolutePath.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value + absolutePath.Substring(kvp.Key.Length);
                }
            }

            return absolutePath;
        }

        public static string ConvertFromEnvironmentPath(string environmentPath)
        {
            if (string.IsNullOrEmpty(environmentPath)) return environmentPath;
            try
            {
                return Environment.ExpandEnvironmentVariables(environmentPath);
            }
            catch
            {
                return environmentPath;
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
            catch (Exception)
            {
                return path;
            }
        }

        public static string ConvertDevicePathToDrivePath(string devicePath)
        {
            if (string.IsNullOrEmpty(devicePath)) return devicePath;

            if (devicePath.Length > 1 && devicePath[1] == ':' && char.IsLetter(devicePath[0]))
                return devicePath;

            var matchingDevice = _deviceMap.Keys.FirstOrDefault(d => devicePath.StartsWith(d, StringComparison.OrdinalIgnoreCase));

            if (matchingDevice != null)
            {
                string relativePath = devicePath.Substring(matchingDevice.Length);

                if (relativePath.StartsWith('\\') && _deviceMap[matchingDevice].EndsWith('\\'))
                {
                    relativePath = relativePath.TrimStart('\\');
                }
                else if (!relativePath.StartsWith('\\') && !_deviceMap[matchingDevice].EndsWith('\\'))
                {
                    relativePath = "\\" + relativePath;
                }

                return _deviceMap[matchingDevice] + relativePath;
            }

            return devicePath;
        }

        [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
    }
}