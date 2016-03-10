using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using apcurium.MK.Common.Entity;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class MarketModel
    {
        public MarketModel()
        {
            CompaniesOrMarket = new List<SelectListItem>();
        }

        [Required]
        [Display(Name = "Name")]
        public string Market { get; set; }

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

        public bool ShowCallDriver { get; set; }

        public Tariff MarketTariff { get; set; }

        public IEnumerable<SelectListItem> CompaniesOrMarket { get; set; }
    }

    public class VehicleModel : Vehicle
    {
        public string Market { get; set; }
    }
}