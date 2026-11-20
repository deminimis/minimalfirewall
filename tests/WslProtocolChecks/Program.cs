using System.Text;
using MinimalFirewall;

int failures = 0;
void Check(bool condition, string message)
{
    if (!condition) { failures++; Console.Error.WriteLine("FAIL " + message); }
}

// Protobuf helpers to build the messages opensnitchd sends.
static byte[] Varint(ulong value)
{
    var bytes = new List<byte>();
    while (value >= 0x80) { bytes.Add((byte)(value | 0x80)); value >>= 7; }
    bytes.Add((byte)value);
    return [.. bytes];
}
static byte[] Bytes(int field, byte[] value) => [.. Varint((ulong)(field << 3 | 2)), .. Varint((ulong)value.Length), .. value];
static byte[] Text(int field, string value) =>
    [.. Varint((ulong)(field << 3 | 2)), .. Varint((ulong)Encoding.UTF8.GetByteCount(value)), .. Encoding.UTF8.GetBytes(value)];
static byte[] Number(int field, ulong value) => [.. Varint((ulong)(field << 3)), .. Varint(value)];
static bool Contains(byte[] data, string text) =>
    Encoding.UTF8.GetString(data).Contains(text, StringComparison.Ordinal);

// ui.proto Connection: protocol=1, dst_ip=4, dst_port=6, user_id=7, process_id=8, process_path=9, process_args=11,
// plus fields the bridge ignores (src_ip=2, process_env map=12).
byte[] connection =
[
    .. Text(1, "tcp"), .. Text(2, "172.22.74.152"), .. Text(4, "93.184.215.14"), .. Number(6, 443),
    .. Number(7, 1000), .. Number(8, 300_000), .. Text(9, "/usr/bin/curl"),
    .. Text(11, "curl"), .. Text(11, "https://example.com"), .. Text(12, "\n\x04HOME\x12\x05/root"),
];
var parsed = OpenSnitchProtocol.ReadConnection(connection);
Check(parsed.Protocol == "tcp" && parsed.DstIp == "93.184.215.14" && parsed.DstPort == 443, "a connection's endpoint must be decoded");
Check(parsed.UserId == 1000 && parsed.ProcessId == 300_000 && parsed.ProcessPath == "/usr/bin/curl", "a connection's process must be decoded (multi-byte varints included)");
Check(parsed.ProcessArgs.SequenceEqual(["curl", "https://example.com"]), "repeated process arguments must be kept in order");

Check(OpenSnitchProtocol.MapDecision("Allow", default) == ("allow", "always"), "Allow must save a permanent allow rule");
Check(OpenSnitchProtocol.MapDecision("Block", default) == ("deny", "always"), "Block must save a permanent deny rule");
Check(OpenSnitchProtocol.MapDecision("TemporaryAllow", TimeSpan.FromMinutes(5)) == ("allow", "300s"), "a temporary allow must use an opensnitch duration");
Check(OpenSnitchProtocol.MapDecision("Ignore", default) == ("deny", "once"), "Ignore must deny this connection only");

string curl = OpenSnitchProtocol.RuleName("/usr/bin/curl");
Check(curl.StartsWith("mfw-usr-bin-curl-", StringComparison.Ordinal), "a rule name must stay readable");
Check(curl == OpenSnitchProtocol.RuleName("/usr/bin/curl"), "a rule name must be stable for the same program");
Check(OpenSnitchProtocol.RuleName("/opt/App/run") != OpenSnitchProtocol.RuleName("/opt/app/run"), "programs differing by case must get different rules");
Check(OpenSnitchProtocol.RuleName("/usr/bin/foo_bar") != OpenSnitchProtocol.RuleName("/usr/bin/foo-bar"), "programs differing by punctuation must get different rules");

var written = new WslRule(curl, true, "deny", "always", "simple", "process.path", "/usr/bin/curl");
byte[] rule = OpenSnitchProtocol.WriteRule(written);
Check(OpenSnitchProtocol.ReadRule(rule) == written, "a rule must survive a write and read round trip");
Check(OpenSnitchProtocol.ReadRule(OpenSnitchProtocol.WriteRule(written with { Enabled = false })).Enabled == false, "a disabled rule must stay disabled");

// ui.proto ClientConfig sent on Subscribe: rules are repeated field 7, next to fields the bridge ignores.
var other = written with { Name = "user-rule", Action = "allow", Duration = "until restart" };
byte[] config = [.. Number(1, 42), .. Text(2, "node"), .. Bytes(7, rule), .. Bytes(7, OpenSnitchProtocol.WriteRule(other))];
Check(OpenSnitchProtocol.ReadClientConfigRules(config).SequenceEqual([written, other]), "every rule of a Subscribe message must be read");

// ui.proto Notification: id=1, type=4 (DELETE_RULE=6, CHANGE_RULE=7 in OpenSnitch 1.5), rules=6.
byte[] notification = OpenSnitchProtocol.WriteNotification(300, OpenSnitchProtocol.ChangeRuleAction, written);
Check(notification.AsSpan(0, 5).SequenceEqual(new byte[] { 0x08, 0xAC, 0x02, 0x20, 0x07 }), "a notification must start with its id and the CHANGE_RULE type");
Check(Contains(notification, curl), "a notification must carry its rule");

Console.WriteLine(failures == 0 ? "WSL protocol checks passed." : $"{failures} WSL protocol check(s) failed.");
return failures == 0 ? 0 : 1;
