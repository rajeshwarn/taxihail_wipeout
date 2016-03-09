using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/creditcard/updatelabel", "POST")]
    public class UpdateCreditCardLabelRequest
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
