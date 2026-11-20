using System.Net;
using Firewall.Traffic;
using MinimalFirewall;

// Checks for the Live Connections filters. Exits with an exception on the first failure.
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static IPEndPoint Ep(string address, int port) => new(IPAddress.Parse(address), port);

var web = (udp: false, local: Ep("192.168.1.5", 50000), remote: Ep("93.184.216.34", 443), name: "chrome", pid: 1200u);
var dns = (udp: true, local: Ep("0.0.0.0", 53), remote: Ep("0.0.0.0", 0), name: "svchost", pid: 900u);
var v6 = (udp: false, local: Ep("::1", 8080), remote: Ep("::1", 51000), name: "node", pid: 77u);

bool Match(string filter, (bool udp, IPEndPoint local, IPEndPoint remote, string name, uint pid) c) =>
    LiveConnectionFilter.Parse(filter).Matches(c.udp, c.local, c.remote, c.name, c.pid);

Check(LiveConnectionFilter.Parse("").IsEmpty && Match("", web) && Match("", dns), "An empty filter shows everything.");
Check(Match("include:remote:tcp:443", web) && !Match("include:remote:tcp:443", dns), "Include keeps only matching rows.");
Check(!Match("include:remote:udp:443", web), "Protocol must match.");
Check(Match("include:both:tcpudp:440-450", web) && !Match("include:local:tcp:440-450", web), "Port ranges and local/remote/both targets.");
Check(Match("include:remote:tcp:93.184.216.0/24", web) && Match("include:remote:tcp:93.184.216.1-93.184.216.40", web), "CIDR and IP ranges.");
Check(!Match("include:remote:tcp:10.0.0.0/8", web), "Address outside the range is hidden.");
Check(Match("include:both:tcp:::1", v6) && !Match("include:both:tcp:127.0.0.1", v6), "IPv6 values keep their colons; families never mix.");
Check(!Match("exclude:process:svchost.exe", dns) && Match("exclude:process:svchost", web), "Exclude by process name, with or without .exe.");
Check(!Match("exclude:process:1200", web) && Match("include:process:1200", web), "Process filter by PID.");
Check(Match("include:remote:tcp:443;include:process:svchost", dns) && Match("include:remote:tcp:443 include:process:svchost", web), "Includes are combined with OR.");
Check(!Match("include:remote:tcp:443 exclude:process:chrome", web), "Exclude wins over include.");
Check(!Match("INCLUDE:REMOTE:TCP:443", dns) && Match("INCLUDE:REMOTE:TCP:443", web), "Keywords ignore case.");

var bad = LiveConnectionFilter.Parse("include:remote:tcp:443 foo exclude:nowhere:tcp:1 include:remote:icmp:1 include:remote:tcp:2.2.2.2-::1 include:local:tcp:90-80");
Check(bad.InvalidFilters.Count == 5 && bad.InvalidFilters[0] == "foo", "Invalid filters are reported, valid ones kept.");
Check(bad.Matches(web.udp, web.local, web.remote, web.name, web.pid), "Valid filter still applies next to invalid ones.");

Check(LiveConnectionsControl.CsvField("plain") == "plain", "Plain CSV field.");
Check(LiveConnectionsControl.CsvField("a,b \"c\"") == "\"a,b \"\"c\"\"\"", "CSV quoting.");
Check(LiveConnectionsControl.CsvField("=cmd()") == "'=cmd()", "Formula prefix is neutralised.");

var rows = TcpTrafficTracker.GetConnections();
Check(rows.Any(r => r.IsUdp) && rows.Any(r => !r.IsUdp), "Both TCP and UDP endpoints are listed.");
Check(rows.Where(r => r.IsUdp).All(r => r.RemoteEndPoint.Port == 0 && r.LocalEndPoint.Port > 0), "UDP rows have a local port and no remote side.");
Check(!rows.Where(r => r.IsUdp || r.State == 2).Any(TcpTrafficTracker.CanClose), "UDP and listening rows cannot be closed.");

