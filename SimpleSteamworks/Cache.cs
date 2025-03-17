using System.Collections.Generic;

namespace SimpleSteamworks
{
    public class Cache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        public TValue Get(TKey key)
        {
            _cache.TryGetValue(key, out TValue value);
            return value;
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        public void Add(TKey key, TValue value)
        {
            _cache[key] = value;
        }

        public void InvalidateCache()
        {
            _cache.Clear();
        }
    }
}
