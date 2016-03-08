using System.Collections.Generic;
using System.Web.Mvc;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class TaxiHailNetworkSettingsModel
    {
        public TaxiHailNetworkSettings TaxiHailNetworkSettings { get; set; }

        public IEnumerable<Market> AvailableMarkets { get; set; } 
    }
}