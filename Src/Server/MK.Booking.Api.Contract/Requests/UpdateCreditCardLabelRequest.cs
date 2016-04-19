using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/creditcard/updatelabel", "POST")]
    public class UpdateCreditCardLabelRequest
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
