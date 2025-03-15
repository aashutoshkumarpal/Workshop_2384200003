using BusinessLayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System;

namespace BusinessLayer.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public void SetCache(string key, string value, int expirationInMinutes)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationInMinutes)
            };

            _cache.SetString(key, value, options);
            Console.WriteLine($"Cache Set: {key} - {value}"); // Debugging
        }

        public string? GetCache(string key)
        {
            var value = _cache.GetString(key);
            Console.WriteLine($"Cache Get: {key} - {value}"); // Debugging
            return value;
        }

        public void RemoveCache(string key)
        {
            _cache.Remove(key);
            Console.WriteLine($"Cache Removed: {key}"); // Debugging
        }
    }
}
