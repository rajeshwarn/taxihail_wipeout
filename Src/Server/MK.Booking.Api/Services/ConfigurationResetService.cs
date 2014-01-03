#region

using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationResetService : Service
    {
        private readonly ICacheClient _cacheClient;

        public ConfigurationResetService(ICacheClient cacheClient)
        {
            _cacheClient = cacheClient;
        }

        public object Get(ConfigurationResetRequest request)
        {
            _cacheClient.Remove(ReferenceDataService.CacheKey);
            return true;
        }
    }
}