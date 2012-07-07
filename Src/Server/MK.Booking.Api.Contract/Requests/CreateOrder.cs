using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/{AccountId}/orders", "POST")] 
    public class CreateOrder
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public string PickupAddress { get; set; }

        public double PickupLongitude { get; set; }

        public double PickupLatitude { get; set; }

        public string PickupApartment { get; set; }

        public string PickupRingCode { get; set; }

        public string DropoffAddress { get; set; }

        public double? DropoffLongitude { get; set; }

        public double? DropoffLatitude { get; set; }        
        
    }
}
