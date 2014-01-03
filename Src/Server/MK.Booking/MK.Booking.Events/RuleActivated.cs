#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class RuleActivated : VersionedEvent
    {
        public Guid RuleId { get; set; }
    }
}