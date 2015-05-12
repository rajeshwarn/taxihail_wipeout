using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UnlinkAccountFromIbs : ICommand
    {
        public UnlinkAccountFromIbs()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}