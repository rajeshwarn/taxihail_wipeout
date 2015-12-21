using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardDeactivated : VersionedEvent
    {
        public OutOfAppPaymentDisabled IsOutOfAppPaymentDisabled { get; set; }
    }
}
