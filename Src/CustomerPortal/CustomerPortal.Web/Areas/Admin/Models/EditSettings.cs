#region

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class EditSettings
    {
        [HiddenInput(DisplayValue = false)]
        public string Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string OldKey { get; set; }

        [Required]
        [Display(Name = "Key")]
        public string Key { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }
    }
}