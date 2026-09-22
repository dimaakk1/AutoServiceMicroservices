using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Cache
{
    public interface ITwoLevelCacheService<T>
    {
        Task<T?> GetOrCreateAsync(
            string key,
            Func<Task<T?>> factory,
            TimeSpan? l1Ttl = null,
            TimeSpan? l2Ttl = null
        );

        Task InvalidateAsync(string key);
    }
}
