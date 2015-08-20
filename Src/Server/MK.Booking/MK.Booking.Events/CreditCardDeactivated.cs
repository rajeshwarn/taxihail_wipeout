using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardDeactivated : VersionedEvent
    {
        public bool IsOutOfAppPaymentDisabled { get; set; }
    }
}
