using System;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/settings/taxihailnetwork", "GET, POST")]
    [Route("/settings/taxihailnetwork/{AccountId}", "GET, POST")]
    public class UserTaxiHailNetworkSettingsRequest
    {
        public Guid? AccountId { get; set; }

        public UserTaxiHailNetworkSettings UserTaxiHailNetworkSettings { get; set; }
    }
}
