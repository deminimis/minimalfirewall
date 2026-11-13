using DarkModeForms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    // Small code-only dialog listing running programs, used to allow one without browsing for its file.
    public partial class ProcessPickerForm : Form
    {
        private const uint ProcessQueryLimitedInformation = 0x1000;

        private readonly ListBox _list = new() { Dock = DockStyle.Fill, IntegralHeight = false, BorderStyle = BorderStyle.None };
        private readonly Button _okButton = new() { Text = "Allow", DialogResult = DialogResult.OK, Enabled = false, AutoSize = true };
        private readonly Button _cancelButton = new() { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };

        public string? SelectedPath => (_list.SelectedItem as Entry)?.Path;

        public ProcessPickerForm(AppSettings appSettings)
        {
            Text = "Allow a Running Program";
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
            TopMost = true;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(560, 420);
            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
            buttons.Controls.Add(_cancelButton);
            buttons.Controls.Add(_okButton);
            Controls.Add(_list);
            Controls.Add(buttons);

            _list.SelectedIndexChanged += (_, _) => _okButton.Enabled = _list.SelectedItem != null;
            _list.DoubleClick += (_, _) => { if (_list.SelectedItem != null) { DialogResult = DialogResult.OK; } };

            bool isDark = appSettings.Theme == "Auto" ? Theme.IsSystemDarkMode() : appSettings.Theme == "Dark";
            Theme.ApplyTitleBarTheme(Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            BackColor = Theme.Colors.Background;
            ForeColor = Theme.Colors.TextActive;
            _list.BackColor = Theme.Colors.Surface;
            _list.ForeColor = Theme.Colors.TextActive;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _list.Items.Add("Loading...");
            try
            {
                var entries = await Task.Run(GetRunningPrograms);
                if (IsDisposed)
                {
                    return;
                }
                _list.Items.Clear();
                _list.Items.AddRange([.. entries]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ProcessPickerForm failed to list processes: {ex.Message}");
                _list.Items.Clear();
            }
        }

        private static List<Entry> GetRunningPrograms()
        {
            var byPath = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    string? path = GetProcessPath(process.Id);
                    if (!string.IsNullOrEmpty(path) && !byPath.ContainsKey(path))
                    {
                        byPath[path] = new Entry(process.ProcessName, path);
                    }
                }
            }
            return [.. byPath.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)];
        }

        // QueryFullProcessImageName needs only limited query rights and avoids loading the module list.
        public static string? GetProcessPath(int processId)
        {
            if (processId <= 4)
            {
                return null;
            }

            IntPtr handle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            if (handle == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                var buffer = new System.Text.StringBuilder(1024);
                int size = buffer.Capacity;
                return QueryFullProcessImageNameW(handle, 0, buffer, ref size) ? buffer.ToString(0, size) : null;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private sealed record Entry(string Name, string Path)
        {
            public override string ToString() => $"{Name}  —  {Path}";
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr OpenProcess(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageNameW(IntPtr process, uint flags, System.Text.StringBuilder exeName, ref int size);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr handle);
    }
}
