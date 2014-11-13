using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Web.Tests
{
    public class DummyCache : ICacheService
    {
        Dictionary<string, object> _cache = new Dictionary<string, object>();
        public T Get<T>(string key) where T : class
        {
            return _cache.ContainsKey(key) ? (T)_cache[key] : default(T);
        }

        public void Set<T>(string key, T obj) where T : class
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }

            _cache.Add(key, obj);
        }

        public void Set<T>(string key, T obj, DateTime expiresAt) where T : class
        {

        }

        public void Clear(string key)
        {
        }

        public void ClearAll()
        {

        }
    }
}
