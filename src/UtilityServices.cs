// UtilityServices.cs
using DarkModeForms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MinimalFirewall
{
    public class AdminTaskService
    {
        public static void ExecutePowerShellRuleCommand(string command)
        {
            string fullCommand = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"";
            Execute(fullCommand, "powershell.exe", out _);
        }

        public static void SetAuditPolicy(bool enable)
        {
            string[] guids =
            {
                "{0CCE9225-69AE-11D9-BED3-505054503030}", // Packet Drop
                "{0CCE9226-69AE-11D9-BED3-505054503030}"  // Connection
             };

            foreach (var guid in guids)
            {
                string arguments = $"/set /subcategory:{guid} /failure:{(enable ? "enable" : "disable")}";
                Execute(arguments, "auditpol.exe", out _);
            }
        }

        public static bool IsAuditPolicyEnabled()
        {
            var packetDropGuid = new Guid("{0CCE9225-69AE-11D9-BED3-505054503030}");
            var connectionGuid = new Guid("{0CCE9226-69AE-11D9-BED3-505054503030}");

            return IsAuditingEnabledForSubcategory(packetDropGuid) && IsAuditingEnabledForSubcategory(connectionGuid);
        }

        #region P/Invoke for Audit Policy
        private const uint AUDIT_FAILURE = 0x00000002;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AuditQuerySystemPolicy(
            [In] Guid pSubCategoryGuids,
            [In] uint PolicyCount,
            [Out] out IntPtr ppPolicy
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AuditFree(
            [In] IntPtr pBuffer
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct AUDIT_POLICY_INFORMATION
        {
            public Guid AuditSubCategoryGuid;
            public uint AuditingInformation;
        }
        #endregion

        private static bool IsAuditingEnabledForSubcategory(Guid subcategoryGuid)
        {
            if (!AuditQuerySystemPolicy(subcategoryGuid, 1, out IntPtr pPolicy))
            {
                Debug.WriteLine($"[AdminTask ERROR] AuditQuerySystemPolicy failed for GUID {subcategoryGuid}.");
                return false;
            }

            try
            {
                var policyInfo = Marshal.PtrToStructure<AUDIT_POLICY_INFORMATION>(pPolicy);
                return (policyInfo.AuditingInformation & AUDIT_FAILURE) == AUDIT_FAILURE;
            }
            finally
            {
                if (pPolicy != IntPtr.Zero)
                {
                    AuditFree(pPolicy);
                }
            }
        }

        private static void Execute(string arguments, string fileName, out string? output)
        {
            Debug.WriteLine($"[AdminTask] Preparing to execute: {fileName} {arguments}");
            output = null;
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            try
            {
                using var process = new Process { StartInfo = startInfo };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                using var outputWaitHandle = new AutoResetEvent(false);
                using var errorWaitHandle = new AutoResetEvent(false);
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null) outputWaitHandle.Set();
                    else outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null) errorWaitHandle.Set();
                    else errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (process.WaitForExit(5000) && outputWaitHandle.WaitOne(5000) && errorWaitHandle.WaitOne(5000))
                {
                    output = outputBuilder.ToString();
                    string errors = errorBuilder.ToString();

                    Debug.WriteLine($"[AdminTask] Exit Code: {process.ExitCode}");
                    if (!string.IsNullOrWhiteSpace(output))
                        Debug.WriteLine($"[AdminTask] Standard Output:\n{output}");

                    if (!string.IsNullOrWhiteSpace(errors))
                        Messenger.MessageBox($"An error occurred during an administrative task:\n\n{errors}", "Admin Task Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    Debug.WriteLine("[AdminTask ERROR] Process timed out or streams did not close.");
                    if (!process.HasExited) process.Kill();
                    Messenger.MessageBox("An administrative task timed out and may not have completed successfully.", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Debug.WriteLine($"[AdminTask] Execution finished for: {fileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminTask FATAL ERROR] {ex}");
                Messenger.MessageBox($"A critical error occurred while trying to run an administrative task:\n\n{ex.Message}", "Execution Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

    public class StartupService
    {
        private const string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly string? _appName;
        private readonly string? _appPath;

        public StartupService()
        {
            _appName = Assembly.GetExecutingAssembly().GetName().Name;
            _appPath = Environment.ProcessPath;
        }

        public void SetStartup(bool isEnabled)
        {
            if (string.IsNullOrEmpty(_appName) || string.IsNullOrEmpty(_appPath)) return;
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    Debug.WriteLine("[ERROR] Could not open registry key for startup settings.");
                    return;
                }

                if (isEnabled)
                {
                    key.SetValue(_appName, $"\"{_appPath}\"");
                }
                else if (key.GetValue(_appName) != null)
                {
                    key.DeleteValue(_appName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to update startup settings: {ex.Message}");
            }
        }
    }
}