using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common;
using apcurium.MK.Common.Services;
using apcurium.MK.Common.Extensions;

#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;

#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient
	{
        private readonly ICryptographyService _cryptographyService;

        public ConfigurationClientService(string url, 
            string sessionId, 
            IPackageInfo packageInfo, 
            IConnectivityService connectivityService, 
            ILogger logger,
            ICryptographyService cryptographyService)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
            _cryptographyService = cryptographyService;
        }

        public async Task<IDictionary<string, string>> GetSettings(bool shouldThrowExceptionIfError = false)
		{
			try
			{
                return await Client.GetAsync<Dictionary<string, string>>("/encryptedsettings", logger: Logger);
			}
			catch (Exception ex)
			{
                Logger.LogError(ex);

                if (shouldThrowExceptionIfError)
                {
                    throw ex;
                }

			    return new Dictionary<string, string>();
			}
		}

		public async Task<ClientPaymentSettings> GetPaymentSettings()
		{
			var paymentSettings = new ClientPaymentSettings();
			try
			{
                var result = await Client.GetAsync<Dictionary<string, string>>("/encryptedsettings/payments", logger: Logger);

                _cryptographyService.SwitchEncryptionStringsDictionary(paymentSettings.GetType(), null, result, false);
				SettingsLoader.InitializeDataObjects(paymentSettings, result, Logger);
			}
			catch (Exception ex)
			{
                Logger.LogError(ex);
			}	
			return paymentSettings;
		}
	}
}