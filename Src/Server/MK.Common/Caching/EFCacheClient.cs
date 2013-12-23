using System;
using System.Collections.Generic;
using ServiceStack.CacheAccess;
using ServiceStack.Text;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Caching
{
    public class EFCacheClient : ICacheClient
    {
        private readonly Func<CachingDbContext> _contextFactory;

        public EFCacheClient(Func<CachingDbContext> contextFactory)
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
            foreach (string key in keys)
            {
                try
                {
                    this.Remove(key);
                }
                catch (Exception ex)
                {
                    //Log.Error(string.Format("Error trying to remove {0} from the cache", key), ex);
                }
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
            return this.CacheAdd(key, value);
        }

        public bool Set<T>(string key, T value)
        {
            return this.CacheSet(key, value);
        }

        public bool Replace<T>(string key, T value)
        {
            return this.CacheReplace(key, value);
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return this.CacheAdd(key, value, expiresAt);
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            return this.CacheSet(key, value, expiresAt);
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return this.CacheReplace(key, value, expiresAt);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return this.CacheAdd(key, value, DateTime.Now.Add(expiresIn));
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            return this.CacheSet(key, value, DateTime.Now.Add(expiresIn));
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return this.CacheReplace(key, value, DateTime.Now.Add(expiresIn));
        }

        public void FlushAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<CacheItem>().ForEach(x => context.Set<CacheItem>().Remove(x));
                context.SaveChanges();
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            var valueMap = new Dictionary<string, T>();
            foreach (string key in keys)
            {
                var value = this.Get<T>(key);
                valueMap[key] = value;
            }
            return valueMap;

        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            foreach (KeyValuePair<string, T> entry in values)
            {
                this.Set<T>(entry.Key, entry.Value);
            }
        }

        private bool CacheAdd<T>(string key, T value)
        {
            return this.CacheAdd(key, value, DateTime.MaxValue);
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
            return this.CacheReplace(key, value, DateTime.MaxValue);
        }

        private bool CacheReplace<T>(string key, T value, DateTime expiresAt)
        {
            return !this.CacheSet(key, value, expiresAt);
        }

        private bool CacheSet<T>(string key, T value)
        {
            return this.CacheSet(key, value, DateTime.MaxValue);
        }

        private bool CacheSet<T>(string key, T value, DateTime expiresAt)
        {
            using (var context = _contextFactory.Invoke())
            {
                var item = context.Find(key);
                if(item == null)
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
