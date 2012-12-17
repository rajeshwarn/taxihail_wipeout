using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PaymentInformationSet: VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? TipPercent { get; set; }
    }
}
