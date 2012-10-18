using System;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class SendBookingConfirmationEmail : ICommand
    {
        public SendBookingConfirmationEmail()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        public BookingSettings Settings { get; set; }

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
    }
}
