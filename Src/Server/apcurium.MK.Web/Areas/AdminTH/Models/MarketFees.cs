﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class MarketFees
    {
        public MarketFees()
        {
            Fees = new Dictionary<string, FeeStructure>();
        }

        public Dictionary<string, FeeStructure> Fees { get; set; }
    }

    public class FeeStructure
    {
        [Display(Name = "Booking")]
        [Range(0, double.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public decimal Booking { get; set; }

        [Display(Name = "Cancellation")]
        [Range(0, double.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public decimal Cancellation { get; set; }

        [Display(Name = "No Show")]
        [Range(0, double.MaxValue, ErrorMessage = "The value must be greater than 0")]
        public decimal NoShow { get; set; }
    }
}