using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apcurium.MK.Booking.ReadModel
{
    public class DeviceDetail
    {
        [Key, Column(Order = 1)]
        public Guid AccountId { get; set; }
        [Key, Column(Order = 2)]
        public string DeviceToken { get; set; }
        public int Platform { get; set; } 
    }
}
