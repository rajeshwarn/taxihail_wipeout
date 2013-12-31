using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using MonoTouch.Foundation;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
    public class AppCacheService : CacheService ,  IAppCacheService
    {
        protected override string CacheKey
        {
            get
            {
                return "MK.Booking.Application.Cache";
            }
        }
    }

    public class CacheService : ICacheService
    {
        protected virtual string CacheKey
        {
            get
            {
                return "MK.Booking.Cache";
            }
        }

        #region ICacheService implementation
        public T Get<T>(string key) where T : class
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;         
            var serialized = NSUserDefaults.StandardUserDefaults.StringForKey(CacheKey + key);
           
            //Console.WriteLine( "-----------------------------ICacheService " + key + " : " + serialized );

            var result = default( T );

            if ((serialized.HasValue()) && (serialized.ToLowerInvariant().Contains("expiresat"))) //We check for expires at in case the value was cached prior of expiration.  In a future version we should be able to remove this
            {
                var cacheItem = JsonSerializer.DeserializeFromString<CacheItem<T>>(serialized);
                if ((cacheItem != null) && (cacheItem.ExpiresAt > DateTime.Now))
                {
                    result = cacheItem.Value;
                }
            }
            else if (serialized.HasValue()) //Support for older cached item
            {
                var item = JsonSerializer.DeserializeFromString<T>(serialized);
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
            JsConfig.DateHandler = JsonDateHandler.ISO8601;    

            var item = new CacheItem<T>(obj, expiresAt);
            var serialized = JsonSerializer.SerializeToString(item);
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(serialized, CacheKey + key);               
        }
        public void Clear(string key)
        {
            Console.WriteLine ( "-----------------------------------  Clear :" + CacheKey + key + " : " + GetType());                        
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, CacheKey + key);               
        }

        private void ClearFullKey(string fullKey)
        {
            Console.WriteLine ( "-----------------------------------  ClearFullKey :" + fullKey + " : " + GetType ());            
            NSUserDefaults.StandardUserDefaults.SetStringOrClear(null, fullKey);               
        }

        public void ClearAll()
        {
            Console.WriteLine ( "-----------------------------------  ClearAll :" + GetType ());            
            var keys = NSUserDefaults.StandardUserDefaults.AsDictionary ().Keys;
            keys.Where (k => k.ToString ().StartsWith(CacheKey)).ForEach (k => ClearFullKey( k.ToString() ) );
        }
        #endregion

    }
    public class CacheItem<T> where T : class
    {
        public CacheItem(T value)
        {
            Value = value;
        }
        public CacheItem(T value, DateTime expireAt)
        {
            Value = value;
            ExpiresAt = expireAt;
        }
        
        public DateTime ExpiresAt { get; set; }
        public T Value { get; set; }
    }
}

