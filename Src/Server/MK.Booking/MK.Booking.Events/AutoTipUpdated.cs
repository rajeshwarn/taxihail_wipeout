using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AutoTipUpdated : VersionedEvent
    {
        public int AutoTipPercentage { get; set; }
    }
}
