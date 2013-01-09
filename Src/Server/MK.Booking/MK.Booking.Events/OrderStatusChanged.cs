using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class OrderStatusChanged : VersionedEvent
    {
        public OrderStatusDetail Status { get; set; }
    }
}