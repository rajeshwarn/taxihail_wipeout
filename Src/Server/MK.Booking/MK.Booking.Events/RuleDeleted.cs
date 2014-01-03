#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class RuleDeleted : VersionedEvent
    {
        public Guid RuleId { get; set; }
    }
}