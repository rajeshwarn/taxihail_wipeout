using apcurium.MK.Common.Enumeration;
using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class VehicleTypeDetail
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }
        public ServiceType ServiceType { get; set; }

        public DateTime CreatedDate { get; set; }

        public int MaxNumberPassengers { get; set; }

        public int? ReferenceNetworkVehicleTypeId { get; set; }

        public bool IsWheelchairAccessible { get; set; }
    }
}