using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CacheChecks")]

namespace MinimalFirewall
{
    // Minimal sliding-expiration cache. Replaces Microsoft.Extensions.Caching.Memory, which pulled
    // six assemblies (about 330 KB) into the single-file exe for a handful of keys.
    internal sealed class SlidingCache
    {
        private sealed class Entry(object value, TimeSpan sliding)
        {
            public readonly object Value = value;
            public readonly long SlidingMs = (long)sliding.TotalMilliseconds;
            public long LastAccessMs = Environment.TickCount64;

            public bool IsExpired(long nowMs) => nowMs - Interlocked.Read(ref LastAccessMs) > SlidingMs;
        }

        private readonly ConcurrentDictionary<string, Entry> _entries = new();

        public bool TryGetValue<T>(string key, out T? value) where T : class
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                long now = Environment.TickCount64;
                if (!entry.IsExpired(now) && entry.Value is T typed)
                {
                    Interlocked.Exchange(ref entry.LastAccessMs, now);
                    value = typed;
                    return true;
                }
                _entries.TryRemove(key, out _);
            }

            value = null;
            return false;
        }

        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            // Drop stale entries on write so per-process keys cannot accumulate forever.
            long now = Environment.TickCount64;
            foreach (var pair in _entries)
            {
                if (pair.Value.IsExpired(now))
                {
                    _entries.TryRemove(pair.Key, out _);
                }
            }
            _entries[key] = new Entry(value, slidingExpiration);
        }

        public void Remove(string key) => _entries.TryRemove(key, out _);
    }
}
