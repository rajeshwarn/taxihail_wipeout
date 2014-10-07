using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;

namespace CustomerPortal.Web.Entities.Network
{
    public class TaxiHailNetworkSettings
    {
                [Display(Name = "IncludeInTaxiHailNetwork", Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }
        public MapRegion Region {get; set; }
    }
}