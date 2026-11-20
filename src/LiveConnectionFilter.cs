using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall
{
    // Advanced filter for the Live Connections tab, using the CurrPorts syntax:
    //   include|exclude : local|remote|both : tcp|udp|tcpudp : <port, port range, IP, IP range or CIDR>
    //   include|exclude : process : <process name or PID>
    // Filters are separated by spaces, ';' or new lines. When at least one include filter
    // exists, a connection must match one of them; any matching exclude filter hides it.
    public sealed class LiveConnectionFilter
    {
        private enum Target { Local, Remote, Both, Process }

        private sealed record Rule(bool Include, Target Target, bool Tcp, bool Udp,
            ushort PortBegin, ushort PortEnd, IPAddressRange? Addresses, string? ProcessName, uint? ProcessId)
        {
            public bool Matches(bool isUdp, IPEndPoint local, IPEndPoint remote, string processName, uint processId)
            {
                if (Target == Target.Process)
                {
                    return ProcessId.HasValue ? ProcessId == processId : string.Equals(ProcessName, processName, StringComparison.OrdinalIgnoreCase);
                }

                if (isUdp ? !Udp : !Tcp)
                {
                    return false;
                }

                return Target switch
                {
                    Target.Local => MatchesEndPoint(local),
                    Target.Remote => MatchesEndPoint(remote),
                    _ => MatchesEndPoint(local) || MatchesEndPoint(remote),
                };
            }

            private bool MatchesEndPoint(IPEndPoint endPoint) => Addresses != null
                ? Addresses.Contains(endPoint.Address)
                : endPoint.Port >= PortBegin && endPoint.Port <= PortEnd;
        }

        private readonly List<Rule> _rules;

        public IReadOnlyList<string> InvalidFilters { get; }
        public bool IsEmpty => _rules.Count == 0;

        private LiveConnectionFilter(List<Rule> rules, List<string> invalid)
        {
            _rules = rules;
            InvalidFilters = invalid;
        }

        public static LiveConnectionFilter Parse(string? text)
        {
            var rules = new List<Rule>();
            var invalid = new List<string>();
            foreach (var token in (text ?? "").Split([' ', '\t', '\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryParseRule(token, out var rule))
                {
                    rules.Add(rule);
                }
                else
                {
                    invalid.Add(token);
                }
            }
            return new LiveConnectionFilter(rules, invalid);
        }

        public bool Matches(bool isUdp, IPEndPoint local, IPEndPoint remote, string processName, uint processId)
        {
            if (_rules.Any(r => r.Include) && !_rules.Any(r => r.Include && r.Matches(isUdp, local, remote, processName, processId)))
            {
                return false;
            }
            return !_rules.Any(r => !r.Include && r.Matches(isUdp, local, remote, processName, processId));
        }

        private static bool TryParseRule(string token, out Rule rule)
        {
            rule = null!;
            // The value may be an IPv6 address, so only the first separators split the token.
            var parts = token.Split(':', 3);
            if (parts.Length != 3)
            {
                return false;
            }

            bool? include = parts[0].ToLowerInvariant() switch { "include" => true, "exclude" => false, _ => null };
            Target? target = parts[1].ToLowerInvariant() switch
            {
                "local" => Target.Local,
                "remote" => Target.Remote,
                "both" => Target.Both,
                "process" => Target.Process,
                _ => null,
            };
            if (include == null || target == null)
            {
                return false;
            }

            if (target == Target.Process)
            {
                string name = parts[2];
                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    name = Path.GetFileNameWithoutExtension(name);
                }
                if (name.Length == 0)
                {
                    return false;
                }
                rule = uint.TryParse(name, out var pid)
                    ? new Rule(include.Value, Target.Process, true, true, 0, 0, null, null, pid)
                    : new Rule(include.Value, Target.Process, true, true, 0, 0, null, name, null);
                return true;
            }

            var protocolAndValue = parts[2].Split(':', 2);
            if (protocolAndValue.Length != 2)
            {
                return false;
            }

            (bool tcp, bool udp)? protocol = protocolAndValue[0].ToLowerInvariant() switch
            {
                "tcp" => (true, false),
                "udp" => (false, true),
                "tcpudp" => (true, true),
                _ => null,
            };
            if (protocol == null)
            {
                return false;
            }

            // Check ports first: IPAddress.TryParse would read "80" as 0.0.0.80.
            string value = protocolAndValue[1];
            var ports = value.Split('-');
            if (ports.Length is 1 or 2 && ushort.TryParse(ports[0], out var begin) && ushort.TryParse(ports[^1], out var end) && begin <= end)
            {
                rule = new Rule(include.Value, target.Value, protocol.Value.tcp, protocol.Value.udp, begin, end, null, null, null);
                return true;
            }

            try
            {
                if (IPAddressRange.TryParse(value, out var range))
                {
                    rule = new Rule(include.Value, target.Value, protocol.Value.tcp, protocol.Value.udp, 0, 0, range, null, null);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                // Mixed address families or a reversed range.
            }

            return false;
        }
    }
}
