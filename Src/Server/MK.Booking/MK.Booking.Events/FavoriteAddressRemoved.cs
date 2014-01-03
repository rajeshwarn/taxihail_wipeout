#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class FavoriteAddressRemoved : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}