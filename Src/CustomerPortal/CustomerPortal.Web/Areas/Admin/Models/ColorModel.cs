using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CustomerPortal.Web.Entities;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class ColorModel
    {
        public Company Company { get; set; }

        [Display(Name = "Company Color")]
        public string CompanyColor { get; set; }


        [Display(Name = "Title Color")]
        public string TitleColor { get; set; }

        [Display(Name = "Login Color")]
        public string LoginColor { get; set; }

        [Display(Name = "Menu Color")]
        public string MenuColor { get; set; }


        [Display(Name = "Web Accent Color")]
        public string WebAccentColor { get; set; }

        [Display(Name = "Email Font Color")]
        public string EmailFontColor { get; set; }

    }
}