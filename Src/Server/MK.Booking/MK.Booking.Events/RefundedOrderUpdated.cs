using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class RefundedOrderUpdated : VersionedEvent
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }
    }
}