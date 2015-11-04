using System;
using apcurium.MK.Common;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class InitiateIbsOrderSwitch : ICommand
    {
        public InitiateIbsOrderSwitch()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public CreateOrder NewOrderCommand { get; set; }

        public int NewIbsAccountId { get; set; }
    }
}
