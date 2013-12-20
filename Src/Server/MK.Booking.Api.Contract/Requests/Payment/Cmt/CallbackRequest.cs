using System;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    [Route("payments/cmt/callback/{OrderId}", "POST")]
    public class CallbackRequest : Trip, IReturnVoid
    {
        public Guid OrderId { get; set; }
    }
}