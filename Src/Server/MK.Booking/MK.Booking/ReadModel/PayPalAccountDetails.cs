using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class PayPalAccountDetails
    {
        [Key]
        public Guid AccountId { get; set; }

        public string EncodedRefreshToken { get; set; }
    }
}
