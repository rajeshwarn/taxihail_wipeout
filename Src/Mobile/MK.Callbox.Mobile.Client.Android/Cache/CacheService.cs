using System;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
	public class CacheService : ICacheService
	{
		private readonly string _cacheKey;

		public CacheService(string cacheKey = null)
		{
			_cacheKey = cacheKey ?? "MK.Booking.Cache";
		}

		public T Get<T>(string key) where T : class
		{
			var pref = Application.Context.GetSharedPreferences(_cacheKey, FileCreationMode.Private);
			var serialized = pref.GetString(key, null);

			if ((serialized.HasValue()) && (serialized.ToLower().Contains("expiresat")))
				//We check for expires at in case the value was cached prior of expiration.  In a future version we should be able to remove this
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
			var pref = Application.Context.GetSharedPreferences(_cacheKey, FileCreationMode.Private);
			pref.Edit().PutString(key, serialized).Commit();
		}


		public void Set<T>(string key, T obj) where T : class
		{
			Set(key, obj, DateTime.MaxValue);
		}


		public void Clear(string key)
		{
			var pref = Application.Context.GetSharedPreferences(_cacheKey, FileCreationMode.Private);
			var serialized = pref.GetString(key, null);
			if (serialized.HasValue())
			{
				pref.Edit().Remove(key).Commit();
			}
		}


		public void ClearAll()
		{
			var pref = Application.Context.GetSharedPreferences(_cacheKey, FileCreationMode.Private);
			pref.Edit().Clear().Commit();
		}
	}
}