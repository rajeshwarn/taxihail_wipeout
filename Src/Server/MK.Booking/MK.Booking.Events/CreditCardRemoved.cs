#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class CreditCardRemoved : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public Guid? NewDefaultCreditCardId { get; set; }
    }
}