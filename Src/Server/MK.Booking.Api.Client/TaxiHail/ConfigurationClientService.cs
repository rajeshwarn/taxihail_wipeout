using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Cryptography;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class ConfigurationClientService : BaseServiceClient
	{
	    private readonly ILogger _logger;

	    public ConfigurationClientService(string url, string sessionId, IPackageInfo packageInfo, ILogger logger)
			: base(url, sessionId, packageInfo)
		{
		    _logger = logger;
		}

	    public Task<IDictionary<string, string>> GetSettings()
		{
			var tcs = new TaskCompletionSource<IDictionary<string, string>>();

			try
			{
				var result = Client.GetAsync<Dictionary<string, string>>("/encryptedsettings").Result;
				tcs.TrySetResult(result);
			}
			catch (Exception ex)
			{
                _logger.LogError(ex);
				tcs.TrySetResult(new Dictionary<string, string>());
			}

			return tcs.Task;
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