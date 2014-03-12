#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payment/ResendConfirmationRequest", "POST")]
    public class ResendPaymentConfirmationRequest : BaseDto, IReturnVoid
    {
        public Guid OrderId { get; set; }
    }
}