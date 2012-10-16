using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class GrantAdminRight : ICommand
    {
        public GrantAdminRight()
        {
            Id = new Guid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
    }
}
