using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using NetFwTypeLib;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    // Secure Profile: while Lockdown Mode is on, restore the profile state when another
    // program turns a profile's firewall off or sets its default outbound action to Allow.
    // Change detection uses registry notifications on the local policy keys, so the guard
    // costs nothing while nothing changes (no polling, no dedicated thread).
    public sealed class LockdownGuard : IDisposable
    {
        private const string PolicyRoot = @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\";
        private const int RegNotifyChangeLastSet = 0x4;
        private const int RegNotifyThreadAgnostic = 0x10000000;
        private const int MaxRestoresPerMinute = 3;

        private static readonly (NET_FW_PROFILE_TYPE2_ Type, string Name, string Key)[] Profiles =
        [
            (NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN, "Domain", "DomainProfile"),
            (NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE, "Private", "StandardProfile"),
            (NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC, "Public", "PublicProfile"),
        ];

        private readonly UserActivityLogger _logger;
        private readonly object _sync = new();
        private readonly List<(RegistryKey Key, AutoResetEvent Signal, RegisteredWaitHandle Wait)> _watches = [];
        private readonly Queue<DateTime> _recentRestores = new();
        private readonly System.Threading.Timer _debounce;
        private bool[] _enabledBaseline = [];
        private bool _locked;
        private bool _gaveUp;
        private bool _disposed;

        // Raised with a user-facing message after a restore, or when the guard stops.
        public event Action<string>? Restored;

        public LockdownGuard(UserActivityLogger logger)
        {
            _logger = logger;
            _debounce = new System.Threading.Timer(_ => Check());
        }

        [DllImport("advapi32.dll")]
        private static extern int RegNotifyChangeKeyValue(SafeRegistryHandle hKey, bool watchSubtree, int notifyFilter, SafeWaitHandle hEvent, bool asynchronous);

        public void Start(bool locked)
        {
            Expect(locked);
            lock (_sync)
            {
                if (_watches.Count > 0 || _disposed)
                {
                    return;
                }

                foreach (var profile in Profiles)
                {
                    RegistryKey? key = Registry.LocalMachine.OpenSubKey(PolicyRoot + profile.Key);
                    if (key == null)
                    {
                        continue;
                    }

                    var signal = new AutoResetEvent(false);
                    if (!Arm(key, signal))
                    {
                        key.Dispose();
                        signal.Dispose();
                        continue;
                    }

                    var wait = ThreadPool.RegisterWaitForSingleObject(signal, (_, _) =>
                    {
                        // Re-arm first so a change made during the check is not missed.
                        if (Arm(key, signal))
                        {
                            try
                            {
                                _debounce.Change(1000, Timeout.Infinite);
                            }
                            catch (ObjectDisposedException)
                            {
                                // Disposed while this callback was running.
                            }
                        }
                    }, null, Timeout.Infinite, false);
                    _watches.Add((key, signal, wait));
                }
            }
        }

        // Call before Minimal Firewall changes the lockdown state itself, so its own change
        // is never mistaken for an outside one.
        public void Expect(bool locked)
        {
            lock (_sync)
            {
                _locked = locked;
                _gaveUp = false;
                _recentRestores.Clear();
                if (locked)
                {
                    _enabledBaseline = ReadEnabledProfiles();
                }
            }
        }

        private static bool Arm(RegistryKey key, AutoResetEvent signal)
        {
            try
            {
                return RegNotifyChangeKeyValue(key.Handle, false, RegNotifyChangeLastSet | RegNotifyThreadAgnostic, signal.SafeWaitHandle, true) == 0;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private static bool[] ReadEnabledProfiles()
        {
            INetFwPolicy2? policy = null;
            try
            {
                policy = FirewallRuleService.GetLocalPolicy();
                return [.. Profiles.Select(p => policy.FirewallEnabled[p.Type])];
            }
            catch (Exception ex) when (ex is COMException or InvalidOperationException)
            {
                return [];
            }
            finally
            {
                if (policy != null)
                {
                    Marshal.ReleaseComObject(policy);
                }
            }
        }

        private void Check()
        {
            string? message = null;
            lock (_sync)
            {
                if (!_locked || _gaveUp || _disposed)
                {
                    return;
                }

                INetFwPolicy2? policy = null;
                try
                {
                    policy = FirewallRuleService.GetLocalPolicy();
                    var drifted = new List<int>();
                    for (int i = 0; i < Profiles.Length; i++)
                    {
                        var type = Profiles[i].Type;
                        bool wasEnabled = i < _enabledBaseline.Length && _enabledBaseline[i];
                        if (policy.DefaultOutboundAction[type] != NET_FW_ACTION_.NET_FW_ACTION_BLOCK ||
                            (wasEnabled && !policy.FirewallEnabled[type]))
                        {
                            drifted.Add(i);
                        }
                    }

                    if (drifted.Count == 0)
                    {
                        return;
                    }

                    while (_recentRestores.Count > 0 && DateTime.UtcNow - _recentRestores.Peek() > TimeSpan.FromMinutes(1))
                    {
                        _recentRestores.Dequeue();
                    }

                    string names = string.Join(", ", drifted.Select(i => Profiles[i].Name));
                    if (_recentRestores.Count >= MaxRestoresPerMinute)
                    {
                        // Another program keeps changing the policy back: stop instead of fighting it.
                        _gaveUp = true;
                        message = $"Another program keeps changing the firewall profile ({names}). Secure Profile stopped restoring it until Lockdown Mode is toggled.";
                        _logger.LogChange("Secure Profile", "Stopped: profile changed repeatedly by another program");
                    }
                    else
                    {
                        foreach (int i in drifted)
                        {
                            var type = Profiles[i].Type;
                            if (i < _enabledBaseline.Length && _enabledBaseline[i])
                            {
                                policy.FirewallEnabled[type] = true;
                            }
                            policy.DefaultOutboundAction[type] = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                        }
                        _recentRestores.Enqueue(DateTime.UtcNow);
                        message = $"Another program changed the firewall profile ({names}). Lockdown Mode was restored.";
                        _logger.LogChange("Secure Profile", $"Restored Lockdown on {names}");
                    }
                }
                catch (Exception ex) when (ex is COMException or InvalidOperationException or UnauthorizedAccessException)
                {
                    _logger.LogException("LockdownGuard.Check", ex);
                }
                finally
                {
                    if (policy != null)
                    {
                        Marshal.ReleaseComObject(policy);
                    }
                }
            }

            if (message != null)
            {
                Restored?.Invoke(message);
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                foreach (var (key, signal, wait) in _watches)
                {
                    wait.Unregister(null);
                    key.Dispose();
                    signal.Dispose();
                }
                _watches.Clear();
            }
            _debounce.Dispose();
        }
    }
}
