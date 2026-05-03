using AutoServiceCatalog.BLL.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceCatalog.Tests.IntegrationTests.Infrastructure
{
    public class FakeTwoLevelCacheService<T> : ITwoLevelCacheService<T>
    {
        public Task<T?> GetOrCreateAsync(
            string key,
            Func<Task<T?>> factory,
            TimeSpan? l1Ttl = null,
            TimeSpan? l2Ttl = null)
        {
            return factory();
        }

        public Task InvalidateAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}
