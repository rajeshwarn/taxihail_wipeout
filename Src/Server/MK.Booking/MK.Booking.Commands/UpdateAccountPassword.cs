using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateAccountPassword : ICommand
    {
        public UpdateAccountPassword()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string Password { get; set; }
    }
}
