using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class VehicleTypeDeleted : VersionedEvent
    {
        public Guid VehicleTypeId { get; set; }
    }
}