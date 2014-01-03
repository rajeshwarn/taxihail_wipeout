#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

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