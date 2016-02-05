using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.EventSourcing;
using System;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardDeactivated : VersionedEvent
    {
        public OutOfAppPaymentDisabled IsOutOfAppPaymentDisabled { get; set; }
        public Guid? CreditCardId { get; set; }
    }
}
