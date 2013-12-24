#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PaymentInformationSet : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public double? TipAmount { get; set; }
        public double? TipPercent { get; set; }
    }
}