using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders", "POST")]    
    public class CreateOrder : BaseDTO
    {

        public CreateOrder()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
            Settings = new BookingSettings();
            Payment = new PaymentSettings();
            Estimate = new RideEstimate();
        }

        public Guid Id { get; set; }
        
        public DateTime? PickupDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }        
         
        public PaymentSettings Payment { get; set; }

        public RideEstimate Estimate { get; set; }                

        public class RideEstimate
        {
            /// <summary>
            /// Price including VAT
            /// </summary>
            public double? Price { get; set; }
            public int Distance { get; set; }
        }
    }
}
