using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient
	{
        public ConfigurationClientService(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
		{
		}

        public async Task<IDictionary<string, string>> GetSettings(bool shouldThrowExceptionIfError = false)
		{
			try
			{
                return await Client.GetAsync<Dictionary<string, string>>("/settings/encrypted", logger: Logger);
			}
			catch (Exception ex)
			{
                Logger.LogError(ex);

                if (shouldThrowExceptionIfError)
                {
                    throw;
                }

			    return new Dictionary<string, string>();
			}
		}

		public async Task<ClientPaymentSettings> GetPaymentSettings()
		{
			var paymentSettings = new ClientPaymentSettings();
			try
			{
                var result = await Client.GetAsync<Dictionary<string, string>>("/settings/encrypted/payments", logger: Logger);

				SettingsEncryptor.SwitchEncryptionStringsDictionary(paymentSettings.GetType(), null, result, false);
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