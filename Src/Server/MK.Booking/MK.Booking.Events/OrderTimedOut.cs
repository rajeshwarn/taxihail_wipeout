using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderTimedOut : VersionedEvent
    {
        public string Market { get; set; }
    }
}
