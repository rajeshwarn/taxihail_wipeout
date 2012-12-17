using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders", "POST")]
    public class CreateOrder : BaseDTO
    {

        public CreateOrder()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
            Payment = new PaymentSettings();
        }

        public Guid Id { get; set; }
        
        public DateTime? PickupDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }        
         
        public PaymentSettings Payment { get; set; }
    }
}
