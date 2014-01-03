#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateAccountPassword : ICommand
    {
        public UpdateAccountPassword()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string Password { get; set; }
        public Guid Id { get; set; }
    }
}