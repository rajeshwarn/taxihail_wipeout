using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class TransactionIdSet : VersionedEvent
    {
        public long TransactionId { get; set; }
    }
}