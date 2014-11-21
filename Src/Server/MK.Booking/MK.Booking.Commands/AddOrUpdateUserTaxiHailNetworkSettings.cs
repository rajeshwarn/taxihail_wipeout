using System;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateUserTaxiHailNetworkSettings : ICommand
    {
        public AddOrUpdateUserTaxiHailNetworkSettings()
        {
            Id = Guid.NewGuid();
        }

        public UserTaxiHailNetworkSettings UserTaxiHailNetworkSettings { get; set; }

        public Guid AccountId { get; set; }

        public Guid Id { get; private set; }
    }
}
