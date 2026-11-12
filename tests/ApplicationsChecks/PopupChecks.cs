using DarkModeForms;

internal static class PopupChecks
{
    public static void Run()
    {
        var directory = Path.Combine(Environment.CurrentDirectory, "artifacts", "applications");
        Directory.CreateDirectory(directory);
        foreach (bool dark in new[] { false, true })
        {
            string message = @"Allow C:\Applications\A very long executable identity\browser.exe" + "\r\n\r\n" +
                string.Join("\r\n\r\n", Enumerable.Repeat("All profiles, protocols, ports and addresses. Existing scoped and third-party rules are replaced. " +
                new string('x', 200), 15));
            Exception? failure = null;
            using var timer = new System.Windows.Forms.Timer { Interval = 100 };
            timer.Tick += (_, _) =>
            {
                var dialog = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Text == "Popup regression");
                if (dialog == null) return;
                timer.Stop();
                try
                {
                    using var image = new Bitmap(dialog.Width, dialog.Height);
                    dialog.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    image.Save(Path.Combine(directory, $"popup-{(dark ? "dark" : "light")}.png"));
                    var working = Screen.FromControl(dialog).WorkingArea;
                    Check(working.Contains(dialog.Bounds), "Popup must fit inside the monitor working area.");
                    var controls = Descendants(dialog).ToList();
                    foreach (var button in controls.OfType<Button>().Where(c => c.Visible))
                    {
                        Check(dialog.RectangleToScreen(dialog.ClientRectangle).Contains(button.RectangleToScreen(button.ClientRectangle)),
                            "Popup buttons must remain inside the visible client area.");
                    }
                    var prompt = controls.Single(c => c.Text == message);
                    Check(prompt.Parent!.ClientRectangle.Contains(prompt.Bounds), "Long popup content must scroll instead of extending beyond its container.");
                    Check(dialog.AcceptButton is Button { DialogResult: DialogResult.No }, "Button2 must be the default for a Yes/No confirmation.");
                }
                catch (Exception ex) { failure = ex; }
                finally { dialog.DialogResult = DialogResult.No; dialog.Close(); }
            };
            timer.Start();
            Messenger.MessageBox(message, "Popup regression", MsgIcon.Warning, MessageBoxButtons.YesNo,
                pIsDarkMode: dark, defaultButton: MessageBoxDefaultButton.Button2);
            if (failure != null) throw failure;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
