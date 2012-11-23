using System;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public  class OrderDeleted: GenericTinyMessage<Guid>
    {

        public OrderDeleted(object sender, Guid orderId, string ownerId)
            : base(sender, orderId)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}