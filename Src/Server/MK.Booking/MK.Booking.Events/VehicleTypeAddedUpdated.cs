using System;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Events
{
    public class VehicleTypeAddedUpdated : VersionedEvent
    {
        public Guid VehicleTypeId { get; set; }
        public string Name { get; set; }
        public string LogoName { get; set; }
        public int ReferenceDataVehicleId { get; set; }
        public int MaxNumberPassengers { get; set; }
        public int? ReferenceNetworkVehicleTypeId { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsWheelchairAccessible { get; set; }
    }
}