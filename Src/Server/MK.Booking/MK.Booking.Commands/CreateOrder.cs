using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class CreateOrder : ICommand
    {
        public CreateOrder()
        {
            Id = Guid.NewGuid();
            Payment = new PaymentInformation();
        }

        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }

        public int IBSOrderId { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public PaymentInformation Payment { get; set; }

        public double? EstimatedFare { get; set; }
        
        public string UserAgent { get; set; }

        public class PaymentInformation
        {
            public bool PayWithCreditCard { get; set; }
            public Guid CreditCardId { get; set; }
            public double? TipAmount { get; set; }
            public double? TipPercent { get; set; }
        }
 
    }
}
