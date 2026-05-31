using AutoserviceOrders.BLL.Cache;
using AutoserviceOrders.BLL.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AutoserviceOrders.Tests.Common;

internal static class TestCacheFactory
{
    public static TwoLevelCacheService<List<OrderDto>> CreateOrdersCache() =>
        new(
            new MemoryCache(new MemoryCacheOptions()),
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            NullLogger<TwoLevelCacheService<List<OrderDto>>>.Instance);
}
