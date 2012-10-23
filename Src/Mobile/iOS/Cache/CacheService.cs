using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class CacheService : ICacheService
    {
        private const string _baseKey = "MK.Booking.Cache";

        public CacheService()
        {
        }
        #region ICacheService implementation
        public T Get<T>(string key)
        {
              
            var serialized = NSUserDefaults.StandardUserDefaults.StringForKey(_baseKey + key);
            if (serialized.HasValue())
            {
                return JsonSerializer.DeserializeFromString<T>(serialized);
            }
            else
            {
                return default(T);
            }
        }

        public void Set<T>(string key, T obj)
        {
            var serialized = JsonSerializer.SerializeToString(obj);            
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(serialized, _baseKey + key);               
        }

        public void Clear(string key)
        {
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, _baseKey + key);               
        }

        private void ClearFullKey(string fullKey)
        {
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, fullKey);               
        }

        public void ClearAll()
        {

            var keys = NSUserDefaults.StandardUserDefaults.AsDictionary ().Keys;
            keys.Where (k => k.ToString ().StartsWith(_baseKey)).ForEach (k => ClearFullKey( k.ToString() ) );
        }
        #endregion

    }
}

