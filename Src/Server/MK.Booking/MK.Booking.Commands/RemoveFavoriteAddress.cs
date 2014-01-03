#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RemoveFavoriteAddress : ICommand
    {
        public RemoveFavoriteAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public Guid AddressId { get; set; }
        public Guid Id { get; set; }
    }
}