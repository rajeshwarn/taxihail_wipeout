using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public string NameOnCard { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CreditCardCompany { get; set; }
    }
}