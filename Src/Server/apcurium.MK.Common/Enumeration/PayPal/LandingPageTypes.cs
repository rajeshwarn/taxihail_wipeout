using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Enumeration.PayPal
{
    public enum LandingPageTypes
    {
        // ReSharper disable once InconsistentNaming
        [Display(Name = "Login")]
        login,

        // ReSharper disable once InconsistentNaming
        [Display(Name = "Billing")]
        billing
    }
}