using System;
using MongoRepository;
using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Web.Entities.Network
{
    [Obsolete("Use the Vehicles list in the new Market entity")]
    public class NetworkVehicle : IEntity
    {
        public string Id { get; set; }

        public int NetworkVehicleId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Logo")]
        public string LogoName { get; set; }

        [Display(Name = "Max # Passengers (0 = no limit)")]
        public int MaxNumberPassengers { get; set; }

        public string Market { get; set; }
    }
}