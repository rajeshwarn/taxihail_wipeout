#region

using System;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/sendreceipt", "POST")]
    public class SendReceipt
    {
        public Guid OrderId { get; set; }
    }

    public class SendReceiptResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}