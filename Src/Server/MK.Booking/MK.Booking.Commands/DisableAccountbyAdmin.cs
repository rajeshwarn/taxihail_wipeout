using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DisableAccountByAdmin : ICommand
    {
        public DisableAccountByAdmin()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
    }
}