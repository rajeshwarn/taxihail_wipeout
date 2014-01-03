#region

using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PopularAddressUpdated : VersionedEvent
    {
        public Address Address { get; set; }
    }
}