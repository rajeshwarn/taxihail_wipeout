using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class VehicleTypeAddedUpdated : VersionedEvent
    {
        public Guid VehicleTypeId { get; set; }
        public string Name { get; set; }
        public string LogoName { get; set; }
        public int ReferenceDataVehicleId { get; set; }
    }
}