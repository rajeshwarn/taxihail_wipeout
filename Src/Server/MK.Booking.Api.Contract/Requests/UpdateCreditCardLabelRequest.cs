using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcard/updatelabel", "POST")]
    public class UpdateCreditCardLabelRequest
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
