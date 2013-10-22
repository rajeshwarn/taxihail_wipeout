using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Properties;

namespace CustomerPortal.Web.Entities
{
    public class IBSSettings
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        [Display(Name = "ServiceUrl", Description = "ServiceUrlHelp", ResourceType = typeof(Resources))]
        [Required]
        [Url]
        public string ServiceUrl { get; set; }
    }
}