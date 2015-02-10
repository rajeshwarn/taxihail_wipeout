#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    [Obsolete("This event is not used anymore, DefaultTipPercent is modified with the BookingSettingsUpdated event", false)]
    public class PaymentProfileUpdated : VersionedEvent
    {
        public Guid? DefaultCreditCard { get; set; }
        public int? DefaultTipPercent { get; set; }
    }
}