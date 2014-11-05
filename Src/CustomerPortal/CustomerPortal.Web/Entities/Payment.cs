using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public class Payment
    {
        [Display(Name = "PO Number")]
        public string PONumber { get; set; }

        [Display(Name = "PO Date")]
        public DateTime? PODate { get; set; }

        [Display(Name = "Qty License")]
        public string QtyLicense { get; set; }

        [Display(Name = "No Charge")]
        public bool NoCharge { get; set; }




        [Display(Name = "Test Link Date")]
        public DateTime? TestLinkDate { get; set; }

        [Display(Name = "Test Link Invoice Number")]
        public string TestLinkInvoiceNumber { get; set; }

        [Display(Name = "Store Date")]
        public DateTime? PublishToStoreDate { get; set; }

        [Display(Name = "Store Invoice Number")]
        public string PublishToStoreInvoiceNumber { get; set; }


    }
}