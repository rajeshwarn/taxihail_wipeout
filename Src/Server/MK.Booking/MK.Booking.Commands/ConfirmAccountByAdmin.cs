using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ConfirmAccountByAdmin : ICommand
    {
        public ConfirmAccountByAdmin()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
    }
}