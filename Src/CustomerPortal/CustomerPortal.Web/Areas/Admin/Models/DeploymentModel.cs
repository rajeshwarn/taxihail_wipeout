#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class DeploymentModel
    {
        public DeploymentModel()
        {
            DeploySide = false;
        }

        [Required]
        [Display(Name = "Company")]
        public IEnumerable<SelectListItem> Company { get; set; }

        [Required]
        [Display(Name = "Server")]
        public IEnumerable<SelectListItem> Environment { get; set; }

        [Required]
        [Display(Name = "Revision")]
        public IEnumerable<SelectListItem> Revision { get; set; }

        public bool DeploySide { get; set; }
    }
}