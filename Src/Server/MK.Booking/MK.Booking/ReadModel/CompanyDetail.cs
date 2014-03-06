using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class CompanyDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string TermsAndConditions { get; set; }

        public string Version { get; set; }
    }
}