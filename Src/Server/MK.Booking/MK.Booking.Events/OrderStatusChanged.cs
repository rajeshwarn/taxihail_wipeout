#region

using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderStatusChanged : VersionedEvent
    {
        public OrderStatusDetail Status { get; set; }
    }
}