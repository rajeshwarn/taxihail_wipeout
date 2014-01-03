#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddRoleToUserAccount : ICommand
    {
        public AddRoleToUserAccount()
        {
            Id = new Guid();
        }

        public Guid AccountId { get; set; }
        public string RoleName { get; set; }
        public Guid Id { get; set; }
    }
}