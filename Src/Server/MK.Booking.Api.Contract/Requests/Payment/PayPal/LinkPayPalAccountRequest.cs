using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal
{
    [Authenticate]
    [Route("/paypal/{AccountId}/link", "POST")]
    public class LinkPayPalAccountRequest
    {
        public Guid AccountId { get; set; }

        public string AuthCode { get; set; }

        public string MetadataId { get; set; }
    }
}