using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CreateOrder : ICommand
    {
        public CreateOrder()
        {
            Id = Guid.NewGuid();         
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }        
        public class Address
        {

            public Guid Id { get; set; }

            public string FriendlyName { get; set; }

            public string FullAddress { get; set; }

            public double Longitude { get; set; }

            public double Latitude { get; set; }

            public string Apartment { get; set; }

            public string RingCode { get; set; }

        }

        public class BookingSettings
        {

            public string Name { get; set; }

            public string Phone { get; set; }

            public int Passengers { get; set; }

            public int VehicleTypeId { get; set; }

            public int ChargeTypeId { get; set; }

            public int ProviderId { get; set; }

            public int NumberOfTaxi { get; set; }
        }
 
    }
}
