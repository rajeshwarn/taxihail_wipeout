#region

using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/cmt", "POST")]
    [Authenticate(ApplyTo.All)]
    [AuthorizationRequired(ApplyTo.All, RoleName.SuperAdmin)]
    public class TestCmtSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
        public ServiceType ServiceType { get; set; }
    }
}