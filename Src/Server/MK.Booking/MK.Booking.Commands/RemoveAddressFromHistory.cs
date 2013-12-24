#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RemoveAddressFromHistory : ICommand
    {
        public RemoveAddressFromHistory()
        {
            Id = Guid.NewGuid();
        }

        public Guid AddressId { get; set; }
        public Guid AccountId { get; set; }
        public Guid Id { get; private set; }
    }
}