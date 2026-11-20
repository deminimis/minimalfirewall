using System.Diagnostics;
using System.Text;

namespace MinimalFirewall
{
    // Acts as the OpenSnitch UI for the opensnitchd daemon running inside the default WSL distro.
    // opensnitchd (1.5.x protocol) dials a gRPC server on unix:///tmp/osui.sock. A small Python
    // script (needs python3-grpcio) serves that socket inside WSL and relays the raw protobuf
    // bytes over wsl.exe stdio, so nothing listens on a Windows port.
    public sealed class WslOpenSnitchBridge : IDisposable
    {
        private const string BridgeScript = """
import base64, os, queue, sys, threading
try:
    import grpc
    from concurrent import futures
except ImportError:
    print('ERROR python3-grpcio is not installed (sudo apt install python3-grpcio)', flush=True)
    sys.exit(1)
if not os.path.exists('/usr/bin/opensnitchd'):
    print('ERROR opensnitchd is not installed (sudo apt install opensnitch)', flush=True)
    sys.exit(1)
if os.system('systemctl is-active --quiet opensnitch') != 0:
    print('ERROR the opensnitch service is not running (needs systemd=true in /etc/wsl.conf, then sudo systemctl enable --now opensnitch)', flush=True)
    sys.exit(1)
SOCK = '/tmp/osui.sock'
lock = threading.Lock()
waiting = {}
seq = [0]

def out(line):
    with lock:
        sys.stdout.write(line + '\n')
        sys.stdout.flush()

def ping(req, ctx):
    # PingReply only carries the request id (field 1), which leads PingRequest.
    if req[:1] != b'\x08':
        return b''
    i = 1
    while req[i] & 0x80:
        i += 1
    return req[:i + 1]

def ask(req, ctx):
    ev = threading.Event()
    with lock:
        seq[0] += 1
        rid = seq[0]
        waiting[rid] = [ev, None]
    out('ASK %d %s' % (rid, base64.b64encode(req).decode()))
    ev.wait(110)
    with lock:
        rule = waiting.pop(rid)[1]
    if rule is None:
        ctx.abort(grpc.StatusCode.DEADLINE_EXCEEDED, 'no decision')
    return rule

def subscribe(req, ctx):
    # The daemon's ClientConfig lists every rule it holds.
    out('RULES ' + base64.b64encode(req).decode())
    return req

streams = []

def notifications(requests, ctx):
    # Rule changes from Minimal Firewall are pushed to the daemon on this stream.
    q = queue.Queue()
    with lock:
        streams.append(q)
    def drain():
        for _ in requests:
            pass
        q.put(None)
    threading.Thread(target=drain, daemon=True).start()
    try:
        while True:
            item = q.get()
            if item is None:
                return
            yield item
    finally:
        with lock:
            streams.remove(q)

def reader():
    for line in sys.stdin:
        parts = line.split()
        if len(parts) == 3 and parts[0] == 'REPLY':
            with lock:
                entry = waiting.get(int(parts[1]))
                if entry:
                    entry[1] = base64.b64decode(parts[2])
                    entry[0].set()
        elif len(parts) == 2 and parts[0] == 'NOTIFY':
            with lock:
                for q in streams:
                    q.put(base64.b64decode(parts[1]))
    os._exit(0)

handlers = grpc.method_handlers_generic_handler('protocol.UI', {
    'Ping': grpc.unary_unary_rpc_method_handler(ping),
    'AskRule': grpc.unary_unary_rpc_method_handler(ask),
    'Subscribe': grpc.unary_unary_rpc_method_handler(subscribe),
    'Notifications': grpc.stream_stream_rpc_method_handler(notifications),
})
server = grpc.server(futures.ThreadPoolExecutor(max_workers=16), handlers=(handlers,))
if os.path.exists(SOCK):
    os.unlink(SOCK)
server.add_insecure_port('unix://' + SOCK)
out('HELLO ' + os.environ.get('WSL_DISTRO_NAME', 'WSL'))
server.start()
reader()
""";

        // The daemon gives up on a question after 120 s; older entries are stale.
        private static readonly TimeSpan AskLifetime = TimeSpan.FromSeconds(115);

        private readonly Action<string> _log;
        private readonly Lock _lock = new();
        private readonly Dictionary<string, (List<long> Ids, DateTime Created)> _waiting = [];
        private Process? _process;
        private string _distro = "WSL";

