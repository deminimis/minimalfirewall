// File: Program.cs
using System.Globalization;
using System.Threading;
namespace MinimalFirewall
{
    internal static class Program
    {
        private const string AppGuid = "6326C497-403B-F991-2F6A-A5FBA67C364C";
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, AppGuid, out bool createdNew))
            {
                if (createdNew)
                {
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                    ApplicationConfiguration.Initialize();

                    var args = Environment.GetCommandLineArgs();
                    bool startMinimized = args.Contains("-tray", StringComparer.OrdinalIgnoreCase);

                    var mainForm = new MainForm(startMinimized);
                    Application.Run(mainForm);
                }
                else
                {
                    MessageBox.Show("Minimal Firewall is already running.", "Application Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}