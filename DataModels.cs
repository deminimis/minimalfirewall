using System.IO;

namespace MinimalFirewall
{
    public enum SearchMode
    {
        Name,
        Path
    }

    public class WildcardRule
    {
        public string FolderPath { get; set; }
        public string ExeName { get; set; }
        public string Action { get; set; }
    }

    public class AdvancedRuleViewModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsEnabled { get; set; }
        public string Direction { get; set; }
        public string Ports { get; set; }
        public string Protocol { get; set; }
        public string ServiceName { get; set; }
        public string RemoteAddresses { get; set; }
        public string Profiles { get; set; }
        public string Description { get; set; }
        public string Grouping { get; set; }
    }

    public class FirewallRuleViewModel
    {
        public string Name { get; set; }
        public string ApplicationName { get; set; }
        public string Status { get; set; }
    }

    public class PendingConnectionViewModel
    {
        public string AppPath { get; set; }
        public string FileName { get { return Path.GetFileName(AppPath); } }
        public string Direction { get; set; }
        public string ServiceName { get; set; }
    }

    public class ProgramViewModel
    {
        public string Name { get; set; }
        public string ExePath { get; set; }
    }

    public class ServiceViewModel
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string ExePath { get; set; }
    }

    public class UwpApp
    {
        public string Name { get; set; }
        public string PackageFamilyName { get; set; }
        public string Status { get; set; } = "Undefined";
    }
}