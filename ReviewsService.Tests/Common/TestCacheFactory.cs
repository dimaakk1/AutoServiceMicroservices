using Application.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ReviewsService.Tests.Common;

internal static class TestCacheFactory
{
    public static TwoLevelCacheService<T> Create<T>() =>
        new(
            new MemoryCache(new MemoryCacheOptions()),
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            NullLogger<TwoLevelCacheService<T>>.Instance);
}
