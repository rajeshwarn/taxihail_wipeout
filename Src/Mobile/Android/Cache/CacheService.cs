using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using apcurium.MK.Common.Extensions;

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

                if ((serialized.HasValue()) && (serialized.Contains("ExpiresAt"))) //We check for expires at in case the value was cached prior of expiration.  In a future version we should be able to remove this
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


    public class CacheItem<T> where T : class
    {
        public CacheItem(T value)
        {
            this.Value = value;
        }
        public CacheItem(T value, DateTime expireAt)
        {
            this.Value = value;
            this.ExpiresAt = expireAt;
        }

        public DateTime ExpiresAt { get; set; }
        public T Value { get; set; }
    }

}