        // Persistent daemon rules by name, shown in the Rules tab.
        private Dictionary<string, WslRule> _rules = [];
        private ulong _notificationId;

        public event Action<PendingConnectionViewModel>? ConnectionAsked;

        // Raised (on any thread) when the persistent rule list changed.
        public event Action? RulesChanged;

        public string Distro => _distro;

        public string ToWindowsPath(string linuxPath) => $@"\\wsl.localhost\{_distro}{linuxPath.Replace('/', '\\')}";

        public List<WslRule> GetRules()
        {
            lock (_lock) return [.. _rules.Values];
        }

        public void SetAction(string ruleName, bool allow)
        {
            WslRule? rule;
            lock (_lock)
            {
                if (!_rules.TryGetValue(ruleName, out rule)) return;
                rule = rule with { Action = allow ? "allow" : "deny" };
                _rules[ruleName] = rule;
            }
            Notify(OpenSnitchProtocol.ChangeRuleAction, rule);
        }

        public void Delete(string ruleName)
        {
            WslRule? rule;
            lock (_lock)
            {
                if (!_rules.Remove(ruleName, out rule)) return;
            }
            Notify(OpenSnitchProtocol.DeleteRuleAction, rule);
        }

        private void Notify(int action, WslRule rule)
        {
            _log($"[WSL] {(action == OpenSnitchProtocol.DeleteRuleAction ? "delete" : "change")} rule {rule.Name}");
            byte[] message = OpenSnitchProtocol.WriteNotification(Interlocked.Increment(ref _notificationId), action, rule);
            Send($"NOTIFY {Convert.ToBase64String(message)}");
            RulesChanged?.Invoke();
        }

        private void Send(params string[] lines)
        {
            try
            {
                var input = _process?.StandardInput;
                if (input == null) return;
                lock (_lock)
                {
                    foreach (string line in lines) input.WriteLine(line);
                    input.Flush();
                }
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException)
            {
                _log($"[WSL] Could not send to the bridge: {ex.Message}");
            }
        }

        // Raised with a user-facing message when a prerequisite is missing.
        public event Action<string>? Failed;

        public WslOpenSnitchBridge(Action<string> log)
        {
            _log = log;
        }

