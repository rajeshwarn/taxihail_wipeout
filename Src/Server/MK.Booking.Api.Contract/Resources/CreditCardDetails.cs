using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class CreditCardDetails : BaseDTO
    {
        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public string FriendlyName { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; } 
    }
}