using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class NotifyOrderTimedOut : ICommand
    {
        public NotifyOrderTimedOut()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }
    }
}
