using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPortal.Contract.Resources;
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

        public bool EnableDriverBonus { get; set; }

        public string ReceiptFooter { get; set; }

        public bool EnableAppFareEstimates { get; set; }

        public double MinimumRate { get; set; }

        public decimal FlatRate { get; set; }

        public double KilometricRate { get; set; }

        public double PerMinuteRate { get; set; }

        public double KilometerIncluded { get; set; }

        public double MarginOfError { get; set; }
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