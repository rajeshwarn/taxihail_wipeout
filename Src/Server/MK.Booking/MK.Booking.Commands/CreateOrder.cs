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
    
        public class BookingSettings
        {

            public string Name { get; set; }

            public string Phone { get; set; }

            public int Passengers { get; set; }

            public int VehicleTypeId { get; set; }

            public int ChargeTypeId { get; set; }

            public int? ProviderId { get; set; }

            public int NumberOfTaxi { get; set; }
        }

        public class PaymentInformation
        {
            public bool PayWithCreditCard { get; set; }
            public Guid CreditCardId { get; set; }
            public decimal? TipAmount { get; set; }
            public decimal? TipPercent { get; set; }
        }
 
    }
}
