using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderCancelledBecauseOfIbsError : VersionedEvent
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}