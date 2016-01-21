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

        [Display(Name = "Receipt Footer")]
        public string ReceiptFooter { get; set; }
    }

    public class VehicleModel : Vehicle
    {
        public string Market { get; set; }
    }
}