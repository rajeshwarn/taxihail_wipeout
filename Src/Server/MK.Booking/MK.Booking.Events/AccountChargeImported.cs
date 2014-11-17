using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountChargeImported : VersionedEvent
    {
        public AccountCharge[] AccountCharges { get; set; }
    }
}