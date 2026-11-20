using MinimalFirewall;
using MinimalFirewall.TypedObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Firewall.Traffic
{
    public static partial class TcpTrafficTracker
    {
        private const uint AF_INET = 2;
        private const uint AF_INET6 = 23;
        private const uint ERROR_INSUFFICIENT_BUFFER = 122;

        private const uint MIB_TCP_STATE_DELETE_TCB = 12;

        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedTcpTable(IntPtr pTcpTable, ref uint pdwSize, [MarshalAs(UnmanagedType.Bool)] bool bOrder, uint ulAf, int TableClass, uint Reserved);

        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedUdpTable(IntPtr pUdpTable, ref uint pdwSize, [MarshalAs(UnmanagedType.Bool)] bool bOrder, uint ulAf, int TableClass, uint Reserved);

        [LibraryImport("iphlpapi.dll")]
        private static partial uint SetTcpEntry(ref MIB_TCPROW pTcpRow);

        public static List<TcpTrafficRow> GetConnections()
        {
            var connections = new List<TcpTrafficRow>();
            connections.AddRange(GetConnectionsForFamily(AF_INET, udp: false));
            connections.AddRange(GetConnectionsForFamily(AF_INET6, udp: false));
            connections.AddRange(GetConnectionsForFamily(AF_INET, udp: true));
            connections.AddRange(GetConnectionsForFamily(AF_INET6, udp: true));
            return connections;
        }

        // Only IPv4 TCP connections can be closed: Windows has no SetTcpEntry equivalent for IPv6.
        public static bool CanClose(TcpTrafficRow row) =>
            !row.IsUdp && row.LocalEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && row.State != 2 && row.State != 12;

        // Returns 0 on success, otherwise the Win32 error code (317 usually means missing administrator rights).
        public static uint CloseConnection(TcpTrafficRow row)
        {
            var native = new MIB_TCPROW
            {
                state = MIB_TCP_STATE_DELETE_TCB,
                localAddr = BitConverter.ToUInt32(row.LocalEndPoint.Address.GetAddressBytes(), 0),
                localPort = (ushort)IPAddress.HostToNetworkOrder((short)row.LocalEndPoint.Port),
                remoteAddr = BitConverter.ToUInt32(row.RemoteEndPoint.Address.GetAddressBytes(), 0),
                remotePort = (ushort)IPAddress.HostToNetworkOrder((short)row.RemoteEndPoint.Port),
            };
            return SetTcpEntry(ref native);
        }

        // Reads the creation time of one connection from the owner-module tables, on demand only.
        // Returns null when the connection is gone or Windows did not record a time.
        public static DateTime? GetCreationTime(TcpTrafficRow row)
        {
            bool v6 = row.LocalEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            uint family = v6 ? AF_INET6 : AF_INET;
            int addressLength = v6 ? 16 : 4;
            // Offsets in MIB_{TCP,TCP6,UDP,UDP6}ROW_OWNER_MODULE: local address, local port, remote address, remote port, PID, create timestamp.
            (int size, int localAddr, int localPort, int remoteAddr, int remotePort, int pid, int timestamp) layout = (row.IsUdp, v6) switch
            {
                (false, false) => (160, 4, 8, 12, 16, 20, 24),
                (false, true) => (192, 0, 20, 24, 44, 52, 56),
                (true, false) => (160, 0, 4, -1, -1, 8, 16),
                (true, true) => (176, 0, 20, -1, -1, 24, 32),
            };
            byte[] localBytes = row.LocalEndPoint.Address.GetAddressBytes();
            byte[] remoteBytes = row.RemoteEndPoint.Address.GetAddressBytes();

            bool BytesEqual(IntPtr pointer, byte[] expected)
            {
                var actual = new byte[addressLength];
                Marshal.Copy(pointer, actual, 0, addressLength);
                return actual.AsSpan().SequenceEqual(expected);
            }
            int Port(IntPtr pointer) => (ushort)IPAddress.NetworkToHostOrder((short)Marshal.ReadInt32(pointer));

            for (int attempt = 0; attempt < 5; attempt++)
            {
                uint size = 0;
                // TCP_TABLE_OWNER_MODULE_ALL = 8, UDP_TABLE_OWNER_MODULE = 2
                uint Query(IntPtr table) => row.IsUdp
                    ? GetExtendedUdpTable(table, ref size, false, family, 2, 0)
                    : GetExtendedTcpTable(table, ref size, false, family, 8, 0);
                Query(IntPtr.Zero);
                if (size == 0) return null;

                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    uint result = Query(buffer);
                    if (result == ERROR_INSUFFICIENT_BUFFER) continue;
                    if (result != 0) return null;

                    int count = Marshal.ReadInt32(buffer);
                    // The rows hold 64-bit fields, so the array starts 8-byte aligned after dwNumEntries.
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr r = buffer + 8 + i * layout.size;
                        if ((uint)Marshal.ReadInt32(r + layout.pid) != row.ProcessId
                            || Port(r + layout.localPort) != row.LocalEndPoint.Port
                            || !BytesEqual(r + layout.localAddr, localBytes))
                        {
                            continue;
                        }
                        if (!row.IsUdp && (Port(r + layout.remotePort) != row.RemoteEndPoint.Port || !BytesEqual(r + layout.remoteAddr, remoteBytes)))
                        {
                            continue;
                        }
                        long fileTime = Marshal.ReadInt64(r + layout.timestamp);
                        return fileTime > 0 ? DateTime.FromFileTime(fileTime) : null;
                    }
                    return null;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            return null;
        }

        public static string GetStateString(uint state)
        {
            return state switch
            {
                1 => "Closed",
                2 => "Listen",
                3 => "Syn-Sent",
                4 => "Syn-Rcvd",
                5 => "Established",
                6 => "Fin-Wait-1",
                7 => "Fin-Wait-2",
                8 => "Close-Wait",
                9 => "Closing",
                10 => "Last-Ack",
                11 => "Time-Wait",
                12 => "Delete-Tcb",
                _ => "Unknown",
            };
        }

        // Retrieves the TCP or UDP table
        private static List<TcpTrafficRow> GetConnectionsForFamily(uint family, bool udp)
        {
            // TCP_TABLE_OWNER_PID_ALL = 5, UDP_TABLE_OWNER_PID = 1
            uint GetTable(IntPtr table, ref uint size) => udp
                ? GetExtendedUdpTable(table, ref size, true, family, 1, 0)
                : GetExtendedTcpTable(table, ref size, true, family, 5, 0);

            IntPtr pTcpTable = IntPtr.Zero;
            uint pdwSize = 0;
            uint retVal;
            int retryCount = 0;

            // determine necessary buffer size
            retVal = GetTable(pTcpTable, ref pdwSize);

            if (retVal != 0 && retVal != ERROR_INSUFFICIENT_BUFFER)
            {
                Debug.WriteLine($"[ERROR] GetExtendedTcpTable sizing failed: {retVal}, Win32: {Marshal.GetLastWin32Error()}");
                return [];
            }

            do
            {
                try
                {
                    pTcpTable = Marshal.AllocHGlobal((int)pdwSize);
                    retVal = GetTable(pTcpTable, ref pdwSize);

                    if (retVal == 0) // Success
                    {
                        int rowCount = Marshal.ReadInt32(pTcpTable);
                        var connections = new List<TcpTrafficRow>(rowCount);
                        IntPtr rowPtr = pTcpTable + Marshal.SizeOf<int>();

                        for (int i = 0; i < rowCount; i++)
                        {
                            if (udp && family == AF_INET)
                            {
                                var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
                                connections.Add(new TcpTrafficRow(row));
                                rowPtr += Marshal.SizeOf<MIB_UDPROW_OWNER_PID>();
                            }
                            else if (udp)
                            {
                                var row = Marshal.PtrToStructure<MIB_UDP6ROW_OWNER_PID>(rowPtr);
                                connections.Add(new TcpTrafficRow(row));
                                rowPtr += Marshal.SizeOf<MIB_UDP6ROW_OWNER_PID>();
                            }
                            else if (family == AF_INET)
                            {
                                var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                                connections.Add(new TcpTrafficRow(row));
                                rowPtr += Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                            }
                            else
                            {
                                var row = Marshal.PtrToStructure<MIB_TCP6ROW_OWNER_PID>(rowPtr);
                                connections.Add(new TcpTrafficRow(row));
                                rowPtr += Marshal.SizeOf<MIB_TCP6ROW_OWNER_PID>();
                            }
                        }
                        return connections;
                    }
                    else if (retVal == ERROR_INSUFFICIENT_BUFFER)
                    {
                        // Buffer too small, retry with new pdwSize
                        Marshal.FreeHGlobal(pTcpTable);
                        pTcpTable = IntPtr.Zero;
                        retryCount++;
                    }
                    else
                    {
                        Debug.WriteLine($"[ERROR] GetExtendedTcpTable fetch failed: {retVal}");
                        return [];
                    }
                }
                finally
                {
                    if (pTcpTable != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pTcpTable);
                        pTcpTable = IntPtr.Zero;
                    }
                }
            } while (retVal == ERROR_INSUFFICIENT_BUFFER && retryCount < 5);

            return [];
        }

        #region Native Structures
        public readonly struct TcpTrafficRow : IEquatable<TcpTrafficRow>
        {
            public readonly IPEndPoint LocalEndPoint;
            public readonly IPEndPoint RemoteEndPoint;
            public readonly uint ProcessId;
            public readonly uint State;
            public readonly bool IsUdp;

            public TcpTrafficRow(MIB_TCPROW_OWNER_PID row)
            {
                LocalEndPoint = new IPEndPoint(row.localAddr, (ushort)IPAddress.NetworkToHostOrder((short)row.localPort));
                RemoteEndPoint = new IPEndPoint(row.remoteAddr, (ushort)IPAddress.NetworkToHostOrder((short)row.remotePort));
                ProcessId = row.owningPid;
                State = row.state;
            }

            public TcpTrafficRow(MIB_TCP6ROW_OWNER_PID row)
            {
                LocalEndPoint = new IPEndPoint(new IPAddress(row.localAddr, row.localScopeId), (ushort)IPAddress.NetworkToHostOrder((short)row.localPort));
                RemoteEndPoint = new IPEndPoint(new IPAddress(row.remoteAddr, row.remoteScopeId), (ushort)IPAddress.NetworkToHostOrder((short)row.remotePort));
                ProcessId = row.owningPid;
                State = row.state;
            }

            // UDP endpoints have no remote side and no state.
            public TcpTrafficRow(MIB_UDPROW_OWNER_PID row)
            {
                LocalEndPoint = new IPEndPoint(row.localAddr, (ushort)IPAddress.NetworkToHostOrder((short)row.localPort));
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                ProcessId = row.owningPid;
                State = 0;
                IsUdp = true;
            }

            public TcpTrafficRow(MIB_UDP6ROW_OWNER_PID row)
            {
                LocalEndPoint = new IPEndPoint(new IPAddress(row.localAddr, row.localScopeId), (ushort)IPAddress.NetworkToHostOrder((short)row.localPort));
                RemoteEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                ProcessId = row.owningPid;
                State = 0;
                IsUdp = true;
            }

            public bool Equals(TcpTrafficRow other)
            {
                return LocalEndPoint.Equals(other.LocalEndPoint) &&
                       RemoteEndPoint.Equals(other.RemoteEndPoint) &&
                       ProcessId == other.ProcessId &&
                       State == other.State &&
                       IsUdp == other.IsUdp;
            }

            public override bool Equals(object? obj) => obj is TcpTrafficRow other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(LocalEndPoint, RemoteEndPoint, ProcessId, State, IsUdp);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public uint localPort;
            public uint remoteAddr;
            public uint remotePort;
            public uint owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW
        {
            public uint state;
            public uint localAddr;
            public uint localPort;
            public uint remoteAddr;
            public uint remotePort;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPROW_OWNER_PID
        {
            public uint localAddr;
            public uint localPort;
            public uint owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] localAddr;
            public uint localScopeId;
            public uint localPort;
            public uint owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] localAddr;
            public uint localScopeId;
            public uint localPort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] remoteAddr;
            public uint remoteScopeId;
            public uint remotePort;
            public uint state;
            public uint owningPid;
        }
        #endregion
    }
}

