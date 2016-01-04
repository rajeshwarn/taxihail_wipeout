using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class VehicleIdMappingDetail
    {
        [Key]
        public Guid OrderId { get; set; }

        public string LegacyDispatchId { get; set; }

        public string DeviceName { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
