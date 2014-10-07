using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Entities;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public enum DeployOptions
    {
        MobileApp = 1,
        Server = 2,
        Both = 3,        
    }

    public class DeployCustomerModel
    {
        public Company Company { get; set; }

        public string CompanyKey { get; set; }

        [Display(Name = "Deploy Options")]
        public DeployOptions DeployOptions { get; set; }

        [Display(Name = "Server")]
        public ServerUrlOptions ServerUrlOptions { get; set; }
        
        [Required]
        [Display(Name = "Revision")]
        public IEnumerable<SelectListItem> Revision { get; set; }

        [Required]
        [Display(Name = "Revision")]
        public string RevisionId { get; set; }

        


    }
}