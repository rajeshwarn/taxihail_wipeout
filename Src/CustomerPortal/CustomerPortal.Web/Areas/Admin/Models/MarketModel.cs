using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using apcurium.MK.Common.Entity;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Properties;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class MarketModel
    {
        public MarketModel()
        {
            CompaniesOrMarket = new List<SelectListItem>();
            Region = new MapRegion();
            OtherMarkets = new List<Market>();
        }

        [Required]
        [Display(Name = "Name")]
        public string Market { get; set; }

        [Display(Name = "RegionTaxiHailNetworkLabel", ResourceType = typeof(Resources))]
        public MapRegion Region { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }

        public IEnumerable<Vehicle> Vehicles { get; set; }

        [Display(Name = "Driver Bonus Enabled")]
        public bool EnableDriverBonus { get; set; }

        [Display(Name = "Future Booking - Enabled")]
        public bool EnableFutureBooking { get; set; }

        [Display(Name = "Future Booking - Reservation Provider")]
        public string FutureBookingReservationProvider { get; set; }

        [Display(Name = "Future Booking - Time Threshold (minutes)")]
        public int FutureBookingTimeThresholdInMinutes { get; set; }

        [Display(Name = "Disable Out Of App Payment")]
        public bool DisableOutOfAppPayment { get; set; }

        [Display(Name = "Receipt Footer")]
        public string ReceiptFooter { get; set; }

        [Display(Name = "App Fare Estimates Enabled")]
        public bool EnableAppFareEstimates { get; set; }

        public Tariff MarketTariff { get; set; }

        public IEnumerable<SelectListItem> CompaniesOrMarket { get; set; }

        public IEnumerable<Market> OtherMarkets { get; set; } 
    }

    public class VehicleModel : Vehicle
    {
        public string Market { get; set; }
    }
}