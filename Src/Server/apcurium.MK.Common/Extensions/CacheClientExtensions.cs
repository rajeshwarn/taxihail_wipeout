using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Caching;

namespace apcurium.MK.Common.Extensions
{
    public static class CacheClientExtensions
    {
        public static TResult GetOrDefault<TResult>(this ICacheClient cacheClient, string key, TResult defaultValue = default(TResult)) where TResult : class
        {
            return cacheClient.Get<TResult>(key) ?? defaultValue;
        }
    }
}
