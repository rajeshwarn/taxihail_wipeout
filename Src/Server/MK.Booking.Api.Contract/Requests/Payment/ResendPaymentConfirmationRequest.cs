using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payment/ResendConfirmationRequest", "POST")]
    public class ResendPaymentConfirmationRequest : BaseDTO, IReturnVoid
    {
        public Guid OrderId { get; set; }
    }
}
