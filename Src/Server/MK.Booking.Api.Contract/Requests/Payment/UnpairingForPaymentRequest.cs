using System;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/payments/unpair", "POST")]
    public class UnpairingForPaymentRequest : IReturn<BasePaymentResponse>
    {
        public Guid OrderId { get; set; }
    }
}