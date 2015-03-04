﻿using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderCancelledBecauseOfError : VersionedEvent
    {
        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }
    }
}