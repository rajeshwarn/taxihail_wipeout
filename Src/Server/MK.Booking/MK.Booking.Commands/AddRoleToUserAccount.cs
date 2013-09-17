using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddRoleToUserAccount : ICommand
    {
        public AddRoleToUserAccount()
        {
            Id = new Guid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string RoleName { get; set; }
    }
}
