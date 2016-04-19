using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server/test/moneris", "POST")]
    public class TestMonerisSettingsRequest : IReturn<TestServerPaymentSettingsResponse>
    {
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
    }
}