using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/brainTree", "POST")]
    [Authenticate(ApplyTo.All)]
    [AuthorizationRequired(ApplyTo.All, Permissions.Admin)]
    public class TestBraintreeSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public BraintreeClientSettings BraintreeClientSettings { get; set; }
        public BraintreeServerSettings BraintreeServerSettings { get; set; }
    }
}
