using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoRepository;

namespace CustomerPortal.Web.Entities.Network
{
    public class Market : IEntity
    {
        public Market()
        {
            Vehicles = new List<Vehicle>();
            DispatcherSettings = new DispatcherSettings();
        }

        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }

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

    public class DispatcherSettings
    {
        public DispatcherSettings()
        {
            NumberOfOffersPerCycle = 0;
            NumberOfCycles = 0;
            DurationOfOfferInSeconds = 15;
        }

        [Required]
        [Display(Name = "Number of Offers Per Cycle (N)", Description = "A value of 0 means we are not handling the dispatch ourselves")]
        [Range(0, 10)]
        public int NumberOfOffersPerCycle { get; set; }

        [Required]
        [Display(Name = "Number of Cycles (C)")]
        [Range(0, 100)]
        public int NumberOfCycles { get; set; }

        [Required]
        [Display(Name = "Duration of Offer In Seconds (D)")]
        [Range(10, 60)]
        public int DurationOfOfferInSeconds { get; set; }
    }
}