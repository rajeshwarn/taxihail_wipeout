using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalPaymentCancellationFailed : VersionedEvent
    {
        public string Reason { get; set; }
    }
}