// Closing a loopback connection: 0 when elevated (the row must disappear), 317 without administrator rights.
using (var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0))
{
    listener.Start();
    using var client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
    client.Connect((IPEndPoint)listener.LocalEndpoint);
    using var server = listener.AcceptTcpClient();
    var local = (IPEndPoint)client.Client.LocalEndPoint!;
    var row = TcpTrafficTracker.GetConnections().Single(r => !r.IsUdp && r.LocalEndPoint.Equals(local));
    Check(TcpTrafficTracker.GetCreationTime(row) is { } created && Math.Abs((DateTime.Now - created).TotalMinutes) < 2, "TCP creation time is read from the owner-module table.");
    Check(TcpTrafficTracker.CanClose(row), "An established IPv4 connection can be closed.");
    uint error = TcpTrafficTracker.CloseConnection(row);
    Check(error is 0 or 317, $"Unexpected SetTcpEntry error {error}.");
    if (error == 0)
    {
        Check(!TcpTrafficTracker.GetConnections().Any(r => !r.IsUdp && r.LocalEndPoint.Equals(local) && r.State == 5), "Closed connection is gone.");
    }
    Console.WriteLine($"Close connection: SetTcpEntry returned {error}.");
}

// Properties lookups: creation time for UDP (IPv4 and IPv6), process user, reverse DNS.
foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
{
    using var udp = new System.Net.Sockets.UdpClient(new IPEndPoint(address, 0));
    var local = (IPEndPoint)udp.Client.LocalEndPoint!;
    var row = TcpTrafficTracker.GetConnections().Single(r => r.IsUdp && r.LocalEndPoint.Equals(local));
    Check(TcpTrafficTracker.GetCreationTime(row) is { } created && Math.Abs((DateTime.Now - created).TotalMinutes) < 2, $"UDP creation time for {address}.");
}
using (var listener6 = new System.Net.Sockets.TcpListener(IPAddress.IPv6Loopback, 0))
{
    listener6.Start();
    using var client6 = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetworkV6);
    client6.Connect((IPEndPoint)listener6.LocalEndpoint);
    var local6 = (IPEndPoint)client6.Client.LocalEndPoint!;
    var row6 = TcpTrafficTracker.GetConnections().Single(r => !r.IsUdp && r.LocalEndPoint.Equals(local6));
    Check(TcpTrafficTracker.GetCreationTime(row6) is { } created6 && Math.Abs((DateTime.Now - created6).TotalMinutes) < 2, "TCPv6 creation time.");
}
Check(ConnectionPropertiesForm.GetProcessUser((uint)Environment.ProcessId) == System.Security.Principal.WindowsIdentity.GetCurrent().Name, "Process user of the current process.");
Check(ConnectionPropertiesForm.GetProcessUser(uint.MaxValue - 1) == "Unavailable", "Unknown PID has no user.");
Check(!string.IsNullOrEmpty(ConnectionPropertiesForm.ResolveHostAsync(IPAddress.Loopback).GetAwaiter().GetResult()), "Reverse DNS returns a name or a placeholder.");

// The filter bar must sit above the grid, not under it. WinForms needs an STA thread.
Exception? uiFailure = null;
var ui = new Thread(() =>
{
    try
    {
        Application.EnableVisualStyles();
        using var form = new Form { Width = 1000, Height = 600 };
        var control = new LiveConnectionsControl { Dock = DockStyle.Fill };
        form.Controls.Add(control);
        form.Show();
        Application.DoEvents();
        var grid = control.Controls.OfType<DataGridView>().Single();
        var bar = control.Controls.OfType<TableLayoutPanel>().Single();
        Check(bar.Height > 0 && grid.Top >= bar.Bottom, $"Filter bar ({bar.Bounds}) must be above the grid ({grid.Bounds}).");
    }
    catch (Exception ex)
    {
        uiFailure = ex;
    }
});
ui.SetApartmentState(ApartmentState.STA);
ui.Start();
ui.Join();
if (uiFailure != null) throw uiFailure;

Console.WriteLine("Live connections checks passed.");
