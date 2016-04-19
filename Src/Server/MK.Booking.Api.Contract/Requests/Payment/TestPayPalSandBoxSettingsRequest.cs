#region

using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server/test/payPal/sandbox", "POST")]
    public class TestPayPalSandboxSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public PayPalServerCredentials ServerCredentials { get; set; }

        public PayPalClientCredentials ClientCredentials { get; set; }
    }
}