namespace Firewall.Traffic.ViewModels
{
    public class TcpConnectionViewModel
    {
        private readonly BackgroundFirewallTaskService _backgroundTaskService;

        public TcpTrafficTracker.TcpTrafficRow Connection { get; }

        // Cached strings 
        public string ProcessName { get; }
        public string ProcessPath { get; }
        public string ServiceName { get; }
        public string DisplayName { get; }
        public string LocalAddress { get; }
        public string RemoteAddress { get; }
        public string State { get; }
        public string Protocol { get; }
        // Identity that survives state changes, used to keep the selection and to spot new rows.
        public string Key { get; }

        public int LocalPort => Connection.LocalEndPoint.Port;
        public int RemotePort => Connection.RemoteEndPoint.Port;
        public int ProcessId => (int)Connection.ProcessId;
        public bool HasRemoteAddress => !Connection.RemoteEndPoint.Address.Equals(IPAddress.Any) && !Connection.RemoteEndPoint.Address.Equals(IPAddress.IPv6Any);

        public ICommand KillProcessCommand { get; }
        public ICommand BlockRemoteIpCommand { get; }

        public TcpConnectionViewModel(TcpTrafficTracker.TcpTrafficRow connection, (string Name, string Path, string ServiceName) processInfo, BackgroundFirewallTaskService backgroundTaskService)
        {
            Connection = connection;
            _backgroundTaskService = backgroundTaskService;

            // Cache static data once
            ProcessName = processInfo.Name;
            ProcessPath = processInfo.Path;
            ServiceName = processInfo.ServiceName;
            DisplayName = string.IsNullOrEmpty(ServiceName) ? ProcessName : $"{ProcessName} ({ServiceName})";
            LocalAddress = connection.LocalEndPoint.Address.ToString();
            RemoteAddress = connection.RemoteEndPoint.Address.ToString();
            State = connection.IsUdp ? string.Empty : TcpTrafficTracker.GetStateString(Connection.State);
            bool isV6 = connection.LocalEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            Protocol = (connection.IsUdp ? "UDP" : "TCP") + (isV6 ? "v6" : "");
            Key = $"{Protocol}|{connection.LocalEndPoint}|{connection.RemoteEndPoint}|{connection.ProcessId}";

            KillProcessCommand = new RelayCommand(KillProcess, CanKillProcess);
            BlockRemoteIpCommand = new RelayCommand(BlockIp, () => HasRemoteAddress);
        }

