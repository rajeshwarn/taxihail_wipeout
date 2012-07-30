using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface ICacheService
    {    
        T Get<T>(string key);

        void Set<T>(string key, T obj);

        void Clear(string key);

        void ClearAll();
	
    }
}