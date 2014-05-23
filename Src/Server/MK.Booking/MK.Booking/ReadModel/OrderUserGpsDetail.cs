using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderUserGpsDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public double? UserLatitude { get; set; }

        public double? UserLongitude { get; set; }
    }
}