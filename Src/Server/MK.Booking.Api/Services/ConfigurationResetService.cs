#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationResetService : Service
    {
        private readonly ICacheClient _cacheClient;
        private readonly IServerSettings _serverSettings;

        public ConfigurationResetService(ICacheClient cacheClient, IServerSettings serverSettings)
        {
            _cacheClient = cacheClient;
            _serverSettings = serverSettings;
        }

        public object Get(ConfigurationResetRequest request)
        {
            _cacheClient.RemoveByPattern(string.Format("{0}*", ReferenceDataService.CacheKey));
            _serverSettings.Reload();
            return true;
        }
    }
}