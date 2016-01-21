using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class MarketModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Market { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }

        public IEnumerable<Vehicle> Vehicles { get; set; }

        [Display(Name = "Driver Bonus Enabled")]
        public bool EnableDriverBonus { get; set; }

        [Display(Name = "Future Booking Enabled")]
        public bool EnableFutureBooking { get; set; }

        [Display(Name = "Receipt Footer")]
        public string ReceiptFooter { get; set; }

        [Display(Name = "App Fare Estimates Enabled")]
        public bool EnableAppFareEstimates { get; set; }
        [Display(Name = "Minimum Rate")]
        public double MinimumRate { get; set; }
        [Display(Name = "Flat Rate")]
        public decimal FlatRate { get; set; }
        [Display(Name = "Per KM Rate")]
        public double KilometricRate { get; set; }
        [Display(Name = "Per Minute Rate")]
        public double PerMinuteRate { get; set; }
        [Display(Name = "Kilometer Included In Flat Rate")]
        public double KilometerIncluded { get; set; }
        [Display(Name = "Overhead")]
        public double MarginOfError { get; set; }
    }

    public class VehicleModel : Vehicle
    {
        public string Market { get; set; }
    }
}