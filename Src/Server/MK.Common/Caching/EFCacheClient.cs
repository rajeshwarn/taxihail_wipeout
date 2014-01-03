#region

using System;
using System.Collections.Generic;
using ServiceStack.CacheAccess;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Common.Caching
{
    public class EfCacheClient : ICacheClient
    {
        private readonly Func<CachingDbContext> _contextFactory;

        public EfCacheClient(Func<CachingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Dispose()
        {
        }

        public bool Remove(string key)
        {
            using (var context = _contextFactory.Invoke())
            {
                var item = context.Find(key);
                if (context.Find(key) == null) return false;
                context.Set<CacheItem>().Remove(item);
                context.SaveChanges();
                return true;
            }
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        public T Get<T>(string key)
        {
            using (var context = _contextFactory.Invoke())
            {
                var item = context.Find(key);
                if (item == null) return default(T);
                if (item.ExpiresAt < DateTime.Now)
                {
                    context.Set<CacheItem>().Remove(item);
                    context.SaveChanges();
                    return default(T);
                }
                return JsonSerializer.DeserializeFromString<T>(item.Value);
            }
        }

        public long Increment(string key, uint amount)
        {
            throw new NotImplementedException();
        }

        public long Decrement(string key, uint amount)
        {
            throw new NotImplementedException();
        }

        public bool Add<T>(string key, T value)
        {
            return CacheAdd(key, value);
        }

        public bool Set<T>(string key, T value)
        {
            return CacheSet(key, value);
        }

        public bool Replace<T>(string key, T value)
        {
            return CacheReplace(key, value);
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return CacheAdd(key, value, expiresAt);
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            return CacheSet(key, value, expiresAt);
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return CacheReplace(key, value, expiresAt);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheAdd(key, value, DateTime.Now.Add(expiresIn));
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheSet(key, value, DateTime.Now.Add(expiresIn));
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheReplace(key, value, DateTime.Now.Add(expiresIn));
        }

        public void FlushAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                foreach (var item in context.Set<CacheItem>())
                {
                    context.Set<CacheItem>().Remove(item);
                }
                context.SaveChanges();
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            var valueMap = new Dictionary<string, T>();
            foreach (var key in keys)
            {
                var value = Get<T>(key);
                valueMap[key] = value;
            }
            return valueMap;
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            foreach (var entry in values)
            {
                Set(entry.Key, entry.Value);
            }
        }

        private bool CacheAdd<T>(string key, T value)
        {
            return CacheAdd(key, value, DateTime.MaxValue);
        }

        private bool CacheAdd<T>(string key, T value, DateTime expiresAt)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (context.Find(key) != null) return false;
                var item = new CacheItem(key, JsonSerializer.SerializeToString(value), expiresAt);
                context.Set<CacheItem>().Add(item);
                context.SaveChanges();
            }
            return true;
        }

        private bool CacheReplace<T>(string key, T value)
        {
            return CacheReplace(key, value, DateTime.MaxValue);
        }

        private bool CacheReplace<T>(string key, T value, DateTime expiresAt)
        {
            return !CacheSet(key, value, expiresAt);
        }

        private bool CacheSet<T>(string key, T value)
        {
            return CacheSet(key, value, DateTime.MaxValue);
        }

        private bool CacheSet<T>(string key, T value, DateTime expiresAt)
        {
            using (var context = _contextFactory.Invoke())
            {
                var item = context.Find(key);
                if (item == null)
                {
                    item = new CacheItem(key, JsonSerializer.SerializeToString(value), expiresAt);
                    context.Set<CacheItem>().Add(item);
                    context.SaveChanges();
                    return true;
                }
                item.Value = JsonSerializer.SerializeToString(value);
                item.ExpiresAt = expiresAt;
                context.SaveChanges();
                return true;
            }
        }
    }
}