        public void Start()
        {
            if (_process != null) return;

            string code = Convert.ToBase64String(Encoding.UTF8.GetBytes(BridgeScript));
            var psi = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "wsl.exe"))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var arg in new[] { "-e", "python3", "-u", "-c", $"import base64;exec(base64.b64decode('{code}'))" })
            {
                psi.ArgumentList.Add(arg);
            }

            try
            {
                _process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                _log($"[WSL] Could not start wsl.exe: {ex.Message}");
                Failed?.Invoke($"Could not start wsl.exe: {ex.Message}");
                return;
            }
            if (_process == null) return;

            var process = _process;
            process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) _log($"[WSL] {e.Data}"); };
            process.BeginErrorReadLine();
            Task.Run(() => ReadLoop(process));
        }

        public void Stop()
        {
            var process = _process;
            _process = null;
            if (process == null) return;

            try
            {
                // Closing stdin makes the script exit; the daemon then applies its DefaultAction.
                process.StandardInput.Close();
                if (!process.WaitForExit(2000)) process.Kill(true);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
            }
            process.Dispose();
            lock (_lock)
            {
                _waiting.Clear();
                _rules = [];
            }
            RulesChanged?.Invoke();
        }

        public void Dispose() => Stop();

        private void ReadLoop(Process process)
        {
            bool started = false, reported = false;
            try
            {
                string? line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    var parts = line.Split(' ', 3);
                    if (parts[0] == "HELLO" && parts.Length > 1)
                    {
                        started = true;
                        _distro = parts[1];
                        _log($"[WSL] OpenSnitch bridge listening in {_distro}.");
                    }
                    else if (parts[0] == "ASK" && parts.Length == 3 && long.TryParse(parts[1], out long id))
                    {
                        OnAsk(id, Convert.FromBase64String(parts[2]));
                    }
                    else if (parts[0] == "RULES" && parts.Length == 2)
                    {
                        // Temporary rules expire inside the daemon without notice, so only persistent ones are listed.
                        var rules = OpenSnitchProtocol.ReadClientConfigRules(Convert.FromBase64String(parts[1]))
                            .Where(r => r.Duration == "always")
                            .ToDictionary(r => r.Name);
                        lock (_lock) _rules = rules;
                        _log($"[WSL] {rules.Count} OpenSnitch rules loaded.");
                        RulesChanged?.Invoke();
                    }
                    else if (parts[0] == "ERROR")
                    {
                        _log($"[WSL] {line}");
                        reported = true;
                        Failed?.Invoke(line[6..]);
                    }
                    else
                    {
                        _log($"[WSL] {line}");
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException or FormatException)
            {
                _log($"[WSL] Bridge stopped: {ex.Message}");
            }

            // wsl.exe failed before the script ran (no WSL 2 distro, no python3) and the user did not stop it.
            if (!started && !reported && ReferenceEquals(_process, process))
            {
                Failed?.Invoke("wsl.exe exited before the bridge started (needs a default WSL 2 distro with python3)");
            }
        }

        private void OnAsk(long id, byte[] connection)
        {
            var con = OpenSnitchProtocol.ReadConnection(connection);

            // The popup and dashboard keep one entry per path, so repeated questions
            // for the same program wait for that single decision.
            lock (_lock)
            {
                if (_waiting.TryGetValue(con.ProcessPath, out var entry) && DateTime.UtcNow - entry.Created < AskLifetime)
                {
                    entry.Ids.Add(id);
                    return;
                }
                _waiting[con.ProcessPath] = ([id], DateTime.UtcNow);
            }

            var pending = new PendingConnectionViewModel
            {
                AppPath = ToWindowsPath(con.ProcessPath),
                Direction = "Outgoing",
                Protocol = con.Protocol.StartsWith("tcp") ? "6" : con.Protocol.StartsWith("udp") ? "17" : con.Protocol,
                RemoteAddress = con.DstIp,
                RemotePort = con.DstPort.ToString(),
                ProcessId = con.ProcessId.ToString(),
                CommandLine = string.Join(' ', con.ProcessArgs),
                ProcessOwner = $"uid {con.UserId}",
            };
            pending.WslReply = (decision, duration) => Reply(con.ProcessPath, decision, duration);
            ConnectionAsked?.Invoke(pending);
        }

        private void Reply(string processPath, string decision, TimeSpan duration)
        {
            List<long> ids;
            lock (_lock)
            {
                if (!_waiting.Remove(processPath, out var entry)) return;
                ids = entry.Ids;
            }

            var (action, ruleDuration) = OpenSnitchProtocol.MapDecision(decision, duration);
            var rule = new WslRule(OpenSnitchProtocol.RuleName(processPath), true, action, ruleDuration, "simple", "process.path", processPath);
            string encoded = Convert.ToBase64String(OpenSnitchProtocol.WriteRule(rule));
            _log($"[WSL] {action} {ruleDuration} {processPath}");
            Send([.. ids.Select(id => $"REPLY {id} {encoded}")]);

            if (ruleDuration == "always")
            {
                lock (_lock) _rules[rule.Name] = rule;
                RulesChanged?.Invoke();
            }
        }
    }

    // A persistent opensnitch rule as the daemon reports it.
    public sealed record WslRule(string Name, bool Enabled, string Action, string Duration, string OperatorType, string Operand, string Data);

    // Minimal protobuf codec for the opensnitch ui.proto (v1.5) messages the bridge needs.
    public static class OpenSnitchProtocol
    {
        public sealed record Connection(string Protocol, string DstIp, uint DstPort,
            uint UserId, uint ProcessId, string ProcessPath, List<string> ProcessArgs);

        // ui.proto Action values used in Notification.type.
        public const int DeleteRuleAction = 6;
        public const int ChangeRuleAction = 7;

        public static Connection ReadConnection(byte[] data)
        {
            string protocol = "", dstIp = "", path = "";
            uint dstPort = 0, uid = 0, pid = 0;
            var args = new List<string>();
            foreach (var (field, value, bytes) in ReadFields(data))
            {
                switch (field)
                {
                    case 1: protocol = Text(bytes); break;
                    case 4: dstIp = Text(bytes); break;
                    case 6: dstPort = (uint)value; break;
                    case 7: uid = (uint)value; break;
                    case 8: pid = (uint)value; break;
                    case 9: path = Text(bytes); break;
                    case 11: args.Add(Text(bytes)); break;
                }
            }
            return new Connection(protocol, dstIp, dstPort, uid, pid, path, args);
        }

        // Reads the rules (field 7) of the ClientConfig sent by the daemon on Subscribe.
        public static List<WslRule> ReadClientConfigRules(byte[] data) =>
            [.. ReadFields(data).Where(f => f.Field == 7 && f.Bytes != null).Select(f => ReadRule(f.Bytes!))];

        public static WslRule ReadRule(byte[] data)
        {
            string name = "", action = "", duration = "", opType = "", operand = "", opData = "";
            bool enabled = false;
            foreach (var (field, value, bytes) in ReadFields(data))
            {
                switch (field)
                {
                    case 1: name = Text(bytes); break;
                    case 2: enabled = value != 0; break;
                    case 4: action = Text(bytes); break;
                    case 5: duration = Text(bytes); break;
                    case 6:
                        foreach (var (opField, _, opBytes) in ReadFields(bytes ?? []))
                        {
                            if (opField == 1) opType = Text(opBytes);
                            else if (opField == 2) operand = Text(opBytes);
                            else if (opField == 3) opData = Text(opBytes);
                        }
                        break;
                }
            }
            return new WslRule(name, enabled, action, duration, opType, operand, opData);
        }

        // Maps a Minimal Firewall decision to an opensnitch rule action and duration.
        public static (string Action, string Duration) MapDecision(string decision, TimeSpan duration) => decision switch
        {
            "Allow" => ("allow", "always"),
            "Block" => ("deny", "always"),
            "TemporaryAllow" => ("allow", duration > TimeSpan.Zero ? $"{(long)duration.TotalSeconds}s" : "until restart"),
            _ => ("deny", "once"),
        };

        // One rule per program: a later decision replaces the earlier one.
        public static string RuleName(string processPath) =>
            "mfw-" + new string(processPath.ToLowerInvariant()
                .Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');

        public static byte[] WriteRule(WslRule rule)
        {
            var op = new List<byte>();
            WriteString(op, 1, rule.OperatorType);
            WriteString(op, 2, rule.Operand);
            WriteString(op, 3, rule.Data);

            var buffer = new List<byte>();
            WriteString(buffer, 1, rule.Name);
            if (rule.Enabled)
            {
                buffer.Add(2 << 3);
                buffer.Add(1);
            }
            WriteString(buffer, 4, rule.Action);
            WriteString(buffer, 5, rule.Duration);
            WriteBytes(buffer, 6, op);
            return [.. buffer];
        }

        public static byte[] WriteNotification(ulong id, int action, WslRule rule)
        {
            var buffer = new List<byte>();
            WriteVarint(buffer, 1 << 3);
            WriteVarint(buffer, id);
            WriteVarint(buffer, 4 << 3);
            WriteVarint(buffer, (ulong)action);
            WriteBytes(buffer, 6, WriteRule(rule));
            return [.. buffer];
        }

        private static List<(int Field, ulong Value, byte[]? Bytes)> ReadFields(byte[] data)
        {
            var fields = new List<(int, ulong, byte[]?)>();
            int pos = 0;
            while (pos < data.Length)
            {
                ulong tag = ReadVarint(data, ref pos);
                int field = (int)(tag >> 3);
                switch ((int)(tag & 7))
                {
                    case 0:
                        fields.Add((field, ReadVarint(data, ref pos), null));
                        break;
                    case 2:
                        int length = (int)ReadVarint(data, ref pos);
                        fields.Add((field, 0, data[pos..(pos + length)]));
                        pos += length;
                        break;
                    case 1: pos += 8; break;
                    case 5: pos += 4; break;
                    default: throw new FormatException($"Unsupported wire type in field {field}.");
                }
            }
            return fields;
        }

        private static string Text(byte[]? bytes) => bytes == null ? "" : Encoding.UTF8.GetString(bytes);

        private static ulong ReadVarint(byte[] data, ref int pos)
        {
            ulong result = 0;
            for (int shift = 0; ; shift += 7)
            {
                byte b = data[pos++];
                result |= (ulong)(b & 0x7F) << shift;
                if (b < 0x80) return result;
            }
        }

        private static void WriteVarint(List<byte> buffer, ulong value)
        {
            while (value >= 0x80)
            {
                buffer.Add((byte)(value | 0x80));
                value >>= 7;
            }
            buffer.Add((byte)value);
        }

        private static void WriteString(List<byte> buffer, int field, string value) =>
            WriteBytes(buffer, field, Encoding.UTF8.GetBytes(value));

        private static void WriteBytes(List<byte> buffer, int field, IReadOnlyCollection<byte> value)
        {
            WriteVarint(buffer, (ulong)(field << 3 | 2));
            WriteVarint(buffer, (ulong)value.Count);
            buffer.AddRange(value);
        }
    }
}
