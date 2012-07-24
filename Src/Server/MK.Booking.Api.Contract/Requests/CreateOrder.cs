using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/{AccountId}/orders", "POST")]
    public class CreateOrder : BaseDTO
    {

        public CreateOrder()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public DateTime? PickupDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }        
         
    }
}
