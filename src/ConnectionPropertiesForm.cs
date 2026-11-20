using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkModeForms;
using Firewall.Traffic;
using Firewall.Traffic.ViewModels;

namespace MinimalFirewall
{
    // Details of one live connection. Every lookup runs only when this window opens,
    // and the whois page is opened only on an explicit click.
    public partial class ConnectionPropertiesForm : Form
    {
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        private const uint TOKEN_QUERY = 0x0008;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr OpenProcess(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint processId);

        [LibraryImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr handle);

        private static readonly Font DetailsFont = new("Consolas", 9.5F);

        private readonly TcpConnectionViewModel _connection;
        private readonly TextBox _details = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = DetailsFont, BorderStyle = BorderStyle.FixedSingle };
        private readonly ThemedButton _whois = new() { Text = "Whois Online", AutoSize = true };
        private readonly ThemedButton _copy = new() { Text = "Copy", AutoSize = true };
        private readonly ThemedButton _close = new() { Text = "Close", AutoSize = true, DialogResult = DialogResult.Cancel };
        private readonly string _user;
        private readonly string _processStarted;
        private readonly string _created;
        private string _remoteHost;

        public ConnectionPropertiesForm(TcpConnectionViewModel connection)
        {
            _connection = connection;
            Text = "Connection Properties";
            Font = new Font("Segoe UI", 9F);
            Size = new Size(620, 400);
            MinimumSize = new Size(420, 280);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            CancelButton = _close;

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 0, 0) };
            buttons.Controls.AddRange([_close, _copy, _whois]);
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            body.Controls.Add(_details);
            body.Controls.Add(buttons);
            Controls.Add(body);
            // Focus the button, otherwise the read-only text opens fully selected.
            ActiveControl = _close;

            _whois.Enabled = connection.HasRemoteAddress;
            _whois.Click += (_, _) => OpenWhois();
            _copy.Click += (_, _) => Clipboard.SetText(_details.Text);

            uint pid = connection.Connection.ProcessId;
            _user = GetProcessUser(pid);
            _processStarted = GetProcessStartTime(pid);
            _created = TcpTrafficTracker.GetCreationTime(connection.Connection)?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not recorded";
            _remoteHost = connection.HasRemoteAddress ? "Resolving..." : "-";
            _details.Text = BuildText();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Applied here, not in the constructor, so the early handle does not break CenterParent.
            bool isDark = Theme.Colors.Surface.R < 128;
            Theme.ApplyTitleBarTheme(Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            BackColor = Theme.Colors.Background;
            ForeColor = Theme.Colors.TextActive;
            _details.BackColor = Theme.Colors.Surface;
            _details.ForeColor = Theme.Colors.TextActive;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!_connection.HasRemoteAddress) return;

            _remoteHost = await ResolveHostAsync(_connection.Connection.RemoteEndPoint.Address);
            if (!IsDisposed) _details.Text = BuildText();
        }

        private string BuildText()
        {
            var c = _connection;
            return string.Join(Environment.NewLine,
                $"Application:         {c.DisplayName}",
                $"Path:                {c.ProcessPath}",
                $"PID:                 {c.ProcessId}",
                $"Process user:        {_user}",
                $"Process started:     {_processStarted}",
                "",
                $"Protocol:            {c.Protocol}",
                $"State:               {(c.State.Length > 0 ? c.State : "-")}",
                $"Local:               {c.Connection.LocalEndPoint}",
                $"Remote:              {(c.HasRemoteAddress ? c.Connection.RemoteEndPoint.ToString() : "-")}",
                $"Remote host:         {_remoteHost}",
                $"Connection created:  {_created}");
        }

        public static async Task<string> ResolveHostAsync(IPAddress address)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(address);
                return string.IsNullOrEmpty(entry.HostName) || entry.HostName == address.ToString() ? "(no name)" : entry.HostName;
            }
            catch (SocketException)
            {
                return "(no name)";
            }
        }

        public static string GetProcessUser(uint pid)
        {
            IntPtr process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (process == IntPtr.Zero) return "Unavailable";
            try
            {
                if (!OpenProcessToken(process, TOKEN_QUERY, out var token)) return "Unavailable";
                try
                {
                    using var identity = new WindowsIdentity(token);
                    return identity.Name;
                }
                finally
                {
                    CloseHandle(token);
                }
            }
            finally
            {
                CloseHandle(process);
            }
        }

        private static string GetProcessStartTime(uint pid)
        {
            try
            {
                using var process = Process.GetProcessById((int)pid);
                return process.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or Win32Exception)
            {
                return "Unavailable";
            }
        }

        private void OpenWhois()
        {
            string url = "https://www.whois.com/whois/" + Uri.EscapeDataString(_connection.Connection.RemoteEndPoint.Address.ToString());
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Win32Exception ex)
            {
                Messenger.MessageBox($"Could not open the browser.\n\nError: {ex.Message}", "Whois Online", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
