using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class FirewallSnapshotService
    {
        private readonly string _snapshotPath;
        private readonly object _fileLock = new object();

        public FirewallSnapshotService()
        {
            _snapshotPath = ConfigPathManager.GetConfigPath("known_state.json");
        }

        public HashSet<string> LoadSnapshot()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_snapshotPath))
                    {
                        using var stream = new FileStream(_snapshotPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        return JsonSerializer.Deserialize(stream, SnapshotContext.Default.HashSetString)
                               ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load snapshot: {ex.Message}");
                }
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public void SaveSnapshot(IEnumerable<string> ruleNames)
        {
            if (ruleNames == null) return;

            lock (_fileLock)
            {
                try
                {
                    var data = new HashSet<string>(ruleNames, StringComparer.OrdinalIgnoreCase);
                    string json = JsonSerializer.Serialize(data, SnapshotContext.Default.HashSetString);

                    string tempPath = _snapshotPath + ".tmp";
                    File.WriteAllText(tempPath, json);

                    File.Move(tempPath, _snapshotPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to save snapshot: {ex.Message}");
                }
            }
        }

        public void DeleteSnapshot()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_snapshotPath))
                    {
                        File.Delete(_snapshotPath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to delete snapshot: {ex.Message}");
                }
            }
        }

        public bool SnapshotExists()
        {
            lock (_fileLock)
            {
                return File.Exists(_snapshotPath);
            }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class SnapshotContext : JsonSerializerContext { }
}