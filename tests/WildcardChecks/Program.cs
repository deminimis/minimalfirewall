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

Console.WriteLine(failures == 0 ? "Wildcard checks passed." : $"{failures} wildcard check(s) failed.");
return failures == 0 ? 0 : 1;
