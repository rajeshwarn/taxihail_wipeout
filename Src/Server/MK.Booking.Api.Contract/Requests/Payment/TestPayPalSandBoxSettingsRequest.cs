#region

using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/payPal/sandbox", "POST")]
    public class TestPayPalSandboxSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public PayPalServerCredentials ServerCredentials { get; set; }

        public PayPalClientCredentials ClientCredentials { get; set; }
    }
}