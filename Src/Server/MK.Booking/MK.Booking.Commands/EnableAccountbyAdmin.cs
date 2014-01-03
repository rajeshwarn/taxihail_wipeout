#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class EnableAccountByAdmin : ICommand
    {
        public EnableAccountByAdmin()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}