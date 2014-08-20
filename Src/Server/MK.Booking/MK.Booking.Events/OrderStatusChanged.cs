#region

using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderStatusChanged : VersionedEvent
    {
        public OrderStatusDetail Status { get; set; }
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
        public double? Tax { get; set; }
        public bool IsCompleted { get; set; }
    }
}