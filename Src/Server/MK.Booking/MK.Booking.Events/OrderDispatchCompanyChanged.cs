using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderDispatchCompanyChanged : VersionedEvent
    {
        public string DispatchCompanyName { get; set; }
        public string DispatchCompanyKey { get; set; }
    }
}
