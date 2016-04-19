#region

using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server/test/cmt", "POST")]
    public class TestCmtSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
    }
}