#region

using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/brainTree", "POST")]
    public class TestBraintreeSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public BraintreeClientSettings BraintreeClientSettings { get; set; }
        public BraintreeServerSettings BraintreeServerSettings { get; set; }
    }
}