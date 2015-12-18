using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderCancelledBecauseOfError : VersionedEvent
    {
        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public bool IsDispatcherTimedOut { get; set; }
        
        public bool CancelWasRequested { get; set; }
    }
}