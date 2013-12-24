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
        public string FriendlyName { get; set; }
        public string Token { get; set; }
        public string Last4Digits { get; set; }
        public string CreditCardCompany { get; set; }
    }
}