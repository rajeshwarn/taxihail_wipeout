using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Guid AddressId { get; set; }
        public Guid AccountId { get; set; }
        public Address Address { get; set; }
    }
}
