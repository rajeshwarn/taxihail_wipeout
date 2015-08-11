using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UnpairOrderForManualRideLinq : ICommand
    {
        public UnpairOrderForManualRideLinq()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
    }
}
