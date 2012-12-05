using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePopularAddress: ICommand
    {
        public UpdatePopularAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}
