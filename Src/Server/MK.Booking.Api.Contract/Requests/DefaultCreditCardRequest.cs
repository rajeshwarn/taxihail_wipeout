using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/creditcard/updatedefault", "POST")]
    public class DefaultCreditCardRequest
    {
        public Guid CreditCardId { get; set; }
    }
}
