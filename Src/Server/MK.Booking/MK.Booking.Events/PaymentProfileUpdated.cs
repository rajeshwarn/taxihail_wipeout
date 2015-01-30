#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PaymentProfileUpdated : VersionedEvent
    {
        public Guid? DefaultCreditCard { get; set; }
        public int? DefaultTipPercent { get; set; }
    }
}