using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OriginalEtaLogged : VersionedEvent
    {
        public long OriginalEta { get; set; }
    }
}
