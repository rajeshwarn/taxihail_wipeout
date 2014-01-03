#region

using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class FavoriteAddressAdded : VersionedEvent
    {
        public Address Address { get; set; }
    }
}