using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/accounts/settleoverduepayment", "POST")]
    public class SettleOverduePaymentRequest : IReturn<SettleOverduePaymentResponse>
    {
        public string KountSessionId { get; set; }

        public string CustomerIpAddress { get; set; }
    }
}