using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class EnableAccountByAdmin : ICommand
    {
        public EnableAccountByAdmin()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
    }
}