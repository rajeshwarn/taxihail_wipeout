using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPortal.Contract.Resources;
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
        [Display(Name = "IncludeInTaxiHailNetwork",
            Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }

        public string Market { get; set; }

        [Required]
        [Display(Name = "RegionTaxiHailNetworkLabel", ResourceType = typeof(Resources))]
        public MapRegion Region {get; set; }

        public string Id { get; set; }

        public List<CompanyPreference> Preferences { get; set; }
    }
}