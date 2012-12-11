using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateFavoriteAddress : ICommand
    {
        public UpdateFavoriteAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Address Address { get; set; }
    }
}
