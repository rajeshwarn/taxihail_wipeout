using System;
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

        public string Id { get; set; }

        [Obsolete("Use the Region in Market instead")]
        public MapRegion Region { get; set; }

        [Required]
        public string Market { get; set; }

        [Required]
        public int FleetId { get; set; }

        [Display(Name = "FleetIdWhiteListLabel", ResourceType = typeof(Resources))]
        public string WhiteListedFleetIds { get; set; }

        [Display(Name = "FleetIdBlackListLabel", ResourceType = typeof(Resources))]
        public string BlackListedFleetIds { get; set; }

        [Required]
        [Display(Name = "IncludeInTaxiHailNetwork",
            Description = "IncludeInTaxiHailNetworkHelp",
            ResourceType = typeof(Resources))]
        public bool IsInNetwork { get; set; }

        public List<CompanyPreference> Preferences { get; set; }
    }
}