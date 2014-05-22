using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountChargeAddedUpdated : VersionedEvent
    {
        public Guid AccountChargeId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public AccountChargeQuestion[] Questions { get; set; }
    }
}