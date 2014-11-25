using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class UserTaxiHailNetworkSettings
    {
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public List<string> DisabledFleets { get; set; }
    }
}
