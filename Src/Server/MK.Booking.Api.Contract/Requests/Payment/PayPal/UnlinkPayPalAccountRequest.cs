using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [Authenticate]
    [Route("/paypal/{AccountId}/unlink", "POST")]
    public class UnlinkPayPalAccountRequest
    {
        public Guid AccountId { get; set; }
    }
}
