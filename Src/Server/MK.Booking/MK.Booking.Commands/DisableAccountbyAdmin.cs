#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class DisableAccountByAdmin : ICommand
    {
        public DisableAccountByAdmin()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}