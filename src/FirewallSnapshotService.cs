using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class FirewallSnapshotService
    {
        private readonly string _snapshotPath;

        public FirewallSnapshotService()
        {
            _snapshotPath = ConfigPathManager.GetConfigPath("known_state.json");
        }

        public HashSet<string> LoadSnapshot()
        {
            try
            {
                if (File.Exists(_snapshotPath))
                {
                    string json = File.ReadAllText(_snapshotPath);
                    return JsonSerializer.Deserialize(json, SnapshotContext.Default.HashSetString)
                           ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void SaveSnapshot(IEnumerable<string> ruleNames)
        {
            try
            {
                var data = new HashSet<string>(ruleNames, StringComparer.OrdinalIgnoreCase);
                string json = JsonSerializer.Serialize(data, SnapshotContext.Default.HashSetString);
                File.WriteAllText(_snapshotPath, json);
            }
            catch
            {
            }
        }

        public void DeleteSnapshot()
        {
            if (File.Exists(_snapshotPath))
            {
                File.Delete(_snapshotPath);
            }
        }

        public bool SnapshotExists()
        {
            return File.Exists(_snapshotPath);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class SnapshotContext : JsonSerializerContext { }
}