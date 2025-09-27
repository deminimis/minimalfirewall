// File: RuleCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace MinimalFirewall
{
    public class RuleCacheService : IDisposable
    {
        private MemoryCache _memoryCache;
        private CancellationTokenSource _cancellationTokenSource;
        private static readonly string DiskCachePath = Path.Combine(AppContext.BaseDirectory, "rules.diskcache");

        private const string ProgramRulesKey = "ProgramRules";
        private const string AdvancedRulesKey = "AdvancedRules";

        public RuleCacheService()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 100_000_000
            });
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public List<UnifiedRuleViewModel> GetProgramRules()
        {
            return _memoryCache.Get<List<UnifiedRuleViewModel>>(ProgramRulesKey) ?? [];
        }

        public List<AdvancedRuleViewModel> GetAdvancedRules()
        {
            return _memoryCache.Get<List<AdvancedRuleViewModel>>(AdvancedRulesKey) ?? [];
        }

        public void UpdateCache(List<UnifiedRuleViewModel>? programRules, List<AdvancedRuleViewModel> advancedRules)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token));
            if (programRules != null)
            {
                _memoryCache.Set(ProgramRulesKey, programRules, cacheOptions);
            }
            _memoryCache.Set(AdvancedRulesKey, advancedRules, cacheOptions);
        }

        public void ClearAllCache()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task PersistCacheToDiskAsync(bool clearMemoryCache = true)
        {
            var programRules = GetProgramRules();
            var advancedRules = GetAdvancedRules();

            if (programRules.Count == 0 && advancedRules.Count == 0)
            {
                return;
            }

            try
            {
                var cacheModel = new RuleCacheModel
                {
                    ProgramRules = JsonSerializer.Serialize(programRules, CacheJsonContext.Default.ListUnifiedRuleViewModel),
                    AdvancedRules = JsonSerializer.Serialize(advancedRules, CacheJsonContext.Default.ListAdvancedRuleViewModel)
                };
                string json = JsonSerializer.Serialize(cacheModel, CacheJsonContext.Default.RuleCacheModel);
                await File.WriteAllTextAsync(DiskCachePath, json);
                if (clearMemoryCache)
                {
                    ClearAllCache();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to persist cache to disk: {ex.Message}");
            }
        }

        public async Task LoadCacheFromDiskAsync()
        {
            if (!File.Exists(DiskCachePath))
            {
                return;
            }

            try
            {
                string json = await File.ReadAllTextAsync(DiskCachePath);
                var cacheModel = JsonSerializer.Deserialize(json, CacheJsonContext.Default.RuleCacheModel);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token));
                if (cacheModel?.ProgramRules != null)
                {
                    var programRules = JsonSerializer.Deserialize(cacheModel.ProgramRules, CacheJsonContext.Default.ListUnifiedRuleViewModel);
                    if (programRules != null) _memoryCache.Set(ProgramRulesKey, programRules, cacheOptions);
                }
                if (cacheModel?.AdvancedRules != null)
                {
                    var advancedRules = JsonSerializer.Deserialize(cacheModel.AdvancedRules, CacheJsonContext.Default.ListAdvancedRuleViewModel);
                    if (advancedRules != null) _memoryCache.Set(AdvancedRulesKey, advancedRules, cacheOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to load cache from disk: {ex.Message}");
                ClearAllCache();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
            _memoryCache.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}