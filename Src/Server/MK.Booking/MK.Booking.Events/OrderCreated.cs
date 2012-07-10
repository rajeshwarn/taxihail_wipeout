using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Events
{
    public class OrderCreated : VersionedEvent
    {
        public OrderCreated()
        {
            Status = OrderStatus.Created.ToString();
        }
        public Guid AccountId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime RequestedDate { get; set; }
        public string PickupAddress { get; set; }

        public double PickupLongitude { get; set; }

        public double PickupLatitude { get; set; }

        public string PickupApartment { get; set; }

        public string PickupRingCode { get; set; }

        public string DropOffAddress { get; set; }

        public double? DropOffLongitude { get; set; }

        public double? DropOffLatitude { get; set; }

        public string Status { get; set; }
    }
}
