#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Route("/payments/cmt/callback/{OrderId}", "POST")]
    public class CallbackRequest : Trip, IReturnVoid
    {
        public Guid OrderId { get; set; }
    }
}