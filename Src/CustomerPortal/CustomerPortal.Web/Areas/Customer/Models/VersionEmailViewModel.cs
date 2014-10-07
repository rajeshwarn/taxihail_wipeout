#region

using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Areas.Customer.Models
{
    public class VersionEmailViewModel
    {
        public Version Version { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Recipient Email Address")]
        public string RecipientEmailAddress { get; set; }
    }
}