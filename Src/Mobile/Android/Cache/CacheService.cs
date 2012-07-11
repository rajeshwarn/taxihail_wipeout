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
    public class CacheService : ICacheService
    {
        private const string _sharedPreferences = "MK.Booking.Cache";

        public CacheService(TaxiMobileApplication app)
        {
            App = app;
        }

        public TaxiMobileApplication App { get; set; }

        public T Get<T>(string key)
        {
            
            var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            var serialized = pref.GetString( key , null );
            if ( serialized.HasValue() )
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
            var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            pref.Edit().PutString( key, serialized ).Commit();
               
        }

        public void Clear(string key)
        {
            var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            var serialized = pref.GetString( key , null );
            if (serialized.HasValue())
            {
                pref.Edit().Remove(key).Commit();
            }
        }


        public void ClearAll()
        {
            var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            pref.Edit().Clear().Commit();
        }

    }
}