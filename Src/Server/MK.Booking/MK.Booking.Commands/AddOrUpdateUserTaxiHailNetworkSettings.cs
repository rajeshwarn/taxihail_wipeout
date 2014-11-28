using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateUserTaxiHailNetworkSettings : ICommand
    {
        public AddOrUpdateUserTaxiHailNetworkSettings()
        {
            Id = Guid.NewGuid();
        }

        public bool IsEnabled { get; set; }

        public string[] DisabledFleets { get; set; }

        public Guid AccountId { get; set; }

        public Guid Id { get; private set; }
    }
}
