using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OriginalEtaSaved : VersionedEvent
    {
        public long OriginalEta { get; set; }
    }
}