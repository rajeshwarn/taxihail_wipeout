#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class DefaultFavoriteAddressRemoved : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}