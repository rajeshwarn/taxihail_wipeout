#region

using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server/test/brainTree", "POST")]
    public class TestBraintreeSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public BraintreeClientSettings BraintreeClientSettings { get; set; }
        public BraintreeServerSettings BraintreeServerSettings { get; set; }
    }
}