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
namespace Firewall.Traffic
{
    public static partial class TcpTrafficTracker
    {
        private const int AF_INET = 2;
        private const int AF_INET6 = 23;

        [LibraryImport("iphlpapi.dll", SetLastError = true)]
        private static partial uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, [MarshalAs(UnmanagedType.Bool)] bool bOrder, int ulAf, int TableClass, uint Reserved);
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

        private static List<TcpTrafficRow> GetConnectionsForFamily(int family)
        {
            IntPtr pTcpTable = IntPtr.Zero;
            int pdwSize = 0;
            _ = GetExtendedTcpTable(pTcpTable, ref pdwSize, true, family, 5, 0);
            pTcpTable = Marshal.AllocHGlobal(pdwSize);
            try
            {
                if (GetExtendedTcpTable(pTcpTable, ref pdwSize, true, family, 5, 0) == 0)
                {
                    int rowCount = Marshal.ReadInt32(pTcpTable);
                    var connections = new List<TcpTrafficRow>(rowCount);
                    IntPtr rowPtr = pTcpTable + 4;
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (family == AF_INET)
                        {

                            var rowStructure = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                            connections.Add(new TcpTrafficRow(rowStructure));
                            rowPtr += Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
                        }
                        else
                        {
                            var rowStructure = Marshal.PtrToStructure<MIB_TCP6ROW_OWNER_PID>(rowPtr);
                            connections.Add(new TcpTrafficRow(rowStructure));
                            rowPtr += Marshal.SizeOf(typeof(MIB_TCP6ROW_OWNER_PID));
                        }
                    }
                    return connections;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pTcpTable);
            }
            return [];
        }

        #region Native Structures
        public readonly struct TcpTrafficRow : IEquatable<TcpTrafficRow>
        {
            public readonly IPEndPoint LocalEndPoint;
            public readonly IPEndPoint RemoteEndPoint;
            public readonly int ProcessId;
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
        public struct MIB_TCPROW_OWNER_PID { public uint state; public uint localAddr; public uint localPort; public uint remoteAddr; public uint remotePort; public int owningPid; }

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
            public int owningPid;
        }
        #endregion
    }
}

namespace Firewall.Traffic.ViewModels
{
    public class TcpConnectionViewModel : INotifyPropertyChanged
    {
        public TcpTrafficTracker.TcpTrafficRow Connection { get; }
        public string ProcessName { get; private set; }
        public string ProcessPath { get; private set; }
        public string LocalAddress => Connection.LocalEndPoint.Address.ToString();
        public int LocalPort => Connection.LocalEndPoint.Port;
        public string RemoteAddress => Connection.RemoteEndPoint.Address.ToString();
        public int RemotePort => Connection.RemoteEndPoint.Port;
        public string State => TcpTrafficTracker.GetStateString(Connection.State);
        public ICommand KillProcessCommand { get; }
        public ICommand BlockRemoteIpCommand { get; }

        public TcpConnectionViewModel(TcpTrafficTracker.TcpTrafficRow connection, (string Name, string Path) processInfo)
        {
            Connection = connection;
            ProcessName = processInfo.Name;
            ProcessPath = processInfo.Path;
            KillProcessCommand = new RelayCommand(KillProcess, CanKillProcess);
            BlockRemoteIpCommand = new RelayCommand(BlockIp, () => true);
        }

        private void KillProcess()
        {
            try
            {
                var process = Process.GetProcessById(Connection.ProcessId);
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
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TrafficMonitorViewModel
    {
        private System.Threading.Timer? _timer;
        private bool _isRefreshing = false;
        private readonly SynchronizationContext? _syncContext;
        public ObservableCollection<TcpConnectionViewModel> ActiveConnections { get; } = [];
        public TrafficMonitorViewModel()
        {
            _syncContext = SynchronizationContext.Current;
        }

        public void StartMonitoring()
        {
            if (_timer != null) return;
            _timer = new System.Threading.Timer(RefreshConnections, null, 0, 5000);
        }

        public void StopMonitoring()
        {
            _timer?.Dispose();
            _timer = null;
            ActiveConnections.Clear();
        }

        private async void RefreshConnections(object? state)
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                var newVms = await Task.Run(() =>
                {
                    var connections = TcpTrafficTracker.GetConnections().Distinct().ToList();
                    var processInfoCache = new Dictionary<int, (string Name, string Path)>();
                    var viewModels = new List<TcpConnectionViewModel>();

                    foreach (var conn in connections)
                    {
                        if (!processInfoCache.TryGetValue(conn.ProcessId, out var info))
                        {
                            try
                            {
                                using (var p = Process.GetProcessById(conn.ProcessId))
                                {
                                    string name = p.ProcessName;
                                    string path = string.Empty;
                                    try { if (p.MainModule != null) path = p.MainModule.FileName; }
                                    catch (Win32Exception) { path = "N/A (Access Denied)"; }
                                    info = (name, path);
                                }
                            }
                            catch (ArgumentException) { info = ("(Exited)", string.Empty); }
                            catch { info = ("System", string.Empty); }
                            processInfoCache[conn.ProcessId] = info;
                        }
                        viewModels.Add(new TcpConnectionViewModel(conn, info));
                    }
                    return viewModels;
                });

                _syncContext?.Post(_ =>
                {
                    if (ActiveConnections == null) return;

                    var newViewModelMap = newVms.ToDictionary(vm => vm.Connection);
                    var existingViewModelMap = ActiveConnections.ToDictionary(vm => vm.Connection);

                    var keysToRemove = existingViewModelMap.Keys.Except(newViewModelMap.Keys).ToList();
                    foreach (var key in keysToRemove)
                    {
                        ActiveConnections.Remove(existingViewModelMap[key]);
                    }

                    var keysToAdd = newViewModelMap.Keys.Except(existingViewModelMap.Keys).ToList();
                    foreach (var key in keysToAdd)
                    {
                        ActiveConnections.Add(newViewModelMap[key]);
                    }
                }, null);
            }
            finally
            {
                _isRefreshing = false;
            }
        }
    }

    public class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? p) => canExecute();
        public void Execute(object? p) => execute();
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}