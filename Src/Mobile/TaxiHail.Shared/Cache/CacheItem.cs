using System;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
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