using System.Reflection;
using System.Runtime.CompilerServices;
using MinimalFirewall;

internal static class ShutdownChecks
{
    [STAThread]
    private static void Main()
    {
        // Missing UI and services make any settings access or cleanup fail this check.
        var form = (MainForm)RuntimeHelpers.GetUninitializedObject(typeof(MainForm));
        var handler = typeof(MainForm).GetMethod("MainForm_FormClosing",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (bool cancel in new[] { false, true })
        {
            var args = new FormClosingEventArgs(CloseReason.WindowsShutDown, cancel);
            handler.Invoke(form, new object?[] { null, args });
            if (args.Cancel)
                throw new InvalidOperationException("Windows shutdown must not be canceled.");
        }
        Console.WriteLine("Windows shutdown checks passed.");
    }
}
