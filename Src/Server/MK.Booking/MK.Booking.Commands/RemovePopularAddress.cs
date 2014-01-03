#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RemovePopularAddress : ICommand
    {
        public RemovePopularAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid AddressId { get; set; }
        public Guid Id { get; set; }
    }
}