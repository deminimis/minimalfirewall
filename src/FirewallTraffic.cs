using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Linq;
using MinimalFirewall;
using MinimalFirewall.TypedObjects;

namespace Firewall.Traffic
{
    public static partial class TcpTrafficTracker
    {
        private const uint AF_INET = 2;
        private const uint AF_INET6 = 23;
        private const uint ERROR_INSUFFICIENT_BUFFER = 122;

        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedTcpTable(IntPtr pTcpTable, ref uint pdwSize, [MarshalAs(UnmanagedType.Bool)] bool bOrder, uint ulAf, int TableClass, uint Reserved);
        public static List<TcpTrafficRow> GetConnections()
        {
            var connections = new List<TcpTrafficRow>();
            connections.AddRange(GetConnectionsForFamily(AF_INET));
            connections.AddRange(GetConnectionsForFamily(AF_INET6));
            return connections;
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

        private static List<TcpTrafficRow> GetConnectionsForFamily(uint family)
        {
            IntPtr pTcpTable = IntPtr.Zero;
            uint pdwSize = 0;
            uint retVal = GetExtendedTcpTable(pTcpTable, ref pdwSize, true, family, 5, 0);

            if (retVal != 0 && retVal != ERROR_INSUFFICIENT_BUFFER)
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"[ERROR] GetExtendedTcpTable failed on initial call with error code: {retVal}, Win32 Error: {error}");
                return [];
            }

            pTcpTable = Marshal.AllocHGlobal((int)pdwSize);
            try
            {
                retVal = GetExtendedTcpTable(pTcpTable, ref pdwSize, true, family, 5, 0);
                if (retVal == 0)
                {
                    int rowCount = Marshal.ReadInt32(pTcpTable);
                    var connections = new List<TcpTrafficRow>(rowCount);
                    IntPtr rowPtr = pTcpTable + Marshal.SizeOf<int>();

                    for (int i = 0; i < rowCount; i++)
                    {
                        if (family == AF_INET)
                        {
                            var rowStructure = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                            connections.Add(new TcpTrafficRow(rowStructure));
                            rowPtr += Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                        }
                        else
                        {
                            var rowStructure = Marshal.PtrToStructure<MIB_TCP6ROW_OWNER_PID>(rowPtr);
                            connections.Add(new TcpTrafficRow(rowStructure));
                            rowPtr += Marshal.SizeOf<MIB_TCP6ROW_OWNER_PID>();
                        }
                    }
                    return connections;
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"[ERROR] GetExtendedTcpTable failed on second call with error code: {retVal}, Win32 Error: {error}");
                }
            }
            finally
            {
                if (pTcpTable != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pTcpTable);
                }
            }
            return [];
        }

        #region Native Structures
        public readonly struct TcpTrafficRow : IEquatable<TcpTrafficRow>
        {
            public readonly IPEndPoint LocalEndPoint;
            public readonly IPEndPoint RemoteEndPoint;
            public readonly uint ProcessId;
            public readonly uint State;

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

            public bool Equals(TcpTrafficRow other)
            {
                return LocalEndPoint.Equals(other.LocalEndPoint) &&
                       RemoteEndPoint.Equals(other.RemoteEndPoint) &&
                       ProcessId == other.ProcessId &&
                       State == other.State;
            }

            public override bool Equals(object? obj)
            {
                return obj is TcpTrafficRow other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(LocalEndPoint, RemoteEndPoint, ProcessId, State);
            }
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
    public class TcpConnectionViewModel : INotifyPropertyChanged
    {
        private readonly BackgroundFirewallTaskService _backgroundTaskService;
        public TcpTrafficTracker.TcpTrafficRow Connection { get; }
        public string ProcessName { get; private set; }
        public string ProcessPath { get; private set; }
        public string ServiceName { get; private set; }

        public string DisplayName => string.IsNullOrEmpty(ServiceName) ? ProcessName : $"{ProcessName} ({ServiceName})";
        public string LocalAddress => Connection.LocalEndPoint.Address.ToString();
        public int LocalPort => Connection.LocalEndPoint.Port;
        public string RemoteAddress => Connection.RemoteEndPoint.Address.ToString();
        public int RemotePort => Connection.RemoteEndPoint.Port;
        public string State => TcpTrafficTracker.GetStateString(Connection.State);

        public ICommand KillProcessCommand { get; }
        public ICommand BlockRemoteIpCommand { get; }

        public TcpConnectionViewModel(TcpTrafficTracker.TcpTrafficRow connection, (string Name, string Path, string ServiceName) processInfo, BackgroundFirewallTaskService backgroundTaskService)
        {
            Connection = connection;
            ProcessName = processInfo.Name;
            ProcessPath = processInfo.Path;
            ServiceName = processInfo.ServiceName;
            _backgroundTaskService = backgroundTaskService;
            KillProcessCommand = new RelayCommand(KillProcess, CanKillProcess);
            BlockRemoteIpCommand = new RelayCommand(BlockIp, () => true);
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
            if (_backgroundTaskService == null) return;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TrafficMonitorViewModel
    {
        public ObservableCollection<TcpConnectionViewModel> ActiveConnections { get; } = new ObservableCollection<TcpConnectionViewModel>();

        public void StopMonitoring()
        {
            ActiveConnections.Clear();
        }
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

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}