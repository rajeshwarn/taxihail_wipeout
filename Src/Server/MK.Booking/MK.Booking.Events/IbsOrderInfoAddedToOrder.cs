using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class IbsOrderInfoAddedToOrder : VersionedEvent
    {
        public int IBSOrderId { get; set; }

        public bool CancelWasRequested { get; set; }
    }
}