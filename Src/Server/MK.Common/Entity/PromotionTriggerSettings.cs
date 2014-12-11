using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Entity
{
    public class PromotionTriggerSettings
    {
        [Display(Name = "Trigger")]
        public int Type { get; set; }

        [Display(Name = "Ride Count")]
        public int RideCount { get; set; }

        [Display(Name = "Amount Spent")]
        public int AmountSpent { get; set; }
    }
}
