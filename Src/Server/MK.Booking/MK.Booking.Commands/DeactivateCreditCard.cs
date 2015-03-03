using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class DeactivateCreditCard : ICommand
    {
        public DeactivateCreditCard()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid AccountId { get; set; }
    }
}
