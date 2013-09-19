using System;
using Android.App;
using Android.Content;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
    public class CacheService : ICacheService
    {
        private const string _cacheKey = "MK.Booking.Cache";

        protected virtual string CacheKey
        {
            get
            {
                return _cacheKey;
            }
        }



        public CacheService()
        {
            
        }



        public T Get<T>(string key) where T : class
        {

            var pref = Application.Context.GetSharedPreferences(CacheKey, FileCreationMode.Private);
            var serialized = pref.GetString(key, null);

            if ((serialized.HasValue()) && (serialized.ToLower().Contains("expiresat"))) //We check for expires at in case the value was cached prior of expiration.  In a future version we should be able to remove this
            {
                var cacheItem = JsonSerializer.DeserializeFromString<CacheItem<T>>(serialized);
                if (cacheItem != null && cacheItem.ExpiresAt > DateTime.Now)
                {
                    return cacheItem.Value;
                }
            }
            else if (serialized.HasValue()) //Support for older cached item
            {
                var item = JsonSerializer.DeserializeFromString<T>(serialized);
                if (item != null)
                {
                    Set(key, item);
                    return item;
                }
            }

       

            return default(T);
        }

        public void Set<T>(string key, T obj, DateTime expiresAt) where T : class
        {
            var item = new CacheItem<T>(obj, expiresAt);
            var serialized = JsonSerializer.SerializeToString(item);
            var pref = Application.Context.GetSharedPreferences(CacheKey, FileCreationMode.Private);
            pref.Edit().PutString( key, serialized ).Commit();
        }




        public void Set<T>(string key, T obj) where T : class
        {
            Set(key, obj, DateTime.MaxValue);
        }

        //public void Set<T>(string key, T obj)
        //{
        //    var serialized = JsonSerializer.SerializeToString(obj);
        //    
               
        //}

        public void Clear(string key)
        {
            var pref = Application.Context.GetSharedPreferences(CacheKey, FileCreationMode.Private);
            var serialized = pref.GetString( key , null );
            if (serialized.HasValue())
            {
                pref.Edit().Remove(key).Commit();
            }
        }


        public void ClearAll()
        {
            var pref = Application.Context.GetSharedPreferences(CacheKey, FileCreationMode.Private);
            pref.Edit().Clear().Commit();
        }

    }
}