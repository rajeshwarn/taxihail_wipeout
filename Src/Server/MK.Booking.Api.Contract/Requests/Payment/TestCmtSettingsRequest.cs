#region

using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/cmt", "POST")]
    public class TestCmtSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
    }
}