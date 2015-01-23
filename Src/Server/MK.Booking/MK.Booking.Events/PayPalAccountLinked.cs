using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PayPalAccountLinked : VersionedEvent
    {
        public string RefreshToken { get; set; }
    }
}
