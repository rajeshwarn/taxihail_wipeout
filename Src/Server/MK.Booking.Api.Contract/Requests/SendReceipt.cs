#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/sendreceipt", "POST")]
    public class SendReceipt
    {
        public Guid OrderId { get; set; }
    }

    [Authenticate]
    [Route("/account/orders/{OrderId}/sendreceipt/{RecipientEmail}", "POST")]
    public class SendReceiptAdmin
    {
        public Guid OrderId { get; set; }
        public string RecipientEmail { get; set; }
    }

    public class SendReceiptResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}