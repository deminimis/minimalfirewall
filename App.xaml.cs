using System;
using System.Linq;
using System.Windows;

namespace MinimalFirewall
{
    public partial class App : Application
    {
        public void ApplyTheme(string themeName)
        {
            var themeFileName = (themeName == "Dark") ? "DarkTheme" : "LightTheme";

            var existingTheme = Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));
            if (existingTheme != null)
            {
                Resources.MergedDictionaries.Remove(existingTheme);
            }

            var newTheme = new ResourceDictionary() { Source = new Uri($"Themes/{themeFileName}.xaml", UriKind.Relative) };
            Resources.MergedDictionaries.Add(newTheme);
        }
    }
}