        private void KillProcess()
        {
            try
            {
                var process = Process.GetProcessById((int)Connection.ProcessId);
                process.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to kill process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CanKillProcess() => !ProcessName.Equals("System", StringComparison.OrdinalIgnoreCase);

        private void BlockIp()
        {
            if (_backgroundTaskService == null)
            {
                return;
            }

            var rule = new AdvancedRuleViewModel
            {
                Name = $"Block Remote IP - {RemoteAddress}",
                Description = $"Blocked remote IP {RemoteAddress} initiated from '{DisplayName}' via Live Connections.",
                IsEnabled = true,
                Grouping = MFWConstants.MainRuleGroup,
                Status = "Block",
                Direction = Directions.Incoming | Directions.Outgoing,
                Protocol = (int)MinimalFirewall.TypedObjects.ProtocolTypes.Any.Value,
                LocalPorts = "*",
                RemotePorts = "*",
                LocalAddresses = "*",
                RemoteAddresses = RemoteAddress,
                Profiles = "All",
                Type = RuleType.Advanced,
                InterfaceTypes = "All",
                IcmpTypesAndCodes = "*"
            };

            var payload = new CreateAdvancedRulePayload { ViewModel = rule, InterfaceTypes = rule.InterfaceTypes, IcmpTypesAndCodes = rule.IcmpTypesAndCodes };
            _backgroundTaskService.EnqueueTask(new FirewallTask(FirewallTaskType.CreateAdvancedRule, payload));

            MessageBox.Show($"Firewall rule queued to block all traffic to/from {RemoteAddress}.", "Rule Queued", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class TrafficMonitorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TcpConnectionViewModel> _activeConnections = [];

        public ObservableCollection<TcpConnectionViewModel> ActiveConnections
        {
            get => _activeConnections;
            set
            {
                if (_activeConnections != value)
                {
                    _activeConnections = value;
                    OnPropertyChanged(nameof(ActiveConnections));
                }
            }
        }

        public void StopMonitoring()
        {
            ActiveConnections.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public bool CanExecute(object? parameter) => _canExecute();
        public void Execute(object? parameter) => _execute();
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}