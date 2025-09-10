// PublisherWhitelistService.cs
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

        public PublisherWhitelistService()
        {
            _configPath = Path.Combine(AppContext.BaseDirectory, "trusted_publishers.json");
            _trustedPublishers = Load();
        }

        private HashSet<string> Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize(json, WhitelistJsonContext.Default.HashSetString) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to load publisher whitelist: {ex.Message}");
            }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_trustedPublishers, WhitelistJsonContext.Default.HashSetString);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to save publisher whitelist: {ex.Message}");
            }
        }

        public List<string> GetTrustedPublishers()
        {
            return _trustedPublishers.OrderBy(p => p).ToList();
        }

        public bool IsTrusted(string publisherName)
        {
            return !string.IsNullOrEmpty(publisherName) && _trustedPublishers.Contains(publisherName);
        }

        public void Add(string publisherName)
        {
            if (!string.IsNullOrEmpty(publisherName) && _trustedPublishers.Add(publisherName))
            {
                Save();
            }
        }

        public void Remove(string publisherName)
        {
            if (_trustedPublishers.Remove(publisherName))
            {
                Save();
            }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HashSet<string>))]
    internal partial class WhitelistJsonContext : JsonSerializerContext { }
}