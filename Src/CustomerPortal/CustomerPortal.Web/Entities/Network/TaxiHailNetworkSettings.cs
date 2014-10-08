using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;

namespace CustomerPortal.Web.Entities.Network

{
    
    public class TaxiHailNetworkSettings
    {
        [Required]
        [Display(Name = "IncludeInTaxiHailNetwork", Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }

         [Display(Name = "RegionTaxhiHailNetworkLabel", ResourceType = typeof(Resources))]
         [Required]
        public MapRegion Region {get; set; }
    }
}