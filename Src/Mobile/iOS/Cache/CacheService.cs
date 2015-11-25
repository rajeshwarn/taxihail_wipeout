using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using Foundation;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
    public class CacheService : ICacheService
    {
        private readonly string _cacheKey;

        public CacheService(string cacheKey = null)
        {
            _cacheKey = cacheKey ?? "MK.Booking.Cache";
        }

        #region ICacheService implementation
        public T Get<T>(string key) where T : class
        {
            var serialized = NSUserDefaults.StandardUserDefaults.StringForKey(_cacheKey + key);
           
            //Console.WriteLine( "-----------------------------ICacheService " + key + " : " + serialized );

            var result = default( T );

            if ((serialized.HasValue()) && (serialized.ToLowerInvariant().Contains("expiresat"))) //We check for expires at in case the value was cached prior of expiration.  In a future version we should be able to remove this
            {
                var cacheItem = serialized.FromJson<CacheItem<T>>();
                if ((cacheItem != null) && (cacheItem.ExpiresAt > DateTime.Now))
                {
                    result = cacheItem.Value;
                }
            }
            else if (serialized.HasValue()) //Support for older cached item
            {
                var item = serialized.FromJson<T>();
                if (item != null)
                {
                    result = item;
                    Set(key, item);
                }
            }

            return result;

        }

        public void Set<T>(string key, T obj) where T : class
        {
            Set(key, obj, DateTime.Now.AddYears(5));
        }

        public void Set<T>(string key, T obj, DateTime expiresAt) where T : class
        {
            var item = new CacheItem<T>(obj, expiresAt);
            var serialized = item.ToJson();
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(serialized, _cacheKey + key);               
        }
        public void Clear(string key)
        {
            Console.WriteLine("-----------------------------------  Clear :" + _cacheKey + key + " : " + GetType());
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, _cacheKey + key);               
        }

        private void ClearFullKey(string fullKey)
        {
            Console.WriteLine ( "-----------------------------------  ClearFullKey :" + fullKey + " : " + GetType ());            
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, fullKey);               
        }

        public void ClearAll()
        {
            Console.WriteLine ( "-----------------------------------  ClearAll :" + GetType ());            
            var keys = NSUserDefaults.StandardUserDefaults.ToDictionary ().Keys;
            keys.Where(k => k.ToString().StartsWith(_cacheKey)).ForEach(k => ClearFullKey(k.ToString()));
        }
        #endregion

    }
    public class CacheItem<T> where T : class
    {
        public CacheItem(T value)
        {
            Value = value;
        }

		[JsonConstructor]
        public CacheItem(T value, DateTime expireAt)
        {
            Value = value;
            ExpiresAt = expireAt;
        }
        
        public DateTime ExpiresAt { get; set; }
        public T Value { get; set; }
    }
}

