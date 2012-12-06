using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class AddPopularAddress: ICommand
    {
        public AddPopularAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Address Address { get; set; }

    }
}