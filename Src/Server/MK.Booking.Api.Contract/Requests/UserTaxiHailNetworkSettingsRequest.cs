using System;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/settings/taxihailnetwork", "GET, POST")]
    [Route("/settings/taxihailnetwork/{AccountId}", "GET, POST")]
    public class UserTaxiHailNetworkSettingsRequest
    {
        public Guid? AccountId { get; set; }

        public UserTaxiHailNetworkSettings UserTaxiHailNetworkSettings { get; set; }
    }
}
