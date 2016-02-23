using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Configuration.Impl
{
	public class ServerSettings : IServerSettings, IAppSettings
    {
        private const int CacheExpiration = 30;  // In seconds
        private const string CacheKey = "MK.Settings";

        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly ILogger _logger;
        private readonly ObjectCache _cache = MemoryCache.Default;

        public ServerSettings(Func<ConfigurationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;

            ServerData = new ServerTaxiHailSetting();
            Load();
        }
        
        public IDictionary<string, string> GetSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AppSetting>().ToArray().ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public ServerPaymentSettings GetPaymentSettings(string companyKey = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var paymentSettings = companyKey.HasValue()
                    ? context.Set<ServerPaymentSettings>().FirstOrDefault(p => p.CompanyKey == companyKey)
                    : context.Set<ServerPaymentSettings>().Find(AppConstants.CompanyId);

                return paymentSettings ?? new ServerPaymentSettings();
            }
        }

        public void Reload()
        {
            ServerData = new ServerTaxiHailSetting();
            Load();
        }

        public TaxiHailSetting Data
        {
            get
            {
                var serverData = _cache[CacheKey];
                if (serverData == null)
                {
                    // Settings expired or not yet cached
                    Reload();

                    _cache.Add(CacheKey, ServerData, DateTime.UtcNow.AddSeconds(CacheExpiration));
                }

                return ServerData;
            }
        }

        public ServerTaxiHailSetting ServerData { get; private set; }

        public Task Load()
        {
            SettingsLoader.InitializeDataObjects(ServerData, GetSettings(), _logger);
            return Task.FromResult(true);
        }

		public string GetServiceUrl()
		{
			throw new NotSupportedException("This method is only supported on mobile.");
		}

        public Task ChangeServerUrl(string serverUrl)
        {
			throw new NotSupportedException("This method is only supported on mobile.");
        }

	    public void SetAppleTestAccountMode(bool isAppleTestAccountUsed)
	    {
		    throw new NotSupportedException("This call is only supported on mobile.");
	    }
    }
}