using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PromotionCreated : VersionedEvent
    {
        public string Name { get; set; }
    }
}