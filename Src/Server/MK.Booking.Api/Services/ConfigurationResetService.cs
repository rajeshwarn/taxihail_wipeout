#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationResetService : Service
    {
        private readonly ICacheClient _cacheClient;
        private readonly IConfigurationManager _configManager;

        public ConfigurationResetService(ICacheClient cacheClient, IConfigurationManager configManager)
        {
            _cacheClient = cacheClient;
            _configManager = configManager;
        }

        public object Get(ConfigurationResetRequest request)
        {
            _cacheClient.Remove(ReferenceDataService.CacheKey);
            _configManager.Reload();
            return true;
        }
    }
}