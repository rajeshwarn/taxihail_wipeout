using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime RequestedDateTime { get; set; }

        public string FriendlyName { get; set; }

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public MK.Common.Enumeration.OrderStatus Status { get; set; }
    }
}
