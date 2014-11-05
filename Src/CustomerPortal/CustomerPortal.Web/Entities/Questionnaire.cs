#region

using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;

#endregion

namespace CustomerPortal.Web.Entities
{
    public class Questionnaire
    {
        [Display(Name = "AppName", Description = "AppNameHelp", ResourceType = typeof (Resources))]
        [StringLength(30)]
        public string AppName { get; set; }


        [Display(Name = "CompanyWebsiteUrl", Description = "CompanyWebsiteUrlHelp", ResourceType = typeof (Resources))]
        [Url]
        public string CompanyWebsiteUrl { get; set; }

        [Display(Name = "CompanyPhoneNumber", Description = "CompanyPhoneNumberHelp", ResourceType = typeof (Resources))
        ]
        public string CompanyPhoneNumber { get; set; }

        [Display(Name = "AboutUsLink", Description = "AboutUsLinkHelp", ResourceType = typeof (Resources))]
        [Url]
        public string AboutUsLink { get; set; }

        [Display(Name = "SupportContactEmail", Description = "SupportContactEmailHelp",
            ResourceType = typeof (Resources))]
        [EmailAddress]
        public string SupportContactEmail { get; set; }

        [Display(Name = "FlagDropRate", Description = "FlagDropRateHelp", ResourceType = typeof (Resources))]
        [Range(0d, 100d)]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        public decimal? FlagDropRate { get; set; }

        [Display(Name = "MileageRate", Description = "MileageRateHelp", ResourceType = typeof (Resources))]
        [Range(0d, 100d)]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        public decimal? MileageRate { get; set; }

        [Display(Name = "UnitOfLength", Description = "UnitOfLengthHelp", ResourceType = typeof (Resources))]
        public UnitOfLength UnitOfLength { get; set; }

        [Display(Name = "CorporateColor", Description = "CorporateColorHelp", ResourceType = typeof (Resources))]
        [UIHint("Color")]
        public string CorporateColor { get; set; }
    }
}