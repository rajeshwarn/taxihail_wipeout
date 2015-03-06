using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class TemporaryOrderCreationInfoDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public string SerializedOrderCreationInfo { get; set; }
    }
}