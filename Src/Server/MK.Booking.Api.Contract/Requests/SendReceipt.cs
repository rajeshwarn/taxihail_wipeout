#region

using System;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}/sendreceipt", "POST")]
    public class SendReceipt
    {
        public Guid OrderId { get; set; }
    }

    [RouteDescription("/accounts/orders/{OrderId}/sendreceipt/{RecipientEmail}", "POST")]
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