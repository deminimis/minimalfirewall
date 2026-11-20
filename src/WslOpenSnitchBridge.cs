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
import base64, os, sys, threading
try:
    import grpc
    from concurrent import futures
except ImportError:
    print('ERROR python3-grpcio is not installed (sudo apt install python3-grpcio)', flush=True)
    sys.exit(1)
if not os.path.exists('/usr/bin/opensnitchd'):
    print('ERROR opensnitchd is not installed (sudo apt install opensnitch)', flush=True)
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

def notifications(requests, ctx):
    for _ in requests:
        pass
    return
    yield

def reader():
    for line in sys.stdin:
        parts = line.split()
        if len(parts) == 3 and parts[0] == 'REPLY':
            with lock:
                entry = waiting.get(int(parts[1]))
                if entry:
                    entry[1] = base64.b64decode(parts[2])
                    entry[0].set()
    os._exit(0)

handlers = grpc.method_handlers_generic_handler('protocol.UI', {
    'Ping': grpc.unary_unary_rpc_method_handler(ping),
    'AskRule': grpc.unary_unary_rpc_method_handler(ask),
    'Subscribe': grpc.unary_unary_rpc_method_handler(lambda req, ctx: req),
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

        public event Action<PendingConnectionViewModel>? ConnectionAsked;

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
            lock (_lock) _waiting.Clear();
        }

        public void Dispose() => Stop();

        private void ReadLoop(Process process)
        {
            try
            {
                string? line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    var parts = line.Split(' ', 3);
                    if (parts[0] == "HELLO" && parts.Length > 1)
                    {
                        _distro = parts[1];
                        _log($"[WSL] OpenSnitch bridge listening in {_distro}.");
                    }
                    else if (parts[0] == "ASK" && parts.Length == 3 && long.TryParse(parts[1], out long id))
                    {
                        OnAsk(id, Convert.FromBase64String(parts[2]));
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
                AppPath = $@"\\wsl.localhost\{_distro}{con.ProcessPath.Replace('/', '\\')}",
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
            string rule = Convert.ToBase64String(OpenSnitchProtocol.WriteRule(processPath, action, ruleDuration));
            _log($"[WSL] {action} {ruleDuration} {processPath}");

            try
            {
                var input = _process?.StandardInput;
                if (input == null) return;
                lock (_lock)
                {
                    foreach (long id in ids) input.WriteLine($"REPLY {id} {rule}");
                    input.Flush();
                }
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException)
            {
                _log($"[WSL] Could not send decision: {ex.Message}");
            }
        }
    }

    // Minimal protobuf codec for the two opensnitch ui.proto (v1.5) messages the bridge needs.
    public static class OpenSnitchProtocol
    {
        public sealed record Connection(string Protocol, string DstIp, uint DstPort,
            uint UserId, uint ProcessId, string ProcessPath, List<string> ProcessArgs);

        public static Connection ReadConnection(byte[] data)
        {
            string protocol = "", dstIp = "", path = "";
            uint dstPort = 0, uid = 0, pid = 0;
            var args = new List<string>();
            int pos = 0;
            while (pos < data.Length)
            {
                ulong tag = ReadVarint(data, ref pos);
                int field = (int)(tag >> 3);
                switch ((int)(tag & 7))
                {
                    case 0:
                        uint value = (uint)ReadVarint(data, ref pos);
                        if (field == 6) dstPort = value;
                        else if (field == 7) uid = value;
                        else if (field == 8) pid = value;
                        break;
                    case 2:
                        int length = (int)ReadVarint(data, ref pos);
                        string text = Encoding.UTF8.GetString(data, pos, length);
                        pos += length;
                        if (field == 1) protocol = text;
                        else if (field == 4) dstIp = text;
                        else if (field == 9) path = text;
                        else if (field == 11) args.Add(text);
                        break;
                    case 1: pos += 8; break;
                    case 5: pos += 4; break;
                    default: throw new FormatException($"Unsupported wire type in field {field}.");
                }
            }
            return new Connection(protocol, dstIp, dstPort, uid, pid, path, args);
        }

        // Maps a Minimal Firewall decision to an opensnitch rule action and duration.
        public static (string Action, string Duration) MapDecision(string decision, TimeSpan duration) => decision switch
        {
            "Allow" => ("allow", "always"),
            "Block" => ("deny", "always"),
            "TemporaryAllow" => ("allow", duration > TimeSpan.Zero ? $"{(long)duration.TotalSeconds}s" : "until restart"),
            _ => ("deny", "once"),
        };

        public static byte[] WriteRule(string processPath, string action, string duration)
        {
            var op = new List<byte>();
            WriteString(op, 1, "simple");
            WriteString(op, 2, "process.path");
            WriteString(op, 3, processPath);

            // One rule per program: a later decision replaces the earlier one.
            string name = "mfw-" + new string(processPath.ToLowerInvariant()
                .Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
            var rule = new List<byte>();
            WriteString(rule, 1, name);
            rule.Add(2 << 3); rule.Add(1); // enabled = true
            WriteString(rule, 4, action);
            WriteString(rule, 5, duration);
            WriteBytes(rule, 6, op);
            return [.. rule];
        }

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
