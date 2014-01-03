#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PopularAddressRemoved : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}