using System;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Data
{
    public class CreditCardInfos
    {	
		public CreditCardInfos()
		{
			Label = CreditCardConstants.Personal;
		}
        public Guid CreditCardId { get; set; }
        public string CardNumber { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
        public string ExpirationMonth { get; set; }        
        public string ExpirationYear { get; set; }        
		public string CCV { get; set; }        
		public string Label { get; set; }        
		public bool IsDefault { get; set; }        
		public string ZipCode { get; set; }        
    }
}

