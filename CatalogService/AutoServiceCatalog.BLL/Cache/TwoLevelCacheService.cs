using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Cache
{
    public class TwoLevelCacheService<T>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TwoLevelCacheService<T>> _logger;

        public TwoLevelCacheService(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<TwoLevelCacheService<T>> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetOrCreateAsync(string key, Func<Task<T?>> factory, TimeSpan? l1Ttl = null, TimeSpan? l2Ttl = null)
        {
            // L1
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogInformation($"L1 cache hit: {key}");
                return value;
            }

            // L2
            var redisValue = await _distributedCache.GetStringAsync(key);
            if (redisValue != null)
            {
                value = JsonSerializer.Deserialize<T>(redisValue);
                _memoryCache.Set(key, value, l1Ttl ?? TimeSpan.FromSeconds(30));
                _logger.LogInformation($"L2 cache hit: {key}");
                return value;
            }

            // DB
            value = await factory();

            if (value != null)
            {
                var serialized = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = l2Ttl ?? TimeSpan.FromMinutes(10)
                });
                _memoryCache.Set(key, value, l1Ttl ?? TimeSpan.FromSeconds(30));
            }

            return value;
        }

        public async Task InvalidateAsync(string key)
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key);
            _logger.LogInformation($"Cache invalidated: {key}");
        }
    }
}
