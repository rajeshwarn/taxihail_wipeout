#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PaymentProfileUpdated : VersionedEvent
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DefaultCreditCard { get; set; }
        public int? DefaultTipPercent { get; set; }
    }
}