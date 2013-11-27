using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Web.Entities
{
    public class IBSSettings
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        
        [Required]
        [Url]
        public string ServiceUrl { get; set; }
    }
}