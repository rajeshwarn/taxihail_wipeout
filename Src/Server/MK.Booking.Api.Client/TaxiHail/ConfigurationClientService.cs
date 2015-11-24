using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
#endif

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

	    public async Task<IDictionary<string, string>> GetSettings()
		{
			try
			{
				return await Client.GetAsync<Dictionary<string, string>>("/settings");
			}
			catch (Exception ex)
			{
                _logger.LogError(ex);
			    return new Dictionary<string, string>();
			}
		}

		public async Task<ClientPaymentSettings> GetPaymentSettings()
		{
			try
			{
				var result = await Client.GetAsync(new PaymentSettingsRequest());
			    return result.ClientPaymentSettings;
			}
			catch (Exception ex)
			{
                _logger.LogError(ex);

			    return new ClientPaymentSettings();
			}
		}
	}
}