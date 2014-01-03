using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UnpairForRideLinqCmtPayment : ICommand
    {
        public UnpairForRideLinqCmtPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
    }
}