﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AppSettingsAdded : VersionedEvent
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
