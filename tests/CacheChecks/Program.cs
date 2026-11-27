using MinimalFirewall;

var cache = new SlidingCache();
int failures = 0;
void Check(bool condition, string message)
{
    if (!condition) { failures++; Console.Error.WriteLine("FAIL " + message); }
}

cache.Set("a", "1", TimeSpan.FromMinutes(1));
Check(cache.TryGetValue("a", out string? a) && a == "1", "a stored value must be returned");
Check(!cache.TryGetValue("missing", out string? _), "a missing key must not be found");
Check(!cache.TryGetValue("a", out List<int>? _), "a value of another type must not be returned");

cache.Set("short", "x", TimeSpan.FromMilliseconds(50));
Thread.Sleep(150);
Check(!cache.TryGetValue("short", out string? _), "an entry must expire after its sliding window");

cache.Set("slide", "y", TimeSpan.FromMilliseconds(300));
for (int i = 0; i < 4; i++)
{
    Thread.Sleep(150);
    Check(cache.TryGetValue("slide", out string? _), "reading an entry must extend its lifetime");
}

cache.Remove("a");
Check(!cache.TryGetValue("a", out string? _), "a removed entry must not be found");

Console.WriteLine(failures == 0 ? "Cache checks passed." : $"{failures} cache check(s) failed.");
return failures == 0 ? 0 : 1;
