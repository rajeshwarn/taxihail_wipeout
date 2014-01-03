#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class TariffDeleted : VersionedEvent
    {
        public Guid TariffId { get; set; }
    }
}