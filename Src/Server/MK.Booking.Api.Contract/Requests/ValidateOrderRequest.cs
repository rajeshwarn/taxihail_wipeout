using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/orders/validate", "POST")]
    public class ValidateOrderRequest : BaseDTO
    {

        public ValidateOrderRequest()
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
