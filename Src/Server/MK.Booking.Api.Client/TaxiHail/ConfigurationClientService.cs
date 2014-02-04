#region
using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Common.Configuration.Impl;


#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient
    {
        public ConfigurationClientService(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public IDictionary<string, string> GetSettings()
        {
            return Client.Get<Dictionary<string, string>>("/settings");
        }

        public ClientPaymentSettings GetPaymentSettings()
        {
            return Client.Get(new PaymentSettingsRequest()).ClientPaymentSettings;
        }
    }
}