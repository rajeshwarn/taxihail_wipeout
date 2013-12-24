#region

using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcards", "GET,POST")]
    [Route("/account/creditcards/{CreditCardId}", "DELETE")]
    public class CreditCardRequest
    {
        public Guid CreditCardId { get; set; }
        public string FriendlyName { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
    }
}