using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server/test/moneris", "POST")]
    public class TestMonerisSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
    }
}