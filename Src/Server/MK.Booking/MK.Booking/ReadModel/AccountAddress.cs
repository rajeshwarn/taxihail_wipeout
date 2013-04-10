using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountAddress
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Account")]
        public AccountDetail AccountId { get; set; }
        
        public string FriendlyName { get; set; }

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }


    }
}
