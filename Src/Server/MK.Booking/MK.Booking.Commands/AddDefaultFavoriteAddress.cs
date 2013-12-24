#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class AddDefaultFavoriteAddress : ICommand
    {
        public AddDefaultFavoriteAddress()
        {
            Id = Guid.NewGuid();
        }

        public Address Address { get; set; }
        public Guid Id { get; set; }
    }
}