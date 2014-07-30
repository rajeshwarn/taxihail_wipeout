#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class CreditCardAdded : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CreditCardCompany { get; set; }

        // not used anymore but to prevent incompatible event deserialization in dbinit
        public string FriendlyName { get; set; } 
    }
}