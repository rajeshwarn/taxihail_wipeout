using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Enumeration
{
    public enum SupportedLanguages
    {
        [Display(Description="Arabic")]
// ReSharper disable once InconsistentNaming
        ar,

        [Display(Description = "English")]
// ReSharper disable once InconsistentNaming
        en,

        [Display(Description = "Spanish")]
// ReSharper disable once InconsistentNaming
        es,

        [Display(Description = "French")]
// ReSharper disable once InconsistentNaming
        fr,

        [Description("Dutch")]
// ReSharper disable once InconsistentNaming
        nl
    }
}
