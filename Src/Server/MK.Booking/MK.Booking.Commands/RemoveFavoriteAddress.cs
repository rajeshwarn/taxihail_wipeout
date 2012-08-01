using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class RemoveFavoriteAddress : ICommand
    {
        public RemoveFavoriteAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid AddressId { get; set; }
    }
}
