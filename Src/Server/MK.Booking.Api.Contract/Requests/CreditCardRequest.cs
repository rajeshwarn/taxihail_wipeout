#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcards/{CreditCardId}", "DELETE")]
    [Route("/account/creditcards", "GET,POST")]
    public class CreditCardRequest : IReturn<CreditCardDetails>
    {
        public Guid CreditCardId { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
        public string ExpirationMonth { get; set; }        
        public string ExpirationYear { get; set; }   
        public string Label { get; set; }
        public string ZipCode { get; set; }
    }
}