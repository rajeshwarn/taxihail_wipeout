using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Common.Entity
{
    public class PromotionTriggerSettings
    {
        [Display(Name = "Trigger")]
        public PromotionTriggerTypes Type { get; set; }

        [Display(Name = "Number of Rides to unlock")]
        public int RideCount { get; set; }

        [Display(Name = "Amount to spend to unlock")]
        public int AmountSpent { get; set; }

        [Display(Name = "Apply to Existing Accounts")]
        public bool ApplyToExisting { get; set; }
    }
}
