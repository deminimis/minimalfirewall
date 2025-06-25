using System.Diagnostics;
using System.Windows;

namespace MinimalFirewall
{
    public class AdminTaskService
    {
        public static void ExecutePowerShellRuleCommand(string command)
        {
            string fullCommand = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"";
            Execute(fullCommand, "powershell.exe");
        }

        public static void SetAuditPolicy(bool enable)
        {
            string subcategory = "\"Filtering Platform Connection\"";
            if (enable)
            {
                string command = "/c auditpol /set /subcategory:" + subcategory + " /failure:enable";
                Execute(command, "cmd.exe");
            }
            else
            {
                string disableFailureCmd = "/c auditpol /set /subcategory:" + subcategory + " /failure:disable";
                Execute(disableFailureCmd, "cmd.exe");
                string disableSuccessCmd = "/c auditpol /set /subcategory:" + subcategory + " /success:disable";
                Execute(disableSuccessCmd, "cmd.exe");
            }
        }

        private static void Execute(string arguments, string fileName)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    MessageBox.Show("Failed to start administrative process.", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(errors) && !errors.Contains("0x00000490"))
                {
                    MessageBox.Show("An error occurred during an administrative task:\n\n" + errors, "Admin Task Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}