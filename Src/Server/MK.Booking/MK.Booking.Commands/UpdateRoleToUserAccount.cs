using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRoleToUserAccount : ICommand
    {

        public UpdateRoleToUserAccount()
        {
            Id = new Guid();
        }

        public Guid AccountId { get; set; }
        public string RoleName { get; set; }
        public Guid Id { get; set; }
    }
}
