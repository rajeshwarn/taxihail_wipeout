using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;
using MongoRepository;

namespace CustomerPortal.Web.Entities.Network
{
    public class TaxiHailNetworkSettings : IEntity
    {
        [Display(Name = "IncludeInTaxiHailNetwork", Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }

         [Display(Name = "RegionTaxhiHailNetworkLabel", ResourceType = typeof(Resources))]
        public MapRegion Region {get; set; }

        public string Id { get; set; }
        public string CompanyId { get; set; }
    }
}