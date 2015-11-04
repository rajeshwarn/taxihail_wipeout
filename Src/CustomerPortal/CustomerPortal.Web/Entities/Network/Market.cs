using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoRepository;

namespace CustomerPortal.Web.Entities.Network
{
    public class Market : IEntity
    {
        public Market()
        {
            Vehicles = new List<Vehicle>();
        }

        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<Vehicle> Vehicles { get; set; } 
    }

    public class Vehicle
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
    }
}