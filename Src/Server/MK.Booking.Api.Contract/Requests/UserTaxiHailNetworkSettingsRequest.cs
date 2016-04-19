using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/settings/taxihailnetwork", "GET, POST")]
    [RouteDescription("/settings/taxihailnetwork/{AccountId}", "GET, POST")]
    public class UserTaxiHailNetworkSettingsRequest
    {
        public Guid? AccountId { get; set; }

        public UserTaxiHailNetworkSettings UserTaxiHailNetworkSettings { get; set; }
    }
}
