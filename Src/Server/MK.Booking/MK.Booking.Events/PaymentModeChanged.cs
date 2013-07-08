using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Events
{
    public class PaymentModeChanged : VersionedEvent
    {
    }
}
