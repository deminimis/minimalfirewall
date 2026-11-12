using DarkModeForms;
using MinimalFirewall;
using MinimalFirewall.TypedObjects;

internal static class PopupChecks
{
    public static void Run()
    {
        var directory = Path.Combine(Environment.CurrentDirectory, "artifacts", "applications");
        Directory.CreateDirectory(directory);
        foreach (bool dark in new[] { false, true })
        {
            Theme.Colors = Theme.GetSystemColors(dark ? 0 : 1);
            var previousColors = Theme.Colors;
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
                    var prompt = controls.Single(c => c.AccessibleName == "Message" || c.Text == message);
                    Check(prompt.Parent!.ClientRectangle.Contains(prompt.Bounds), "Long popup content must scroll instead of extending beyond its container.");
                    Check(dialog.AcceptButton is Button { DialogResult: DialogResult.No }, "Button2 must be the default for a Yes/No confirmation.");
                }
                catch (Exception ex) { failure = ex; }
                finally { dialog.DialogResult = DialogResult.No; dialog.Close(); }
            };
            timer.Start();
            Messenger.MessageBox(message, "Popup regression", MsgIcon.Warning, MessageBoxButtons.YesNo,
                defaultButton: MessageBoxDefaultButton.Button2);
            if (failure != null) throw failure;
            Check(ReferenceEquals(Theme.Colors, previousColors), "Closing a popup must preserve the application's palette.");
        }
        foreach (bool dark in new[] { false, true })
        foreach (int percent in new[] { 100, 125, 150, 200 })
        {
            var settings = new AppSettings { Theme = dark ? "Dark" : "Light" };
            string warning = "Allow C:\\Applications\\Browser\\browser.exe\r\n\r\n" +
                "Direction: Outbound.\r\nAll network profiles, protocols, ports and addresses; all services using this executable.\r\n\r\n" +
                "Existing rules in this direction, including scoped and third-party rules, will be replaced.\r\n\r\n" +
                "Minimal Firewall general block rules may also be removed in either direction. Other firewall rules may still affect traffic. Continue?";
            using (var confirmation = Messenger.CreateMessageDialog(warning, "Confirm Allow", MsgIcon.Warning,
                MessageBoxButtons.YesNo, dark, MessageBoxDefaultButton.Button2))
            {
                using var font = new Font(confirmation.Font.FontFamily, confirmation.Font.SizeInPoints * percent / 100f);
                confirmation.Font = font;
                confirmation.Show();
                ValidateLayout(confirmation);
                Capture(confirmation, $"confirmation-{(dark ? "dark" : "light")}-font-{percent}.png", directory);
            }
            using (var status = new StatusForm("Scanning firewall rules and application identities, please wait...", settings))
            {
                using var font = new Font(status.Font.FontFamily, status.Font.SizeInPoints * percent / 100f);
                status.Font = font;
                status.Show();
                status.UpdateProgress(100);
                ValidateLayout(status);
                status.Complete("Application rules refreshed successfully.");
                ValidateLayout(status);
                Capture(status, $"status-{(dark ? "dark" : "light")}-font-{percent}.png", directory);
            }
            var rule = new AdvancedRuleViewModel
            {
                Name = "Scoped browser rule", ApplicationName = @"C:\Applications\Browser\browser.exe",
                Direction = Directions.Outgoing, Status = "Allow", Protocol = 6, ProtocolName = "TCP", IsEnabled = true,
                Profiles = "Private", InterfaceTypes = "All", LocalPorts = "*", RemotePorts = "443", RemoteAddresses = "192.0.2.0/24", LocalAddresses = "*"
            };
            // The editor reads groups; no action service is called unless a user saves a rule.
            using (var editor = new CreateAdvancedRuleForm(null!, rule, settings))
            {
                using var font = new Font(editor.Font.FontFamily, editor.Font.SizeInPoints * percent / 100f);
                editor.Font = font;
                editor.Show();
                ValidateLayout(editor);
                editor.MinimumSize = new Size(480, 320);
                editor.ClientSize = new Size(600, 480);
                ValidateLayout(editor);
                var protocol = Descendants(editor).OfType<ComboBox>().Single(c => c.Name == "protocolComboBox");
                foreach (var option in protocol.Items.Cast<MinimalFirewall.ProtocolTypes>().Where(p => p.Value is 1 or 6 or 256).ToArray())
                {
                    protocol.SelectedItem = option;
                    ValidateLayout(editor);
                }
                Capture(editor, $"editor-{(dark ? "dark" : "light")}-font-{percent}.png", directory);
            }
            using (var notification = new NotifierForm(new PendingConnectionViewModel
            {
                AppPath = @"C:\Applications\Browser\browser.exe", Direction = "Outbound", ServiceName = "Example service",
                RemoteAddress = "192.0.2.1", RemotePort = "443"
            }, dark))
            {
                using var font = new Font(notification.Font.FontFamily, notification.Font.SizeInPoints * percent / 100f);
                notification.Font = font;
                notification.Show();
                ValidateLayout(notification);
                var publisher = Descendants(notification).OfType<CheckBox>().Single(c => c.Name == "trustPublisherCheckBox");
                publisher.Text = "Trust: Example Publisher";
                publisher.Visible = true;
                notification.ClientSize = new Size(450, 330);
                ValidateLayout(notification);
                Capture(notification, $"notification-{(dark ? "dark" : "light")}-font-{percent}.png", directory);
            }
        }
        Console.WriteLine("Popup bounds, layout, large-font and default-button checks passed.");
    }

    private static void Capture(Form form, string filename, string directory)
    {
        using var image = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
        image.Save(Path.Combine(directory, filename));
    }

    private static void ValidateLayout(Form form)
    {
        form.PerformLayout();
        Application.DoEvents();
        Capture(form, $"layout-{form.GetType().Name}-{form.Font.SizeInPoints:0.00}.png",
            Path.Combine(Environment.CurrentDirectory, "artifacts", "applications"));
        Check(Screen.FromControl(form).WorkingArea.Contains(form.Bounds), $"{form.Text}: dialog must remain inside the working area.");
        foreach (var parent in Descendants(form).Prepend(form))
        {
            var children = parent.Controls.Cast<Control>().Where(c => c.Visible).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (parent is not ScrollableControl { AutoScroll: true })
                    Check(parent.ClientRectangle.Contains(child.Bounds), $"{form.Text}: {child.Name} ({child.GetType().Name}) {child.Bounds} exceeds {parent.GetType().Name} {parent.ClientRectangle}.");
                for (int j = i + 1; j < children.Count; j++)
                    Check(!child.Bounds.IntersectsWith(children[j].Bounds), $"{form.Text}: {child.Name} overlaps {children[j].Name}.");
                if (child is Label label && label.Text.Length > 0)
                    Check(label.Height >= label.GetPreferredSize(new Size(label.Width, 0)).Height,
                        $"{form.Text}: label '{label.Text}' is clipped.");
            }
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
