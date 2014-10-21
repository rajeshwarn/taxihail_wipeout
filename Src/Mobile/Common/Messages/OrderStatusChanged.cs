using System;
using apcurium.MK.Common.Entity;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderStatusChanged : GenericTinyMessage<Guid>
    {
        public OrderStatusChanged(object sender, Guid orderId, OrderStatus status, string ownerId)
            : base(sender, orderId)
        {
            OwnerId = ownerId;
            Status = status;
        }

        public string OwnerId { get; private set; }

        public OrderStatus Status { get; private set; }
    }
}