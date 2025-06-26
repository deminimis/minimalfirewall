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

        // THIS METHOD IS REWRITTEN TO BE MORE ROBUST
        public static void SetAuditPolicy(bool enable)
        {
            string subcategory = "\"Filtering Platform Connection\"";
            if (enable)
            {
                string arguments = $"/set /subcategory:{subcategory} /failure:enable";
                Execute(arguments, "auditpol.exe");
            }
            else
            {
                string disableFailureArgs = $"/set /subcategory:{subcategory} /failure:disable";
                Execute(disableFailureArgs, "auditpol.exe");
                string disableSuccessArgs = $"/set /subcategory:{subcategory} /success:disable";
                Execute(disableSuccessArgs, "auditpol.exe");
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