#region

using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;
using System;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class GratuitySent : VersionedEvent
    {
        public int Percentage { get; set; }
    }
}