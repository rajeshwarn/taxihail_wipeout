using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    [Obsolete]
    public class PayPalAccountDetails
    {
        [Key]
        public Guid AccountId { get; set; }

        public string EncryptedRefreshToken { get; set; }
    }
}
