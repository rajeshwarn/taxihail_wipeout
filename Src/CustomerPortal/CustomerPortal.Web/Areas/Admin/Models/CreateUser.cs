#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class CreateUser
    {
        public bool IsAdmin { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class EditUser
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }

    public class ChangePassword
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// For model state only
        /// </summary>
        public string EmailAddress { get; set; }
    }
}