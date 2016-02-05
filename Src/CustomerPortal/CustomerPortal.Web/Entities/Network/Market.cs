using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;
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
            MarketTariff = new Tariff();
        }

        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }

        public List<Vehicle> Vehicles { get; set; }

        public bool EnableDriverBonus { get; set; }

        public bool EnableFutureBooking { get; set; }

        public string FutureBookingReservationProvider { get; set; }

        public int FutureBookingTimeThresholdInMinutes { get; set; }

        public bool DisableOutOfAppPayment { get; set; }

        public string ReceiptFooter { get; set; }

        public bool EnableAppFareEstimates { get; set; }

        public Tariff MarketTariff { get; set; }
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