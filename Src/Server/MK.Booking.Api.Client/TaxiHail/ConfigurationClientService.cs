using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Common.Diagnostic;

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

		public Task<ClientPaymentSettings> GetPaymentSettings()
		{
			var tcs = new TaskCompletionSource<ClientPaymentSettings>();

			try
			{
				var result = Client.GetAsync<PaymentSettingsResponse>(new PaymentSettingsRequest()).Result;
				tcs.TrySetResult(result.ClientPaymentSettings);
			}
			catch (Exception ex)
			{
                _logger.LogError(ex);
				tcs.TrySetResult(new ClientPaymentSettings());
			}

			return tcs.Task;
		}
	}
}