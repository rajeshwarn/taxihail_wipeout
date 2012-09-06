﻿using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AddressRemovedFromHistory : VersionedEvent
    {
        public Guid AddressId { get; set; }
    }
}