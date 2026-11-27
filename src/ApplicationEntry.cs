namespace MinimalFirewall;

public sealed class ApplicationEntry
{
    public string AppPath { get; init; } = string.Empty;
    public string Name => Path.GetFileName(AppPath.Replace('\\', '/'));
    public List<AdvancedRuleViewModel> Rules { get; } = [];
    public List<PendingConnectionViewModel> Attempts { get; } = [];
    public DateTime? LatestAttempt => Attempts.Count == 0 ? null : Attempts.Max(a => a.ObservedAt);
    public string RuleSummary => Rules.Count switch
    {
        0 => "No explicit executable rules",
        1 => $"{Rules[0].Status} rule ({(Rules[0].IsEnabled ? "enabled" : "disabled")})",
        _ => Rules.Select(r => r.Status).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1
            ? $"Mixed ({Rules.Count} rules)" : $"Multiple rules ({Rules.Count})"
    };

    public bool Matches(string search, int filter) =>
        (Name.Contains(search, StringComparison.OrdinalIgnoreCase) || AppPath.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
        (filter switch
        {
            1 => Attempts.Count > 0,
            2 => Rules.Any(r => r.Status.Equals("Allow", StringComparison.OrdinalIgnoreCase)),
            3 => Rules.Any(r => r.Status.Equals("Block", StringComparison.OrdinalIgnoreCase)),
            _ => true
        });

    public static List<ApplicationEntry> Build(IEnumerable<AdvancedRuleViewModel> rules, IEnumerable<PendingConnectionViewModel> attempts)
    {
        var entries = new Dictionary<string, ApplicationEntry>(StringComparer.OrdinalIgnoreCase);
        ApplicationEntry Get(string path)
        {
            if (!entries.TryGetValue(path, out var entry))
            {
                entry = new ApplicationEntry { AppPath = path };
                entries.Add(path, entry);
            }
            return entry;
        }

        // Global rules and package identities cannot be attributed to one executable.
        foreach (var rule in rules.Where(r => IsExecutablePath(r.ApplicationName)))
        {
            Get(rule.ApplicationName).Rules.Add(rule);
        }
        foreach (var attempt in attempts.Where(a => IsExecutablePath(a.AppPath)))
        {
            Get(attempt.AppPath).Attempts.Add(attempt);
        }
        return entries.Values.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.AppPath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool IsExecutablePath(string path) =>
        !string.IsNullOrWhiteSpace(path) && !path.Contains('*') && Path.IsPathFullyQualified(path);
}
