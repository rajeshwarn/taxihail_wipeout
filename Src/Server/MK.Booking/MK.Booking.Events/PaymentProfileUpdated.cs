using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class PaymentProfileUpdated : VersionedEvent
    {

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DefaultCreditCard { get; set; }
        public double? DefaultTipAmount { get; set; }
        public double? DefaultTipPercent { get; set; } 
    }
}