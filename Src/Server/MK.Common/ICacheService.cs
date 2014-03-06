using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface ICacheService
    {
        T Get<T>(string key) where T : class;

        void Set<T>(string key, T obj) where T : class;

        void Set<T>(string key, T obj, DateTime expiresAt) where T : class;
		
        void Clear(string key);

        void ClearAll();
	
    }
}