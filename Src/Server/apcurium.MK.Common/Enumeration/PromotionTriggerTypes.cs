using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Enumeration
{
    public enum PromotionTriggerTypes
    {
        [Display(Name = "No Trigger")]
        NoTrigger = 0,

        [Display(Name = "Account Created")]
        AccountCreated = 1,

        [Display(Name = "Ride Count")]
        RideCount = 2,

        [Display(Name = "Amount Spent")]
        AmountSpent = 3,

        [Display(Name = "Customer Support")]
        CustomerSupport = 4
    }
}