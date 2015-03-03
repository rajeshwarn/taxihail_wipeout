using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CustomerPortal.Web.Entities;
using System.Web.Mvc;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class ChecklistModel
    {
        public Company Company { get; set; }

        [Display(Name = "AppStore Credentials")]
        public string AppStoreCred { get; set; }
        
        [Display(Name = "PlayStore Credentials")]
        public string PlayStoreCred { get; set; }

        [Display(Name = "Unique Device Identification")]
        public string UDID { get; set; }

        [Display(Name = "IBS")]
        public string IBS { get; set; }

        [Display(Name = "PO#")]
        public string PONumber { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

    }
}