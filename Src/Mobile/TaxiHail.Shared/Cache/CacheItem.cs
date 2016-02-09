using System;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
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