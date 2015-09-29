using System;

namespace apcurium.MK.Booking.Mobile.Data
{
    public class CreditCardInfos
    {
        public Guid CreditCardId { get; set; }
        public string CardNumber { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
        public string ExpirationMonth { get; set; }        
        public string ExpirationYear { get; set; }        
		public string CCV { get; set; }        
		public bool IsDefault { get; set; }        
    }
}

