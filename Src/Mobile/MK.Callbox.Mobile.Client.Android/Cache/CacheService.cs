using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Callbox.Mobile.Client.Cache
{
    public class CacheService : ICacheService
    {
        private const string _sharedPreferences = "MK.Booking.Cache";

        public CacheService()
        {
            
        }

        

        public T Get<T>(string key)
        {

            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
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
            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            pref.Edit().PutString( key, serialized ).Commit();
               
        }

        public void Clear(string key)
        {
            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            var serialized = pref.GetString( key , null );
            if (serialized.HasValue())
            {
                pref.Edit().Remove(key).Commit();
            }
        }


        public void ClearAll()
        {
            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
            pref.Edit().Clear().Commit();
        }

    }
}