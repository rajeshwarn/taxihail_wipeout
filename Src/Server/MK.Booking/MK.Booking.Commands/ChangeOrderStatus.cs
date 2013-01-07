using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class ChangeOrderStatus : ICommand
    {
        public ChangeOrderStatus()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }

        public OrderStatusDetail Status { get; set; }
    }
}