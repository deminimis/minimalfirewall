using MinimalFirewall;

int failures = 0;

void Check(string path, string folder, bool expected)
{
    bool actual = WildcardRuleService.IsInFolder(path, folder);
    if (actual != expected)
    {
        failures++;
        Console.Error.WriteLine($"FAIL IsInFolder(\"{path}\", \"{folder}\") = {actual}, expected {expected}");
    }
}

Check(@"C:\Tools\app.exe", @"C:\Tools", true);
Check(@"C:\Tools\app.exe", @"C:\Tools\", true);
Check(@"C:\Tools\sub\app.exe", @"C:\Tools", true);
Check(@"c:\tools\APP.exe", @"C:\Tools", true);
Check(@"C:\app.exe", @"C:\", true);
Check(@"C:\ToolsEvil\app.exe", @"C:\Tools", false);
Check(@"C:\Tools", @"C:\Tools", false);
Check(@"D:\Tools\app.exe", @"C:\Tools", false);
Check(@"C:\Tools\app.exe", "", false);

void Suggest(string exePath, string expected)
{
    string? actual = WildcardRuleService.SuggestFolder(exePath);
    if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
    {
        failures++;
        Console.Error.WriteLine($"FAIL SuggestFolder(\"{exePath}\") = \"{actual}\", expected \"{expected}\"");
    }
}

Suggest(@"D:\Apps\Discord\app-1.0.9015\Discord.exe", @"D:\Apps\Discord");
Suggest(@"D:\Apps\Chrome\Application\120.0.6099.71\chrome.exe", @"D:\Apps\Chrome\Application");
Suggest(@"D:\Apps\Tool\2.1\bin\1.0\tool.exe", @"D:\Apps\Tool\2.1\bin");
Suggest(@"D:\Apps\Steam\steam.exe", @"D:\Apps\Steam");
// A digit alone is not a version: 7-Zip stays, and nothing climbs to a drive root.
Suggest(@"D:\7-Zip\7z.exe", @"D:\7-Zip");
Suggest(@"D:\1.0\app.exe", @"D:\1.0");
// Never suggest a shared system folder such as Program Files.
string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
Suggest(Path.Combine(programFiles, "1.2.3", "app.exe"), Path.Combine(programFiles, "1.2.3"));

Console.WriteLine(failures == 0 ? "Wildcard checks passed." : $"{failures} wildcard check(s) failed.");
return failures == 0 ? 0 : 1;
