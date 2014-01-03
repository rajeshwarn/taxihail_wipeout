#region

using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/payPal/production", "POST")]
    [Authenticate(ApplyTo.All)]
    [AuthorizationRequired(ApplyTo.All, RoleName.SuperAdmin)]
    public class TestPayPalProductionSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public PayPalCredentials Credentials { get; set; }
    }
}