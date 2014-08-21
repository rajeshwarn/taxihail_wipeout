using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardErrorThrown : VersionedEvent
    {
        public string Reason { get; set; }
    }
}
