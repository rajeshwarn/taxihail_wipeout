﻿using Infrastructure.EventSourcing;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Events
{
    public class ServiceTypeSettingsUpdated : VersionedEvent
    {
        public ServiceTypeSettings ServiceTypeSettings { get; set; }
    }
}