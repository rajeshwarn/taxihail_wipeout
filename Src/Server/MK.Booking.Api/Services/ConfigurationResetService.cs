using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationResetService : RestServiceBase<ConfigurationResetRequest>
    {
        private IConfigurationManager _configManager;
        private ICacheClient _cacheClient;
        public ConfigurationResetService(IConfigurationManager configManager, ICacheClient cacheClient)
        {
            _cacheClient = cacheClient;
            _configManager = configManager;
        }

        public override object OnGet(ConfigurationResetRequest request)
        {
            _cacheClient.Remove(ReferenceDataService.CacheKey);
            _configManager.Reset();
            return true;
        }
    }
}
