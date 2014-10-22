using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Client.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient
    {
        public ConfigurationClientService(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public async Task<IDictionary<string, string>> GetSettings()
        {
            return await Client.GetAsync<Dictionary<string, string>>("/settings");
        }

        public async Task<ClientPaymentSettings> GetPaymentSettings()
        {
            var paymentSettings = await Client.GetAsync<PaymentSettingsResponse> (new PaymentSettingsRequest ());
            return paymentSettings.ClientPaymentSettings;
        }
    }
}