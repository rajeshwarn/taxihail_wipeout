using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient
	{
	    private readonly ILogger _logger;

        public ConfigurationClientService(string url, string sessionId, IPackageInfo packageInfo, ILogger logger, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
		{
		    _logger = logger;
		}

        public async Task<IDictionary<string, string>> GetSettings(bool shouldThrowExceptionIfError = false)
		{
			try
			{
                return await Client.GetAsync<Dictionary<string, string>>("/encryptedsettings");
			}
			catch (Exception ex)
			{
                if (shouldThrowExceptionIfError)
                {
                    throw;
                }

                _logger.LogError(ex);

                return new Dictionary<string, string>();
			}
		}

		public async Task<ClientPaymentSettings> GetPaymentSettings()
		{
			var paymentSettings = new ClientPaymentSettings();
			try
			{
				var result = await Client.GetAsync<Dictionary<string, string>>("/encryptedsettings/payments");

				SettingsEncryptor.SwitchEncryptionStringsDictionary(paymentSettings.GetType(), null, result, false);
				SettingsLoader.InitializeDataObjects(paymentSettings, result, _logger);
			}
			catch (Exception ex)
			{
                _logger.LogError(ex);
			}	
			return paymentSettings;
		}
	}
}