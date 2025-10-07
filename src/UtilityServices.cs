// File: UtilityServices.cs
using DarkModeForms;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NetFwTypeLib;
using System;

namespace MinimalFirewall
{
    public static class AdminTaskService
    {
        public static void ResetFirewall()
        {
            try
            {
                Type fwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                if (fwMgrType != null)
                {
                    INetFwMgr fwMgr = (INetFwMgr)Activator.CreateInstance(fwMgrType);
                    fwMgr.RestoreDefaults();
                    Debug.WriteLine("[AdminTask] Firewall reset to defaults using COM interface.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdminTask ERROR] Firewall reset failed: {ex.Message}");
                Messenger.MessageBox($"Could not reset Windows Firewall.\n\nError: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SetAuditPolicy(bool enable)
        {
            string[] guids =
            {
                "{0CCE9225-69AE-11D9-BED3-505054503030}",
                "{0CCE9226-69AE-11D9-BED3-505054503030}"
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
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or ObjectDisposedException or InvalidOperationException)
            {
                Debug.WriteLine($"[AdminTask FATAL ERROR] {ex}");
                Messenger.MessageBox($"A critical error occurred while trying to run an administrative task:\n\n{ex.Message}", "Execution Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class StartupService
    {
        private const string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly string? _appName;
        private readonly string? _appPath;
        private readonly string _taskName;

        public StartupService()
        {
            _appName = Assembly.GetExecutingAssembly().GetName().Name;
            _appPath = Environment.ProcessPath;
            if (_appName != null)
            {
                _taskName = _appName + " Startup";
            }
            else
            {
                _taskName = "MinimalFirewall Startup";
            }
        }

        public void SetStartup(bool isEnabled)
        {
            if (string.IsNullOrEmpty(_appName) || string.IsNullOrEmpty(_appPath)) return;

            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key?.GetValue(_appName) != null)
                {
                    key.DeleteValue(_appName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Startup] Failed to remove old registry key: {ex.Message}");
            }

            if (isEnabled)
            {
                string arguments = $"/create /tn \"{_taskName}\" /tr \"\\\"{_appPath}\\\" -tray\" /sc onlogon /rl highest /f";
                Execute("schtasks.exe", arguments);
            }
            else
            {
                string arguments = $"/delete /tn \"{_taskName}\" /f";
                Execute("schtasks.exe", arguments);
            }
        }

        private void Execute(string fileName, string arguments)
        {
            Debug.WriteLine($"[Startup] Executing: {fileName} {arguments}");
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            try
            {
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string? stdOut = process.StandardOutput.ReadToEnd();
                    string? stdErr = process.StandardError.ReadToEnd();
                    process.WaitForExit(5000);

                    if (process.ExitCode != 0)
                    {
                        Debug.WriteLine($"[Startup ERROR] Process exited with code {process.ExitCode}.");
                        if (!string.IsNullOrEmpty(stdOut)) Debug.WriteLine($"[Startup ERROR] STDOUT: {stdOut}");
                        if (!string.IsNullOrEmpty(stdErr)) Debug.WriteLine($"[Startup ERROR] STDERR: {stdErr}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Startup FATAL ERROR] {ex.Message}");
            }
        }
    }
}

