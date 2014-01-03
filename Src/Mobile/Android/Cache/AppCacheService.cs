using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Cache
{
    public class AppCacheService : CacheService, IAppCacheService
    {
        protected override string CacheKey
        {
            get { return "MK.Booking.Application.Cache"; }
        }
    }
}