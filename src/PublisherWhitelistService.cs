using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinimalFirewall
{
    public class PublisherWhitelistService
    {
        private readonly string _configPath;
        private HashSet<string> _trustedPublishers;
        private readonly object _lock = new object();

        public PublisherWhitelistService()
        {
            _configPath = ConfigPathManager.GetConfigPath("trusted_publishers.json");
            _trustedPublishers = Load();
        }

        private HashSet<string> Load()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_configPath))
                    {
                        string json = File.ReadAllText(_configPath);
                        if (string.IsNullOrWhiteSpace(json))
                            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        return JsonSerializer.Deserialize(json, WhitelistJsonContext.Default.HashSetString)
                               ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to load publisher whitelist: {ex.Message}");
                }
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Save()
        {
            lock (_lock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_trustedPublishers, WhitelistJsonContext.Default.HashSetString);

                    string tempPath = _configPath + ".tmp";
                    File.WriteAllText(tempPath, json);

                    File.Move(tempPath, _configPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] Failed to save publisher whitelist: {ex.Message}");
                }
            }
        }

        public List<string> GetTrustedPublishers()
        {
            lock (_lock)
            {
                return _trustedPublishers.OrderBy(p => p).ToList();
            }
        }

        public bool IsTrusted(string publisherName)
        {
            if (string.IsNullOrEmpty(publisherName)) return false;

            lock (_lock)
            {
                return _trustedPublishers.Contains(publisherName);
            }
        }

        public void Add(string publisherName)
        {
            if (string.IsNullOrEmpty(publisherName)) return;

            lock (_lock)
            {
                if (_trustedPublishers.Add(publisherName))
                {
                    Save();
                }
            }
        }

        public void Remove(string publisherName)
        {
            lock (_lock)
            {
                if (_trustedPublishers.Remove(publisherName))
                {
                    Save();
                }
            }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class WhitelistJsonContext : JsonSerializerContext { }
}