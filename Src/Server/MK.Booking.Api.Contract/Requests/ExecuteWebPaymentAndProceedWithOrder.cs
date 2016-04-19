using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/account/orders/{OrderId}/proceed")]
    public class ExecuteWebPaymentAndProceedWithOrder
    {
        public Guid OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Token { get; set; }
        public string PayerId { get; set; }
        public bool Cancel { get; set; }
    }
}