using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Orders
{
    [Authenticate]
    [Route("/order/payments/{TransactionId}", "POST")]
    public class CapturePaymentRequest : IReturnVoid
    {
        public long TransactionId { get; set; }

        public Guid OrderId { get; set; }

        public int IbsOrderNumber { get; set; }

        public string CarNumber { get; set; }

        public double Amount { get; set; }
    }
}

