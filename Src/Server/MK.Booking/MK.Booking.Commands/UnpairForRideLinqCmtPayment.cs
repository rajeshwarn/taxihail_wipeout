#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UnpairForRideLinqCmtPayment : ICommand
    {
        public UnpairForRideLinqCmtPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public Guid Id { get; private set; }
    }
}