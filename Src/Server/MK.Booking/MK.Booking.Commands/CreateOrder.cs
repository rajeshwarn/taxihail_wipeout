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

        public string PickupAddress { get; set; }

        public double PickupLongitude { get; set; }

        public double PickupLatitude { get; set; }

        public string PickupApartment { get; set; }

        public string PickupRingCode { get; set; }

        public string DropOffAddress { get; set; }

        public double? DropOffLongitude { get; set; }

        public double? DropOffLatitude { get; set; }        
    }
}
