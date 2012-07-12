using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class Order
    {
        public Guid Id { get; set; }

        public int? IBSOrderId { get; set; }
        
        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

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
