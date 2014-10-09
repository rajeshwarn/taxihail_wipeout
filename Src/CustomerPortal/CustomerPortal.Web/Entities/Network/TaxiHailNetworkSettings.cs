using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;
using MongoRepository;

namespace CustomerPortal.Web.Entities.Network

{
    public class TaxiHailNetworkSettings : IEntity
    {
        public TaxiHailNetworkSettings()
        {
            Preferences= new List<CompanyPreference>();
        }

        [Required]
        [Display(Name = "IncludeInTaxiHailNetwork", Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }

         [Display(Name = "RegionTaxiHailNetworkLabel", ResourceType = typeof(Resources))]
         [Required]
        public MapRegion Region {get; set; }

        public string Id { get; set; }
        public string CompanyId { get; set; }

        public List<CompanyPreference> Preferences { get; set; }
    }
}