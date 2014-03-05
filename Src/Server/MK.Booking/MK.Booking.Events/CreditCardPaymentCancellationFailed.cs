using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardPaymentCancellationFailed : VersionedEvent
    {
        public string Reason { get; set; }
    }
}
