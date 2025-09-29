using System.Globalization;
using System.Threading;

namespace MinimalFirewall
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            ApplicationConfiguration.Initialize();

            var args = Environment.GetCommandLineArgs();
            bool startMinimized = args.Contains("-tray", StringComparer.OrdinalIgnoreCase);

            Application.Run(new MainForm(startMinimized));
        }
    }
}
