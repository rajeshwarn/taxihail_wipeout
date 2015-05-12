using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/proceed")]
    public class ExecuteWebPaymentAndProceedWithOrder
    {
        public Guid OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Token { get; set; }
        public string PayerId { get; set; }
        public bool Cancel { get; set; }
    }
}