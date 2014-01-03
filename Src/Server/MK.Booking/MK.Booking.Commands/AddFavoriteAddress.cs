#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddFavoriteAddress : ICommand
    {
        public AddFavoriteAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Address Address { get; set; }
        public Guid Id { get; set; }
    }
}