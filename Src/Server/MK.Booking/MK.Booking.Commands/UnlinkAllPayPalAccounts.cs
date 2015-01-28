using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UnlinkAllPayPalAccounts : ICommand
    {
        public UnlinkAllPayPalAccounts()
        {
            Id = Guid.NewGuid();
        }

        public Guid[] AccountIds { get; set; }

        public Guid Id { get; private set; }
    }
}
